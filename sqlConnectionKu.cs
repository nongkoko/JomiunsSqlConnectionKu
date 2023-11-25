#if NET6_0_OR_GREATER
using jomiunsExtensions;
#endif
using Microsoft.Data.Sqlite;
using netCore_sqlConnectionKu;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;

namespace JomiunsCom
{
    public partial class sqlConnectionKu : IDisposable
    {
        private sqlCommandKu _cmdLastCommand;
        public enDatabaseType databaseType { get; private set; }

        internal DbConnection theSQLconn { get; set; }

        public sqlConnectionKuInfo SQLconnInfo { get; set; }

#if NET5_0_OR_GREATER
        public object saveDataToTable(string tableName, object toSave, bool getIdentity)
        {
            var retVal = null as object;
            var theType = toSave.GetType();
            var properties = theType.GetProperties();

            var yeah = properties.Select(oo => new
            {
                columnName = oo.Name
                               .Replace("open", "[open]", StringComparison.CurrentCultureIgnoreCase)
                               .Replace("close", "[close]", StringComparison.CurrentCultureIgnoreCase),
                parameter = this.databaseType == enDatabaseType.SQLServer ?
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
            
            var dsOut = this.getSP(SqlCommand)
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
        public static sqlConnectionKu createSQLserver(string instrConnectionString)
        {
            var aTemp = new sqlConnectionKu
            {
                theSQLconn = new System.Data.SqlClient.SqlConnection(instrConnectionString),
                databaseType = enDatabaseType.SQLServer
            };
            return aTemp;
        }
        public static sqlConnectionKu create(sqlConnectionKuInfo incInfo)
        {
            string strConnectionString = $"data source={incInfo.SQLServer}; uid={incInfo.UserName}; password={incInfo.Password}; initial catalog={incInfo.InitialCatalog}";
            var sqlconnReturnValue = new sqlConnectionKu
            {
                SQLconnInfo = incInfo,
                theSQLconn = new System.Data.SqlClient.SqlConnection(strConnectionString),
                databaseType = enDatabaseType.SQLServer
            };
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

            sqlConnectionKu objectku = sqlConnectionKu.create(aInfo);
            objectku.databaseType = enDatabaseType.SQLServer;
            return objectku;
        }

        public sqlCommandKu getSP(string instrNamaSP)
        {
            _cmdLastCommand?.Dispose();
            _cmdLastCommand = null;
            _cmdLastCommand = new sqlCommandKu(instrNamaSP, this);
            return _cmdLastCommand;
        }

        public DataSet getDataSet(System.Reflection.MethodInfo inMethodInfo, ref object[] inListValues)
        {
            var strMethodName = inMethodInfo.Name;
            var intIndex = -1;
            var aDict = new Dictionary<int, DbParameter>();
            DataSet dsResult = null;
            strMethodName = strMethodName.Replace("__", ".");
            var aCommand = getSP(strMethodName);

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


#if NET6_0_OR_GREATER
        public List<T> getAllFromTable<T>(string tableName, object criteriaAND, bool IsWithNolock, params string[] columnNameToSelect)
        {
            var aType = criteriaAND.GetType();
            var properties = aType.GetProperties();
            var yeah = properties.Select(oo => new
            {
                columnName = oo.Name,
                parameter = this.databaseType == enDatabaseType.SQLServer ?
                            (DbParameter)new SqlParameter($"@{oo.Name}", oo.GetValue(criteriaAND, null) ?? DBNull.Value) :
                            (DbParameter)new SqliteParameter($"@{oo.Name}", oo.GetValue(criteriaAND, null) ?? DBNull.Value)
                            
            });

            var arrWhereCriteria = yeah.Select(ooo =>
            {
                if (ooo.parameter.Value == DBNull.Value)
                    return $"{ooo.columnName} is null";
                return $"{ooo.columnName} = {ooo.parameter.ParameterName}";
            }).ToArray();

            var whereCriteria = string.Join(" and ", arrWhereCriteria);
            var listWhereParam = yeah.Select(ooo => ooo.parameter)
                                .Where(oo => oo.Value != DBNull.Value)
                                .ToArray();

            var listColumnNamesToSelect = "";
            if (columnNameToSelect == null || columnNameToSelect.Length < 1)
                listColumnNamesToSelect = "*";
            else
                listColumnNamesToSelect = string.Join(",", columnNameToSelect);

            var WithNolock = IsWithNolock ? "with (nolock)" : "";

            var sqlCommandPart01 = $@"
select {listColumnNamesToSelect} 
from dbo.{tableName} {WithNolock}
";
            var sqlCommand = string.Join(" where ", new[] { sqlCommandPart01, whereCriteria });

            var dsOut = this.getSP(sqlCommand)
                .setCommandTypeAsText()
                .addParams(listWhereParam)
                .getDataSet();
            
            var aTable = dsOut.Tables[0];
            var returnValue = aTable.toListObject<T>();
            return returnValue;
        }
#endif

        public sqlConnectionKuInfo currentSQLinfo()
        {
            return SQLconnInfo;
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

    }

    public enum enDatabaseType
    {
        SQLServer = 1,
        SqLite = 2
    }

}
