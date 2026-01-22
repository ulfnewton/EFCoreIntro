namespace EFCoreIntro.Models
{
    public class Channel
    {
        public int Id { get; set; }
        public string ChannelName { get; set; }

        public ICollection<Message> Messages { get; set; }
        public ICollection<Membership> Memberships { get; set; }
    }
}
