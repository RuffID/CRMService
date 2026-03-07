using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.Infrastructure.DataBase.ModelsConfigure.OkdeskCloud
{
    public class ManufacturerOkdeskConfigure : IEntityTypeConfiguration<Manufacturer>
    {
        public void Configure(EntityTypeBuilder<Manufacturer> builder)
        {
            builder.ToTable("equipment_manufacturers");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.Code).HasColumnName("code");
            builder.Property(x => x.Name).HasColumnName("name");
            builder.Ignore(x => x.Visible);
            builder.Ignore(x => x.Description);
            builder.Ignore(x => x.Equipment);
            builder.Ignore(x => x.Models);
        }
    }
}