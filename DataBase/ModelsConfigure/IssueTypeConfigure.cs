using CRMService.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure
{
    public class IssueTypeConfigure : IEntityTypeConfiguration<IssueType>
    {
        public void Configure(EntityTypeBuilder<IssueType> builder)
        {
            builder.HasKey(e => e.Id).HasName("PRIMARY");

            builder.ToTable("issue_type");

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            builder.Property(e => e.AvailableForClient).HasColumnName("available_for_client");

            builder.Property(e => e.Code)
                .HasMaxLength(60)
                .HasColumnName("code");

            builder.Property(e => e.Default).HasColumnName("default");

            builder.Property(e => e.Inner).HasColumnName("inner");

            builder.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");

            builder.Property(e => e.Type)
                .HasMaxLength(45)
                .HasColumnName("type");
        }
    }
}
