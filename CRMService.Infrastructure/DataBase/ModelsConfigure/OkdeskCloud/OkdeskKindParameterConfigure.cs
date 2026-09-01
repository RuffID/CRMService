using CRMService.Application.Models.OkdeskSource;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.Infrastructure.DataBase.ModelsConfigure.OkdeskCloud
{
    public class OkdeskKindParameterConfigure : IEntityTypeConfiguration<OkdeskKindParameterRecord>
    {
        public void Configure(EntityTypeBuilder<OkdeskKindParameterRecord> builder)
        {
            builder.ToTable("equipment_parameters");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.Code).HasColumnName("code");
            builder.Property(x => x.Name).HasColumnName("name");
            builder.Property(x => x.FieldType).HasColumnName("field_type");
        }
    }
}
