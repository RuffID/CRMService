using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.Infrastructure.DataBase.ModelsConfigure.OkdeskEntity
{
    public class GroupConfigure : IEntityTypeConfiguration<Group>
    {
        public void Configure(EntityTypeBuilder<Group> builder)
        {
            builder.ToTable("Group");

            builder.Property(e => e.Id)
                .ValueGeneratedNever();

            builder.Property(e => e.Description)
                .HasMaxLength(200);

            builder.Property(e => e.Name)
                .HasMaxLength(100);
        }
    }
}



