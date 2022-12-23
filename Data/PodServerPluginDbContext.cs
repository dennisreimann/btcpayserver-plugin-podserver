using BTCPayServer.Plugins.PodServer.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BTCPayServer.Plugins.PodServer.Data;

public class PodServerPluginDbContext : DbContext
{
    private readonly bool _designTime;

    public PodServerPluginDbContext(DbContextOptions<PodServerPluginDbContext> options, bool designTime = false)
        : base(options)
    {
        _designTime = designTime;
    }

    public DbSet<Podcast> Podcasts { get; set; }
    public DbSet<Episode> Episodes { get; set; }
    public DbSet<Editor> Editors { get; set; }
    public DbSet<Person> People { get; set; }
    public DbSet<Season> Seasons { get; set; }
    public DbSet<Import> Imports { get; set; }
    public DbSet<Enclosure> Enclosures { get; set; }
    public DbSet<Contribution> Contributions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("BTCPayServer.Plugins.PodServer");
        if (Database.IsSqlite() && !_designTime)
        {
            // SQLite does not have proper support for DateTimeOffset via Entity Framework Core, see the limitations
            // here: https://docs.microsoft.com/en-us/ef/core/providers/sqlite/limitations#query-limitations
            // To work around this, when the Sqlite database provider is used, all model properties of type DateTimeOffset
            // use the DateTimeOffsetToBinaryConverter
            // Based on: https://github.com/aspnet/EntityFrameworkCore/issues/10784#issuecomment-415769754
            // This only supports millisecond precision, but should be sufficient for most use cases.
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var properties = entityType.ClrType.GetProperties()
                    .Where(p => p.PropertyType == typeof(DateTimeOffset));
                foreach (var property in properties)
                {
                    modelBuilder
                        .Entity(entityType.Name)
                        .Property(property.Name)
                        .HasConversion(
                            new Microsoft.EntityFrameworkCore.Storage.ValueConversion.
                                DateTimeOffsetToBinaryConverter());
                }
            }
        }

        Contribution.OnModelCreating(modelBuilder);
        Editor.OnModelCreating(modelBuilder);
        Enclosure.OnModelCreating(modelBuilder);
        Episode.OnModelCreating(modelBuilder);
        Import.OnModelCreating(modelBuilder);
        Person.OnModelCreating(modelBuilder);
        Podcast.OnModelCreating(modelBuilder);
        Season.OnModelCreating(modelBuilder);
    }
}
