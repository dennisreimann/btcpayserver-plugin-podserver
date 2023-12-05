using System.Collections.Generic;
using System.Threading.Tasks;
using BTCPayServer.Plugins.PodServer.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace BTCPayServer.Plugins.PodServer.Services.Imports;

public class ImportRepository(PodServerPluginDbContextFactory dbContextFactory)
{
    public async Task<IEnumerable<Import>> GetUnfinishedImports()
    {
        await using var dbContext = dbContextFactory.CreateContext();
        return await dbContext.Imports
            .Where(i => i.Status != ImportStatus.Succeeded && i.Status != ImportStatus.Failed)
            .ToListAsync();
    }

    public async Task<Import> GetImport(string importId)
    {
        await using var dbContext = dbContextFactory.CreateContext();
        return await dbContext.Imports
            .Where(i => i.ImportId == importId)
            .FirstOrDefaultAsync();
    }

    public async Task<Import> CreateImport(string rss, string podcastId, string userId)
    {
        var import = new Import { PodcastId = podcastId, UserId = userId, Raw = rss };
        return await AddOrUpdateImport(import);
    }

    public async Task UpdateStatus(Import import, ImportStatus status, string log = null)
    {
        await using var dbContext = dbContextFactory.CreateContext();

        import.Status = status;
        if (!string.IsNullOrEmpty(log))
            import.Log += log;

        dbContext.Imports.Update(import);
        await dbContext.SaveChangesAsync();
    }

    public async Task RemoveImport(Import import)
    {
        await using var dbContext = dbContextFactory.CreateContext();
        dbContext.Imports.Remove(import);
        await dbContext.SaveChangesAsync();
    }

    private async Task<Import> AddOrUpdateImport(Import import)
    {
        await using var dbContext = dbContextFactory.CreateContext();

        EntityEntry entry;
        if (string.IsNullOrEmpty(import.ImportId))
        {
            entry = await dbContext.Imports.AddAsync(import);
        }
        else
        {
            entry = dbContext.Update(import);
        }
        await dbContext.SaveChangesAsync();

        return (Import)entry.Entity;
    }
}
