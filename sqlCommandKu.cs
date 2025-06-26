using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
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

        internal sqlCommandKu(string instrSPName, sqlConnectionKu insqlParentSQLconn)
        {
            clearLastCommand();

            _parentSqlConn = insqlParentSQLconn;
            _cmdOleDBcommand = _parentSqlConn.theSQLconn.CreateCommand();
            _cmdOleDBcommand.CommandText = instrSPName;
            if (insqlParentSQLconn.databaseType != enDatabaseType.SqLite)
            {
                _cmdOleDBcommand.CommandType = System.Data.CommandType.StoredProcedure; //defaultnya adalah command SP
            }
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
            doOpen();
            var dtadptDataAdapter = new Microsoft.Data.SqlClient.SqlDataAdapter(_cmdOleDBcommand as Microsoft.Data.SqlClient.SqlCommand);
            var dsReturnValue = new DataSet();
            _ = dtadptDataAdapter.Fill(dsReturnValue);
            doClose();
            return dsReturnValue;
        }

        public sqlCommandKu addParams(params IDbDataParameter[] parameter)
        {
            if (parameter == null)
                return this;
            _cmdOleDBcommand.Parameters.AddRange(parameter);
            return this;
        }

        public sqlCommandKu addParamsFromObject(object parameterAndValues)
        {
            var aType = parameterAndValues.GetType();
            var properties = aType.GetProperties();

            foreach (var oo in properties)
            {
                if (_parentSqlConn.databaseType == enDatabaseType.SQLServer)
                {
                    var aValue = oo.GetValue(parameterAndValues, null) ?? DBNull.Value;
                    _cmdOleDBcommand.Parameters.Add(new SqlParameter($"@{oo.Name}", aValue));
                }

                if (_parentSqlConn.databaseType == enDatabaseType.SqLite)
                {
                    var aValue = oo.GetValue(parameterAndValues, null) ?? DBNull.Value;
                    _cmdOleDBcommand.Parameters.Add(new SqliteParameter($"@{oo.Name}", aValue));
                }
            }

            return this;
        }

        public sqlCommandKu addParams(params (string paramName, object paramValue)[] parameters)
        {
            System.Collections.Generic.IEnumerable<DbParameter> aNewProjection =
                parameters.Select((things) =>
               {
                   return _parentSqlConn.databaseType == enDatabaseType.SQLServer
                       ? new SqlParameter(things.paramName, things.paramValue)
                       : _parentSqlConn.databaseType == enDatabaseType.SqLite
                       ? new Microsoft.Data.Sqlite.SqliteParameter(things.paramName, things.paramValue)
                       : (DbParameter)null;
               });

            _cmdOleDBcommand.Parameters.AddRange(aNewProjection.ToArray());
            return this;
        }

        public sqlCommandKu addParamWithValue(string instrParamName, object inoValue, Action<DbParameter> SqlParamCreatedCallBack)
        {
            DbParameter odprmReturnValue = null;
            if (_parentSqlConn.databaseType == enDatabaseType.SQLServer)
            {
                odprmReturnValue = new Microsoft.Data.SqlClient.SqlParameter(instrParamName, inoValue);
            }

            if (_parentSqlConn.databaseType == enDatabaseType.SqLite)
            {
                odprmReturnValue = new Microsoft.Data.Sqlite.SqliteParameter(instrParamName, inoValue);
            }

            this.addParams(odprmReturnValue);
            SqlParamCreatedCallBack?.Invoke(odprmReturnValue);
            return this;
        }

        public sqlCommandKu addParamWithValue(string instrParamName, object inoValue)
        {
            return addParamWithValue(true, instrParamName, inoValue);
        }

        public sqlCommandKu addParamWithValue(bool trueToAdd, string instrParamName, object inoValue)
        {
            return trueToAdd ? addParamWithValue(instrParamName, inoValue, null) : this;
        }

        public T executeScalar<T>()
        {
            doOpen();
            object aObject = _cmdOleDBcommand.ExecuteScalar();
            doClose();
            return (T)aObject;
        }

        public void executeNonQuery(bool inblnAutoClose)
        {
            doOpen();
            _ = _cmdOleDBcommand.ExecuteNonQuery();

            if (inblnAutoClose)
            {
                doClose();
            }
        }

        public void executeNonQuery()
        {
            executeNonQuery(true);
        }

        private void clearLastCommand()
        {
            _cmdOleDBcommand?.Dispose();
        }

        public void Dispose()
        {
            //throw new Exception("The method or operation is not implemented.");
            // lakukan bersih bersih disini
            clearLastCommand();
        }
    }
}
