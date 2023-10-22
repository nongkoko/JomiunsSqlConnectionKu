using Microsoft.Data.Sqlite;

namespace JomiunsCom
{
    public partial class sqlConnectionKu //for sqlite
    {
        public static sqlConnectionKu create(string instrSqLiteDBpath)
        {
            var aReturnValue = new sqlConnectionKu();

            var connectionStringBuilder = new SqliteConnectionStringBuilder()
            {
                DataSource = instrSqLiteDBpath
            };
            aReturnValue.theSQLconn = new SqliteConnection(connectionStringBuilder.ConnectionString);
            aReturnValue.databaseType = enDatabaseType.SqLite;
            return aReturnValue;
        }
    }
}
