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
            entity.HasKey(e => e.Id).HasName("PRIMARY");
            entity.ToTable("employee_roles");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
            entity.Property(e => e.EmployeeId).HasColumnName("employeeId");
            entity.Property(e => e.RoleId).HasColumnName("roleId");
        }
    }
}
