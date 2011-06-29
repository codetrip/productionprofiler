using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;
using ProductionProfiler.Core.Extensions;

namespace ProductionProfiler.Persistence.SqlServer
{
    public static class DataModelBuilder
    {
        public static void BuildDataModel(SqlConfiguration configuration)
        {
            using (TransactionScope transaction = new TransactionScope(TransactionScopeOption.Required, TimeSpan.Zero))
            {
                string[] scripts = Regex.Split(GetScript(configuration), @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.IgnoreCase);

                foreach (string script in scripts)
                {
                    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings[configuration.ConnectionStringName].ConnectionString))
                    {
                        connection.Open();

                        using (SqlCommand command = new SqlCommand())
                        {
                            command.CommandTimeout = 36000;
                            command.Connection = connection;
                            command.CommandType = CommandType.Text;
                            command.CommandText = script;
                            command.ExecuteNonQuery();
                        }
                    }
                }

                transaction.Complete();
            }
        }

        private static string GetScript(SqlConfiguration configuration)
        {
            StringBuilder script = new StringBuilder();

            if(configuration.SchemaName.IsNotNullOrEmpty())
            {
                script.AppendLine("IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'{0}')".FormatWith(configuration.SchemaName));
                script.AppendLine("CREATE SCHEMA [{0}] AUTHORIZATION [dbo]".FormatWith(configuration.SchemaName));
                script.AppendLine("GO");
            }

            string schema = configuration.SchemaName.IsNullOrEmpty() ? "dbo" : configuration.SchemaName;

            script.Append(
                @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[{0}].[ProfiledRequest]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [{0}].[ProfiledRequest](
	                    [Id] [uniqueidentifier] NOT NULL,
	                    [Url] [nvarchar](1024) NOT NULL UNIQUE,
	                    [ElapsedMilliseconds] [bigint] NOT NULL,
	                    [ProfilingCount] [int] NOT NULL,
	                    [ProfiledOnUtc] [datetime] NULL,
	                    [Server] [nvarchar](128) NOT NULL,
	                    [HttpMethod] [nvarchar](8) NOT NULL,
	                    [Enabled] [bit] NOT NULL DEFAULT 0
                    PRIMARY KEY NONCLUSTERED 
                    (
	                    [Id] ASC
                    )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
                    ) ON [PRIMARY]
                END
                GO

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[{0}].[ProfiledRequest]') AND name = N'IX_ProfiledRequest_Url')
                BEGIN
                    CREATE CLUSTERED INDEX [IX_ProfiledRequest_Url] ON [{0}].[ProfiledRequest]
                    (
	                    [Url] ASC
                    )
                END
                GO
            ".FormatWith(schema));

            script.Append(
                @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[{0}].[ProfiledRequestData]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [{0}].[ProfiledRequestData](
	                    [Id] [uniqueidentifier] NOT NULL,
	                    [Url] [nvarchar](1024) NOT NULL,
	                    [Data] [varbinary](max) NOT NULL
                    PRIMARY KEY NONCLUSTERED 
                    (
	                    [Id] ASC
                    )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
                    ) ON [PRIMARY]
                END
                GO

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[{0}].[ProfiledRequestData]') AND name = N'IX_ProfiledRequestData_Url')
                BEGIN
                    CREATE CLUSTERED INDEX [IX_ProfiledRequestData_Url] ON [{0}].[ProfiledRequestData]
                    (
	                    [Url] ASC
                    )
                END
                GO
            ".FormatWith(schema));

            script.Append(
                @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[{0}].[ProfiledResponse]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [{0}].[ProfiledResponse](
	                    [Id] [uniqueidentifier] NOT NULL,
	                    [Url] [nvarchar](1024) NOT NULL,
	                    [Data] [nvarchar](max) NOT NULL
                    PRIMARY KEY NONCLUSTERED 
                    (
	                    [Id] ASC
                    )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
                    ) ON [PRIMARY]
                END
                GO

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[{0}].[ProfiledResponse]') AND name = N'IX_ProfiledResponse_Url')
                BEGIN
                    CREATE CLUSTERED INDEX [IX_ProfiledResponse_Url] ON [{0}].[ProfiledResponse]
                    (
	                    [Url] ASC
                    )
                END
                GO
            ".FormatWith(schema));

            return script.ToString();
        }
    }
}
