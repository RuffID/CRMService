using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.Infrastructure.DataBase.ModelsConfigure.OkdeskCloud
{
    public class KindsParameterOkdeskConfigure : IEntityTypeConfiguration<KindsParameter>
    {
        public void Configure(EntityTypeBuilder<KindsParameter> builder)
        {
            builder.ToTable("equipment_parameters");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.Code).HasColumnName("code");
            builder.Property(x => x.Name).HasColumnName("name");
            builder.Property(x => x.FieldTypeRaw).HasColumnName("field_type");
            builder.Ignore(x => x.Equipment_kind_codes);
            builder.Ignore(x => x.FieldType);
            builder.Ignore(x => x.KindParams);
            builder.Ignore(x => x.Parameters);
        }
    }
}
