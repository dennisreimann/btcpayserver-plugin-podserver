#nullable enable
using System;
using System.Threading.Tasks;
using BTCPayServer.Abstractions.Contracts;

namespace BTCPayServer.Plugins.PodServer.Data.Models;

public class Player
{
    private readonly IFileService _fileService;
    public Episode Episode { get; set; }

    public Player(Episode episode, IFileService fileService)
    {
        _fileService = fileService;

        Episode = episode;
    }

    public Podcast Podcast { get => Episode.Podcast; }

    public async Task<string?> GetEnclosureUrl(Uri rootUri) =>
        Episode.MainEnclosure?.FileId != null
            ? await _fileService.GetFileUrl(rootUri, Episode.MainEnclosure.FileId)
            : null;

    public async Task<string?> GetImageUrl(Uri rootUri) =>
        Episode.ImageFileId != null
            ? await _fileService.GetFileUrl(rootUri, Episode.ImageFileId)
            : null;
}
