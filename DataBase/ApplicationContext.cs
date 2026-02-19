using CRMService.DataBase.ModelsConfigure.Authorization;
using CRMService.DataBase.ModelsConfigure.CrmEntity;
using CRMService.DataBase.ModelsConfigure.OkdeskEntity;
using CRMService.Models.Authorization;
using CRMService.Models.CrmEntities;
using CRMService.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;

namespace CRMService.DataBase;

public partial class ApplicationContext(DbContextOptions<ApplicationContext> options) : DbContext(options)
{
    public virtual DbSet<Company> Companies { get; set; }
    public virtual DbSet<CompanyCategory> CompanyCategories { get; set; }
    public virtual DbSet<Employee> Employees { get; set; }
    public virtual DbSet<EmployeeGroup> EmployeeGroups { get; set; }
    public virtual DbSet<EmployeeRole> EmployeeRoles { get; set; }
    public virtual DbSet<Equipment> Equipment { get; set; }
    public virtual DbSet<Group> Groups { get; set; }
    public virtual DbSet<Issue> Issues { get; set; }
    public virtual DbSet<IssuePriority> IssuePriorities { get; set; }
    public virtual DbSet<IssueStatus> IssueStatuses { get; set; }
    public virtual DbSet<IssueType> IssueTypes { get; set; }
    public virtual DbSet<Kind> Kinds { get; set; }
    public virtual DbSet<KindParam> KindParams { get; set; }
    public virtual DbSet<KindsParameter> KindsParameters { get; set; }
    public virtual DbSet<MaintenanceEntity> MaintenanceEntities { get; set; }
    public virtual DbSet<Manufacturer> Manufacturers { get; set; }
    public virtual DbSet<Model> Models { get; set; }
    public virtual DbSet<EquipmentParameter> Parameters { get; set; }
    public virtual DbSet<OkdeskRole> OkdeskRoles { get; set; }
    public virtual DbSet<TimeEntry> TimeEntries { get; set; }
    public virtual DbSet<BlockReason> BlockReasons { get; set; }
    public virtual DbSet<CrmRole> CrmRoles { get; set; }
    public virtual DbSet<Session> Sessions { get; set; }
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<UserRole> UserRoles { get; set; }
    public virtual DbSet<PlanColorScheme> PlanColorSchemes { get; set; }
    public virtual DbSet<PlanSetting> PlanSetting { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .ApplyConfiguration(new CompanyConfigure())
            .ApplyConfiguration(new CompanyCategoryConfigure())
            .ApplyConfiguration(new EmployeeConfigure())
            .ApplyConfiguration(new EmployeeGroupConfigure())
            .ApplyConfiguration(new EmployeeRoleConfigure())
            .ApplyConfiguration(new EquipmentConfigure())
            .ApplyConfiguration(new GroupConfigure())
            .ApplyConfiguration(new IssueConfigure())
            .ApplyConfiguration(new IssuePriorityConfigure())
            .ApplyConfiguration(new IssueStatusConfigure())
            .ApplyConfiguration(new IssueTypeConfigure())
            .ApplyConfiguration(new IssueTypeGroupConfigure())
            .ApplyConfiguration(new KindConfigure())
            .ApplyConfiguration(new KindsParameterConfigure())
            .ApplyConfiguration(new KindParamConfigure())
            .ApplyConfiguration(new MaintenanceEntityConfigure())
            .ApplyConfiguration(new ModelConfigure())
            .ApplyConfiguration(new ManufacturerConfigure())
            .ApplyConfiguration(new EquipmentParameterConfigure())
            .ApplyConfiguration(new OkdeskRoleConfigure())
            .ApplyConfiguration(new TimeEntryConfigure())
            .ApplyConfiguration(new UserConfigure())
            .ApplyConfiguration(new CrmRoleConfigure())
            .ApplyConfiguration(new BlockReasonConfigure())
            .ApplyConfiguration(new SessionConfigure())
            .ApplyConfiguration(new UserRoleConfigure())
            .ApplyConfiguration(new PlanSettingsConfigure())
            .ApplyConfiguration(new PlanColorSchemeConfigure());

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
