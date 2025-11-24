# PostgreSQL Distributed Cache for .NET Core | Hlidac Statu edition

## Introduction

We used `Community.Microsoft.Extensions.Caching.PostgreSQL` and modified its way of working. Mainly added Create factory, so we are not dependent on MS Dependency Injection magic.
Secondly we moved


### 2. Basic Configuration

Add the following line to your `Startup.cs` or `Program.cs`'s `ConfigureServices` method:

```csharp
services.AddDistributedPostgreSqlCache(setup =>
{
    setup.ConnectionString = configuration["ConnectionString"];
    setup.SchemaName = configuration["SchemaName"];
    setup.TableName = configuration["TableName"];
    setup.DisableRemoveExpired = configuration["DisableRemoveExpired"];
    // Optional - DisableRemoveExpired default is FALSE
    setup.CreateInfrastructure = configuration["CreateInfrastructure"];
    // CreateInfrastructure is optional, default is TRUE
    // This means that every time the application starts the
    // creation of the table and database functions will be verified.
    setup.ExpiredItemsDeletionInterval = TimeSpan.FromMinutes(30);
    // ExpiredItemsDeletionInterval is optional
    // This is the periodic interval to scan and delete expired items in the cache. Default is 30 minutes.
    // Minimum allowed is 5 minutes. - If you need less than this please share your use case 😁, just for curiosity...
});
```

