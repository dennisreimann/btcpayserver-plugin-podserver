using System;
using BTCPayServer.Abstractions.Contracts;
using BTCPayServer.Abstractions.Models;
using BTCPayServer.Plugins.PodServer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace BTCPayServer.Plugins.PodServer.Services;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<PodServerPluginDbContext>
{
    public PodServerPluginDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<PodServerPluginDbContext>();

        // FIXME: Somehow the DateTimeOffset column types get messed up when not using Postgres
        // https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/providers?tabs=dotnet-core-cli
        // builder.UseSqlite("Data Source=temp.db");
        builder.UseNpgsql("User ID=postgres;Host=127.0.0.1;Port=39372;Database=designtimebtcpay");

        return new PodServerPluginDbContext(builder.Options);
    }
}

public class PodServerPluginDbContextFactory(IOptions<DatabaseOptions> options)
    : BaseDbContextFactory<PodServerPluginDbContext>(options, "BTCPayServer.Plugins.PodServer")
{
    public override PodServerPluginDbContext CreateContext(Action<NpgsqlDbContextOptionsBuilder> npgsqlOptionsAction = null)
    {
        var builder = new DbContextOptionsBuilder<PodServerPluginDbContext>();
        ConfigureBuilder(builder);
        return new PodServerPluginDbContext(builder.Options);
    }
}
