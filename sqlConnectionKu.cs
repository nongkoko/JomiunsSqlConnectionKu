#if NET6_0_OR_GREATER
using ImpromptuInterface;
using jomiunsExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Identity.Client;

#endif
using netCore_sqlConnectionKu;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace JomiunsCom;

public interface iSqlConnectionKu
{
    DbConnection theSQLconn { get; set; }
    sqlCommandKu getSP(string instrNamaSP);
    enDatabaseType databaseType { get; internal set; }
    sqlConnectionKuInfo SQLconnInfo { get; set; }
#if NET6_0_OR_GREATER
    List<T> getAllFromTable<T>(string tableName, object criteriaAND, bool IsWithNolock, params string[] columnNameToSelect);
    object saveDataToTable(string tableName, object toSave, bool getIdentity);
#endif
}

public class factory()
{
    public static iSqlConnectionKu createSQLserver(string instrConnectionString)
    {
        var aTemp = new sqlConnectionKu
        {
            theSQLconn = new Microsoft.Data.SqlClient.SqlConnection(instrConnectionString),
        };
        (aTemp as iSqlConnectionKu).databaseType = enDatabaseType.SQLServer;
        return aTemp;
    }

    public static sqlConnectionKu create(sqlConnectionKuInfo incInfo)
    {
        string strConnectionString = $"data source={incInfo.SQLServer}; uid={incInfo.UserName}; password={incInfo.Password}; initial catalog={incInfo.InitialCatalog}";
        var sqlconnReturnValue = new sqlConnectionKu
        {
            theSQLconn = new Microsoft.Data.SqlClient.SqlConnection(strConnectionString),
        };
        var aTemp = sqlconnReturnValue as iSqlConnectionKu;
        aTemp.SQLconnInfo = incInfo;
        aTemp.databaseType = enDatabaseType.SQLServer;
        return sqlconnReturnValue;
    }

    public static sqlConnectionKu create(string instrServerDanInstance, string instrUsername, string instrPassword, string instrInitialCatalog)
    {
        var aInfo = new sqlConnectionKuInfo()
        {
            SQLServer = instrServerDanInstance,
            UserName = instrUsername,
            Password = instrPassword,
            InitialCatalog = instrInitialCatalog
        };

        var objectku = factory.create(aInfo);
        (objectku as iSqlConnectionKu).databaseType = enDatabaseType.SQLServer;
        return objectku;
    }
#if NET6_0_OR_GREATER
    public static sqlConnectionKu createSQLlite(string instrSqLiteDBpath)
    {
        var aReturnValue = new sqlConnectionKu();

        var connectionStringBuilder = new SqliteConnectionStringBuilder()
        {
            DataSource = instrSqLiteDBpath
        };
        aReturnValue.theSQLconn = new SqliteConnection(connectionStringBuilder.ConnectionString);
        (aReturnValue as iSqlConnectionKu).databaseType = enDatabaseType.SqLite;
        return aReturnValue;
    }
#endif
}

public class sqlConnectionKu : IDisposable, iSqlConnectionKu
{
    private sqlCommandKu _cmdLastCommand;

    internal DbConnection theSQLconn { get; set; }


    enDatabaseType iSqlConnectionKu.databaseType { get; set; }
    DbConnection iSqlConnectionKu.theSQLconn { get; set; }
    sqlConnectionKuInfo iSqlConnectionKu.SQLconnInfo { get; set; }

    public DataSet getDataSet(System.Reflection.MethodInfo inMethodInfo, ref object[] inListValues)
    {
        var strMethodName = inMethodInfo.Name;
        var intIndex = -1;
        var aDict = new Dictionary<int, DbParameter>();
        DataSet dsResult = null;
        strMethodName = strMethodName.Replace("__", ".");
        var aCommand = (this as iSqlConnectionKu).getSP(strMethodName);

        foreach (var aParameterInfo in inMethodInfo.GetParameters())
        {
            intIndex++;
            _ = aCommand.addParamWithValue(
                instrParamName: $"@{aParameterInfo.Name}",
                inoValue: inListValues[intIndex],
                SqlParamCreatedCallBack: (agas) =>
                {
                    if (aParameterInfo.ParameterType.IsByRef)
                    {
                        agas.Direction = ParameterDirection.InputOutput;
                        agas.Size = -1;
                    }
                    aDict.Add(intIndex, agas);
                });
        }

        dsResult = aCommand.getDataSet();
        foreach (KeyValuePair<int, DbParameter> kvp in aDict)
        {
            inListValues[kvp.Key] = kvp.Value.Value;
        }
        return dsResult;
    }

    public void Dispose()
    {
        _cmdLastCommand?.Dispose();

        if (theSQLconn != null)
        {
            if (theSQLconn.State == ConnectionState.Open)
            {
                theSQLconn.Close();
            }

            theSQLconn.Dispose();
        }
    }

    sqlCommandKu iSqlConnectionKu.getSP(string instrNamaSP)
    {
        _cmdLastCommand?.Dispose();
        _cmdLastCommand = null;
        _cmdLastCommand = new sqlCommandKu(instrNamaSP, this);
        return _cmdLastCommand;
    }

#if NET6_0_OR_GREATER
    List<T> iSqlConnectionKu.getAllFromTable<T>(string tableName, object criteriaAND, bool IsWithNolock, params string[] columnNameToSelect)
    {
        var querySelectPart01ResolverInstance = new querySelectPart01Resolver(Impromptu.ActLike<iSomething>(new { tableName, IsWithNolock, columnNameToSelect }));
        var part01 = querySelectPart01ResolverInstance.solvePart01();
        var aList = new List<string> { part01 };
        var part02resolver = new selectQueryPart02Resolver(Impromptu.ActLike<iSelectQueryPart02>(new { criteriaAND, ((iSqlConnectionKu)this).databaseType }));
        var (whereCriteriaResult, listParamResult) = part02resolver.solvePart02();

        if (whereCriteriaResult.isNotNullOrEmpty())
            aList.Add(whereCriteriaResult);

        var sqlCommand = string.Join(" where ", aList);

        var dsOut = (this as iSqlConnectionKu).getSP(sqlCommand)
            .setCommandTypeAsText()
            .addParams(listParamResult)
            .getDataSet();

        var aTable = dsOut.Tables[0];
        var returnValue = aTable.toListObject<T>();
        return returnValue;
    }

    object iSqlConnectionKu.saveDataToTable(string tableName, object toSave, bool getIdentity)
    {
        var retVal = null as object;
        var theType = toSave.GetType();
        var properties = theType.GetProperties();

        var yeah = properties.Select(oo => new
        {
            columnName = oo.Name
                           .Replace("open", "[open]", StringComparison.CurrentCultureIgnoreCase)
                           .Replace("close", "[close]", StringComparison.CurrentCultureIgnoreCase),
            parameter = (this as iSqlConnectionKu).databaseType == enDatabaseType.SQLServer ?
                        (DbParameter)new SqlParameter($"@{oo.Name}", oo.GetValue(toSave, null) ?? DBNull.Value) :
                        (DbParameter)new SqliteParameter($"@{oo.Name}", oo.GetValue(toSave, null) ?? DBNull.Value)
        });

        var columnName = string.Join(",", yeah.Select(oo => oo.columnName).ToArray());
        var paramName = string.Join(",", yeah.Select(oo => oo.parameter.ParameterName).ToArray());
        var listParam = yeah.Select(oo => oo.parameter).ToList();

        var SqlCommand = $@"
insert into dbo.{tableName} ({columnName})
select {paramName}";

        if (getIdentity)
        {
            var temp = new[] { SqlCommand, "select @@IDENTITY" };
            SqlCommand = string.Join(";" + Environment.NewLine, temp);
        }

        var dsOut = ((iSqlConnectionKu)this).getSP(SqlCommand)
                        .setCommandTypeAsText()
                        .addParams(listParam.ToArray())
                        .getDataSet();

        if (getIdentity)
        {
            var dtTable = dsOut.Tables[0];
            var drRow = dtTable.Rows[0];
            retVal = drRow[0];
        }

        return retVal;
    }
#endif
}

public enum enDatabaseType
{
    SQLServer = 1,
    SqLite = 2
}
