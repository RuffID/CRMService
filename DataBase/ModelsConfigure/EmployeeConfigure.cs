using CRMService.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure
{
    public class EmployeeConfigure : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.ToTable("employee");

            

            builder.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");

            builder.Property(e => e.Active).HasColumnName("active");

            builder.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");

            builder.Property(e => e.FirstName)
                .HasMaxLength(70)
                .HasColumnName("first_name");

            builder.Property(e => e.LastName)
                .HasMaxLength(70)
                .HasColumnName("last_name");

            builder.Property(e => e.Login)
                .HasMaxLength(45)
                .HasColumnName("login");

            builder.Property(e => e.Patronymic)
                .HasMaxLength(70)
                .HasColumnName("patronymic");

            builder.Property(e => e.Phone)
                .HasMaxLength(35)
                .HasColumnName("phone");

            builder.Property(e => e.Position)
                .HasMaxLength(70)
                .HasColumnName("position");
        }
    }
}
