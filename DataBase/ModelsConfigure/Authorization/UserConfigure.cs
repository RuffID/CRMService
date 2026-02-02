using CRMService.Models.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure.Authorization
{
    public class UserConfigure : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("User");            

            builder.HasIndex(e => e.Login, "login_UNIQUE").IsUnique();

            builder.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            builder.Property(e => e.Password)
                .HasMaxLength(256);

            builder.Property(e => e.Login)
                .HasMaxLength(45);

            builder.Property(e => e.Name)
                .HasMaxLength(100);

            builder
                .HasMany(u => u.Roles)
                .WithMany(r => r.Users)
                .UsingEntity<UserRole>();
        }
    }
}
