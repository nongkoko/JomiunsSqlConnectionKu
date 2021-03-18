using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.Sqlite;

namespace JomiunsCom
{
    public partial class sqlConnectionKu //for sqlite
    {
        public static sqlConnectionKu create(string instrSqLiteDBpath)
        {
            var aReturnValue = new sqlConnectionKu();

            var connectionStringBuilder = new SqliteConnectionStringBuilder();
            connectionStringBuilder.DataSource = instrSqLiteDBpath;
            aReturnValue.theSQLconn = new SqliteConnection(connectionStringBuilder.ConnectionString);
            aReturnValue.databaseType = enDatabaseType.SqLite;
            return aReturnValue;
        }
    }
}
