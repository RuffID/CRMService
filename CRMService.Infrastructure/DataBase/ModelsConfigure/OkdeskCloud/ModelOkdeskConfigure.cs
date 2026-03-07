using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.Infrastructure.DataBase.ModelsConfigure.OkdeskCloud
{
    public class ModelOkdeskConfigure : IEntityTypeConfiguration<Model>
    {
        public void Configure(EntityTypeBuilder<Model> builder)
        {
            builder.ToTable("equipment_models");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.Code).HasColumnName("code");
            builder.Property(x => x.Name).HasColumnName("name");
            builder.Property(x => x.KindId).HasColumnName("equipment_kind_id");
            builder.Property(x => x.ManufacturerId).HasColumnName("equipment_manufacturer_id");
            builder.Ignore(x => x.Visible);
            builder.Ignore(x => x.Description);
            builder.Ignore(x => x.Equipment);
            builder.Ignore(x => x.Kind);
            builder.Ignore(x => x.Manufacturer);
        }
    }
}