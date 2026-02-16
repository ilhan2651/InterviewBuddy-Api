using Buddy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Buddy.Persistence.Configurations
{
    public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
    {
        public void Configure(EntityTypeBuilder<Conversation> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.SessionId).IsRequired().HasMaxLength(100);
            builder.Property(c => c.AnonymousId).HasMaxLength(100);
            builder.Property(c => c.StartedAt).IsRequired();

            // One-to-Many: User (Creator) can have many Conversations. Relationship is nullable.
            builder.HasOne(c => c.User)
                   .WithMany() // Uni-directional: User has no "CreatedConversations" collection
                   .HasForeignKey(c => c.UserId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
