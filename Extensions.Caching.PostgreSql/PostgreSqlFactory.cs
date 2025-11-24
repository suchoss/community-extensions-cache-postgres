using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace HlidacStatu.Caching.PostgreSql
{
    public sealed class PostgreSqlFacory
    {
        public static IDistributedCache Create(PostgreSqlCacheOptions options, ILoggerFactory loggerFactory)
        {
            IOptions<PostgreSqlCacheOptions> opt = Options.Create(options);
            
            ILogger<DatabaseOperations> dbopsLogger = loggerFactory.CreateLogger<DatabaseOperations>();
            
            DatabaseOperations dbops = new DatabaseOperations(opt, dbopsLogger);
            
            return new PostgreSqlCache(opt, dbops);
        }
    }
}
