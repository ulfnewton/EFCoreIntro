namespace EFCoreIntro.Models
{
    public class Membership
    {
        // foreign key
        public int UserId { get; set; }
        public int ChannelId { get; set; }

        public DateTime JoinedAtUtc { get; set; }

        // navigation-property
        public User User { get; set; }
        public Channel Channel { get; set; }
    }
}
