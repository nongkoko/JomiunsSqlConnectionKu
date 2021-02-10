using System;
using System.Data;
using System.Data.SqlClient;

namespace lib_sqlConnectionKu
{
    public class sqlCommandKu : IDisposable
    {
        private SqlCommand _cmdOleDBcommand;
        private sqlConnectionKu _parentSqlConn;

        public sqlCommandKu(string instrNamaSP, sqlConnectionKu insqlParentSQLconn)
        {
            this.clearLastCommand();

            _parentSqlConn = insqlParentSQLconn;
            _cmdOleDBcommand = _parentSqlConn.theSQLconn.CreateCommand();
            _cmdOleDBcommand.CommandText = instrNamaSP;
            _cmdOleDBcommand.CommandType = System.Data.CommandType.StoredProcedure; //defaultnya adalah command SP
        }

        public sqlCommandKu setCommandTypeMenjadiText()
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

        public DataSet GetDataSet()
        {
            this.doOpen();
            SqlDataAdapter dtadptDataAdapter = new SqlDataAdapter(_cmdOleDBcommand);
            DataSet dsReturnValue = new DataSet();
            dtadptDataAdapter.Fill(dsReturnValue);
            this.doClose();
            return dsReturnValue;
        }

        public SqlParameter AddParamWithValue(string instrParamName, object inoValue)
        {
            SqlParameter odprmReturnValue;
            odprmReturnValue = _cmdOleDBcommand.Parameters.AddWithValue(instrParamName, inoValue);
            return odprmReturnValue;
        }

        public T executeScalar<T>()
        {
            this.doOpen();
            object aObject = _cmdOleDBcommand.ExecuteScalar();
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
