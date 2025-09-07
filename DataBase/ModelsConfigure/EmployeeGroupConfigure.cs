using CRMService.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure
{
    public class EmployeeGroupConfigure : IEntityTypeConfiguration<EmployeeGroup>
    {
        public void Configure(EntityTypeBuilder<EmployeeGroup> builder)
        {
            builder.ToTable("employee_groups");

            builder.HasKey(e => new { e.EmployeeId, e.GroupId });

            builder.Property(e => e.EmployeeId)
                .HasColumnName("employeeId");

            builder.Property(e => e.GroupId)
                .HasColumnName("groupId");

            builder.HasOne(e => e.Employee)
                .WithMany(e => e.EmployeeGroups)
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Group)
                .WithMany(g => g.EmployeeGroups)
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
