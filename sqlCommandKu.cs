using System;
using System.Data;
using System.Data.Common;
using System.Linq;


namespace JomiunsCom
{
    public partial class sqlCommandKu : IDisposable
    {
        private readonly DbCommand _cmdOleDBcommand;
        private readonly sqlConnectionKu _parentSqlConn;

        public sqlCommandKu(string instrSPName, sqlConnectionKu insqlParentSQLconn)
        {
            this.clearLastCommand();

            _parentSqlConn = insqlParentSQLconn;
            _cmdOleDBcommand = _parentSqlConn.theSQLconn.CreateCommand();
            _cmdOleDBcommand.CommandText = instrSPName;
            if (insqlParentSQLconn.databaseType != enDatabaseType.SqLite) _cmdOleDBcommand.CommandType = System.Data.CommandType.StoredProcedure; //defaultnya adalah command SP
        }

        public sqlCommandKu setCommandTypeAsText()
        {
            _cmdOleDBcommand.CommandType = System.Data.CommandType.Text;
            return this;
        }

        

        private void doClose()
        {
            if (_parentSqlConn.theSQLconn.State == ConnectionState.Open)
            {
                _parentSqlConn.theSQLconn.Close();
            }
        }

        private void doOpen()
        {
            if (_parentSqlConn.theSQLconn.State != System.Data.ConnectionState.Open) _parentSqlConn.theSQLconn.Open();
        }

        public DataSet getDataSet()
        {
            this.doOpen();
            var dtadptDataAdapter = new System.Data.SqlClient.SqlDataAdapter(_cmdOleDBcommand as System.Data.SqlClient.SqlCommand);
            var dsReturnValue = new DataSet();
            dtadptDataAdapter.Fill(dsReturnValue);
            this.doClose();
            return dsReturnValue;
        }

        public sqlCommandKu addParams(params (String paramName, object paramValue)[] parameters)
        {
            System.Collections.Generic.IEnumerable<DbParameter> aNewProjection = 
                parameters.Select<(String paramName, object paramValue), DbParameter> ((things) =>
                {
                    if (this._parentSqlConn.databaseType == enDatabaseType.SQLServer) return new System.Data.SqlClient.SqlParameter(things.paramName, things.paramValue);
                    if (this._parentSqlConn.databaseType == enDatabaseType.SqLite) return new Microsoft.Data.Sqlite.SqliteParameter(things.paramName, things.paramValue);
                    return null;
                });

            (_cmdOleDBcommand.Parameters as DbParameterCollection).AddRange(aNewProjection.ToArray());
            return this;
        }

        public sqlCommandKu addParamWithValue(string instrParamName, object inoValue, Action<DbParameter> SqlParamCreatedCallBack)
        {
            DbParameter odprmReturnValue = null;
            if (this._parentSqlConn.databaseType == enDatabaseType.SQLServer) odprmReturnValue = new System.Data.SqlClient.SqlParameter(instrParamName, inoValue);
            if (this._parentSqlConn.databaseType == enDatabaseType.SqLite) odprmReturnValue = new Microsoft.Data.Sqlite.SqliteParameter(instrParamName, inoValue);

            _cmdOleDBcommand.Parameters.Add(odprmReturnValue);
            SqlParamCreatedCallBack?.Invoke(odprmReturnValue);
            return this;
        }

        public sqlCommandKu addParamWithValue(string instrParamName, object inoValue)
        {
            return this.addParamWithValue(instrParamName, inoValue, null);
        }

        public T executeScalar<T>()
        {
            this.doOpen();
            var aObject = _cmdOleDBcommand.ExecuteScalar();
            this.doClose();
            return (T)aObject;
        }

        private void clearLastCommand()
        {
            if (this._cmdOleDBcommand != null)
            {
                this._cmdOleDBcommand.Dispose();
            }
        }

        public void Dispose()
        {
            //throw new Exception("The method or operation is not implemented.");
            // lakukan bersih bersih disini
            this.clearLastCommand();
        }
    }
}
