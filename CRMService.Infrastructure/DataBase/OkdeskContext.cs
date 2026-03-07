using CRMService.Domain.Models.OkdeskEntity;
using CRMService.Infrastructure.DataBase.ModelsConfigure.OkdeskCloud;
using Microsoft.EntityFrameworkCore;

namespace CRMService.Infrastructure.DataBase
{
    public partial class OkdeskContext(DbContextOptions<OkdeskContext> options) : DbContext(options)
    {
        public DbSet<CompanyCategory> CompanyCategories { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Kind> Kinds { get; set; }
        public DbSet<KindsParameter> KindsParameters { get; set; }
        public DbSet<KindParam> KindParams { get; set; }
        public DbSet<MaintenanceEntity> MaintenanceEntities { get; set; }
        public DbSet<Manufacturer> Manufacturers { get; set; }
        public DbSet<Model> Models { get; set; }
        public DbSet<Equipment> Equipments { get; set; }
        public DbSet<Issue> Issues { get; set; }
        public DbSet<IssueStatus> IssueStatuses { get; set; }
        public DbSet<IssuePriority> IssuePriorities { get; set; }
        public DbSet<IssueType> IssueTypes { get; set; }
        public DbSet<TimeEntry> TimeEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .ApplyConfiguration(new CompanyCategoryOkdeskConfigure())
                .ApplyConfiguration(new CompanyOkdeskConfigure())
                .ApplyConfiguration(new EmployeeOkdeskConfigure())
                .ApplyConfiguration(new GroupOkdeskConfigure())
                .ApplyConfiguration(new KindOkdeskConfigure())
                .ApplyConfiguration(new KindsParameterOkdeskConfigure())
                .ApplyConfiguration(new KindParamOkdeskConfigure())
                .ApplyConfiguration(new MaintenanceEntityOkdeskConfigure())
                .ApplyConfiguration(new ManufacturerOkdeskConfigure())
                .ApplyConfiguration(new ModelOkdeskConfigure())
                .ApplyConfiguration(new EquipmentOkdeskConfigure())
                .ApplyConfiguration(new IssueOkdeskConfigure())
                .ApplyConfiguration(new IssueStatusOkdeskConfigure())
                .ApplyConfiguration(new IssuePriorityOkdeskConfigure())
                .ApplyConfiguration(new IssueTypeOkdeskConfigure())
                .ApplyConfiguration(new TimeEntryOkdeskConfigure());

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}