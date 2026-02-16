using Buddy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Buddy.Persistence.Configurations
{
    public class QuizAnswerConfiguration : IEntityTypeConfiguration<QuizAnswer>
    {
        public void Configure(EntityTypeBuilder<QuizAnswer> builder)
        {
            builder.HasKey(qa => qa.Id);

            builder.Property(qa => qa.UserAnswer)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(qa => qa.Feedback)
                .HasMaxLength(2000);

            builder.HasOne(qa => qa.QuizQuestion)
                .WithOne(qq => qq.Answer)
                .HasForeignKey<QuizAnswer>(qa => qa.QuizQuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
