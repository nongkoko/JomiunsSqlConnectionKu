using System;

namespace JomiunsCom
{
    public partial class sqlCommandKu
    {
        public void doSqliteExecuteReader(Action<Microsoft.Data.Sqlite.SqliteDataReader> onGotRecordSet, bool inblnAutoCloseConn)
        {
            doOpen();
            using (System.Data.Common.DbDataReader reader = _cmdOleDBcommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    onGotRecordSet?.Invoke(reader as Microsoft.Data.Sqlite.SqliteDataReader);
                }
            }

            if (inblnAutoCloseConn)
            {
                doClose();
            }
        }

        public void doSqliteExecuteReader(Action<Microsoft.Data.Sqlite.SqliteDataReader> onGotRecordSet)
        {
            doSqliteExecuteReader(onGotRecordSet, true);
        }
    }
}
