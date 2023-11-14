using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BTCPayServer.Plugins.PodServer.Data.Models;

public class Podcast
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [DisplayName("Podcast ID")]
    public string PodcastId { get; set; }

    // Relations
    public ICollection<Editor> Editors { get; set; } = new List<Editor>();

    public ICollection<Season> Seasons { get; set; } = new List<Season>();

    public ICollection<Episode> Episodes { get; set; } = new List<Episode>();

    public ICollection<Person> People { get; set; } = new List<Person>();
    public ICollection<Contribution> Contributions { get; set; } = new List<Contribution>();
    public ICollection<Import> Imports { get; set; } = new List<Import>();

    // Properties
    [Required]
    [DisplayName("User ID")]
    public string OwnerId { get; set; }

    [Required]
    public string Title { get; set; }

    [DisplayName("Link ID")]
    public string Slug { get; set; }

    [Required]
    public string Description { get; set; }

    [Required]
    public string Medium { get; set; } = "podcast";

    [Required]
    public string Language { get; set; }

    [Required]
    public string Category { get; set; }

    public string ImageFileId { get; set; }

    public string Owner { get; set; }

    public string Email { get; set; }

    [DisplayName("Website URL")]
    public string Url { get; set; }

    [NotMapped]
    public EditorRole Role { get; set; }

    internal static void OnModelCreating(ModelBuilder builder)
    {
        builder
            .Entity<Podcast>()
            .HasIndex(p => p.Slug)
            .IsUnique();
    }
}
