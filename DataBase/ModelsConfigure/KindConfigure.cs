using CRMService.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure
{
    public class KindConfigure : IEntityTypeConfiguration<Kind>
    {
        public void Configure(EntityTypeBuilder<Kind> builder)
        {
            builder.ToTable("kind");

            

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            builder.Property(e => e.Code)
                .HasMaxLength(30)
                .HasColumnName("code");

            builder.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("description");

            builder.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");

            builder.Property(e => e.Visible).HasColumnName("visible");
        }
    }
}
