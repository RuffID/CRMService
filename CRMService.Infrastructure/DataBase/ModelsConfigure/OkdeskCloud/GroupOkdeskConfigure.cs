using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.Infrastructure.DataBase.ModelsConfigure.OkdeskCloud
{
    public class GroupOkdeskConfigure : IEntityTypeConfiguration<Group>
    {
        public void Configure(EntityTypeBuilder<Group> builder)
        {
            builder.ToTable("groups");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("sequential_id");
            builder.Property(x => x.Name).HasColumnName("name");
            builder.Ignore(x => x.Active);
            builder.Ignore(x => x.Description);
            builder.Ignore(x => x.Employees);
            builder.Ignore(x => x.EmployeeGroups);
        }
    }
}