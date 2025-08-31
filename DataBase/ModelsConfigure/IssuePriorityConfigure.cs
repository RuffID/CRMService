using CRMService.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure
{
    public class IssuePriorityConfigure : IEntityTypeConfiguration<IssuePriority>
    {
        public void Configure(EntityTypeBuilder<IssuePriority> builder)
        {
            builder.HasKey(e => e.Id).HasName("PRIMARY");

            builder.ToTable("issue_priority");

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            builder.Property(e => e.Code)
                .HasMaxLength(45)
                .HasColumnName("code");

            builder.Property(e => e.Color)
                .HasMaxLength(45)
                .HasColumnName("color");

            builder.Property(e => e.Name)
                .HasMaxLength(45)
                .HasColumnName("name");

            builder.Property(e => e.Position).HasColumnName("position");
        }
    }
}
