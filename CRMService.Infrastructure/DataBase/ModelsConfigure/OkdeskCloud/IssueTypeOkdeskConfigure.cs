using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.Infrastructure.DataBase.ModelsConfigure.OkdeskCloud
{
    public class IssueTypeOkdeskConfigure : IEntityTypeConfiguration<IssueType>
    {
        public void Configure(EntityTypeBuilder<IssueType> builder)
        {
            builder.ToTable("issue_work_types");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.Code).HasColumnName("code");
            builder.Property(x => x.Name).HasColumnName("name");
            builder.Property(x => x.IsInner).HasColumnName("inner");
            builder.Property(x => x.GroupId).HasColumnName("group_id");
            builder.Ignore(x => x.IsDefault);
            builder.Ignore(x => x.AvailableForClient);
            builder.Ignore(x => x.Group);
            builder.Ignore(x => x.Issues);
        }
    }
}