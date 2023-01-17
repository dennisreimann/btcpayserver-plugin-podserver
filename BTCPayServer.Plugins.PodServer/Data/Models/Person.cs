using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BTCPayServer.Plugins.PodServer.Data.Models;

public class Person
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [DisplayName("Person ID")]
    public string PersonId { get; set; }

    // Relations
    [Required]
    public string PodcastId { get; set; }
    public Podcast Podcast { get; set; }

    public ICollection<Contribution> Contributions { get; set; } = new List<Contribution>();

    // Properties
    [Required]
    public string Name { get; set; }

    public string Url { get; set; }

    public string ImageFileId { get; set; }

    public ValueRecipient ValueRecipient { get; set; } = new();

    internal static void OnModelCreating(ModelBuilder builder)
    {
        builder
            .Entity<Person>()
            .HasOne(p => p.Podcast)
            .WithMany(p => p.People)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Person>()
            .OwnsOne(p => p.ValueRecipient,
                vr =>
                    vr.Property(e => e.Type)
                        .HasConversion<string>());

    }
}
