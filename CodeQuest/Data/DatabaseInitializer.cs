// Purpose: Creates and seeds the configured CodeQuest database on first use.
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.Hosting;

namespace CodeQuest.Data
{
    internal static class DatabaseInitializer
    {
        private static readonly object InitializationLock = new object();
        private static readonly Regex BatchSeparator = new Regex(
            @"^\s*GO\s*(?:--.*)?$",
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant);

        private static bool initialized;

        public static void EnsureInitialized(string connectionString)
        {
            if (initialized)
            {
                return;
            }

            lock (InitializationLock)
            {
                if (initialized)
                {
                    return;
                }

                if (!ReadBooleanSetting("CodeQuestAutoInitializeDatabase", true))
                {
                    initialized = true;
                    return;
                }

                Initialize(connectionString);
                initialized = true;
            }
        }

        private static void Initialize(string connectionString)
        {
            SqlConnectionStringBuilder applicationBuilder =
                new SqlConnectionStringBuilder(connectionString);

            if (string.IsNullOrWhiteSpace(applicationBuilder.InitialCatalog))
            {
                throw new ConfigurationErrorsException(
                    "CodeQuestDb must specify an Initial Catalog (database name).");
            }

            string databaseName = applicationBuilder.InitialCatalog;
            SqlConnectionStringBuilder setupBuilder =
                new SqlConnectionStringBuilder(applicationBuilder.ConnectionString)
                {
                    InitialCatalog = "master",
                    MultipleActiveResultSets = false
                };

            List<string> scripts = new List<string>
            {
                "CodeQuest_Database.sql",
                "Progress_Extension.sql"
            };

            if (ReadBooleanSetting("CodeQuestSeedDemoContent", true))
            {
                scripts.Add("Seed_Demo_Content.sql");
            }

            using (SqlConnection connection = new SqlConnection(setupBuilder.ConnectionString))
            {
                connection.Open();

                foreach (string scriptName in scripts)
                {
                    ExecuteScript(connection, ReadScript(scriptName), databaseName);
                }
            }
        }

        private static void ExecuteScript(
            SqlConnection connection,
            string script,
            string databaseName)
        {
            string safeIdentifier = "[" + databaseName.Replace("]", "]]") + "]";
            string safeLiteral = "N'" + databaseName.Replace("'", "''") + "'";
            string configuredScript = script
                .Replace("[CodeQuestDB]", safeIdentifier)
                .Replace("N'CodeQuestDB'", safeLiteral);

            foreach (string batch in BatchSeparator.Split(configuredScript))
            {
                if (string.IsNullOrWhiteSpace(batch))
                {
                    continue;
                }

                using (SqlCommand command = new SqlCommand(batch, connection))
                {
                    command.CommandTimeout = 120;
                    command.ExecuteNonQuery();
                }
            }
        }

        private static string ReadScript(string fileName)
        {
            string databaseDirectory = HostingEnvironment.MapPath("~/Database");
            if (string.IsNullOrWhiteSpace(databaseDirectory))
            {
                databaseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database");
            }

            string path = Path.Combine(databaseDirectory, fileName);
            if (!File.Exists(path))
            {
                throw new ConfigurationErrorsException(
                    "Database setup script was not found: " + path);
            }

            return File.ReadAllText(path);
        }

        private static bool ReadBooleanSetting(string key, bool defaultValue)
        {
            string value = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrWhiteSpace(value))
            {
                return defaultValue;
            }

            bool parsedValue;
            if (!bool.TryParse(value, out parsedValue))
            {
                throw new ConfigurationErrorsException(
                    "App setting '" + key + "' must be true or false.");
            }

            return parsedValue;
        }
    }
}
