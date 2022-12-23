using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BTCPayServer.Plugins.PodServer.Data.Models;

public class Import
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [DisplayName("Import ID")]
    public string ImportId { get; set; }

    // Properties
    [Required]
    public string Raw { get; set; }

    public string Log { get; set; }

    [Required]
    public ImportStatus Status { get; set; } = ImportStatus.Created;

    [DisplayName("Import date")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Relations
    [Required]
    [DisplayName("Podcast ID")]
    public string PodcastId { get; set; }

    public Podcast Podcast { get; set; }

    [Required]
    [DisplayName("User ID")]
    public string UserId { get; set; }

    internal static void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Import>()
            .Property(i => i.Status)
            .HasConversion<string>();

        builder
            .Entity<Import>()
            .HasOne(e => e.Podcast)
            .WithMany(p => p.Imports)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public enum ImportStatus
{
    Created,
    Running,
    Cancelled,
    Succeeded,
    Failed
}
