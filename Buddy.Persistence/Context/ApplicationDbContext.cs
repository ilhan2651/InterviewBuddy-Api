using Buddy.Domain.Entities;
using Buddy.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Buddy.Persistence.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserApiKey> UserApiKeys { get; set; }
        public DbSet<Conversation> Conversations { get; set; } = null!;
        public DbSet<Message> Messages { get; set; } = null!;
        public DbSet<Quiz> Quizzes { get; set; } = null!;
        public DbSet<QuizQuestion> QuizQuestions { get; set; } = null!;
        public DbSet<QuizAnswer> QuizAnswers { get; set; } = null!;
        public DbSet<InterviewSession> InterviewSessions { get; set; } = null!;
        public DbSet<InterviewQuestion> InterviewQuestions { get; set; } = null!;
        public DbSet<InterviewAnswer> InterviewAnswers { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new ConversationConfiguration());
            modelBuilder.ApplyConfiguration(new MessageConfiguration());
            modelBuilder.ApplyConfiguration(new QuizConfiguration());
            modelBuilder.ApplyConfiguration(new QuizQuestionConfiguration());
            modelBuilder.ApplyConfiguration(new QuizAnswerConfiguration());

            modelBuilder.Entity<InterviewSession>()
             .HasMany(s => s.Questions)
             .WithOne(q => q.InterviewSession)
             .HasForeignKey(q => q.InterviewSessionId)
             .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InterviewQuestion>()
                .HasOne(q => q.Answer)
                .WithOne(a => a.InterviewQuestion)
                .HasForeignKey<InterviewAnswer>(a => a.InterviewQuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasOne(u => u.ApiKeys)
                .WithOne(a => a.User)
                .HasForeignKey<UserApiKey>(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InterviewQuestion>()
                .HasOne(q => q.Parent)
                .WithMany(q => q.FollowUpQuestions)
                .HasForeignKey(q => q.ParentId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading deletes on self-reference if unwanted, or use Cascade
        }
    }
}
