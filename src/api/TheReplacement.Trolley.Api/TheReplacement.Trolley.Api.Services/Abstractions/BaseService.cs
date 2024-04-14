using MongoDB.Driver;
using TheReplacement.Trolley.Api.Services.Enums;

namespace TheReplacement.Trolley.Api.Services.Abstractions
{
    public abstract class BaseService
    {
        private const string ConnectionStringEnvVarName = "MongoDBConnectionString";
        private const string DatabaseEnvVarName = "TrialByTrolleyDatabase";

        internal static IMongoCollection<T> GetMongoCollection<T>(MongoCollection collection)
        {
            var connectionString = Environment.GetEnvironmentVariable(ConnectionStringEnvVarName, EnvironmentVariableTarget.Process);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new NullReferenceException($"{ConnectionStringEnvVarName} environment variable need to be set to access MongoDB");
            }

            var settings = MongoClientSettings.FromConnectionString(connectionString);
            settings.SslSettings = new SslSettings()
            {
                EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12
            };

            var client = new MongoClient(settings);
            var databaseName = Environment.GetEnvironmentVariable(DatabaseEnvVarName, EnvironmentVariableTarget.Process);
            var database = client.GetDatabase(databaseName);

            return database.GetCollection<T>(collection.ToString());
        }
    }
}
