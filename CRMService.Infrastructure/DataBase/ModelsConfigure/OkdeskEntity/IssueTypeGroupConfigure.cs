using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.Infrastructure.DataBase.ModelsConfigure.OkdeskEntity
{
    public class IssueTypeGroupConfigure : IEntityTypeConfiguration<IssueTypeGroup>
    {
        public void Configure(EntityTypeBuilder<IssueTypeGroup> builder)
        {
            builder.ToTable("IssueTypeGroups");

            builder.Property(e => e.Id).ValueGeneratedNever();

            builder.Property(e => e.Name).HasMaxLength(100);

            builder.Property(e => e.Code).HasMaxLength(60);

            builder.HasOne(x => x.Parent)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentGroupId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}


