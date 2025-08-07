using CRMService.Models.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure.Authorization
{
    public class UserRoleConfigure : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.HasKey(e => e.Id).HasName("PRIMARY");

            builder.ToTable("user_role");

            builder.HasIndex(e => e.RoleId, "role_id_idx");

            builder.HasIndex(e => e.UserId, "user_id_idx");

            builder.Property(e => e.Id)
                .HasMaxLength(36)
                .HasColumnName("id");
            builder.Property(e => e.RoleId)
                .HasMaxLength(36)
                .HasColumnName("role_id");
            builder.Property(e => e.UserId)
                .HasMaxLength(36)
                .HasColumnName("user_id");

            builder.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("role_id");

            builder.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("user_id");
        }
    }
}
