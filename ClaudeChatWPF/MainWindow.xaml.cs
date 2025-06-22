using System.Windows;
using ClaudeChatWPF.Data;

namespace ClaudeChatWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Инициализация базы данных
            InitializeDatabase();

            this.Title = "Claude Chat WPF - Ready";
        }

        private void InitializeDatabase()
        {
            try
            {
                using var context = new ChatDbContext();
                // Убеждаемся, что база данных создана
                context.Database.EnsureCreated();

                // Проверяем количество чатов для отладки
                var chatCount = context.ChatThreads.Count();
                this.Title = $"Claude Chat WPF - Chats: {chatCount}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при инициализации базы данных: {ex.Message}",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}