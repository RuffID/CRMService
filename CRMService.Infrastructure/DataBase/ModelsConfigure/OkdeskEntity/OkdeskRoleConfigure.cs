using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.Infrastructure.DataBase.ModelsConfigure.OkdeskEntity
{
    public class OkdeskRoleConfigure : IEntityTypeConfiguration<OkdeskRole>
    {
        public void Configure(EntityTypeBuilder<OkdeskRole> builder)
        {
            builder.ToTable("OkdeskRole");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .ValueGeneratedNever();

            builder.Property(e => e.Name)
                .HasMaxLength(100);
        }
    }
}



