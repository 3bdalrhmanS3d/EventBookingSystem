using Microsoft.EntityFrameworkCore;
using EventBookingSystemV1.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventBookingSystemV1.Data
{
    /// <summary>
    /// Application DbContext with soft-delete support and indexes.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        // Add-Migration M1
        // Update-Database
        // Remove-Migration

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets for all entities
        public DbSet<User> Users { get; set; }
        public DbSet<AccountActivation> AccountActivation { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventCategory> EventCategories { get; set; }
        public DbSet<EventTag> EventTags { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Venue> Venues { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply global filter for soft-delete entities
            // Any entity implementing ISoftDelete will exclude IsDeleted == true rows
            foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                       .Where(t => typeof(ISoftDelete).IsAssignableFrom(t.ClrType)))
            {
                var method = typeof(ApplicationDbContext)
                    .GetMethod(nameof(ConfigureSoftDelete), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                    ?.MakeGenericMethod(entityType.ClrType);

                method?.Invoke(null, new object[] { modelBuilder });
            }

            // Ensure unique email addresses for users
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Prevent duplicate favorites for the same user and event
            modelBuilder.Entity<Favorite>()
                .HasIndex(f => new { f.UserId, f.EventId })
                .IsUnique();

            // Speed up lookups for bookings by user and event
            modelBuilder.Entity<Booking>()
                .HasIndex(b => new { b.UserId, b.EventId });

            // Optimize queries filtering by booked date
            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.BookedAt);

            // Speed up retrieval of activation tokens by token string
            modelBuilder.Entity<AccountActivation>()
                .HasIndex(a => a.Code);

            // Prevent duplicate reviews by the same user for the same event
            modelBuilder.Entity<Review>()
                .HasIndex(r => new { r.UserId, r.EventId })
                .IsUnique();

            // Global filter for Reviews: exclude reviews if the user is soft-deleted
            modelBuilder.Entity<Review>()
                .HasQueryFilter(r => !r.IsDeleted && !r.User.IsDeleted);

            // Global filter for Bookings: exclude bookings if the user is soft-deleted
            modelBuilder.Entity<Booking>()
                .HasQueryFilter(b => !b.IsDeleted && !b.User.IsDeleted);

            // Global filter for Favorites: exclude favorites if the user is soft-deleted
            modelBuilder.Entity<Favorite>()
                .HasQueryFilter(f => !f.IsDeleted && !f.User.IsDeleted);


        }

        /// <summary>
        /// Configures the soft-delete global filter for entities.
        /// </summary>
        private static void ConfigureSoftDelete<TEntity>(ModelBuilder builder)
            where TEntity : class, ISoftDelete
        {
            builder.Entity<TEntity>()
                   .HasQueryFilter(e => !e.IsDeleted);
        }

        /// <summary>
        /// Overrides SaveChanges to convert deletes into soft-deletes.
        /// </summary>
        public override int SaveChanges()
        {
            foreach (var entry in ChangeTracker.Entries<ISoftDelete>()
                             .Where(e => e.State == EntityState.Deleted))
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
            }

            return base.SaveChanges();
        }

        /// <summary>
        /// Overrides SaveChangesAsync to convert deletes into soft-deletes.
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<ISoftDelete>()
                             .Where(e => e.State == EntityState.Deleted))
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
