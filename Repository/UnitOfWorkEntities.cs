using CRMService.DataBase;
using CRMService.Interfaces.Repository;
using CRMService.Interfaces.Repository.Authorization;
using CRMService.Interfaces.Repository.Entity;
using CRMService.Interfaces.Repository.Report;
using CRMService.Models.ConfigClass;
using CRMService.Repository.Authorization;
using CRMService.Repository.Entity;
using CRMService.Repository.Report;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;

namespace CRMService.Repository
{
    public class UnitOfWorkEntities(IOptions<DatabaseSettings> databaseSettings, ILoggerFactory logger) : IUnitOfWork
    {
        private bool disposed = false;
        private readonly ApplicationContext _context = new(databaseSettings);
        private readonly ILogger<UnitOfWorkEntities> _logger = logger.CreateLogger<UnitOfWorkEntities>();
        private ICompanyRepository? companyRepository;
        private ICompanyCategoryRepository? categoryRepository;
        private IParameterRepository? parameter;
        private IEmployeeGroupRepository? employeeGroupRepository;
        private IEmployeeRepository? employeeRepository;
        private IEmployeeRoleRepository? employeeRoleRepository;
        private IGroupRepository? groupRepository;
        private IIssuePriorityRepository? issuePriorityRepository;
        private IIssueRepository? issueRepository;
        private IIssueStatusRepository? issueStatusRepository;
        private IIssueTypeRepository? issueTypeRepository;
        private IKindParamsRepository? kindParamsRepository;
        private IKindRepository? kindRepository;
        private IKindParameterRepository? kindParameterRepository;
        private IMaintenanceEntityRepository? maintenanceEntityRepository;
        private IManufacturerRepository? manufacturerRepository;
        private IModelRepository? modelRepository;
        private IRoleRepository? roleRepository;
        private ITimeEntryRepository? timeEntryRepository;
        private IEquipmentRepository? equipmentRepository;
        private IReportRepository? reportRepository;
        private IBlockReasonRepository? _blockRepository;
        private ICrmRoleRepository? _roleRepository;
        private ISessionRepository? _sessionRepository;
        private IUserRepository? _userRepository;
        private IUserRoleRepository? _userRoleRepository;

        public ICompanyRepository Company
        {
            get
            {
                companyRepository ??= new CompanyRepository(_context, logger);
                return companyRepository;
            }
        }

        public ICompanyCategoryRepository CompanyCategory
        {
            get
            {
                categoryRepository ??= new CategoryRepository(_context, logger);
                return (CategoryRepository)categoryRepository;
            }
        }

        public IParameterRepository Parameter
        {
            get
            {
                parameter ??= new ParameterRepository(_context, logger);
                return (ParameterRepository)parameter;
            }
        }

        public IEmployeeGroupRepository EmployeeGroup
        {
            get
            {
                employeeGroupRepository ??= new EmployeeGroupRepository(_context, logger);
                return (EmployeeGroupRepository)employeeGroupRepository;
            }
        }

        public IEmployeeRepository Employee
        {
            get
            {
                employeeRepository ??= new EmployeeRepository(_context, logger);
                return (EmployeeRepository)employeeRepository;
            }
        }

        public IEmployeeRoleRepository EmployeeRole
        {
            get
            {
                employeeRoleRepository ??= new EmployeeRoleRepository(_context, logger);
                return (EmployeeRoleRepository)employeeRoleRepository;
            }
        }

        public IGroupRepository Group
        {
            get
            {
                groupRepository ??= new GroupRepository(_context, logger);
                return (GroupRepository)groupRepository;
            }
        }

        public IIssuePriorityRepository IssuePriority
        {
            get
            {
                issuePriorityRepository ??= new IssuePriorityRepository(_context, logger);
                return (IssuePriorityRepository)issuePriorityRepository;
            }
        }

        public IIssueRepository Issue
        {
            get
            {
                issueRepository ??= new IssueRepository(_context, logger);
                return (IssueRepository)issueRepository;
            }
        }

        public IIssueStatusRepository IssueStatus
        {
            get
            {
                issueStatusRepository ??= new IssueStatusRepository(_context, logger);
                return (IssueStatusRepository)issueStatusRepository;
            }
        }

        public IIssueTypeRepository IssueType
        {
            get
            {
                issueTypeRepository ??= new IssueTypeRepository(_context, logger);
                return (IssueTypeRepository)issueTypeRepository;
            }
        }

        public IKindParamsRepository KindParams
        {
            get
            {
                kindParamsRepository ??= new KindParamsRepository(_context, logger);
                return (KindParamsRepository)kindParamsRepository;
            }
        }

        public IKindRepository Kind
        {
            get
            {
                kindRepository ??= new KindRepository(_context, logger);
                return (KindRepository)kindRepository;
            }
        }

        public IKindParameterRepository KindParameter
        {
            get
            {
                kindParameterRepository ??= new KindParameterRepository(_context, logger);
                return (KindParameterRepository)kindParameterRepository;
            }
        }

        public IMaintenanceEntityRepository MaintenanceEntity
        {
            get
            {
                maintenanceEntityRepository ??= new MaintenanceEntityRepository(_context, logger);
                return (MaintenanceEntityRepository)maintenanceEntityRepository;
            }
        }

        public IManufacturerRepository Manufacturer
        {
            get
            {
                manufacturerRepository ??= new ManufacturerRepository(_context, logger);
                return (ManufacturerRepository)manufacturerRepository;
            }
        }

        public IModelRepository Model
        {
            get
            {
                modelRepository ??= new ModelRepository(_context, logger);
                return (ModelRepository)modelRepository;
            }
        }

        public IRoleRepository Role
        {
            get
            {
                roleRepository ??= new RoleRepository(_context, logger);
                return (RoleRepository)roleRepository;
            }
        }

        public ITimeEntryRepository TimeEntry
        {
            get
            {
                timeEntryRepository ??= new TimeEntryRepository(_context, logger);
                return (TimeEntryRepository)timeEntryRepository;
            }
        }

        public IEquipmentRepository Equipment
        {
            get
            {
                equipmentRepository ??= new EquipmentRepository(_context, logger);
                return (EquipmentRepository)equipmentRepository;
            }
        }

        public IReportRepository Report
        {
            get
            {
                reportRepository ??= new ReportRepository(_context, logger);
                return (ReportRepository)reportRepository;
            }
        }

        public IBlockReasonRepository BlockReason
        {
            get
            {
                _blockRepository ??= new BlockReasonRepository(_context, logger);
                return (BlockReasonRepository)_blockRepository;
            }
        }

        public ICrmRoleRepository Role
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

        public async Task SaveAsync([CallerMemberName] string caller = "")
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError("[Method:{MethodName}] DbUpdateConcurrencyException error EF. Caller from: {CallerMemberName}", nameof(SaveAsync), caller);
                foreach (var entry in ex.Entries)
                {
                    _logger.LogError("Conflict on entity type: {EntityType}, keys: {Keys}",
                        entry.Entity.GetType().Name,
                        string.Join(", ", entry.Properties.Select(p => $"{p.Metadata.Name}={p.CurrentValue}"))
                    );
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error saving changes to entity framework. Caller from: {CallerMemberName}", nameof(SaveAsync), caller);
            }
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
