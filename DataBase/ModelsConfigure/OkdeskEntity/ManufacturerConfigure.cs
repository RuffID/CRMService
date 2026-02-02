using CRMService.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure.OkdeskEntity
{
    public class ManufacturerConfigure : IEntityTypeConfiguration<Manufacturer>
    {
        public void Configure(EntityTypeBuilder<Manufacturer> builder)
        {
            builder.ToTable("Manufacturers");

            builder.Property(e => e.Id)
                .ValueGeneratedNever();

            builder.Property(e => e.Code)
                .HasMaxLength(30);

            builder.Property(e => e.Description)
                .HasMaxLength(45);

            builder.Property(e => e.Name)
                .HasMaxLength(45);
        }
    }
}
