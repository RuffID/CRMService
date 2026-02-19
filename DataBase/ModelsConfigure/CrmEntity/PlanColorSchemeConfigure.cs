using CRMService.Models.CrmEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure.CrmEntity
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
        }
    }
}