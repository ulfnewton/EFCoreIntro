// ------------------------------------------------------------
// Bogus-boost för LINQ-minigolf (TEMPORÄRT, inga schemaändringar)
// ------------------------------------------------------------
// Målsättning på dataset (miniminivåer):
//  - Users: >= 8 (minst 1 superaktiv (>10 msg), minst 1 tyst (0 msg))
//  - Channels: >= 4 (1 tom kanal, 1 mycket aktiv, 2 medelaktiva)
//  - Memberships: >= 12, med overlap (>=3 users i minst 2 kanaler)
//  - Messages: >= 40, tidsstämplar över >= 3 olika dagar
//  - Några meddelanden delar samma timestamp (stabil sort-test)
//
// Kräver NuGet: Bogus
//   dotnet add EFCoreIntro/EFCoreIntro.csproj package Bogus
// ------------------------------------------------------------
using Bogus;

using Microsoft.EntityFrameworkCore;

using EFCoreIntro.Models;
using EFCoreIntro.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFCoreIntro.Data
{
    public static class DbSeeder
    {
        #region BOOST_SEED_WITH_BOGUS
        public static async Task SeedAsync(this ChatDbContext ctx)
        {
            // Kör bara om vi inte redan nått miniminivåerna
            var needsBoost =
                ctx.Users.Count() < 8 ||
                ctx.Channels.Count() < 4 ||
                ctx.Memberships.Count() < 12 ||
                ctx.Messages.Count() < 40;

            if (!needsBoost) return;

            // Töm databasen för att undvika dubbletter
            ctx.Memberships.RemoveRange(ctx.Memberships);
            ctx.Messages.RemoveRange(ctx.Messages);
            ctx.Users.RemoveRange(ctx.Users);
            ctx.Channels.RemoveRange(ctx.Channels);
            await ctx.SaveChangesAsync();

            // Deterministisk seed → samma data hos alla
            Randomizer.Seed = new Random(20260303);
            var rng = new Random(20260303);

            // ---------- 1) Users ----------
            int userTarget = 8;
            int existingUsersCount = ctx.Users.Count();
            int addUsers = (userTarget > existingUsersCount) ? (userTarget - existingUsersCount) : 0;

            if (addUsers > 0)
            {
                var userFaker = new Faker<User>("sv")
                    .RuleFor(u => u.UserName, f => f.Internet.UserName());

                var toAdd = new List<User>();
                for (int i = 0; i < addUsers; i++)
                    toAdd.Add(userFaker.Generate());

                ctx.Users.AddRange(toAdd);
                await ctx.SaveChangesAsync();
            }

            // Läs in användare till lista (arbetar sedan med List<T>)
            var users = await ctx.Users.AsNoTracking().ToListAsync();
            if (users.Count == 0) return;

            // Välj deterministiskt superaktiv (första) och tyst (andra eller sista)
            var superActiveUser = users[0];
            var quietUser = (users.Count > 1) ? users[1] : users[0];

            // ---------- 2) Channels ----------
            string[] channelNamesPool = { "allmänt", "events", "verktyg", "random", "jobb", "fika" };
            int channelTarget = 4;

            var existingChannels = await ctx.Channels.ToListAsync();
            var existingNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < existingChannels.Count; i++)
                existingNames.Add(existingChannels[i].ChannelName);

            int j = 0;
            while (ctx.Channels.Count() < channelTarget && j < channelNamesPool.Length)
            {
                string name = channelNamesPool[j++];
                if (!existingNames.Contains(name))
                {
                    ctx.Channels.Add(new Channel
                    {
                        ChannelName = name,
                        Messages = new List<Message>(),
                        Memberships = new List<Membership>()
                    });
                    existingNames.Add(name);
                }
            }
            await ctx.SaveChangesAsync();

            // Hämta kanaler till lista och sortera deterministiskt efter Id utan LINQ
            var channels = await ctx.Channels.ToListAsync();
            for (int a = 0; a < channels.Count - 1; a++)
            {
                for (int b = 0; b < channels.Count - a - 1; b++)
                {
                    if (channels[b].Id > channels[b + 1].Id)
                    {
                        var tmp = channels[b];
                        channels[b] = channels[b + 1];
                        channels[b + 1] = tmp;
                    }
                }
            }

            var emptyChannel = channels[0];                         // lämnas utan meddelanden
            var activeChannel = (channels.Count > 1) ? channels[1] : channels[0];
            var mediumChannels = new List<Channel>();
            for (int idx = 0; idx < channels.Count && mediumChannels.Count < 2; idx++)
            {
                var ch = channels[idx];
                if (ch.Id != emptyChannel.Id && ch.Id != activeChannel.Id)
                    mediumChannels.Add(ch);
            }

            // ---------- 3) Memberships ----------
            // Snabb koll av (UserId, ChannelId) utan LINQ
            var membershipPairs = new HashSet<string>();
            var existingMemberships = await ctx.Memberships.AsNoTracking().ToListAsync();
            for (int m = 0; m < existingMemberships.Count; m++)
                membershipPairs.Add($"{existingMemberships[m].UserId}::{existingMemberships[m].ChannelId}");

            void EnsureMembership(int userId, int channelId, DateTime joinedAtUtc)
            {
                string key = $"{userId}::{channelId}";
                if (!membershipPairs.Contains(key))
                {
                    ctx.Memberships.Add(new Membership
                    {
                        UserId = userId,
                        ChannelId = channelId,
                        JoinedAtUtc = joinedAtUtc
                    });
                    membershipPairs.Add(key);
                }
            }

            // Bygg lista av UserIds
            var userIds = new List<int>();
            for (int i = 0; i < users.Count; i++)
                userIds.Add(users[i].Id);

            // Lägg alla (utom "quiet") i activeChannel
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].Id != quietUser.Id)
                    EnsureMembership(users[i].Id, activeChannel.Id, DateTime.UtcNow.AddDays(-rng.Next(1, 14)));
            }

            // Mediumkanaler: slumpa 4 unika användare per kanal (utan LINQ)
            for (int mc = 0; mc < mediumChannels.Count; mc++)
            {
                var chosen = new HashSet<int>();
                while (chosen.Count < 4 && chosen.Count < userIds.Count)
                {
                    int pick = userIds[rng.Next(userIds.Count)];
                    chosen.Add(pick);
                }
                foreach (var uid in chosen)
                    EnsureMembership(uid, mediumChannels[mc].Id, DateTime.UtcNow.AddDays(-rng.Next(1, 14)));
            }

            // Tom kanal: medlemmar men inga meddelanden
            var chosenEmpty = new HashSet<int>();
            while (chosenEmpty.Count < Math.Min(3, userIds.Count))
            {
                int pick = userIds[rng.Next(userIds.Count)];
                chosenEmpty.Add(pick);
            }
            foreach (var uid in chosenEmpty)
                EnsureMembership(uid, emptyChannel.Id, DateTime.UtcNow.AddDays(-rng.Next(1, 14)));

            // Fyll på memberships tills vi når >= 12
            while (membershipPairs.Count < 12 && channels.Count > 0 && users.Count > 0)
            {
                int uid = userIds[rng.Next(userIds.Count)];
                var ch = channels[rng.Next(channels.Count)];
                EnsureMembership(uid, ch.Id, DateTime.UtcNow.AddDays(-rng.Next(1, 14)));
            }
            await ctx.SaveChangesAsync();

            // ---------- 4) Messages ----------
            var msgFaker = new Faker<Message>("sv")
                .RuleFor(m => m.Text, f => f.Lorem.Sentence(rng.Next(4, 12)))
                .RuleFor(m => m.SentAtUtc, f => f.Date.Between(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow).ToUniversalTime());

            void AddMessageSafe(int userId, int channelId, DateTime whenUtc, string text)
            {
                string key = $"{userId}::{channelId}";
                if (!membershipPairs.Contains(key))
                {
                    ctx.Memberships.Add(new Membership
                    {
                        UserId = userId,
                        ChannelId = channelId,
                        JoinedAtUtc = whenUtc.AddDays(-1)
                    });
                    membershipPairs.Add(key);
                }
                ctx.Messages.Add(new Message
                {
                    UserId = userId,
                    ChannelId = channelId,
                    SentAtUtc = whenUtc,
                    Text = text
                });
            }

            // 4a) Superaktiv användare (>10 inlägg): aktiv + medelkanaler
            int superPosts = rng.Next(12, 18);
            for (int i = 0; i < superPosts; i++)
            {
                var targetCh = (i % 3 == 0 || mediumChannels.Count == 0)
                    ? activeChannel
                    : mediumChannels[rng.Next(mediumChannels.Count)];
                var sample = msgFaker.Generate();
                AddMessageSafe(superActiveUser.Id, targetCh.Id, sample.SentAtUtc, sample.Text);
            }

            // 4b) Vanliga inlägg från övriga (ej quiet/super)
            var nonEmptyChannels = new List<Channel>();
            for (int i = 0; i < channels.Count; i++)
                if (channels[i].Id != emptyChannel.Id) nonEmptyChannels.Add(channels[i]);

            for (int i = 0; i < users.Count; i++)
            {
                var u = users[i];
                if (u.Id == quietUser.Id || u.Id == superActiveUser.Id) continue;

                int posts = rng.Next(3, 7);
                for (int k = 0; k < posts; k++)
                {
                    var ch = nonEmptyChannels[rng.Next(nonEmptyChannels.Count)];
                    var sample = msgFaker.Generate();
                    AddMessageSafe(u.Id, ch.Id, sample.SentAtUtc, sample.Text);
                }
            }

            // 4c) Se till att vi har >= 40 meddelanden
            int currentMsgCount = ctx.Messages.Count();
            int toGo = 40 - currentMsgCount;
            while (toGo-- > 0)
            {
                var anyUser = users[rng.Next(users.Count)];
                if (anyUser.Id == quietUser.Id) { toGo++; continue; } // håll "tyst" tyst
                var ch = nonEmptyChannels[rng.Next(nonEmptyChannels.Count)];
                var sample = msgFaker.Generate();
                AddMessageSafe(anyUser.Id, ch.Id, sample.SentAtUtc, sample.Text);
            }

            // 4d) Identiska timestamps (stabil sort-test)
            if (ctx.Messages.Count() >= 5)
            {
                DateTime stamp = DateTime.UtcNow.AddHours(-12);
                int created = 0, guard = 0;
                while (created < 3 && guard < 100)
                {
                    guard++;
                    var u = users[rng.Next(users.Count)];
                    if (u.Id == quietUser.Id) continue;
                    AddMessageSafe(u.Id, activeChannel.Id, stamp, "Synkad timestamp för sort‑test");
                    created++;
                }
            }

            await ctx.SaveChangesAsync();

            // ---------- 5) Sammanfattning (utan LINQ) ----------
            int usersCount = ctx.Users.Count();
            int channelsCount = ctx.Channels.Count();
            int membershipsCount = ctx.Memberships.Count();
            int messagesCount = ctx.Messages.Count();

            Console.WriteLine("=== Bogus-boostad seed (TEMP) ===");
            Console.WriteLine($"Users:       {usersCount}");
            Console.WriteLine($"Channels:    {channelsCount} (tom: '{emptyChannel.ChannelName}')");
            Console.WriteLine($"Memberships: {membershipsCount}");
            Console.WriteLine($"Messages:    {messagesCount}");

            // Enkel top-3 för kanaler utan LINQ (loopar + uppslag)
            var allMessages = await ctx.Messages.AsNoTracking().ToListAsync();
            var stats = new Dictionary<int, (int Count, DateTime? LastAt)>();
            for (int i = 0; i < channels.Count; i++)
                stats[channels[i].Id] = (0, null);

            for (int i = 0; i < allMessages.Count; i++)
            {
                var m = allMessages[i];
                if (!stats.ContainsKey(m.ChannelId)) stats[m.ChannelId] = (0, null);
                var tuple = stats[m.ChannelId];
                int newCount = tuple.Count + 1;
                DateTime? newLast = (!tuple.LastAt.HasValue || m.SentAtUtc > tuple.LastAt.Value)
                    ? m.SentAtUtc
                    : tuple.LastAt;
                stats[m.ChannelId] = (newCount, newLast);
            }

            var top3 = new List<(string Name, int Count, DateTime? LastAt)>();
            for (int picks = 0; picks < 3 && picks < channels.Count; picks++)
            {
                int bestIdx = -1;
                int bestCount = -1;
                DateTime? bestLast = null;

                for (int i = 0; i < channels.Count; i++)
                {
                    var ch = channels[i];

                    bool alreadyTaken = false;
                    for (int t = 0; t < top3.Count; t++)
                        if (top3[t].Name == ch.ChannelName) { alreadyTaken = true; break; }
                    if (alreadyTaken) continue;

                    var s = stats[ch.Id];
                    bool better =
                        s.Count > bestCount ||
                        (s.Count == bestCount && (bestLast == null || (s.LastAt ?? DateTime.MinValue) > bestLast));

                    if (better)
                    {
                        bestIdx = i;
                        bestCount = s.Count;
                        bestLast = s.LastAt;
                    }
                }

                if (bestIdx >= 0)
                {
                    var ch = channels[bestIdx];
                    var s = stats[ch.Id];
                    top3.Add((ch.ChannelName, s.Count, s.LastAt));
                }
            }

            await ctx.SaveChangesAsync();

            Console.WriteLine("-- Toppkanaler (antal, senaste) --");

            for (int i = 0; i < top3.Count; i++)
                Console.WriteLine($"{top3[i].Name,-12}  {top3[i].Count,3}  {top3[i].LastAt:yyyy-MM-dd HH:mm:ss}");

            Console.WriteLine($"Superaktiv användare:  {superActiveUser.UserName} (>10 inlägg)");
            Console.WriteLine($"Tyst användare:        {quietUser.UserName} (0 inlägg)");
            Console.WriteLine("=== /Bogus-boost klar ===");
        }
        #endregion BOOST_SEED_WITH_BOGUS
    }
}
