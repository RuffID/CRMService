using CRMService.Domain.Models.CrmEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.Infrastructure.DataBase.ModelsConfigure.CrmEntity
{
    public class PlanColorSchemeConfigure : IEntityTypeConfiguration<PlanColorScheme>
    {
        public void Configure(EntityTypeBuilder<PlanColorScheme> builder)
        {
            builder.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            builder.Property(e => e.Color)
                .HasMaxLength(50);

            builder.HasIndex(e => new { e.PlanId, e.FromPercent, e.ToPercent });

            builder
                .HasOne(e => e.Plan)
                .WithMany(p => p.PlanColorSchemes)
                .HasForeignKey(e => e.PlanId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}


