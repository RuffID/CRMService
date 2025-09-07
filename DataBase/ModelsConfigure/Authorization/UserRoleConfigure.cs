using CRMService.Models.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure.Authorization
{
    public class UserRoleConfigure : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.ToTable("user_role");

            builder.HasKey(e => new { e.RoleId, e.UserId });

            builder.Property(e => e.RoleId)
                .HasMaxLength(36)
                .HasColumnName("role_id");

            builder.Property(e => e.UserId)
                .HasMaxLength(36)
                .HasColumnName("user_id");

            builder.HasOne(d => d.Role)
                .WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("role_id")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(d => d.User)
                .WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("user_id")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
