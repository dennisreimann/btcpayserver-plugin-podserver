using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace BTCPayServer.Plugins.PodServer.Data.Models;

public enum ValueRecipientType
{
    Node,
    Wallet,
    LNbank
}

[Owned]
public class ValueRecipient
{
    // Properties
    [Required]
    public ValueRecipientType Type { get; set; }

    public string Address { get; set; }

    public string CustomKey { get; set; }

    public string CustomValue { get; set; }
}
