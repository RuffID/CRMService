using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.Infrastructure.DataBase.ModelsConfigure.OkdeskEntity
{
    public class EmployeeRoleConfigure : IEntityTypeConfiguration<EmployeeRole>
    {
        public void Configure(EntityTypeBuilder<EmployeeRole> entity)
        {
            entity.ToTable("EmployeeRoles");

            entity.HasKey(e => new { e.EmployeeId, e.RoleId });

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



