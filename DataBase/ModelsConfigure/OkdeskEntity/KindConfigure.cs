using CRMService.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure.OkdeskEntity
{
    public class KindConfigure : IEntityTypeConfiguration<Kind>
    {
        public void Configure(EntityTypeBuilder<Kind> builder)
        {
            builder.ToTable("Kind");

            builder.Property(e => e.Id)
                .ValueGeneratedNever();

            builder.Property(e => e.Code)
                .HasMaxLength(30);

            builder.Property(e => e.Description)
                .HasMaxLength(200);

            builder.Property(e => e.Name)
                .HasMaxLength(100);
        }
    }
}
