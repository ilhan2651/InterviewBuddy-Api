using Buddy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Buddy.Persistence.Configurations
{
    public class QuizConfiguration : IEntityTypeConfiguration<Quiz>
    {
        public void Configure(EntityTypeBuilder<Quiz> builder)
        {
            builder.HasKey(q => q.Id);

            builder.Property(q => q.Topic)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(q => q.FeedbackSummary)
                .HasMaxLength(2000);

            builder.HasOne(q => q.Conversation)
                .WithMany(c => c.Quizzes)
                .HasForeignKey(q => q.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
