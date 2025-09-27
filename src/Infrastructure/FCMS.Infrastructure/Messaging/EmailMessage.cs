namespace FCMS.Infrastructure.Messaging;

public class EmailMessage
{
    public string To { get; set; } = default!;
    public string Subject { get; set; } = default!;
    public string Body { get; set; } = default!;
    public byte[]? Attachment { get; set; }
}
