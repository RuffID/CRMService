using CRMService.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure.OkdeskEntity
{
    public class ManufacturerConfigure : IEntityTypeConfiguration<Manufacturer>
    {
        public void Configure(EntityTypeBuilder<Manufacturer> builder)
        {
            builder.ToTable("manufacturers");

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            builder.Property(e => e.Code)
                .HasMaxLength(30)
                .HasColumnName("code");

            builder.Property(e => e.Description)
                .HasMaxLength(45)
                .HasColumnName("description");

            builder.Property(e => e.Name)
                .HasMaxLength(45)
                .HasColumnName("name");

            builder.Property(e => e.Visible).HasColumnName("visible");
        }
    }
}
