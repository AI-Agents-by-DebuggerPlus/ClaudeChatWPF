using ClaudeChatWPF.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Data;

namespace ClaudeChatWPF.ViewModels
{
    public partial class MessageViewModel : ObservableObject
    {
        [ObservableProperty]
        private string content = string.Empty;

        [ObservableProperty]
        private string role = string.Empty;

        [ObservableProperty]
        private DateTime timestamp = DateTime.Now;

        public bool IsUser => Role == "user";
        public bool IsAssistant => Role == "assistant";

        public MessageViewModel()
        {
        }

        public MessageViewModel(Message message)
        {
            Content = message.Content;
            Role = message.Role;
            Timestamp = message.Timestamp;
        }

        public static MessageViewModel FromMessage(Message message)
        {
            return new MessageViewModel(message);
        }

        public Message ToMessage(string threadId)
        {
            return new Message
            {
                ThreadId = threadId,
                Role = Role,
                Content = Content,
                Timestamp = Timestamp
            };
        }
    }
}