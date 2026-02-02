using CRMService.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure.OkdeskEntity
{
    public class KindParamConfigure : IEntityTypeConfiguration<KindParam>
    {
        public void Configure(EntityTypeBuilder<KindParam> builder)
        {
            builder.ToTable("KindParams");

            builder.HasKey(e => new { e.KindId, e.KindParameterId });

            builder.HasIndex(e => e.KindId, "kindKey_idx");

            builder.HasIndex(e => e.KindParameterId, "kindParameters_idx");

            builder.HasOne(d => d.Kind)
                .WithMany(p => p.KindParams)
                .HasForeignKey(d => d.KindId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(d => d.KindParameter)
                .WithMany(p => p.KindParams)
                .HasForeignKey(d => d.KindParameterId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
