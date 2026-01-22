using EFCoreIntro.Data;
using EFCoreIntro.Models;
using Microsoft.EntityFrameworkCore;

using var db = new ChatDbContext();

// Tillämpa migrationer istället för EnsureCreated
db.Database.Migrate();

// Seed endast om databasen är tom
if (!db.Users.Any())
{
    // Create user, channel, membership, message
    var user = new User { UserName = "pelle" };
    var channel = new Channel { ChannelName = "allmänt" };

    // spara user och channel i db för att få Id
    db.AddRange(user, channel);
    db.SaveChanges();

    // skapa membership med user.Id och channel.Id
    var membership = new Membership
    {
        UserId = user.Id,
        ChannelId = channel.Id,
        JoinedAtUtc = DateTime.UtcNow,
    };
    db.AddRange(membership);
    db.SaveChanges();

    // skapa message med user.Id och channel.Id
    var message = new Message
    {
        Text = "Hej allihop!",
        SentAtUtc = DateTime.UtcNow,
        UserId = user.Id,
        ChannelId = channel.Id,
    };
    db.AddRange(message);
    db.SaveChanges();
}

// hämta en lista av object med channel-namn, kanalens medlemsantal, samt senaste meddelande
var channels = db.Channels
    // Välj ut kanalnamn, medlemsantal och senaste meddelande
    .Select(channel => new
    {
        channel.ChannelName,
        MemberCount = channel.Memberships.Count,
        LastMessage = channel.Messages
                             .OrderByDescending(message => message.SentAtUtc)
                             .Select(message => new
                             {
                                 message.Text,
                                 message.SentAtUtc
                             })
                             .FirstOrDefault()
    });

// skriv ut samtliga kanalers info.
foreach (var c in channels)
{
    var lastMessage = c.LastMessage is null
        ? "(inga meddelanden)"
        : $"{c.LastMessage.Text} @ {c.LastMessage.SentAtUtc}";

    Console.WriteLine($"Kanal: {c.ChannelName} | Medlemmar: {c.MemberCount} | Senast: {lastMessage}");
}
