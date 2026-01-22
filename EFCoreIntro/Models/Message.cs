namespace EFCoreIntro.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime SentAtUtc { get; set; }

        // foreign key
        public int UserId { get; set; }
        public int ChannelId { get; set; }

        // navigation-property
        public User User { get; set; }
        public Channel Channel { get; set; }
    }
}
