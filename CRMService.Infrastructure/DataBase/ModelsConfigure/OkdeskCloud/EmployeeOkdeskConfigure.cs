using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.Infrastructure.DataBase.ModelsConfigure.OkdeskCloud
{
    public class EmployeeOkdeskConfigure : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.ToTable("users");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("sequential_id");
            builder.Property<int>("InternalId").HasColumnName("id");
            builder.HasAlternateKey("InternalId");
            builder.Property<string>("Type").HasColumnName("type");
            builder.Property(x => x.FirstName).HasColumnName("first_name");
            builder.Property(x => x.LastName).HasColumnName("last_name");
            builder.Property(x => x.Patronymic).HasColumnName("patronymic");
            builder.Property(x => x.Position).HasColumnName("position");
            builder.Property(x => x.Active).HasColumnName("active");
            builder.Property(x => x.Email).HasColumnName("email");
            builder.Property(x => x.Login).HasColumnName("login");
            builder.Property(x => x.Phone).HasColumnName("phone");

            builder.Ignore(x => x.Roles);
            builder.Ignore(x => x.Issues);
            builder.Ignore(x => x.TimeEntries);
            builder.Ignore(x => x.EmployeeGroups);
            builder.Ignore(x => x.EmployeeRoles);
            builder.Ignore(x => x.PlanSettings);
        }
    }
}
