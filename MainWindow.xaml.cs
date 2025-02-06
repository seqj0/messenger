using System;
using System.Collections.Generic;
using System.Windows;
using WebSocketSharp;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System.Windows.Controls;
using System.IO;
using System.Windows.Input;

namespace SHOOTER_MESSANGER
{
    public partial class MainWindow : Window
    {
        private Dictionary<int, string> friends;        // Список друзей: ID -> Ник
        private Dictionary<int, List<string>> chats;    // Чаты для каждого друга по ID

        private int selectedFriendId;                   // Текущий выбранный друг

        private WebSocket _webSocket;
        public int CurrentUserId { get; private set; }  // ID текущего пользователя

        public event Action<Friend> FriendAdded;        // Событие для уведомления

        public MainWindow(int userId)
        {
            InitializeComponent();
            CurrentUserId = userId; // Получаем ID пользователя из окна входа
            InitializeWebSocket();
            friends = new Dictionary<int, string>();
            chats = new Dictionary<int, List<string>>();
            LoadFriendsFromDatabase();
        }

        private async void CheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            var update = new Update();
            await update.CheckForUpdatesAsync();
        }

        private void InitializeWebSocket()
        {
            try
            {
                //string scheme = HttpClientProvider.IsSecure ? "wss" : "ws";
                //_webSocket = new WebSocket($"{scheme}://{HttpClientProvider.GetBaseUrl()}/ws");
                _webSocket = new WebSocket("ws://192.168.1.34:5000/ws");

                _webSocket.OnOpen += (sender, e) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        AppendMessage("Соединение установлено.");
                    });
                    // Запускаем цикл пинга, чтобы поддерживать соединение (опционально)
                    StartPingLoop();
                };

                _webSocket.OnMessage += (sender, e) =>
                {
                    Dispatcher.Invoke(() => AppendMessage("Сервер: " + e.Data));
                };

                _webSocket.OnClose += (sender, e) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        AppendMessage("Соединение разорвано.");
                    });
                    // Автоматическое переподключение через 5 секунд
                    Task.Delay(5000).ContinueWith(t => ReconnectWebSocket());
                };

                _webSocket.OnError += (sender, e) =>
                {
                    Dispatcher.Invoke(() => AppendMessage("Ошибка: " + e.Message));
                    // При ошибке тоже можно попробовать переподключиться
                    Task.Delay(5000).ContinueWith(t => ReconnectWebSocket());
                };

                _webSocket.ConnectAsync();
            }
            catch (Exception ex)
            {
                AppendMessage($"Ошибка подключения: {ex.Message}");
            }
        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage();
                e.Handled = true; // Предотвращаем добавление новой строки в TextBox
            }
        }

        private void SendMessage()
        {
            if (!string.IsNullOrWhiteSpace(InputBox.Text) && selectedFriendId != -1)
            {
                string messageText = InputBox.Text.Trim();
                InputBox.Text = ""; // Очищаем поле ввода

                var messageObject = new
                {
                    From = CurrentUserId,
                    To = selectedFriendId,
                    Message = messageText
                };

                string jsonMessage = JsonConvert.SerializeObject(messageObject);

                if (_webSocket != null && _webSocket.IsAlive)
                {
                    _webSocket.Send(jsonMessage);
                    AppendMessage($"Вы: {messageText}"); // Добавляем сообщение в чат
                }
                else
                {
                    AppendMessage("Ошибка: соединение с сервером отсутствует.");
                }
            }
        }

        private void ReconnectWebSocket()
        {
            try
            {
                AppendMessage("Попытка переподключения...");
                _webSocket.ConnectAsync();
            }
            catch (Exception ex)
            {
                AppendMessage($"Ошибка переподключения: {ex.Message}");
            }
        }

        private async void StartPingLoop()
        {
            while (_webSocket != null && _webSocket.IsAlive)
            {
                try
                {
                    _webSocket.Ping();
                }
                catch (Exception ex)
                {
                    AppendMessage("Ошибка при отправке ping: " + ex.Message);
                }
                await Task.Delay(10000); // Отправляем ping каждые 10 секунд
            }
        }

        private async Task SendFileAsync(string filePath, int recipientId)
        {
            if (!File.Exists(filePath))
            {
                AppendMessage("Файл не найден.");
                return;
            }

            byte[] fileBytes = File.ReadAllBytes(filePath);
            string base64File = Convert.ToBase64String(fileBytes);
            string fileName = Path.GetFileName(filePath);

            var fileMessage = new
            {
                From = CurrentUserId,
                To = recipientId,
                Message = $"[FILE] {fileName}",
                FileData = base64File
            };

            string jsonMessage = JsonConvert.SerializeObject(fileMessage);
            _webSocket.Send(jsonMessage);
            AppendMessage($"Вы отправили файл {fileName}");
        }

        private async void LoadFriendsFromDatabase()
        {
            try
            {
                var response = await HttpClientProvider.Client.GetAsync($"{HttpClientProvider.GetBaseUrl()}/api/Friends/friends?userId={CurrentUserId}");

                if (!response.IsSuccessStatusCode)
                {
                    AppendMessage($"Ошибка загрузки друзей: {response.StatusCode}");
                    return;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var friendList = JsonConvert.DeserializeObject<List<Friend>>(jsonResponse);

                // Очищаем текущий список друзей и чатов
                friends.Clear();
                chats.Clear();

                // Добавляем новых друзей в словарь и чаты
                foreach (var friend in friendList)
                {
                    friends[friend.FriendId] = friend.FriendName;
                    chats[friend.FriendId] = new List<string>(); // Инициализируем пустой чат
                }

                // Обновляем список друзей в интерфейсе
                FriendsListBox.ItemsSource = friends.Values;
            }
            catch (Exception ex)
            {
                AppendMessage($"Ошибка загрузки друзей: {ex.Message}");
            }
        }


        private void OnFriendAdded(Friend newFriend)
        {
            if (!friends.ContainsKey(newFriend.FriendId))
            {
                friends[newFriend.FriendId] = newFriend.FriendName;
                chats[newFriend.FriendId] = new List<string>(); // Создаём пустой чат

                // Обновляем интерфейс
                FriendsListBox.Items.Add(newFriend.FriendName);
            }
            else
            {
                MessageBox.Show($"{newFriend.FriendName} уже в списке друзей!");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var searchWindow = new SearchFriendsWindow(CurrentUserId);

            // Подписываемся на событие добавления друга
            searchWindow.FriendAdded += OnFriendAdded;

            searchWindow.Show();
            this.Close();
        }

        private void FriendsListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (FriendsListBox.SelectedItem != null)
            {
                // Находим ID выбранного друга
                var friendName = FriendsListBox.SelectedItem.ToString();
                selectedFriendId = GetFriendIdByName(friendName);

                FriendNicknameTextBlock.Text = $"Чат с {friendName}";

                // Отображаем историю чата
                ChatTextBlock.Text = string.Join(Environment.NewLine, chats[selectedFriendId]);
            }
        }

        private int GetFriendIdByName(string friendName)
        {
            foreach (var friend in friends)
            {
                if (friend.Value == friendName)
                {
                    return friend.Key;
                }
            }
            return -1; // Возвращаем -1, если не нашли друга
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedFriendId == -1)
            {
                MessageBox.Show("Выберите друга для чата.");
                return;
            }

            string message = InputBox.Text.Trim();

            if (string.IsNullOrEmpty(message))
            {
                MessageBox.Show("Введите сообщение.");
                return;
            }

            // Добавляем сообщение в чат
            AppendMessage($"Вы: {message}");

            // Отправляем сообщение через WebSocket
            _webSocket.Send(JsonConvert.SerializeObject(new
            {
                From = CurrentUserId,
                To = selectedFriendId,
                Message = message
            }));

            // Очищаем поле ввода
            InputBox.Clear();
        }

        private void AppendMessage(string message)
        {
            if (!chats.ContainsKey(selectedFriendId))
            {
                chats[selectedFriendId] = new List<string>();
            }

            chats[selectedFriendId].Add(message);

            // Обновляем историю чата
            ChatTextBlock.Text = string.Join(Environment.NewLine, chats[selectedFriendId]);

            // Прокручиваем чат вниз
            ScrollViewer scrollViewer = (ScrollViewer)ChatTextBlock.Parent;
            scrollViewer.ScrollToEnd();
        }

    }
}
