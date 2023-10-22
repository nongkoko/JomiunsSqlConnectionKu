using netCore_sqlConnectionKu;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace JomiunsCom
{
    public partial class sqlConnectionKu : IDisposable
    {
        private sqlCommandKu _cmdLastCommand;
        public enDatabaseType databaseType { get; private set; }

        internal DbConnection theSQLconn { get; set; }

        public sqlConnectionKuInfo SQLconnInfo { get; set; }

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
            string strMethodName = inMethodInfo.Name;
            int intIndex = -1;
            var aDict = new Dictionary<int, DbParameter>();
            DataSet dsResult = null;
            strMethodName = strMethodName.Replace("__", ".");
            sqlCommandKu aCommand = getSP(strMethodName);
            foreach (System.Reflection.ParameterInfo aParameterInfo in inMethodInfo.GetParameters())
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

        public sqlConnectionKuInfo currentSQLinfo()
        {
            return SQLconnInfo;
        }

        public void Dispose()
        {
            _cmdLastCommand?.Dispose();

            if (theSQLconn != null)
            {
                if (theSQLconn.State == System.Data.ConnectionState.Open)
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
