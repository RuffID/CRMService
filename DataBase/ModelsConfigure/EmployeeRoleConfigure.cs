using CRMService.Interfaces.Entity;
using CRMService.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure
{
    public class EmployeeRoleConfigure : IEntityTypeConfiguration<EmployeeRole>
    {
        public void Configure(EntityTypeBuilder<EmployeeRole> entity)
        {
            entity.ToTable("employee_roles");

            entity.HasKey(e => new { e.EmployeeId, e.RoleId })
               .HasName("PRIMARY");

            entity.Property(e => e.EmployeeId)
                .HasColumnName("employeeId");

            entity.Property(e => e.RoleId)
                .HasColumnName("roleId");

            entity.HasOne(e => e.Employee)
              .WithMany(e => e.EmployeeRoles)
              .HasForeignKey(e => e.EmployeeId)
              .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Role)
                  .WithMany(r => r.EmployeeRoles)
                  .HasForeignKey(e => e.RoleId)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
