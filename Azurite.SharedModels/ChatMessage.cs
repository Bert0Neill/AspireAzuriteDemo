namespace Azurite.SharedModels
{
    public class ChatMessage
    {
        public string User { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
