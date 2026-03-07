using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.Infrastructure.DataBase.ModelsConfigure.OkdeskCloud
{
    public class KindParamOkdeskConfigure : IEntityTypeConfiguration<KindParam>
    {
        public void Configure(EntityTypeBuilder<KindParam> builder)
        {
            builder.ToTable("equipment_kind_parameters");
            builder.HasKey(x => new { x.KindId, x.KindParameterId });

            builder.Property(x => x.KindId).HasColumnName("equipment_kind_id");
            builder.Property(x => x.KindParameterId).HasColumnName("parameter_id");
            builder.Ignore(x => x.Kind);
            builder.Ignore(x => x.KindParameter);
        }
    }
}