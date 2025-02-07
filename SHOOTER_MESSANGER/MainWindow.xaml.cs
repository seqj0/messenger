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

        private void InitializeWebSocket()
        {
            try
            {
                _webSocket = new WebSocket("ws://192.168.1.34:5000/ws");

                _webSocket.OnOpen += (sender, e) =>
                {
                    StartPingLoop(); // Оставляем пинг, но не пишем "Соединение установлено"
                };

                _webSocket.OnClose += (sender, e) =>
                {
                    MessageBox.Show("Соединение с сервером разорвано.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Task.Delay(5000).ContinueWith(t => ReconnectWebSocket());
                };

                _webSocket.OnError += (sender, e) =>
                {
                    MessageBox.Show($"Ошибка WebSocket: {e.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    Task.Delay(5000).ContinueWith(t => ReconnectWebSocket());
                };

                // Обработчик для получения сообщений через WebSocket
                _webSocket.OnMessage += (sender, e) =>
                {
                    var receivedMessage = JsonConvert.DeserializeObject<Messages>(e.Data);

                    Dispatcher.Invoke(() =>
                    {
                        AppendMessage(receivedMessage.Message, receivedMessage.SenderUsername, receivedMessage.Timestamp);
                    });
                };

                _webSocket.ConnectAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void CheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            var update = new Update();
            await update.CheckForUpdatesAsync();
        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage();
                e.Handled = true; // Предотвращаем добавление новой строки в TextBox
            }
        }

        private void ReconnectWebSocket()
        {
            try
            {
                _webSocket.ConnectAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка переподключения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    MessageBox.Show($"Ошибка при отправке ping: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                await Task.Delay(10000);
            }
        }

        private async void LoadFriendsFromDatabase()
        {
            try
            {
                var response = await HttpClientProvider.Client.GetAsync($"{HttpClientProvider.GetBaseUrl()}/api/Friends/friends?userId={CurrentUserId}");

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Ошибка загрузки друзей: {response.StatusCode}");
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
                MessageBox.Show($"Ошибка загрузки друзей: {ex.Message}");
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
            if (selectedFriendId == -1 || CurrentUserId == -1)
            {
                MessageBox.Show("Выберите друга и убедитесь, что вы авторизованы.");
                return;
            }

            if (_webSocket == null || !_webSocket.IsAlive)
            {
                MessageBox.Show("Ошибка: соединение с сервером отсутствует.");
                return;
            }

            string message = InputBox.Text.Trim();

            if (string.IsNullOrEmpty(message))
            {
                MessageBox.Show("Введите сообщение.");
                return;
            }
            Console.WriteLine($"Отправитель: {CurrentUserId}, Получатель: {selectedFriendId}, Сообщение: {message}");

            // Добавляем сообщение в чат
            MessageBox.Show($"Вы: {message}");

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

        private void AppendMessage(string message, string senderNick, DateTime timestamp)
        {
            if (!chats.ContainsKey(selectedFriendId))
            {
                chats[selectedFriendId] = new List<string>();
            }

            // Форматирование сообщения с временем и ником отправителя
            string formattedMessage = $"[{timestamp:HH:mm}] {senderNick}: {message}";

            chats[selectedFriendId].Add(formattedMessage);

            // Обновление UI
            ChatTextBlock.Text = string.Join(Environment.NewLine, chats[selectedFriendId]);

            // Прокрутка вниз
            ScrollViewer scrollViewer = (ScrollViewer)ChatTextBlock.Parent;
            scrollViewer.ScrollToEnd();

            // Обновленная версия обработки WebSocket сообщений
            _webSocket.OnMessage += (sender, e) =>
            {
                var receivedMessage = JsonConvert.DeserializeObject<Messages>(e.Data);

                Dispatcher.Invoke(() =>
                {
                    // Вместо использования senderNick, теперь получаем его с сервера
                    AppendMessage(receivedMessage.Message, receivedMessage.SenderUsername, receivedMessage.Timestamp);
                });
            };
        }
        private void SendMessage()
        {
            if (!string.IsNullOrWhiteSpace(InputBox.Text) && selectedFriendId != -1)
            {
                string messageText = InputBox.Text.Trim();
                InputBox.Text = ""; // Очищаем поле ввода

                string senderNick = friends.ContainsKey(CurrentUserId) ? friends[CurrentUserId] : "Вы";

                var messageObject = new
                {
                    SenderId = CurrentUserId,
                    ReceiverId = selectedFriendId,
                    Message = messageText,
                    Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") // Отправляем время в UTC
                };

                string jsonMessage = JsonConvert.SerializeObject(messageObject);

                if (_webSocket != null && _webSocket.IsAlive)
                {
                    _webSocket.Send(jsonMessage);
                    AppendMessage(messageText, senderNick, DateTime.UtcNow); // Отображаем у себя в чате с ником
                }
                else
                {
                    AppendMessage("Ошибка: соединение с сервером отсутствует.", "Система", DateTime.UtcNow);
                }
            }
        }

        private async void LoadMessageHistory(int friendId)
        {
            try
            {
                var response = await HttpClientProvider.Client.GetAsync(
    $"{HttpClientProvider.GetBaseUrl()}/api/messages/history?UserId={CurrentUserId}&FriendId={selectedFriendId}&page=1&pageSize=20"
);
                ;

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Ошибка загрузки истории сообщений: {response.StatusCode}");
                    return;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var messageList = JsonConvert.DeserializeObject<List<Messages>>(jsonResponse);

                // Очистим текущие сообщения перед загрузкой новых
                chats[friendId].Clear();

                // Добавим загруженные сообщения в чат
                foreach (var message in messageList)
                {
                    string formattedMessage = $"[{message.Timestamp:HH:mm}] {message.SenderUsername}: {message.Message}";
                    chats[friendId].Add(formattedMessage);
                }

                // Обновим интерфейс
                ChatTextBlock.Text = string.Join(Environment.NewLine, chats[friendId]);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки истории сообщений: {ex.Message}");
            }
        }

        private void FriendsListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (FriendsListBox.SelectedItem != null)
            {
                // Находим ID выбранного друга
                var friendName = FriendsListBox.SelectedItem.ToString();
                selectedFriendId = GetFriendIdByName(friendName);

                FriendNicknameTextBlock.Text = $"Чат с {friendName}";

                // Загружаем историю сообщений
                LoadMessageHistory(selectedFriendId);
            }
        }

    }
}
