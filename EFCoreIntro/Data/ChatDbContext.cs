using EFCoreIntro.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFCoreIntro.Data
{
    public class ChatDbContext : DbContext
    {
        public DbSet<User> Users => Set<User>(); // DbSet håller reda på ChangeTracking om entitetens tillstånd
        public DbSet<Channel> Channels => Set<Channel>();
        public DbSet<Message> Messages => Set<Message>();
        public DbSet<Membership> Memberships => Set<Membership>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            var projectDir = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.FullName;
            var dbPath = Path.Combine(projectDir, "chat.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}"); optionsBuilder.UseSqlite("Data Source=chat.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // memberhsip: kompositnyckel
            modelBuilder.Entity<Membership>()
                        .HasKey(membership => new { membership.UserId, membership.ChannelId });

            // membership: relationer
            modelBuilder.Entity<Membership>()
                        .HasOne(membership => membership.User)          // har en user
                        .WithMany(user => user.Memberships)             //varje user har en eller flera memberships
                        .HasForeignKey(membership => membership.UserId) // varje membership har en FK UserId
                        .OnDelete(DeleteBehavior.Cascade);              // Om en User raderas ska membership också raderas

            modelBuilder.Entity<Membership>()
                        .HasOne(membership => membership.Channel)
                        .WithMany(channel => channel.Memberships)
                        .HasForeignKey(membership => membership.ChannelId)
                        .OnDelete(DeleteBehavior.Cascade);

            // message: relationer
            modelBuilder.Entity<Message>()
                        .HasOne(message => message.User)
                        .WithMany(user => user.Messages)
                        .HasForeignKey(message => message.UserId)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Message>()
                        .HasOne(message => message.Channel)
                        .WithMany(channel => channel.Messages)
                        .HasForeignKey(message => message.ChannelId)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                        .Property(user => user.UserName)
                        .HasMaxLength(50)
                        .IsRequired();

            modelBuilder.Entity<Channel>()
                        .Property(channel => channel.ChannelName)
                        .HasMaxLength(50)
                        .IsRequired();

            modelBuilder.Entity<Message>()
                        .Property(message => message.Text)
                        .HasMaxLength(500)
                        .IsRequired();
        }
    }
}
