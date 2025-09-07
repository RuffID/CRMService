using CRMService.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure
{
    public class ModelConfigure : IEntityTypeConfiguration<Model>
    {
        public void Configure(EntityTypeBuilder<Model> builder)
        {
            builder.ToTable("model");            

            builder.HasIndex(e => e.KindId, "kindId_idx");

            builder.HasIndex(e => e.ManufacturerId, "manufacturerId_idx");

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            builder.Property(e => e.Code)
                .HasMaxLength(30)
                .HasColumnName("code");

            builder.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("description");

            builder.Property(e => e.KindId).HasColumnName("kindId");

            builder.Property(e => e.ManufacturerId).HasColumnName("manufacturerId");

            builder.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");

            builder.Property(e => e.Visible).HasColumnName("visible");

            builder.HasOne(d => d.Kind).WithMany(p => p.Models)
                .HasForeignKey(d => d.KindId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("kindId");

            builder.HasOne(d => d.Manufacturer).WithMany(p => p.Models)
                .HasForeignKey(d => d.ManufacturerId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("manufacturerId");
        }
    }
}
