using CRMService.Interfaces.Repository.Auth;
using LoginService.DataBase;
using LoginService.Model.Entity;
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
                return await context.Sessions.OrderBy(s => s.ExpirationRefreshToken).Skip(range.Start.Value).Take(range.End.Value - range.Start.Value).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving session list.");
                return null;
            }
        }

        public async Task<Session?> GetItem(Session item)
        {
            try
            {
                return await context.Sessions.FirstOrDefaultAsync(se => se.RefreshToken == item.RefreshToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving session.");
                return null;
            }
        }

        public async Task<int> GetCountOfItems()
        {
            try
            {
                return await context.Sessions.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving count of sessions.");
                return 0;
            }
        }

        public void Create(Session item)
        {
            context.Sessions.Add(item);
        }

        public void Update(Session item)
        {
            context.Entry(item).State = EntityState.Modified;
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
                _logger.LogError(ex, "Error deleting sessions by user id.");                
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
                _logger.LogError(ex, "Error deleting expired refresh tokens.");
            }
        }
    }
}
