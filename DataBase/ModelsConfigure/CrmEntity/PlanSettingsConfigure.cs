using CRMService.Models.CrmEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure.CrmEntity
{
    public class PlanSettingsConfigure : IEntityTypeConfiguration<PlanSetting>
    {
        public void Configure(EntityTypeBuilder<PlanSetting> builder)
        {
            builder.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            builder.HasIndex(p => p.EmployeeId).IsUnique();

            builder
                .HasOne(p => p.Employee)
                .WithOne(e => e.PlanSetting)
                .HasForeignKey<PlanSetting>(p => p.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}