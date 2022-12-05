using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BTCPayServer.Plugins.PodServer.Data.Models;

public class Season
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [DisplayName("Season ID")]
    public string SeasonId { get; set; }
    
    // Relations
    [Required]
    public string PodcastId { get; set; }
    public Podcast Podcast { get; set; }
    
    public ICollection<Episode> Episodes { get; set; }
    
    // Properties
    [Required]
    [Range(1, int.MaxValue)]
    public int Number { get; set; }

    [MaxLength(128)]
    public string Name { get; set; }
    
    internal static void OnModelCreating(ModelBuilder builder)
    {
        builder
            .Entity<Season>()
            .HasIndex("PodcastId", "Number")
            .IsUnique();
        
        builder
            .Entity<Season>()
            .HasOne(e => e.Podcast)
            .WithMany(p => p.Seasons)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
