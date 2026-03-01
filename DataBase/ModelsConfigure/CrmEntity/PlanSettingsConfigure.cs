using CRMService.Models.CrmEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure.CrmEntity
{
    public class PlanSettingsConfigure : IEntityTypeConfiguration<PlanSetting>
    {
        public void Configure(EntityTypeBuilder<PlanSetting> builder)
        {
            builder.HasKey(p => new { p.PlanId, p.EmployeeId });

            builder
                .HasOne(p => p.Employee)
                .WithMany(e => e.PlanSettings)
                .HasForeignKey(p => p.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasOne(p => p.Plan)
                .WithMany(p => p.PlanSettings)
                .HasForeignKey(p => p.PlanId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}