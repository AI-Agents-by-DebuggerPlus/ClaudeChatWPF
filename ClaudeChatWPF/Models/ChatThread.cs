using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClaudeChatWPF.Models
{
    public class ChatThread
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string Title { get; set; } = "New Chat";

        public DateTime Created { get; set; } = DateTime.Now;

        public DateTime LastUpdated { get; set; } = DateTime.Now;

        // Навигационное свойство для EF
        public List<Message> Messages { get; set; } = new List<Message>();
    }
}