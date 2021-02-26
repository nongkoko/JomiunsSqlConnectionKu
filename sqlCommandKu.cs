using System;
using System.Data;
using System.Linq;
using System.Data.SqlClient;

namespace JomiunsCom
{
    public class sqlCommandKu : IDisposable
    {
        private readonly SqlCommand _cmdOleDBcommand;
        private readonly sqlConnectionKu _parentSqlConn;

        public sqlCommandKu(string instrSPName, sqlConnectionKu insqlParentSQLconn)
        {
            this.clearLastCommand();

            _parentSqlConn = insqlParentSQLconn;
            _cmdOleDBcommand = _parentSqlConn.theSQLconn.CreateCommand();
            _cmdOleDBcommand.CommandText = instrSPName;
            _cmdOleDBcommand.CommandType = System.Data.CommandType.StoredProcedure; //defaultnya adalah command SP
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
            if (_parentSqlConn.theSQLconn.State != System.Data.ConnectionState.Open)
            {
                _parentSqlConn.theSQLconn.Open();
            }
        }

        public DataSet getDataSet()
        {
            this.doOpen();
            var dtadptDataAdapter = new SqlDataAdapter(_cmdOleDBcommand);
            var dsReturnValue = new DataSet();
            dtadptDataAdapter.Fill(dsReturnValue);
            this.doClose();
            return dsReturnValue;
        }

        public sqlCommandKu addParams(params (String paramName, object paramValue)[] parameters)
        {
            var aNewProjection = parameters.Select((things) => new SqlParameter(things.paramName, things.paramValue));
            _cmdOleDBcommand.Parameters.AddRange(aNewProjection.ToArray());
            return this;
        }

        public sqlCommandKu addParamWithValue(string instrParamName, object inoValue, Action<SqlParameter> SqlParamCreatedCallBack)
        {
            var odprmReturnValue = _cmdOleDBcommand.Parameters.AddWithValue(instrParamName, inoValue);
            SqlParamCreatedCallBack.Invoke(odprmReturnValue);
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
