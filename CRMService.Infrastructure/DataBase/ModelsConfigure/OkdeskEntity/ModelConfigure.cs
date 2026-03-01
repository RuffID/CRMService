using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.Infrastructure.DataBase.ModelsConfigure.OkdeskEntity
{
    public class ModelConfigure : IEntityTypeConfiguration<Model>
    {
        public void Configure(EntityTypeBuilder<Model> builder)
        {
            builder.ToTable("Model");            

            builder.HasIndex(e => e.KindId, "kindId_idx");

            builder.HasIndex(e => e.ManufacturerId, "manufacturerId_idx");

            builder.Property(e => e.Id)
                .ValueGeneratedNever();

            builder.Property(e => e.Code)
                .HasMaxLength(30);

            builder.Property(e => e.Description)
                .HasMaxLength(200);

            builder.Property(e => e.Name)
                .HasMaxLength(100);


            builder.HasOne(d => d.Kind)
                .WithMany(p => p.Models)
                .HasForeignKey(d => d.KindId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.Manufacturer)
                .WithMany(p => p.Models)
                .HasForeignKey(d => d.ManufacturerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}



