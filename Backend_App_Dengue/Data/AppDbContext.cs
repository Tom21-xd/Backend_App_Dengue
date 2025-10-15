using Backend_App_Dengue.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend_App_Dengue.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<TypeOfBlood> TypesOfBlood { get; set; }
        public DbSet<Hospital> Hospitals { get; set; }
        public DbSet<Case> Cases { get; set; }
        public DbSet<CaseState> CaseStates { get; set; }
        public DbSet<TypeOfDengue> TypesOfDengue { get; set; }
        public DbSet<Symptom> Symptoms { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<FCMToken> FCMTokens { get; set; }
        public DbSet<Publication> Publications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Password).IsRequired();

                // Relationships
                entity.HasOne(e => e.Role)
                    .WithMany(r => r.Users)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.City)
                    .WithMany(c => c.Users)
                    .HasForeignKey(e => e.CityId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.BloodType)
                    .WithMany(b => b.Users)
                    .HasForeignKey(e => e.BloodTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Genre)
                    .WithMany(g => g.Users)
                    .HasForeignKey(e => e.GenreId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Case entity with multiple User relationships
            modelBuilder.Entity<Case>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Patient)
                    .WithMany(u => u.CasesAsPatient)
                    .HasForeignKey(e => e.PatientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.MedicalStaff)
                    .WithMany(u => u.CasesAsMedicalStaff)
                    .HasForeignKey(e => e.MedicalStaffId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Hospital)
                    .WithMany(h => h.Cases)
                    .HasForeignKey(e => e.HospitalId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.State)
                    .WithMany(s => s.Cases)
                    .HasForeignKey(e => e.StateId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.TypeOfDengue)
                    .WithMany(t => t.Cases)
                    .HasForeignKey(e => e.TypeOfDengueId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Hospital entity
            modelBuilder.Entity<Hospital>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.City)
                    .WithMany(c => c.Hospitals)
                    .HasForeignKey(e => e.CityId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure City entity
            modelBuilder.Entity<City>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Department)
                    .WithMany(d => d.Cities)
                    .HasForeignKey(e => e.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Publication entity
            modelBuilder.Entity<Publication>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Publications)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Notification entity
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Notifications)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure FCMToken entity
            modelBuilder.Entity<FCMToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.FCMTokens)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure catalog entities
            modelBuilder.Entity<Role>().HasKey(e => e.Id);
            modelBuilder.Entity<Genre>().HasKey(e => e.Id);
            modelBuilder.Entity<Department>().HasKey(e => e.Id);
            modelBuilder.Entity<TypeOfBlood>().HasKey(e => e.Id);
            modelBuilder.Entity<TypeOfDengue>().HasKey(e => e.Id);
            modelBuilder.Entity<CaseState>().HasKey(e => e.Id);
            modelBuilder.Entity<Symptom>().HasKey(e => e.Id);

            // Configure default values
            ConfigureDefaultValues(modelBuilder);
        }

        private void ConfigureDefaultValues(ModelBuilder modelBuilder)
        {
            // Timestamps will be set in code (SaveChangesAsync)

            // Set default active states
            modelBuilder.Entity<User>()
                .Property(e => e.IsActive)
                .HasDefaultValue(true);

            modelBuilder.Entity<Hospital>()
                .Property(e => e.IsActive)
                .HasDefaultValue(true);

            modelBuilder.Entity<Case>()
                .Property(e => e.IsActive)
                .HasDefaultValue(true);

            modelBuilder.Entity<Publication>()
                .Property(e => e.IsActive)
                .HasDefaultValue(true);

            modelBuilder.Entity<Notification>()
                .Property(e => e.IsActive)
                .HasDefaultValue(true);

            modelBuilder.Entity<Notification>()
                .Property(e => e.IsRead)
                .HasDefaultValue(false);
        }

        // Override SaveChangesAsync to update timestamps automatically
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.Now;

            // Handle new entities (CreatedAt)
            var newEntries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added);

            foreach (var entry in newEntries)
            {
                if (entry.Entity is Case caseEntity)
                    caseEntity.CreatedAt = now;
                else if (entry.Entity is Notification notification)
                    notification.CreatedAt = now;
                else if (entry.Entity is Publication publication)
                    publication.CreatedAt = now;
                else if (entry.Entity is FCMToken fcmToken)
                {
                    fcmToken.CreatedAt = now;
                    fcmToken.UpdatedAt = now;
                }
            }

            // Handle modified FCMTokens (UpdatedAt)
            var modifiedTokens = ChangeTracker.Entries<FCMToken>()
                .Where(e => e.State == EntityState.Modified);

            foreach (var entry in modifiedTokens)
            {
                entry.Entity.UpdatedAt = now;
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
