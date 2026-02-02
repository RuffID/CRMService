using CRMService.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure.OkdeskEntity
{
    public class IssuePriorityConfigure : IEntityTypeConfiguration<IssuePriority>
    {
        public void Configure(EntityTypeBuilder<IssuePriority> builder)
        {
            builder.ToTable("IssuePriority");

            builder.Property(e => e.Id)
                .ValueGeneratedNever();

            builder.Property(e => e.Code)
                .HasMaxLength(45);

            builder.Property(e => e.Color)
                .HasMaxLength(45);

            builder.Property(e => e.Name)
                .HasMaxLength(45);
        }
    }
}
