using CRMService.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure
{
    public class OkdeskRoleConfigure : IEntityTypeConfiguration<OkdeskRole>
    {
        public void Configure(EntityTypeBuilder<OkdeskRole> builder)
        {
            builder.ToTable("okdesk_role");

            

            builder.Property(e => e.Id).HasColumnName("id");

            builder.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        }
    }
}
