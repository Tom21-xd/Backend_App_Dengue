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
        public DbSet<TypeOfDengueSymptom> TypeOfDengueSymptoms { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<FCMToken> FCMTokens { get; set; }
        public DbSet<Publication> Publications { get; set; }
        public DbSet<PublicationCategory> PublicationCategories { get; set; }
        public DbSet<PublicationReaction> PublicationReactions { get; set; }
        public DbSet<PublicationComment> PublicationComments { get; set; }
        public DbSet<PublicationView> PublicationViews { get; set; }
        public DbSet<PublicationTag> PublicationTags { get; set; }
        public DbSet<PublicationTagRelation> PublicationTagRelations { get; set; }
        public DbSet<SavedPublication> SavedPublications { get; set; }

        // Quiz entities
        public DbSet<QuizCategory> QuizCategories { get; set; }
        public DbSet<QuizQuestion> QuizQuestions { get; set; }
        public DbSet<QuizAnswer> QuizAnswers { get; set; }
        public DbSet<QuizAttempt> QuizAttempts { get; set; }
        public DbSet<QuizUserAnswer> QuizUserAnswers { get; set; }
        public DbSet<Certificate> Certificates { get; set; }

        // Authentication
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<UserApprovalRequest> UserApprovalRequests { get; set; }

        // Prevention Content
        public DbSet<PreventionCategory> PreventionCategories { get; set; }
        public DbSet<PreventionCategoryImage> PreventionCategoryImages { get; set; }
        public DbSet<PreventionItem> PreventionItems { get; set; }
        public DbSet<PreventionItemImage> PreventionItemImages { get; set; }

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

                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Publications)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.City)
                    .WithMany()
                    .HasForeignKey(e => e.CityId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Department)
                    .WithMany()
                    .HasForeignKey(e => e.DepartmentId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.CategoryId);
                entity.HasIndex(e => e.Priority);
                entity.HasIndex(e => e.IsPinned);
                entity.HasIndex(e => e.CreatedAt);
            });

            // Configure PublicationCategory entity
            modelBuilder.Entity<PublicationCategory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Configure PublicationReaction entity
            modelBuilder.Entity<PublicationReaction>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Publication)
                    .WithMany(p => p.Reactions)
                    .HasForeignKey(e => e.PublicationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // One reaction per user per publication
                entity.HasIndex(e => new { e.PublicationId, e.UserId }).IsUnique();
            });

            // Configure PublicationComment entity
            modelBuilder.Entity<PublicationComment>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Publication)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(e => e.PublicationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ParentComment)
                    .WithMany(c => c.Replies)
                    .HasForeignKey(e => e.ParentCommentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.PublicationId);
                entity.HasIndex(e => e.ParentCommentId);
            });

            // Configure PublicationView entity
            modelBuilder.Entity<PublicationView>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Publication)
                    .WithMany(p => p.Views)
                    .HasForeignKey(e => e.PublicationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.PublicationId, e.ViewedAt });
            });

            // Configure PublicationTag entity
            modelBuilder.Entity<PublicationTag>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Configure PublicationTagRelation entity (many-to-many)
            modelBuilder.Entity<PublicationTagRelation>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Publication)
                    .WithMany(p => p.PublicationTags)
                    .HasForeignKey(e => e.PublicationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Tag)
                    .WithMany(t => t.PublicationRelations)
                    .HasForeignKey(e => e.TagId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Unique constraint: one tag can only be applied once per publication
                entity.HasIndex(e => new { e.PublicationId, e.TagId }).IsUnique();
            });

            // Configure SavedPublication entity
            modelBuilder.Entity<SavedPublication>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Publication)
                    .WithMany(p => p.SavedByUsers)
                    .HasForeignKey(e => e.PublicationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Unique constraint: user can only save a publication once
                entity.HasIndex(e => new { e.PublicationId, e.UserId }).IsUnique();
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

            // Configure TypeOfDengueSymptom many-to-many relationship
            modelBuilder.Entity<TypeOfDengueSymptom>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Create unique constraint on the combination of TypeOfDengueId and SymptomId
                entity.HasIndex(e => new { e.TypeOfDengueId, e.SymptomId })
                    .IsUnique();

                // Configure relationship with TypeOfDengue
                entity.HasOne(e => e.TypeOfDengue)
                    .WithMany(t => t.TypeOfDengueSymptoms)
                    .HasForeignKey(e => e.TypeOfDengueId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Configure relationship with Symptom
                entity.HasOne(e => e.Symptom)
                    .WithMany(s => s.TypeOfDengueSymptoms)
                    .HasForeignKey(e => e.SymptomId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Permission entity
            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasIndex(e => e.Category);
            });

            // Configure RolePermission entity (many-to-many)
            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Role)
                    .WithMany(r => r.RolePermissions)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Permission)
                    .WithMany(p => p.RolePermissions)
                    .HasForeignKey(e => e.PermissionId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Unique constraint: one permission per role
                entity.HasIndex(e => new { e.RoleId, e.PermissionId }).IsUnique();
            });

            // Update Case entity configuration
            modelBuilder.Entity<Case>(entity =>
            {
                // Existing Case configuration...
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

                entity.HasOne(e => e.RegisteredBy)
                    .WithMany()
                    .HasForeignKey(e => e.RegisteredByUserId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Indexes for epidemiological queries
                entity.HasIndex(e => e.Year);
                entity.HasIndex(e => e.Neighborhood);
                entity.HasIndex(e => new { e.Latitude, e.Longitude });
                entity.HasIndex(e => e.RegisteredByUserId);
            });

            // Configure Quiz entities
            modelBuilder.Entity<QuizCategory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            modelBuilder.Entity<QuizQuestion>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Questions)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.CategoryId);
                entity.HasIndex(e => e.IsActive);
            });

            modelBuilder.Entity<QuizAnswer>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Question)
                    .WithMany(q => q.Answers)
                    .HasForeignKey(e => e.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.QuestionId);
            });

            modelBuilder.Entity<QuizAttempt>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.StartedAt);
                entity.HasIndex(e => e.Status);
            });

            modelBuilder.Entity<QuizUserAnswer>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Attempt)
                    .WithMany(a => a.UserAnswers)
                    .HasForeignKey(e => e.AttemptId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Question)
                    .WithMany(q => q.UserAnswers)
                    .HasForeignKey(e => e.QuestionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.SelectedAnswer)
                    .WithMany(a => a.UserAnswers)
                    .HasForeignKey(e => e.SelectedAnswerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.AttemptId);
                entity.HasIndex(e => e.QuestionId);
            });

            modelBuilder.Entity<Certificate>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Attempt)
                    .WithOne(a => a.Certificate)
                    .HasForeignKey<Certificate>(c => c.AttemptId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.VerificationCode).IsUnique();
                entity.HasIndex(e => e.UserId);
            });

            // Configure RefreshToken entity
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.Token).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.ExpiresAt);
            });

            // Configure UserApprovalRequest entity
            modelBuilder.Entity<UserApprovalRequest>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.RequestedRole)
                    .WithMany()
                    .HasForeignKey(e => e.RequestedRoleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ApprovedByAdmin)
                    .WithMany()
                    .HasForeignKey(e => e.ApprovedByAdminId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.RequestDate);
            });

            // Configure catalog entities
            modelBuilder.Entity<Role>().HasKey(e => e.Id);
            modelBuilder.Entity<Genre>().HasKey(e => e.Id);
            modelBuilder.Entity<Department>().HasKey(e => e.Id);
            modelBuilder.Entity<TypeOfBlood>().HasKey(e => e.Id);
            modelBuilder.Entity<TypeOfDengue>().HasKey(e => e.Id);
            modelBuilder.Entity<CaseState>().HasKey(e => e.Id);
            modelBuilder.Entity<Symptom>().HasKey(e => e.Id);

            // Configure Prevention entities
            modelBuilder.Entity<PreventionCategory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.DisplayOrder);
                entity.HasIndex(e => e.IsActive);
            });

            modelBuilder.Entity<PreventionCategoryImage>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Images)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.CategoryId);
                entity.HasIndex(e => e.DisplayOrder);
            });

            modelBuilder.Entity<PreventionItem>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Items)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.CategoryId);
                entity.HasIndex(e => e.DisplayOrder);
                entity.HasIndex(e => e.IsActive);
            });

            modelBuilder.Entity<PreventionItemImage>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Item)
                    .WithMany(i => i.Images)
                    .HasForeignKey(e => e.ItemId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.ItemId);
                entity.HasIndex(e => e.DisplayOrder);
            });

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

            modelBuilder.Entity<Publication>()
                .Property(e => e.IsPublished)
                .HasDefaultValue(true);

            modelBuilder.Entity<PublicationTag>()
                .Property(e => e.IsActive)
                .HasDefaultValue(true);

            // Prevention default values
            modelBuilder.Entity<PreventionCategory>()
                .Property(e => e.IsActive)
                .HasDefaultValue(true);

            modelBuilder.Entity<PreventionItem>()
                .Property(e => e.IsActive)
                .HasDefaultValue(true);

            modelBuilder.Entity<PreventionItem>()
                .Property(e => e.IsWarning)
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
                else if (entry.Entity is RefreshToken refreshToken)
                {
                    refreshToken.CreatedAt = now;
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
