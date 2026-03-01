using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.Infrastructure.DataBase.ModelsConfigure.OkdeskEntity
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

            builder.HasMany(x => x.PlanSettings)
                .WithOne(x => x.Employee)
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}


