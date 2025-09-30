using CRMService.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure.OkdeskEntity
{
    public class IssueStatusConfigure : IEntityTypeConfiguration<IssueStatus>
    {
        public void Configure(EntityTypeBuilder<IssueStatus> builder)
        {
            

            builder.ToTable("issue_status");

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
        }
    }
}
