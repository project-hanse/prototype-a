using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace PipelineService.Helper
{
    public static class DatabaseHelper
    {
        /// <summary>
        /// Creates a Sqlite connection string from a configured connection string.
        /// </summary>
        public static string GetSqLiteConnectionString(
            string key,
            IConfiguration configuration,
            string defaultConnectionString = "DataSource=app.db")
        {
            var connectionString = configuration.GetConnectionString(key);

            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = defaultConnectionString;
            }

            return new SqliteConnectionStringBuilder(connectionString)
            {
                Mode = SqliteOpenMode.ReadWriteCreate
            }.ToString();
        }
    }
}