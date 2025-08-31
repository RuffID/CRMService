using CRMService.Models.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure.Authorization
{
    public class UserConfigure : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("user");

            builder.HasKey(e => e.Id).HasName("PRIMARY");

            builder.HasIndex(e => e.Login, "login_UNIQUE").IsUnique();

            builder.Property(e => e.Id)
                .HasMaxLength(36)
                .HasColumnName("id")
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("UUID()");

            builder.Property(e => e.Password)
                .HasMaxLength(150)
                .HasColumnName("password_hash");

            builder.Property(e => e.Login)
                .HasMaxLength(45)
                .HasColumnName("login");

            builder.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");

            builder.Property(e => e.Active).HasColumnName("active");

            builder
                .HasMany(u => u.Roles)
                .WithMany(r => r.Users)
                .UsingEntity<UserRole>();
        }
    }
}
