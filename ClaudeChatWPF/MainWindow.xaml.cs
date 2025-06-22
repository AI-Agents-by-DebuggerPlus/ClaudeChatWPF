// MainWindow.xaml.cs - обновленная версия
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClaudeChatWPF.Data;
using ClaudeChatWPF.Models;
using ClaudeChatWPF.Models.ApiModels;
using ClaudeChatWPF.Services;
using ClaudeChatWPF.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClaudeChatWPF
{
    public partial class MainWindow : Window
    {
        private readonly IClaudeApiService _claudeApiService;
        private ObservableCollection<ChatThread> _chatThreads;
        private ObservableCollection<MessageViewModel> _messages;
        private ChatThread? _currentChatThread;
        private bool _isLoading = false;

        public MainWindow()
        {
            InitializeComponent();

            // Инициализация DI контейнера
            var services = ConfigureServices();
            var serviceProvider = services.BuildServiceProvider();

            _claudeApiService = serviceProvider.GetRequiredService<IClaudeApiService>();

            _chatThreads = new ObservableCollection<ChatThread>();
            _messages = new ObservableCollection<MessageViewModel>();

            ChatListBox.ItemsSource = _chatThreads;
            MessagesPanel.ItemsSource = _messages;

            // Инициализация базы данных
            InitializeDatabase();

            // Загрузка чатов
            LoadChatThreads();

            // Привязка событий
            MessageInput.TextChanged += MessageInput_TextChanged;

            // Проверка конфигурации API
            CheckApiConfiguration();

            this.Title = "Claude Chat - Ready";
        }

        private IServiceCollection ConfigureServices()
        {
            var services = new ServiceCollection();
            services.AddClaudeServices();
            return services;
        }

        private void CheckApiConfiguration()
        {
            if (!_claudeApiService.IsConfigured)
            {
                MessageBox.Show(
                    "Claude API не настроен.\n\n" +
                    "Пожалуйста, добавьте ваш API ключ в файл appsettings.json:\n" +
                    "{\n" +
                    "  \"Claude\": {\n" +
                    "    \"ApiKey\": \"your-api-key-here\"\n" +
                    "  }\n" +
                    "}",
                    "Конфигурация API",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                this.Title = "Claude Chat - API Not Configured";
            }
        }

        private void InitializeDatabase()
        {
            try
            {
                using var context = new ChatDbContext();
                context.Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации базы данных: {ex.Message}",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadChatThreads()
        {
            try
            {
                using var context = new ChatDbContext();
                var threads = context.ChatThreads
                    .OrderByDescending(ct => ct.LastUpdated)
                    .ToList();

                _chatThreads.Clear();
                foreach (var thread in threads)
                {
                    _chatThreads.Add(thread);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки чатов: {ex.Message}",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadMessages(ChatThread chatThread)
        {
            try
            {
                using var context = new ChatDbContext();
                var messages = context.Messages
                    .Where(m => m.ThreadId == chatThread.Id)
                    .OrderBy(m => m.Timestamp)
                    .ToList();

                _messages.Clear();
                foreach (var message in messages)
                {
                    _messages.Add(MessageViewModel.FromMessage(message));
                }

                ScrollToBottom();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки сообщений: {ex.Message}",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ScrollToBottom()
        {
            MessagesScrollViewer.ScrollToBottom();
        }

        private void NewChat_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newThread = new ChatThread
                {
                    Title = "New Chat",
                    Created = DateTime.Now,
                    LastUpdated = DateTime.Now
                };

                using var context = new ChatDbContext();
                context.ChatThreads.Add(newThread);
                context.SaveChanges();

                _chatThreads.Insert(0, newThread);
                ChatListBox.SelectedItem = newThread;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания нового чата: {ex.Message}",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChatListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChatListBox.SelectedItem is ChatThread selectedThread)
            {
                _currentChatThread = selectedThread;
                ChatTitleText.Text = selectedThread.Title;
                ClearChatButton.Visibility = Visibility.Visible;

                LoadMessages(selectedThread);
                MessageInput.Focus();
            }
            else
            {
                _currentChatThread = null;
                ChatTitleText.Text = "Select a chat or start a new one";
                ClearChatButton.Visibility = Visibility.Collapsed;
                _messages.Clear();
            }
        }

        private void ClearChat_Click(object sender, RoutedEventArgs e)
        {
            if (_currentChatThread == null) return;

            var result = MessageBox.Show(
                "Вы уверены, что хотите очистить все сообщения в этом чате?",
                "Очистить чат",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using var context = new ChatDbContext();
                    var messages = context.Messages
                        .Where(m => m.ThreadId == _currentChatThread.Id);

                    context.Messages.RemoveRange(messages);
                    context.SaveChanges();

                    _messages.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка очистки чата: {ex.Message}",
                                   "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MessageInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            SendButton.IsEnabled = !string.IsNullOrWhiteSpace(MessageInput.Text) &&
                                  !_isLoading &&
                                  _claudeApiService.IsConfigured;
        }

        private void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    // Shift+Enter = новая строка (по умолчанию)
                    return;
                }
                else
                {
                    // Enter = отправка сообщения
                    e.Handled = true;
                    SendMessage_Click(sender, e);
                }
            }
        }

        private async void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentChatThread == null ||
                string.IsNullOrWhiteSpace(MessageInput.Text) ||
                !_claudeApiService.IsConfigured)
                return;

            var messageText = MessageInput.Text.Trim();
            MessageInput.Text = string.Empty;

            try
            {
                SetLoadingState(true);

                // Добавляем сообщение пользователя
                var userMessage = new MessageViewModel
                {
                    Role = "user",
                    Content = messageText,
                    Timestamp = DateTime.Now
                };

                _messages.Add(userMessage);
                ScrollToBottom();

                // Сохраняем в базу данных
                using (var context = new ChatDbContext())
                {
                    var dbMessage = userMessage.ToMessage(_currentChatThread.Id);
                    context.Messages.Add(dbMessage);

                    // Обновляем время последнего обновления чата
                    var thread = context.ChatThreads.Find(_currentChatThread.Id);
                    if (thread != null)
                    {
                        thread.LastUpdated = DateTime.Now;

                        // Обновляем заголовок чата, если это первое сообщение
                        if (thread.Title == "New Chat")
                        {
                            thread.Title = messageText.Length > 50
                                ? messageText.Substring(0, 50) + "..."
                                : messageText;

                            ChatTitleText.Text = thread.Title;

                            // Обновляем в списке чатов
                            var chatInList = _chatThreads.FirstOrDefault(ct => ct.Id == thread.Id);
                            if (chatInList != null)
                            {
                                chatInList.Title = thread.Title;
                            }
                        }
                    }

                    context.SaveChanges();
                }

                // Получаем ответ от Claude
                await GetClaudeResponse();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отправки сообщения: {ex.Message}",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        private async Task GetClaudeResponse()
        {
            try
            {
                // Собираем историю сообщений для контекста
                var claudeMessages = new List<ClaudeMessage>();

                foreach (var msg in _messages.TakeLast(10)) // Берем последние 10 сообщений для контекста
                {
                    claudeMessages.Add(new ClaudeMessage
                    {
                        Role = msg.Role,
                        Content = msg.Content
                    });
                }

                // Отправляем запрос к Claude API
                var response = await _claudeApiService.SendMessageAsync(claudeMessages);

                // Добавляем ответ ассистента
                var assistantMessage = new MessageViewModel
                {
                    Role = "assistant",
                    Content = response,
                    Timestamp = DateTime.Now
                };

                _messages.Add(assistantMessage);
                ScrollToBottom();

                // Сохраняем ответ в базу данных
                using var context = new ChatDbContext();
                var dbMessage = assistantMessage.ToMessage(_currentChatThread!.Id);
                context.Messages.Add(dbMessage);

                // Обновляем время последнего обновления чата
                var thread = context.ChatThreads.Find(_currentChatThread.Id);
                if (thread != null)
                {
                    thread.LastUpdated = DateTime.Now;
                }

                context.SaveChanges();
            }
            catch (Exception ex)
            {
                var errorMessage = new MessageViewModel
                {
                    Role = "assistant",
                    Content = $"Извините, произошла ошибка: {ex.Message}",
                    Timestamp = DateTime.Now
                };

                _messages.Add(errorMessage);
                ScrollToBottom();
            }
        }

        private void SetLoadingState(bool isLoading)
        {
            _isLoading = isLoading;
            LoadingOverlay.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
            SendButton.IsEnabled = !isLoading &&
                                  !string.IsNullOrWhiteSpace(MessageInput.Text) &&
                                  _claudeApiService.IsConfigured;
            MessageInput.IsEnabled = !isLoading;
        }
    }
}