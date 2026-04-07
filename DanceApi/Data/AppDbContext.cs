using DanceApi.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DanceApi.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<TypeOfMeeting> TypeOfMeetings { get; set; }
        public DbSet<MeetingParticipant> MeetingParticipants { get; set; }
        public DbSet<NewsArticle> NewsArticles { get; set; }
        public DbSet<NewsComment> NewsComments { get; set; }
        public DbSet<SubPage> SubPages { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<MembershipPlan> MembershipPlans { get; set; }
        public DbSet<UserMembership> UserMemberships { get; set; }
        public DbSet<FAQ> FAQs { get; set; }
        public DbSet<InstructorProfile> InstructorProfiles { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }

        public DbSet<GuestUser> GuestUsers { get; set; }
        public DbSet<GuestUserProfile> GuestUserProfiles { get; set; }
        public DbSet<GuestInstructorProfile> GuestInstructorProfiles { get; set; }
        public DbSet<MeetingGuestParticipant> MeetingGuestParticipants { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // SOFT DELETE
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var method = typeof(AppDbContext)
                        .GetMethod(nameof(SetGlobalQueryFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                        .MakeGenericMethod(entityType.ClrType);

                    method.Invoke(this, new object[] { modelBuilder });
                }
            }

            // BASE ENTITY AUDIT RELATIONS
            ConfigureBaseEntityRelationships<NewsComment>(modelBuilder);
            ConfigureBaseEntityRelationships<Meeting>(modelBuilder);
            ConfigureBaseEntityRelationships<Location>(modelBuilder);
            ConfigureBaseEntityRelationships<TypeOfMeeting>(modelBuilder);
            ConfigureBaseEntityRelationships<Review>(modelBuilder);
            ConfigureBaseEntityRelationships<MembershipPlan>(modelBuilder);
            ConfigureBaseEntityRelationships<UserMembership>(modelBuilder);

            modelBuilder.Entity<MeetingParticipant>()
                .HasOne(mp => mp.Meeting)
                .WithMany(m => m.MeetingParticipants)
                .HasForeignKey(mp => mp.MeetingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MeetingParticipant>()
                .HasOne(mp => mp.User)
                .WithMany(u => u.MeetingParticipants)
                .HasForeignKey(mp => mp.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Meeting>()
                .HasOne(m => m.Instructor)
                .WithMany()
                .HasForeignKey(m => m.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Meeting>()
                .HasOne(m => m.GuestInstructor)
                .WithMany()
                .HasForeignKey(m => m.GuestInstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MeetingGuestParticipant>()
                .HasOne(mgp => mgp.Meeting)
                .WithMany(m => m.MeetingGuestParticipants)
                .HasForeignKey(mgp => mgp.MeetingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MeetingGuestParticipant>()
                .HasOne(mgp => mgp.GuestUser)
                .WithMany(gu => gu.MeetingGuestParticipants)
                .HasForeignKey(mgp => mgp.GuestUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<NewsComment>()
                .HasOne(nc => nc.NewsArticle)
                .WithMany(na => na.Comments)
                .HasForeignKey(nc => nc.NewsArticleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.TypeOfMeeting)
                .WithMany(t => t.Reviews)
                .HasForeignKey(r => r.TypeOfMeetingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserMembership>()
                .HasOne(um => um.User)
                .WithMany(u => u.UserMemberships)
                .HasForeignKey(um => um.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserMembership>()
                .HasOne(um => um.MembershipPlan)
                .WithMany()
                .HasForeignKey(um => um.MembershipPlanId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MembershipPlan>()
                .HasMany(mp => mp.AccessibleMeetingTypes)
                .WithMany(mt => mt.MembershipPlans);

            modelBuilder.Entity<InstructorProfile>()
                .HasOne(ip => ip.User)
                .WithOne(u => u.InstructorProfile)
                .HasForeignKey<InstructorProfile>(ip => ip.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserProfile>()
                .HasOne(up => up.User)
                .WithOne(u => u.UserProfile)
                .HasForeignKey<UserProfile>(up => up.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GuestUserProfile>()
                .HasOne(gup => gup.GuestUser)
                .WithOne(gu => gu.GuestUserProfile)
                .HasForeignKey<GuestUserProfile>(gup => gup.GuestUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GuestInstructorProfile>()
                .HasOne(gip => gip.GuestUser)
                .WithOne(gu => gu.GuestInstructorProfile)
                .HasForeignKey<GuestInstructorProfile>(gip => gip.GuestUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private void ConfigureBaseEntityRelationships<TEntity>(ModelBuilder modelBuilder)
            where TEntity : BaseEntity
        {
            modelBuilder.Entity<TEntity>()
                .HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TEntity>()
                .HasOne(e => e.UpdatedBy)
                .WithMany()
                .HasForeignKey(e => e.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TEntity>()
                .HasOne(e => e.DeletedBy)
                .WithMany()
                .HasForeignKey(e => e.DeletedById)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private void SetGlobalQueryFilter<TEntity>(ModelBuilder modelBuilder)
            where TEntity : BaseEntity
        {
            modelBuilder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
        }
    }
}