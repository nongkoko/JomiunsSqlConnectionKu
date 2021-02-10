using System;
using System.Collections.Generic;
using System.Data;

namespace lib_sqlConnectionKu
{
    public partial class sqlConnectionKu : IDisposable
    {
        private sqlCommandKu _cmdLastCommand;

        public System.Data.SqlClient.SqlConnection theSQLconn { get; set; }

        public sqlConnectionKuInfo  SQLconnInfo { get; set; }

        public static sqlConnectionKu create(sqlConnectionKuInfo incInfo)
        {
            var strConnectionString = $"data source={incInfo.SQLServer}; uid={incInfo.UserName}; password={incInfo.Password}; initial catalog={incInfo.InitialCatalog}";
            var sqlconnReturnValue = new sqlConnectionKu
            {
                SQLconnInfo = incInfo,
                theSQLconn = new System.Data.SqlClient.SqlConnection(strConnectionString)
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
            return objectku;
        }

        public sqlCommandKu getSP(string instrNamaSP)
        {
            this._cmdLastCommand = null;
            this._cmdLastCommand = new sqlCommandKu(instrNamaSP, this);
            return this._cmdLastCommand;
        }

        public DataSet getDataSet(System.Reflection.MethodInfo inMethodInfo, ref object[] inListValues)
        {
            string strMethodName = inMethodInfo.Name;
            int intIndex = -1;
            Dictionary<int, System.Data.SqlClient.SqlParameter> aDict = new Dictionary<int, System.Data.SqlClient.SqlParameter>();
            DataSet dsResult = null;
            strMethodName = strMethodName.Replace("__", ".");
            sqlCommandKu aCommand = this.getSP(strMethodName);
            foreach (System.Reflection.ParameterInfo aParameterInfo in inMethodInfo.GetParameters())
            {
                intIndex++;
                System.Data.SqlClient.SqlParameter aParam = aCommand.AddParamWithValue($"@{aParameterInfo.Name}", inListValues[intIndex]);
                if (aParameterInfo.ParameterType.IsByRef)
                {
                    aParam.Direction = ParameterDirection.InputOutput;
                    aParam.Size = -1;
                }

                aDict.Add(intIndex, aParam);
            }

            dsResult = aCommand.GetDataSet();
            foreach (KeyValuePair<int, System.Data.SqlClient.SqlParameter> kvp in aDict)
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
                if (this.theSQLconn.State == System.Data.ConnectionState.Open )
                    this.theSQLconn.Close();
                this.theSQLconn.Dispose();
            }            
        }
    }
}
