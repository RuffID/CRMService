using CRMService.DataBase.ModelsConfigure;
using CRMService.Models.ConfigClass;
using CRMService.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CRMService.DataBase;

public partial class CrmEntitiesContext : DbContext
{
    private readonly DatabaseSettings _dbSettings;
    public CrmEntitiesContext(IOptions<DatabaseSettings> dbSetings)
    {
        _dbSettings = dbSetings.Value;
        Database.EnsureCreated();
    }

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

    public virtual DbSet<Parameter> Parameters { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<TimeEntry> TimeEntries { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySql(_dbSettings.MySqlMainCRM,
                new MySqlServerVersion(new Version(_dbSettings.MySqlVersion)));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

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
            .ApplyConfiguration(new KindConfigure())
            .ApplyConfiguration(new KindsParameterConfigure())
            .ApplyConfiguration(new KindParamConfigure())
            .ApplyConfiguration(new MaintenanceEntityConfigure())
            .ApplyConfiguration(new ModelConfigure())
            .ApplyConfiguration(new ManufacturerConfigure())
            .ApplyConfiguration(new ParameterConfigure())
            .ApplyConfiguration(new RoleConfigure())
            .ApplyConfiguration(new TimeEntryConfigure());                   
        
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
