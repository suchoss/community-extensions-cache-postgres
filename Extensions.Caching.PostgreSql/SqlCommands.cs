namespace HlidacStatu.Caching.PostgreSql;

public class SqlCommands
{
    private readonly string _schemaName;
    private readonly string _tableName;
    private readonly int _expiredItemsDeletionIntervalMinutes;

    public SqlCommands(string schemaName, string tableName, int expiredItemsDeletionIntervalMinutes)
    {
        _schemaName = schemaName;
        _tableName = tableName;
        _expiredItemsDeletionIntervalMinutes = expiredItemsDeletionIntervalMinutes;
    }

    public string CreateSchemaAndTableSql =>
        $"""
         CREATE SCHEMA IF NOT EXISTS "{_schemaName}";

         CREATE TABLE IF NOT EXISTS "{_schemaName}"."{_tableName}"
         (
             "Id" text COLLATE pg_catalog."default" NOT NULL,
             "Value" bytea,
             "ExpiresAtTime" timestamp with time zone,
             "SlidingExpirationInSeconds" double precision,
             "AbsoluteExpiration" timestamp with time zone,
             CONSTRAINT "DistCache_pkey" PRIMARY KEY ("Id")
         );
         """;

    public string GetCacheItemSql =>
        $"""
        SELECT "Value"
        FROM "{_schemaName}"."{_tableName}"
        WHERE "Id" = @Id AND @UtcNow <= "ExpiresAtTime"
        """;

    public string SetCacheSql =>
        $"""
        INSERT INTO "{_schemaName}"."{_tableName}" ("Id", "Value", "ExpiresAtTime", "SlidingExpirationInSeconds", "AbsoluteExpiration")
            VALUES (@Id, @Value, @ExpiresAtTime, @SlidingExpirationInSeconds, @AbsoluteExpiration)
        ON CONFLICT("Id") DO
        UPDATE SET
            "Value" = EXCLUDED."Value",
            "ExpiresAtTime" = EXCLUDED."ExpiresAtTime",
            "SlidingExpirationInSeconds" = EXCLUDED."SlidingExpirationInSeconds",
            "AbsoluteExpiration" = EXCLUDED."AbsoluteExpiration"
        """;

    public string UpdateCacheItemSql =>
        $"""
        UPDATE "{_schemaName}"."{_tableName}"
        SET "ExpiresAtTime" = LEAST("AbsoluteExpiration", @UtcNow + "SlidingExpirationInSeconds" * interval '1 second')
        WHERE "Id" = @Id
            AND @UtcNow <= "ExpiresAtTime" 
            AND "SlidingExpirationInSeconds" IS NOT NULL
            AND ("AbsoluteExpiration" IS NULL OR "AbsoluteExpiration" <> "ExpiresAtTime")
        """;

    public string DeleteCacheItemSql =>
        $"""
        DELETE FROM "{_schemaName}"."{_tableName}"
        WHERE "Id" = @Id
        """;

    public string CreateDeleteExpiredCacheCronJobSql =>
        $"""
        -- pg_cron extension (safe to run multiple times)
        CREATE EXTENSION IF NOT EXISTS pg_cron;
        
        -- create cleanup job if it does not exist
        SELECT cron.schedule(
            job_name := '{_schemaName}_{_tableName}_cleanup',
            schedule := $cron$*/{_expiredItemsDeletionIntervalMinutes} * * * *$cron$,
            command  := $cmd$
                DELETE FROM "{_schemaName}"."{_tableName}"
                WHERE now() > "ExpiresAtTime";
            $cmd$
        )
        WHERE NOT EXISTS (
            SELECT 1
            FROM cron.job
            WHERE jobname = '{_schemaName}_{_tableName}_cleanup'
        );
        """;
}