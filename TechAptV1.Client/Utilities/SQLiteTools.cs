// Copyright © 2025 Always Active Technologies PTY Ltd

using System.Data.SQLite;
using Microsoft.Data.Sqlite;

namespace TechAptV1.Client.Utilities
{
    public static class SQLiteTools
    {

        public static void InitDb(IConfiguration configuration)
        {
            if(!File.Exists("NumbersDb.sqlite"))
            {
                // this creates a zero-byte file
                SQLiteConnection.CreateFile("NumbersDb.sqlite");
            }
            var connectionString = new SqliteConnectionStringBuilder(configuration.GetConnectionString("Default")).ToString();
            SQLiteConnection m_dbConnection = new SQLiteConnection(connectionString);
            m_dbConnection.Open();

            string sql = @"CREATE TABLE IF NOT EXISTS ""Number"" (
                            ""Value"" INTEGER NOT NULL,
                            ""IsPrime"" INTEGER NOT NULL DEFAULT 0
                        );
                        CREATE INDEX IF NOT EXISTS numbers_idx ON Number (Value, IsPrime);
";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
            m_dbConnection.Close();
        }
    }
}
