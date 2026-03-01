using CRMService.Domain.Models.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.Infrastructure.DataBase.ModelsConfigure.Authorization
{
    public class CrmRoleConfigure : IEntityTypeConfiguration<CrmRole>
    {
        public void Configure(EntityTypeBuilder<CrmRole> entity)
        {
            entity.ToTable("CrmRole");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            entity.Property(e => e.Name)
                .HasMaxLength(45);
        }
    }
}



