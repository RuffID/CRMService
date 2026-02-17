using CRMService.Models.CrmEntities;
using CRMService.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure.OkdeskEntity
{
    public class EmployeeConfigure : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.ToTable("Employee");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .ValueGeneratedNever();

            builder.Property(e => e.Email)
                .HasMaxLength(100);

            builder.Property(e => e.FirstName)
                .HasMaxLength(70);

            builder.Property(e => e.LastName)
                .HasMaxLength(70);

            builder.Property(e => e.Login)
                .HasMaxLength(45);

            builder.Property(e => e.Patronymic)
                .HasMaxLength(70);

            builder.Property(e => e.Phone)
                .HasMaxLength(35);

            builder.Property(e => e.Position)
                .HasMaxLength(70);

            builder.HasOne(x => x.PlanSetting)
                .WithOne(x => x.Employee)
                .HasForeignKey<PlanSetting>(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
