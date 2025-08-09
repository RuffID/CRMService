using CRMService.DataBase;
using CRMService.Interfaces.Repository.Authorization;
using CRMService.Models.Authorization;
using Microsoft.EntityFrameworkCore;

namespace CRMService.Repository.Authorization
{
    public class UserRepository(CrmAuthorizationContext context, ILoggerFactory logger) : IUserRepository
    {
        private readonly CrmAuthorizationContext _context = context;
        private readonly ILogger<UserRepository> _logger = logger.CreateLogger<UserRepository>();

        public async Task<IEnumerable<User>?> GetAllItem(Range range)
        {
            try
            {
                return await _context.Users.AsNoTracking().Skip(range.Start.Value).Take(range.End.Value - range.Start.Value).OrderBy(u => u.Login).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving user list.", nameof(GetAllItem));
                return null;
            }
        }

        public async Task<User?> GetItem(User user, bool? trackable = null)
        {
            try
            {
                if (trackable == null || trackable == true)
                    return await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id || u.Login == user.Login || u.Email == user.Email);

                return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == user.Id || u.Login == user.Login || u.Email == user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving user.", nameof(GetItem));
                return null;
            }
        }

        public void Update(User oldItem, User newItem)
        {
            oldItem.CopyData(newItem);
        }        

        public void Create(User item)
        {            
            _context.Users.Add(item);
        }
    }
}
