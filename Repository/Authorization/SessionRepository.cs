using CRMService.DataBase;
using CRMService.Interfaces.Repository.Authorization;
using CRMService.Models.Authorization;
using Microsoft.EntityFrameworkCore;

namespace CRMService.Repository.Authorization
{
    public class SessionRepository(CrmAuthorizationContext context, ILoggerFactory logger) : ISessionRepository
    {
        private readonly ILogger<SessionRepository> _logger = logger.CreateLogger<SessionRepository>();

        public async Task<IEnumerable<Session>?> GetAllItem(Range range)
        {
            try
            {
                return await context.Sessions.Skip(range.Start.Value).Take(range.End.Value - range.Start.Value).OrderBy(s => s.ExpirationRefreshToken).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving session list.", nameof(GetAllItem));
                return null;
            }
        }

        public async Task<Session?> GetItem(Session item, bool? trackable = null)
        {
            try
            {
                if (trackable == null || trackable == true)
                    return await context.Sessions.FirstOrDefaultAsync(se => se.RefreshToken == item.RefreshToken);

                return await context.Sessions.AsNoTracking().FirstOrDefaultAsync(se => se.RefreshToken == item.RefreshToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving session.", nameof(GetItem));
                return null;
            }
        }

        public async Task<int> GetCountOfItems()
        {
            try
            {
                return await context.Sessions.AsNoTracking().CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving count of sessions.", nameof(GetCountOfItems));
                return 0;
            }
        }

        public void Create(Session item)
        {
            context.Sessions.Add(item);
        }

        public void Update(Session oldItem, Session newItem)
        {
            oldItem.CopyData(newItem);
        }

        public void Delete(Session item)
        {
            context.Sessions.Remove(item);
        }

        public async Task DeleteByUserId(Guid userId)
        {
            try
            {
                await context.Sessions.Where(s => s.UserId == userId).ExecuteDeleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error deleting sessions by user id.", nameof(DeleteByUserId));                
            }
        }

        public async Task DeleteSessionsWithExpiredRefreshTokens()
        {
            try
            {
                DateTime date = DateTime.UtcNow;

                await context.Sessions.Where(s => s.ExpirationRefreshToken < date).ExecuteDeleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error deleting expired refresh tokens.", nameof(DeleteSessionsWithExpiredRefreshTokens));
            }
        }
    }
}
