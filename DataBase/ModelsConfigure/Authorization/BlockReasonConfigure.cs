using CRMService.Models.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure.Authorization
{
    public class BlockReasonConfigure : IEntityTypeConfiguration<BlockReason>
    {
        public void Configure(EntityTypeBuilder<BlockReason> builder)
        {
            builder.ToTable("block_reason");

            builder.HasKey(e => e.Id).HasName("PRIMARY");

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .HasMaxLength(36)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("UUID()");

            builder.Property(e => e.BlockingDate)
                .HasColumnType("datetime")
                .HasColumnName("blocking_date");

            builder.Property(e => e.UnblockingDate)
                .HasColumnType("datetime")
                .HasColumnName("unblocking_date");

            builder.Property(e => e.ReasonBlock)
                .HasMaxLength(150)
                .HasColumnName("reason_block");

            builder.Property(e => e.UnblockingReason)
                .HasMaxLength(150)
                .HasColumnName("reason_unblock");

            builder.Property(e => e.UserId)
                .HasMaxLength(36)
                .HasColumnName("user_id");

            builder.HasOne(d => d.User).WithOne(p => p.BlockReason)
                .HasForeignKey<BlockReason>(d => d.UserId)
                .HasConstraintName("user_block_id")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
