using CRMService.DataBase;
using CRMService.Interfaces.Repository;
using CRMService.Interfaces.Repository.Entity;
using CRMService.Interfaces.Repository.Report;
using CRMService.Repository.Entity;
using CRMService.Repository.Report;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace CRMService.Repository
{
    public class UnitOfWorkEntities(CRMEntitiesContext entitiesContext, ILoggerFactory logger) : IUnitOfWorkEntities
    {
        private bool disposed = false;
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

        public ICompanyRepository Company
        {
            get
            {
                companyRepository ??= new CompanyRepository(entitiesContext, logger);
                return companyRepository;
            }
        }

        public ICompanyCategoryRepository CompanyCategory
        {
            get
            {
                categoryRepository ??= new CategoryRepository(entitiesContext, logger);
                return (CategoryRepository)categoryRepository;
            }
        }

        public IParameterRepository Parameter
        {
            get
            {
                parameter ??= new ParameterRepository(entitiesContext, logger);
                return (ParameterRepository)parameter;
            }
        }

        public IEmployeeGroupRepository EmployeeGroup
        {
            get
            {
                employeeGroupRepository ??= new EmployeeGroupRepository(entitiesContext, logger);
                return (EmployeeGroupRepository)employeeGroupRepository;
            }
        }

        public IEmployeeRepository Employee
        {
            get
            {
                employeeRepository ??= new EmployeeRepository(entitiesContext, logger);
                return (EmployeeRepository)employeeRepository;
            }
        }

        public IEmployeeRoleRepository EmployeeRole
        {
            get
            {
                employeeRoleRepository ??= new EmployeeRoleRepository(entitiesContext, logger);
                return (EmployeeRoleRepository)employeeRoleRepository;
            }
        }

        public IGroupRepository Group
        {
            get
            {
                groupRepository ??= new GroupRepository(entitiesContext, logger);
                return (GroupRepository)groupRepository;
            }
        }

        public IIssuePriorityRepository IssuePriority
        {
            get
            {
                issuePriorityRepository ??= new IssuePriorityRepository(entitiesContext, logger);
                return (IssuePriorityRepository)issuePriorityRepository;
            }
        }

        public IIssueRepository Issue
        {
            get
            {
                issueRepository ??= new IssueRepository(entitiesContext, logger);
                return (IssueRepository)issueRepository;
            }
        }

        public IIssueStatusRepository IssueStatus
        {
            get
            {
                issueStatusRepository ??= new IssueStatusRepository(entitiesContext, logger);
                return (IssueStatusRepository)issueStatusRepository;
            }
        }

        public IIssueTypeRepository IssueType
        {
            get
            {
                issueTypeRepository ??= new IssueTypeRepository(entitiesContext, logger);
                return (IssueTypeRepository)issueTypeRepository;
            }
        }

        public IKindParamsRepository KindParams
        {
            get
            {
                kindParamsRepository ??= new KindParamsRepository(entitiesContext, logger);
                return (KindParamsRepository)kindParamsRepository;
            }
        }

        public IKindRepository Kind
        {
            get
            {
                kindRepository ??= new KindRepository(entitiesContext, logger);
                return (KindRepository)kindRepository;
            }
        }

        public IKindParameterRepository KindParameter
        {
            get
            {
                kindParameterRepository ??= new KindParameterRepository(entitiesContext, logger);
                return (KindParameterRepository)kindParameterRepository;
            }
        }

        public IMaintenanceEntityRepository MaintenanceEntity
        {
            get
            {
                maintenanceEntityRepository ??= new MaintenanceEntityRepository(entitiesContext, logger);
                return (MaintenanceEntityRepository)maintenanceEntityRepository;
            }
        }

        public IManufacturerRepository Manufacturer
        {
            get
            {
                manufacturerRepository ??= new ManufacturerRepository(entitiesContext, logger);
                return (ManufacturerRepository)manufacturerRepository;
            }
        }

        public IModelRepository Model
        {
            get
            {
                modelRepository ??= new ModelRepository(entitiesContext, logger);
                return (ModelRepository)modelRepository;
            }
        }

        public IRoleRepository Role
        {
            get
            {
                roleRepository ??= new RoleRepository(entitiesContext, logger);
                return (RoleRepository)roleRepository;
            }
        }

        public ITimeEntryRepository TimeEntry
        {
            get
            {
                timeEntryRepository ??= new TimeEntryRepository(entitiesContext, logger);
                return (TimeEntryRepository)timeEntryRepository;
            }
        }

        public IEquipmentRepository Equipment
        {
            get
            {
                equipmentRepository ??= new EquipmentRepository(entitiesContext, logger);
                return (EquipmentRepository)equipmentRepository;
            }
        }

        public IReportRepository Report
        {
            get
            {
                reportRepository ??= new ReportRepository(entitiesContext, logger);
                return (ReportRepository)reportRepository;
            }
        }

        public async Task SaveAsync([CallerMemberName] string caller = "")
        {
            try
            {
                await entitiesContext.SaveChangesAsync();
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
                    entitiesContext.Dispose();
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
