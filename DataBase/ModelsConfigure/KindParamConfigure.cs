using CRMService.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure
{
    public class KindParamConfigure : IEntityTypeConfiguration<KindParam>
    {
        public void Configure(EntityTypeBuilder<KindParam> builder)
        {
            builder.HasKey(e => e.Id).HasName("PRIMARY");

            builder.ToTable("kindparams");

            builder.HasIndex(e => e.KindId, "kindKey_idx");

            builder.HasIndex(e => e.KindParameterId, "kindParameters_idx");

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
            builder.Property(e => e.KindId).HasColumnName("kindId");
            builder.Property(e => e.KindParameterId).HasColumnName("kindParameterId");

            builder.HasOne(d => d.Kind).WithMany(p => p.KindParams)
                .HasForeignKey(d => d.KindId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("kindPrm");

            builder.HasOne(d => d.KindParameter).WithMany(p => p.KindParams)
                .HasForeignKey(d => d.KindParameterId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("kindParameters");
        }
    }
}
