using CRMService.Models.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure.Authorization
{
    public class CrmRoleConfigure : IEntityTypeConfiguration<CrmRole>
    {
        public void Configure(EntityTypeBuilder<CrmRole> entity)
        {
            entity.ToTable("crm_role");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("NEWID()");

            entity.Property(e => e.Name)
                .HasMaxLength(45)                
                .HasColumnName("name");
        }
    }
}
