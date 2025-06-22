using Microsoft.EntityFrameworkCore;
using ClaudeChatWPF.Models;

namespace ClaudeChatWPF.Data
{
    public class ChatDbContext : DbContext
    {
        public DbSet<ChatThread> ChatThreads { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=Data/ChatDatabase.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Настройка связи между ChatThread и Message
            modelBuilder.Entity<Message>()
                .HasOne(m => m.ChatThread)
                .WithMany(ct => ct.Messages)
                .HasForeignKey(m => m.ThreadId)
                .HasPrincipalKey(ct => ct.Id);

            // Настройка индексов для быстрого поиска
            modelBuilder.Entity<Message>()
                .HasIndex(m => m.ThreadId);

            modelBuilder.Entity<Message>()
                .HasIndex(m => m.Timestamp);

            base.OnModelCreating(modelBuilder);
        }
    }
}