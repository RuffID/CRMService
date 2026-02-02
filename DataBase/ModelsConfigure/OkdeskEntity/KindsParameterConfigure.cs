using CRMService.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure.OkdeskEntity
{
    public class KindsParameterConfigure : IEntityTypeConfiguration<KindsParameter>
    {
        public void Configure(EntityTypeBuilder<KindsParameter> builder)
        {
            builder.ToTable("KindsParameters");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .ValueGeneratedNever();

            builder.Property(e => e.Code)
                .HasMaxLength(30);

            builder.Property(e => e.FieldType)
                .HasMaxLength(30);

            builder.Property(e => e.Name)
                .HasMaxLength(200);
        }
    }
}
