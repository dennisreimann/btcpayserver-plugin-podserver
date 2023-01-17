using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace BTCPayServer.Plugins.PodServer.Data.Models;

public enum EditorRole
{
    Admin,
    Editor,
    [Display(Name = "Read-only")]
    ReadOnly,
}

public class Editor
{
    // Relations
    public string UserId { get; set; }
    public string PodcastId { get; set; }
    public Podcast Podcast { get; set; }

    // Properties
    public EditorRole Role { get; set; }

    internal static void OnModelCreating(ModelBuilder builder)
    {
        builder
            .Entity<Editor>()
            .HasKey(t => new { t.UserId, t.PodcastId });

        builder
            .Entity<Editor>()
            .HasOne(e => e.Podcast)
            .WithMany(p => p.Editors)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Editor>()
            .Property(e => e.Role)
            .HasConversion<string>();
    }
}
