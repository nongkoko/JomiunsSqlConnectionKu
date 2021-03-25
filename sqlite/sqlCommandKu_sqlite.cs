using System;

namespace JomiunsCom
{
    public partial class sqlCommandKu
    {
        public void doSqliteExecuteReader(Action<Microsoft.Data.Sqlite.SqliteDataReader> onGotRecordSet, bool inblnAutoCloseConn)
        {
            this.doOpen();
            using (var reader = _cmdOleDBcommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    onGotRecordSet?.Invoke(reader as Microsoft.Data.Sqlite.SqliteDataReader);
                }
            }

            if (inblnAutoCloseConn) this.doClose();
        }

        public void doSqliteExecuteReader(Action<Microsoft.Data.Sqlite.SqliteDataReader> onGotRecordSet)
        {
            this.doSqliteExecuteReader(onGotRecordSet, true);
        }
    }
}
