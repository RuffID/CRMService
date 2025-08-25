using CRMService.Models.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure.Authorization
{
    public class CrmRoleConfigure : IEntityTypeConfiguration<CrmRole>
    {
        public void Configure(EntityTypeBuilder<CrmRole> entity)
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("crm_role");

            entity.HasIndex(e => e.Id, "id_UNIQUE").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(45)                
                .HasColumnName("name");
        }
    }
}
