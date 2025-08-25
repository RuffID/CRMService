using CRMService.Models.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure.Authorization
{
    public class SessionConfigure : IEntityTypeConfiguration<Session>
    {
        public void Configure(EntityTypeBuilder<Session> builder)
        {
            builder.HasKey(e => e.Id).HasName("PRIMARY");

            builder.ToTable("session");

            builder.HasIndex(e => e.Id, "session_id_UNIQUE").IsUnique();

            builder.HasIndex(e => e.UserId, "user_session_id_idx");

            builder.Property(e => e.Id)
                .HasMaxLength(36)
                .HasColumnName("id");
            builder.Property(e => e.ExpirationRefreshToken)
                .HasColumnType("datetime")
                .HasColumnName("expiration_refresh_token");
            builder.Property(e => e.RefreshToken)
                .HasMaxLength(88)
                .HasColumnName("refresh_token");
            builder.Property(e => e.UserId)
                .HasMaxLength(36)
                .HasColumnName("user_id");

            builder.HasOne(d => d.User).WithMany(p => p.Sessions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("user_session_id");
        }
    }
}
