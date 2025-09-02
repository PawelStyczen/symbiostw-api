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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // SOFT DELETE
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var method = typeof(AppDbContext).GetMethod(nameof(SetGlobalQueryFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        .MakeGenericMethod(entityType.ClrType);

                    method.Invoke(this, new object[] { modelBuilder });
                }
            }

            // BASE ENTITY
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
         
        }
        
        
        
        // BASE ENTITY
        private void ConfigureBaseEntityRelationships<TEntity>(ModelBuilder modelBuilder) where TEntity : BaseEntity
        {
            modelBuilder.Entity<TEntity>()
                .HasOne<User>(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TEntity>()
                .HasOne<User>(e => e.UpdatedBy)
                .WithMany()
                .HasForeignKey(e => e.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TEntity>()
                .HasOne<User>(e => e.DeletedBy)
                .WithMany()
                .HasForeignKey(e => e.DeletedById)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private void SetGlobalQueryFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : BaseEntity
        {
            modelBuilder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
        }
        
        
    }
    
}