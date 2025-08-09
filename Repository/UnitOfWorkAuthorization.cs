using CRMService.DataBase;
using CRMService.Interfaces.Repository;
using CRMService.Interfaces.Repository.Authorization;
using CRMService.Models.ConfigClass;
using CRMService.Repository.Authorization;
using Microsoft.Extensions.Options;

namespace CRMService.Repository
{
    public class UnitOfWorkAuthorization(IOptions<DatabaseSettings> databaseSettings, ILoggerFactory logger) : IUnitOfWorkAuthorization
    {
        private bool disposed = false;
        private readonly CrmAuthorizationContext _context = new (databaseSettings);
        private IBlockReasonRepository? _blockRepository;
        private IRoleRepository? _roleRepository;
        private ISessionRepository? _sessionRepository;
        private IUserRepository? _userRepository;
        private IUserRoleRepository? _userRoleRepository;

        public IBlockReasonRepository BlockReason
        {
            get
            {
                _blockRepository ??= new BlockReasonRepository(_context, logger);
                return (BlockReasonRepository)_blockRepository;
            }
        }

        public IRoleRepository Role
        {
            get
            {
                _roleRepository ??= new RoleRepository(_context, logger);
                return (RoleRepository)_roleRepository;
            }
        }

        public ISessionRepository Session
        {
            get
            {
                _sessionRepository ??= new SessionRepository(_context, logger);
                return (SessionRepository)_sessionRepository;
            }
        }

        public IUserRepository User
        {
            get
            {
                _userRepository ??= new UserRepository(_context, logger);
                return (UserRepository)_userRepository;
            }
        }

        public IUserRoleRepository UserRole
        {
            get
            {
                _userRoleRepository ??= new UserRoleRepository(_context, logger);
                return (UserRoleRepository)_userRoleRepository;
            }
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
