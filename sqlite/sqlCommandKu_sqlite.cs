using System;

namespace JomiunsCom
{
    public partial class sqlCommandKu
    {
        public void doSqliteExecuteReader(Action<Microsoft.Data.Sqlite.SqliteDataReader> onGotRecordSet)
        {
            this._parentSqlConn.theSQLconn.Open();
            using (var reader = _cmdOleDBcommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    onGotRecordSet?.Invoke(reader as Microsoft.Data.Sqlite.SqliteDataReader);
                }
            }
            this._parentSqlConn.theSQLconn.Close();
        }
    }
}
