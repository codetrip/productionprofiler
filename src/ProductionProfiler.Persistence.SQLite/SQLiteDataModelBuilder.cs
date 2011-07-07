using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;
using ProductionProfiler.Core.Extensions;

namespace ProductionProfiler.Persistence.SQLite
{
    public static class SQLiteDataModelBuilder
    {
        public static void BuildDataModel(SQLiteConfiguration configuration)
        {
            string scriptContents = GetScript();
            var dbFile = ExtractFilename(ConfigurationManager.ConnectionStrings[configuration.ConnectionStringName].ConnectionString); 

            try
            {
                if (File.Exists(dbFile))
                    return;

                using (var connection = new SQLiteConnection(ConfigurationManager.ConnectionStrings[configuration.ConnectionStringName].ConnectionString))
                {
                    using (TransactionScope transaction = new TransactionScope(TransactionScopeOption.Required, TimeSpan.Zero))
                    {
                        connection.Open();

                        string[] scripts = Regex.Split(scriptContents, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.IgnoreCase);

                        foreach (string script in scripts)
                        {
                            if (script.Trim().IsNullOrEmpty())
                                continue;

                            System.Diagnostics.Debug.WriteLine("SCRIPT");
                            System.Diagnostics.Debug.WriteLine("============================================================================================================");
                            System.Diagnostics.Debug.Write(script);
                            System.Diagnostics.Debug.WriteLine("============================================================================================================");

                            using (var command = connection.CreateCommand())
                            {
                                command.CommandTimeout = 36000;
                                command.Connection = connection;
                                command.CommandType = CommandType.Text;
                                command.CommandText = script;
                                command.ExecuteNonQuery();
                            }
                        }

                        transaction.Complete();
                    }
                }
            }
            catch (Exception e)
            {
                File.Delete(dbFile);
                throw;
            }
        }

        private static string ExtractFilename(string connectionString)
        {
            var parser = new DbConnectionStringBuilder { ConnectionString = connectionString };
            return parser["Data Source"].ToString();
        }

        private static string GetScript()
        {
            StringBuilder script = new StringBuilder();

            script.Append(
                @"
                DROP TABLE IF EXISTS ProfiledRequest
                GO
                DROP TABLE IF EXISTS ProfiledRequestData
                GO
                DROP TABLE IF EXISTS ProfiledResponse
                GO
                CREATE TABLE ProfiledRequest(
	                Id uniqueidentifier NOT NULL PRIMARY KEY ASC,
	                Url TEXT NOT NULL UNIQUE,
	                ElapsedMilliseconds INTEGER NULL,
	                ProfilingCount INTEGER NULL,
	                ProfiledOnUtc TEXT NULL,
	                Server TEXT NULL,
	                HttpMethod TEXT NULL,
	                Enabled BOOL NOT NULL DEFAULT 0
                )
                GO

                CREATE UNIQUE INDEX IX_ProfiledRequest_Url ON ProfiledRequest(Url);
                GO
            ");

            script.Append(
                @"
                CREATE TABLE ProfiledRequestData(
	                Id uniqueidentifier NOT NULL PRIMARY KEY ASC,
	                Url TEXT NOT NULL,
	                Data BLOB NOT NULL,
                    CapturedOnUtc TEXT NOT NULL
                ) 
                GO

                CREATE INDEX IX_ProfiledRequestData_Url ON ProfiledRequest(Url);
                GO
            ");

            script.Append(
                @"
                CREATE TABLE ProfiledResponse(
	                Id uniqueidentifier NOT NULL PRIMARY KEY ASC,
	                Url TEXT NOT NULL,
	                Body TEXT NOT NULL
                )
                GO

                CREATE INDEX IX_ProfiledResponse_Url ON ProfiledResponse(Url);
                GO
            ");

            return script.ToString();
        }
    }
}
