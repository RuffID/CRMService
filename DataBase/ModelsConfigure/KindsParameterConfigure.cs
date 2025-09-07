using CRMService.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure
{
    public class KindsParameterConfigure : IEntityTypeConfiguration<KindsParameter>
    {
        public void Configure(EntityTypeBuilder<KindsParameter> builder)
        {
            

            builder.ToTable("kinds_parameters");

            builder.Property(e => e.Id).HasColumnName("id");

            builder.Property(e => e.Code)
                .HasMaxLength(30)
                .HasColumnName("code");

            builder.Property(e => e.FieldType)
                .HasMaxLength(30)
                .HasColumnName("fieldType");

            builder.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
        }
    }
}
