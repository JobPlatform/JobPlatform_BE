using JobPlatform.Domain.Entities.Applications;

namespace JobPlatform.Domain.Entities.Chats;
public class Conversation : BaseEntity
{
    public Guid JobApplicationId { get; set; }
    public JobApplication JobApplication { get; set; } = default!;

    public ICollection<ConversationMember> Members { get; set; } = new List<ConversationMember>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}

public class ConversationMember
{
    public Guid ConversationId { get; set; }
    public Conversation Conversation { get; set; } = default!;

    public Guid UserId { get; set; }
}

public class Message : BaseEntity
{
    public Guid ConversationId { get; set; }
    public Conversation Conversation { get; set; } = default!;

    public Guid SenderUserId { get; set; }
    public required string Content { get; set; }
    public DateTimeOffset SentAt { get; set; } = DateTimeOffset.UtcNow;
}
