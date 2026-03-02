using EFCoreIntro.Data;
using EFCoreIntro.Models;

using Microsoft.EntityFrameworkCore;

using var db = new ChatDbContext();

// Tillämpa migrationer istället för EnsureCreated
db.Database.Migrate();

await db.SeedAsync();

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
