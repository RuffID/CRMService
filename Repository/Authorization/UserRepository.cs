using CRMService.Interfaces.Repository.Auth;
using LoginService.DataBase;
using LoginService.Model.Entity;
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
                return await _context.Users.OrderBy(u => u.Login).Skip(range.Start.Value).Take(range.End.Value - range.Start.Value).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user list.");
                return null;
            }
        }

        public async Task<User?> GetItem(User user)
        {
            try
            {
                return await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id || u.Login == user.Login || u.Email == user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user.");
                return null;
            }
        }

        public void Update(User item)
        {
            _context.Entry(item).State = EntityState.Modified;
        }        

        public void Create(User item)
        {            
            _context.Users.Add(item);
        }
    }
}
