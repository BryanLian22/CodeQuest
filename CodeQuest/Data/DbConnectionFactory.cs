// Purpose: Creates SQL Server connections from the single CodeQuestDb configuration entry.
using System;
using System.Configuration;
using System.Data.SqlClient;

namespace CodeQuest.Data
{
    /// <summary>
    /// Creates SQL Server connections using the CodeQuestDb connection string
    /// in Web.config. Keep connection strings out of page code.
    /// </summary>
    public static class DbConnectionFactory
    {
        public static SqlConnection Create()
        {
            ConnectionStringSettings settings =
                ConfigurationManager.ConnectionStrings["CodeQuestDb"];

            if (settings == null || string.IsNullOrWhiteSpace(settings.ConnectionString))
            {
                throw new ConfigurationErrorsException(
                    "Add a connection string named 'CodeQuestDb' to Web.config.");
            }

            return new SqlConnection(settings.ConnectionString);
        }
    }
}
