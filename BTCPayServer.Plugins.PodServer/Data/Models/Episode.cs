using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace BTCPayServer.Plugins.PodServer.Data.Models;

public class Episode
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [DisplayName("Episode ID")]
    public string EpisodeId { get; set; }

    [Required]
    public string Title { get; set; }

    [DisplayName("Link ID")]
    public string Slug { get; set; }

    [Required]
    public string Description { get; set; }

    [DisplayName("Publish date")]
    public DateTimeOffset? PublishedAt { get; set; }

    [DisplayName("Last update")]
    public DateTimeOffset LastUpdatedAt { get; set; }

    public string ImageFileId { get; set; }

    [Range(1, int.MaxValue)]
    public int? Number { get; set; }

    public string ImportGuid { get; set; }

    // Relations
    [Required]
    public string PodcastId { get; set; }
    public Podcast Podcast { get; set; }

    public string SeasonId { get; set; }
    public Season Season { get; set; }

    public ICollection<Contribution> Contributions { get; set; } = new List<Contribution>();
    public ICollection<Enclosure> Enclosures { get; set; } = new List<Enclosure>();

    public Enclosure MainEnclosure
    {
        get => Enclosures.FirstOrDefault(e => !e.IsAlternate);
    }

    public bool IsPublished
    {
        get => PublishedAt <= DateTime.UtcNow && MainEnclosure != null;
    }

    internal static void OnModelCreating(ModelBuilder builder)
    {
        builder
            .Entity<Episode>()
            .HasOne(e => e.Podcast)
            .WithMany(p => p.Episodes)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Entity<Episode>()
            .HasOne(e => e.Season)
            .WithMany(s => s.Episodes)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .Entity<Episode>()
            .HasIndex(e => new { e.PodcastId, e.Slug })
            .IsUnique();
    }
}
