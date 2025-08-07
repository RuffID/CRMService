using CRMService.Models.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure.Authorization
{
    public class UserConfigure : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(e => e.Id).HasName("PRIMARY");

            builder.ToTable("user");

            builder.HasIndex(e => e.Email, "email_UNIQUE").IsUnique();

            builder.HasIndex(e => e.Id, "id_UNIQUE").IsUnique();

            builder.HasIndex(e => e.Login, "login_UNIQUE").IsUnique();

            builder.Property(e => e.Id)
                .HasMaxLength(36)
                .HasColumnName("id");
            builder.Property(e => e.PasswordHash)
                .HasMaxLength(150)
                .HasColumnName("password_hash");
            builder.Property(e => e.Login)
                .HasMaxLength(45)
                .HasColumnName("login");
            builder.Property(e => e.Email)
                .HasMaxLength(45)
                .HasColumnName("email");
            builder.Property(e => e.Active).HasColumnName("active");
        }
    }
}
