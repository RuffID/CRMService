using CRMService.Models.CrmEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure.CrmEntity
{
    public class PlanConfigure : IEntityTypeConfiguration<Plan>
    {
        public void Configure(EntityTypeBuilder<Plan> builder)
        {
            builder.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            builder.Property(e => e.Name)
                .HasMaxLength(200);

            builder.Property(e => e.PlanColor)
                .HasMaxLength(50);

            builder.Property(e => e.Period)
                .IsRequired()
                .HasMaxLength(16);

            builder.HasIndex(e => e.Name)
                .IsUnique();
        }
    }
}
