using System;
using System.ComponentModel.DataAnnotations;

namespace ClaudeChatWPF.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ThreadId { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty; // "user" или "assistant"

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.Now;

        // Навигационное свойство для EF
        public ChatThread? ChatThread { get; set; }
    }
}