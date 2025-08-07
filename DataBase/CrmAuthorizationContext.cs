using CRMService.DataBase.ModelsConfigure.Authorization;
using CRMService.Models.Authorization;
using CRMService.Models.ConfigClass;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CRMService.DataBase
{
    public partial class CrmAuthorizationContext : DbContext
    {
        private readonly DatabaseSettings _databaseSettings;

        public CrmAuthorizationContext(IOptions<DatabaseSettings> dbSetings)
        {
            _databaseSettings = dbSetings.Value;
        }

        public virtual DbSet<BlockReason> BlockReasons { get; set; } = null!;

        public virtual DbSet<Role> Roles { get; set; } = null!;

        public virtual DbSet<Session> Sessions { get; set; } = null!;

        public virtual DbSet<User> Users { get; set; } = null!;

        public virtual DbSet<UserRole> UserRoles { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(_databaseSettings.MySqlServerAuthorization ?? "", 
                new MySqlServerVersion(new Version(_databaseSettings.MySqlVersion ?? "")));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .UseCollation("utf8mb4_0900_ai_ci")
                .HasCharSet("utf8mb4");

            modelBuilder
                .ApplyConfiguration(new UserConfigure())
                .ApplyConfiguration(new RoleConfigure())
                .ApplyConfiguration(new BlockReasonConfigure())
                .ApplyConfiguration(new SessionConfigure())
                .ApplyConfiguration(new UserRoleConfigure());

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}