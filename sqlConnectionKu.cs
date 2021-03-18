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
            var strConnectionString = $"data source={incInfo.SQLServer}; uid={incInfo.UserName}; password={incInfo.Password}; initial catalog={incInfo.InitialCatalog}";
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

            var objectku = sqlConnectionKu.create(aInfo);
            objectku.databaseType = enDatabaseType.SQLServer;
            return objectku;
        }

        public sqlCommandKu getSP(string instrNamaSP)
        {
            this._cmdLastCommand?.Dispose();
            this._cmdLastCommand = null;
            this._cmdLastCommand = new sqlCommandKu(instrNamaSP, this);
            return this._cmdLastCommand;
        }

        public DataSet getDataSet(System.Reflection.MethodInfo inMethodInfo, ref object[] inListValues)
        {
            var strMethodName = inMethodInfo.Name;
            var intIndex = -1;
            var aDict = new Dictionary<int, DbParameter>();
            var dsResult = (DataSet)null;
            strMethodName = strMethodName.Replace("__", ".");
            var aCommand = this.getSP(strMethodName);
            foreach (var aParameterInfo in inMethodInfo.GetParameters())
            {
                intIndex++;
                aCommand.addParamWithValue(
                    instrParamName: $"@{aParameterInfo.Name}",
                    inoValue: inListValues[intIndex],
                    SqlParamCreatedCallBack: (agas) => {
                        if (aParameterInfo.ParameterType.IsByRef)
                        {
                            agas.Direction = ParameterDirection.InputOutput;
                            agas.Size = -1;
                        }
                        aDict.Add(intIndex, agas);
                    });
            }

            dsResult = aCommand.getDataSet();
            foreach (var kvp in aDict)
            {
                inListValues[kvp.Key] = kvp.Value.Value;
            }
            return dsResult;
        }

        public sqlConnectionKuInfo currentSQLinfo()
        {
            return this.SQLconnInfo;
        }

        public void Dispose()
        {
            if (_cmdLastCommand != null)
            {
                _cmdLastCommand.Dispose();
            }

            if (this.theSQLconn != null)
            {
                if (this.theSQLconn.State == System.Data.ConnectionState.Open)
                    this.theSQLconn.Close();
                this.theSQLconn.Dispose();
            }
        }

    }

    public enum enDatabaseType
    {
        SQLServer = 1,
        SqLite = 2
    }

}
