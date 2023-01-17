using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BTCPayServer.Plugins.PodServer.Data.Models;

public class Enclosure
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [DisplayName("Enclosure ID")]
    public string EnclosureId { get; set; }

    // Relations
    [Required]
    public string EpisodeId { get; set; }
    public Episode Episode { get; set; }

    // Properties
    [Required]
    public string FileId { get; set; }

    [Required]
    public string Type { get; set; }

    [Required]
    public long Length { get; set; }

    public bool IsAlternate { get; set; }

    public string Title { get; set; }

    internal static void OnModelCreating(ModelBuilder builder)
    {
        builder
            .Entity<Enclosure>()
            .HasOne(e => e.Episode)
            .WithMany(s => s.Enclosures)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
