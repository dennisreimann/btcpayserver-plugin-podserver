using BTCPayServer.Plugins.PodServer.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BTCPayServer.Plugins.PodServer.Data;

public class PodServerPluginDbContext(DbContextOptions<PodServerPluginDbContext> options) : DbContext(options)
{
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
