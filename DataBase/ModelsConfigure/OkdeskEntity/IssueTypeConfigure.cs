using CRMService.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure.OkdeskEntity
{
    public class IssueTypeConfigure : IEntityTypeConfiguration<IssueType>
    {
        public void Configure(EntityTypeBuilder<IssueType> builder)
        {
            builder.ToTable("IssueType");

            builder.Property(e => e.Id)
                .ValueGeneratedNever();

            builder.Property(e => e.Name).HasMaxLength(100);

            builder.HasIndex(e => e.GroupId, "groupId_idx");

            builder.Property(e => e.Code).HasMaxLength(60);

            builder.HasOne(x => x.Group)
                .WithMany(g => g.Types)
                .HasForeignKey(x => x.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
