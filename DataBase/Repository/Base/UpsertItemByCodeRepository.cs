using CRMService.Interfaces.Database;
using CRMService.Interfaces.Entity;
using CRMService.Interfaces.Repository.Base;
using Microsoft.EntityFrameworkCore;

namespace CRMService.DataBase.Repository.Base
{
    public sealed class UpsertItemByCodeRepository<TEntity>(IAppDbContext context) : IUpsertItemByCodeRepository<TEntity>
        where TEntity : class, IHasCode, ICopyable<TEntity>
    {
        public async Task UpsertByCode(TEntity item, CancellationToken ct = default)
        {
            TEntity? existing = await context.Set<TEntity>()
                .FirstOrDefaultAsync(e => e.Code == item.Code, ct);

            if (existing is null)
            {
                context.Set<TEntity>().Add(item);
                return;
            }

            existing.CopyData(item);
        }

        public async Task UpsertByCodes(List<TEntity> items, CancellationToken ct = default)
        {
            List<TEntity> incoming = items.Where(i => !string.IsNullOrWhiteSpace(i.Code)).ToList();

            List<string> codes = incoming.Select(i => i.Code).Distinct().ToList();

            List<TEntity> existingList = codes.Count == 0 
                ? new List<TEntity>()
                : await context.Set<TEntity>().Where(e => codes.Contains(e.Code)).ToListAsync(ct);

            Dictionary<string, TEntity> existingByCode = existingList.ToDictionary(e => e.Code);

            foreach (TEntity item in incoming)
            {
                if (existingByCode.TryGetValue(item.Code, out TEntity? existing))
                    existing.CopyData(item);
                else
                    context.Set<TEntity>().Add(item);
            }
        }

        public async Task UpsertByCode(string oldCode, TEntity item, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(oldCode))
            {
                await UpsertByCode(item, ct);
                return;
            }

            TEntity? existing = await context.Set<TEntity>().FirstOrDefaultAsync(e => e.Code == oldCode, ct);

            if (existing is null)
            {
                context.Set<TEntity>().Add(item);
                return;
            }

            existing.CopyData(item);
        }

        public async Task UpsertByCodePairs(IEnumerable<(string OldCode, TEntity Item)> items, CancellationToken ct = default)
        {
            List<(string OldCode, TEntity Item)> incoming = items
                .Where(p => !string.IsNullOrWhiteSpace(p.OldCode) || !string.IsNullOrWhiteSpace(p.Item.Code))
                .ToList();

            List<string> lookupCodes = incoming
                .Select(p => p.OldCode)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .ToList();

            List<TEntity> existingList = lookupCodes.Count == 0
                ? new List<TEntity>()
                : await context.Set<TEntity>()
                    .Where(e => lookupCodes.Contains(e.Code))
                    .ToListAsync(ct);

            Dictionary<string, TEntity> existingByOld = existingList.ToDictionary(e => e.Code);

            foreach ((string OldCode, TEntity Item) in incoming)
            {
                if (!string.IsNullOrWhiteSpace(OldCode) && existingByOld.TryGetValue(OldCode, out TEntity? existing))
                    existing.CopyData(Item);
                else
                    context.Set<TEntity>().Add(Item);
            }
        }
    }
}
