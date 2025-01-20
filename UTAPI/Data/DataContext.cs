using Microsoft.EntityFrameworkCore;
using UTAPI.Models;

namespace UTAPI.Data
{
    /// <summary>
    /// Represents the application's DataContext, inheriting from DbContext.
    /// This context manages the interaction between the application and the database,
    /// and includes DbSets for all entities used in the application.
    /// </summary>
    public class DataContext : DbContext
    {
        // Uncomment and inject the AuditSaveChangesInterceptor if you plan to use it for auditing.
        // private readonly AuditSaveChangesInterceptor _auditInterceptor;

        // Constructor accepting DbContextOptions for configuration and an optional interceptor for auditing.
        // public DataContext(DbContextOptions<DataContext> options, AuditSaveChangesInterceptor auditInterceptor) : base(options)
        // {
        //     _auditInterceptor = auditInterceptor;
        // }

        /// <summary>
        /// Constructor to initialize the DataContext with the provided options.
        /// </summary>
        /// <param name="options">DbContext options for configuration.</param>
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        // DbSets for each entity in the database
        public DbSet<User> User { get; set; }
        public DbSet<Entity> Entity { get; set; }
        public DbSet<Region> Region { get; set; }
        public DbSet<Stop> Stop { get; set; }

        public DbSet<RouteLine> RouteLine { get; set; }

        public DbSet<RouteStop> RouteStop { get; set; }
        public DbSet<Models.Route> Route { get; set; }
        public DbSet<FavRoute> FavRoute { get; set; }
        public DbSet<Audit> Audit { get; set; }
        public DbSet<Session> Session { get; set; }

        public DbSet<EntityDriver> EntityDriver { get; set; }
        public DbSet<RouteHistory> RouteHistory { get; set; }
        public DbSet<DriverRoute> DriverRoute { get; set; }

        public DbSet<PriceTable> PriceTable { get; set; }

        public DbSet<PriceTableContent> PriceTableContent { get; set; }

        /// <summary>
        /// Configures the model during the creation process. This includes entity configurations
        /// and mappings to control how the entities interact with the database.
        /// </summary>
        /// <param name="modelBuilder">The ModelBuilder instance to configure the model.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the User entity by ignoring sensitive fields that don't need to be mapped to the database
            modelBuilder.Entity<User>()
                .Ignore(c => c.AccessFailedCount)       // Ignored as it's not part of the table mapping
                .Ignore(c => c.EmailConfirmed)         // Ignored as it's not part of the table mapping
                .Ignore(c => c.ConcurrencyStamp)       // Ignored as it's not part of the table mapping
                .Ignore(c => c.PhoneNumberConfirmed)   // Ignored as it's not part of the table mapping
                .Ignore(c => c.LockoutEnabled)         // Ignored as it's not part of the table mapping
                .Ignore(c => c.LockoutEnd)             // Ignored as it's not part of the table mapping
                .Ignore(c => c.NormalizedEmail)        // Ignored as it's not part of the table mapping
                .Ignore(c => c.PasswordHash)           // Ignored as it's not part of the table mapping
                .Ignore(c => c.NormalizedUserName)     // Ignored as it's not part of the table mapping
                .Ignore(c => c.PhoneNumber)            // Ignored as it's not part of the table mapping
                .Ignore(c => c.UserName)               // Ignored as it's not part of the table mapping
                .Ignore(c => c.SecurityStamp)          // Ignored as it's not part of the table mapping
                .Ignore(c => c.TwoFactorEnabled);      // Ignored as it's not part of the table mapping

            // Set a custom table name for the User entity in the database
            modelBuilder.Entity<User>().ToTable("User");
        }
    }
}
