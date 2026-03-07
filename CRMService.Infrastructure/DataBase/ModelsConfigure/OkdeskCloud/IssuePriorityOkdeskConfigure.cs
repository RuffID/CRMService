using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.Infrastructure.DataBase.ModelsConfigure.OkdeskCloud
{
    public class IssuePriorityOkdeskConfigure : IEntityTypeConfiguration<IssuePriority>
    {
        public void Configure(EntityTypeBuilder<IssuePriority> builder)
        {
            builder.ToTable("issue_priorities");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.Code).HasColumnName("code");
            builder.Property(x => x.Name).HasColumnName("name");
            builder.Ignore(x => x.Position);
            builder.Ignore(x => x.Color);
            builder.Ignore(x => x.Issues);
        }
    }
}