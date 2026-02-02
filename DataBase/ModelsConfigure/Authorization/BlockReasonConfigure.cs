using CRMService.Models.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure.Authorization
{
    public class BlockReasonConfigure : IEntityTypeConfiguration<BlockReason>
    {
        public void Configure(EntityTypeBuilder<BlockReason> builder)
        {
            builder.ToTable("BlockReason");            

            builder.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            builder.Property(e => e.ReasonBlock)
                .HasMaxLength(150);

            builder.Property(e => e.UnblockingReason)
                .HasMaxLength(150);

            builder.Property(e => e.UserId)
                .HasMaxLength(36);

            builder.HasOne(d => d.User)
                .WithOne(p => p.BlockReason)
                .HasForeignKey<BlockReason>(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
