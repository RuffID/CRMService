using CRMService.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure.OkdeskEntity
{
    public class OkdeskRoleConfigure : IEntityTypeConfiguration<OkdeskRole>
    {
        public void Configure(EntityTypeBuilder<OkdeskRole> builder)
        {
            builder.ToTable("okdesk_role");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            builder.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        }
    }
}
