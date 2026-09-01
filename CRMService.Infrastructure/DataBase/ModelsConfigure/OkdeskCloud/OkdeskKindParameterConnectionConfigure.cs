using CRMService.Application.Models.OkdeskSource;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.Infrastructure.DataBase.ModelsConfigure.OkdeskCloud
{
    public class OkdeskKindParameterConnectionConfigure : IEntityTypeConfiguration<OkdeskKindParameterConnectionRecord>
    {
        public void Configure(EntityTypeBuilder<OkdeskKindParameterConnectionRecord> builder)
        {
            builder.ToTable("equipment_kind_parameters");
            builder.HasKey(x => new { x.KindId, x.KindParameterId });

            builder.Property(x => x.KindId).HasColumnName("equipment_kind_id");
            builder.Property(x => x.KindParameterId).HasColumnName("parameter_id");
        }
    }
}
