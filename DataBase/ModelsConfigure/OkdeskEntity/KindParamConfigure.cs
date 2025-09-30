using CRMService.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure.OkdeskEntity
{
    public class KindParamConfigure : IEntityTypeConfiguration<KindParam>
    {
        public void Configure(EntityTypeBuilder<KindParam> builder)
        {
            builder.ToTable("kindparams");

            builder.HasKey(e => new { e.KindId, e.KindParameterId });

            builder.HasIndex(e => e.KindId, "kindKey_idx");

            builder.HasIndex(e => e.KindParameterId, "kindParameters_idx");

            builder.Property(e => e.KindId)
                .HasColumnName("kindId");

            builder.Property(e => e.KindParameterId)
                .HasColumnName("kindParameterId");

            builder.HasOne(d => d.Kind)
                .WithMany(p => p.KindParams)
                .HasForeignKey(d => d.KindId)
                .HasConstraintName("kindPrm")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(d => d.KindParameter)
                .WithMany(p => p.KindParams)
                .HasForeignKey(d => d.KindParameterId)
                .HasConstraintName("kindParameters")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
