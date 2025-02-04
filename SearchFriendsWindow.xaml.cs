using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SHOOTER_MESSANGER
{
    public partial class SearchFriendsWindow : Window
    {
        private int currentUserId;
        private CancellationTokenSource cts; // Для отмены предыдущих запросов

        public event Action<Friend> FriendAdded; // Событие для уведомления MainWindow

        public SearchFriendsWindow(int userId)
        {
            InitializeComponent();
            currentUserId = userId;

            // Загрузка всех пользователей при открытии окна
            _ = LoadAllUsersAsync();
            _ = LoadFriendsListAsync();
            _ = LoadIncomingRequestsAsync();
            _ = LoadOutgoingRequestsAsync();
        }

        // Загрузка всех пользователей при открытии
        private async Task LoadAllUsersAsync()
        {
            try
            {
                var response = await HttpClientProvider.Client.GetAsync($"{HttpClientProvider.GetBaseUrl()}/api/Friends/all-users");

                if (!response.IsSuccessStatusCode)
                {
                    MessageBoxText.Text = $"Ошибка загрузки пользователей: {response.StatusCode} - {response.ReasonPhrase}";
                    return;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(jsonResponse))
                {
                    MessageBoxText.Text = "Ничего не найдено.";
                    SearchResultsListBox.ItemsSource = null; // Очищаем список
                    return;
                }

                // Десериализация данных
                var users = JsonConvert.DeserializeObject<List<Friend>>(jsonResponse);

                // Если десериализация прошла успешно, связываем данные с ListBox
                if (users != null && users.Count > 0)
                {
                    SearchResultsListBox.ItemsSource = users;
                }
                else
                {
                    MessageBoxText.Text = "Нет пользователей для отображения.";
                    SearchResultsListBox.ItemsSource = null; // Очищаем список
                }
            }
            catch (Exception ex)
            {
                MessageBoxText.Text = $"Ошибка загрузки: {ex.Message}";
            }
        }


        // Загрузка списка друзей
        private async Task LoadFriendsListAsync()
        {
            try
            {
                var response = await HttpClientProvider.Client.GetAsync($"{HttpClientProvider.GetBaseUrl()}/api/Friends/friends?userId={currentUserId}");

                if (!response.IsSuccessStatusCode)
                {
                    MessageBoxText.Text = $"Ошибка загрузки списка друзей: {response.StatusCode} - {response.ReasonPhrase}";
                    return;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var friendsList = JsonConvert.DeserializeObject<List<Friend>>(jsonResponse);
                FriendsListBox.ItemsSource = friendsList;
            }
            catch (Exception ex)
            {
                MessageBoxText.Text = $"Ошибка загрузки списка друзей: {ex.Message}";
            }
        }

        // Загрузка входящих заявок
        private async Task LoadIncomingRequestsAsync()
        {
            try
            {
                var response = await HttpClientProvider.Client.GetAsync($"{HttpClientProvider.GetBaseUrl()}/api/Friends/incoming-requests?userId={currentUserId}");

                if (!response.IsSuccessStatusCode)
                {
                    MessageBoxText.Text = $"Ошибка загрузки входящих заявок: {response.StatusCode} - {response.ReasonPhrase}";
                    return;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var incomingRequests = JsonConvert.DeserializeObject<List<FriendRequest>>(jsonResponse);
                IncomingRequestsListBox.ItemsSource = incomingRequests;
            }
            catch (Exception ex)
            {
                MessageBoxText.Text = $"Ошибка загрузки входящих заявок: {ex.Message}";
            }
        }

        // Загрузка исходящих заявок
        private async Task LoadOutgoingRequestsAsync()
        {
            try
            {
                var response = await HttpClientProvider.Client.GetAsync($"{HttpClientProvider.GetBaseUrl()}/api/Friends/outgoing-requests?userId={currentUserId}");

                if (!response.IsSuccessStatusCode)
                {
                    MessageBoxText.Text = $"Ошибка загрузки исходящих заявок: {response.StatusCode} - {response.ReasonPhrase}";
                    return;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var outgoingRequests = JsonConvert.DeserializeObject<List<FriendRequest>>(jsonResponse);
                OutgoingRequestsListBox.ItemsSource = outgoingRequests;
            }
            catch (Exception ex)
            {
                MessageBoxText.Text = $"Ошибка загрузки исходящих заявок: {ex.Message}";
            }
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка, если запрос пустой, загрузим всех пользователей
            string query = SearchBox.Text.Trim();
            if (string.IsNullOrEmpty(query))
            {
                await LoadAllUsersAsync();
            }
            else
            {
                await PerformSearchAsync(query);
            }
        }

        // Отклонение заявки
        private async void RejectRequestButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var requesterId = (int)button.Tag; // Получаем ID пользователя, отправившего заявку

            try
            {
                // Отправка запроса на сервер для отклонения заявки
                var response = await HttpClientProvider.Client.PostAsync(
                    $"{HttpClientProvider.GetBaseUrl()}/api/Friends/accept-or-reject",
                    new StringContent(JsonConvert.SerializeObject(new
                    {
                        UserId = currentUserId, // Текущий пользователь
                        FriendId = requesterId, // ID пользователя, отправившего заявку
                        Action = "reject" // Отклонить заявку
                    }), Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                {
                    MessageBoxText.Text = $"Ошибка отклонения заявки: {response.StatusCode} - {response.ReasonPhrase}";
                    return;
                }

                // После успешного отклонения заявки
                MessageBoxText.Text = "Заявка отклонена.";

                // Удаляем заявку из списка (если есть)
                var requestToRemove = IncomingRequestsListBox.Items.Cast<FriendRequest>()
                    .FirstOrDefault(request => request.RequesterId == requesterId);
                if (requestToRemove != null)
                {
                    var incomingRequests = IncomingRequestsListBox.ItemsSource as List<FriendRequest>;
                    incomingRequests?.Remove(requestToRemove);
                    IncomingRequestsListBox.Items.Refresh(); // Обновляем UI
                }
            }
            catch (Exception ex)
            {
                MessageBoxText.Text = $"Ошибка при отклонении заявки: {ex.Message}";
            }
        }

        // Принятие заявки
        private async void AcceptRequestButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var requesterId = (int)button.Tag; // Получаем ID пользователя, отправившего заявку

            try
            {
                // Отправка запроса на сервер для принятия заявки
                var response = await HttpClientProvider.Client.PostAsync(
                    $"{HttpClientProvider.GetBaseUrl()}/api/Friends/accept-or-reject",
                    new StringContent(JsonConvert.SerializeObject(new
                    {
                        UserId = currentUserId, // Текущий пользователь
                        FriendId = requesterId, // ID пользователя, отправившего заявку
                        Action = "accept" // Принять заявку
                    }), Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                {
                    MessageBoxText.Text = $"Ошибка принятия заявки: {response.StatusCode} - {response.ReasonPhrase}";
                    return;
                }

                // После успешного принятия заявки обновляем список друзей
                MessageBoxText.Text = "Заявка принята.";

                // Уведомляем об изменении в MainWindow
                // (например, через событие, которое обновит список)
                FriendAdded?.Invoke(new Friend { UserId = requesterId });

                // Удаляем заявку из списка входящих
                var friendToAdd = IncomingRequestsListBox.Items.Cast<FriendRequest>()
                    .FirstOrDefault(request => request.RequesterId == requesterId);
                if (friendToAdd != null)
                {
                    var incomingRequests = IncomingRequestsListBox.ItemsSource as List<FriendRequest>;
                    incomingRequests?.Remove(friendToAdd);
                    IncomingRequestsListBox.Items.Refresh(); // Обновляем UI
                }
            }
            catch (Exception ex)
            {
                MessageBoxText.Text = $"Ошибка при принятии заявки: {ex.Message}";
            }
        }


        private async Task PerformSearchAsync(string query)
        {
            try
            {
                // Логирование запроса для отладки
                Console.WriteLine($"Поиск с запросом: {query}");

                var response = await HttpClientProvider.Client.GetAsync($"{HttpClientProvider.GetBaseUrl()}/api/Friends/search?query={query}");

                // Логирование ответа от сервера
                Console.WriteLine($"Ответ от сервера: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    MessageBoxText.Text = $"Ошибка поиска: {response.StatusCode} - {response.ReasonPhrase}";
                    return;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();

                // Логирование JSON-ответа
                Console.WriteLine($"Ответ: {jsonResponse}");

                if (string.IsNullOrEmpty(jsonResponse))
                {
                    MessageBoxText.Text = "Ничего не найдено. Показываем всех пользователей.";
                    await LoadAllUsersAsync();
                    return;
                }

                // Попробуем десериализовать ответ
                try
                {
                    var searchResults = JsonConvert.DeserializeObject<List<Friend>>(jsonResponse);
                    SearchResultsListBox.ItemsSource = searchResults;
                }
                catch (JsonException jsonEx)
                {
                    MessageBoxText.Text = $"Ошибка десериализации: {jsonEx.Message}";
                    Console.WriteLine($"Ошибка при десериализации: {jsonEx.Message}");
                }
            }
            catch (Exception ex)
            {
                MessageBoxText.Text = $"Произошла ошибка: {ex.Message}";
                Console.WriteLine($"Ошибка при запросе: {ex.Message}");
            }
        }


        // Поиск в реальном времени
        private async void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            cts?.Cancel(); // Отменяем предыдущие запросы
            cts = new CancellationTokenSource();

            string query = SearchBox.Text.Trim();

            if (string.IsNullOrEmpty(query))
            {
                // Если запрос пустой, показываем всех пользователей
                await LoadAllUsersAsync();
                return;
            }

            try
            {
                await Task.Delay(300, cts.Token); // Задержка 300 мс перед выполнением поиска

                // Выполняем поиск
                await PerformSearchAsync(query);
            }
            catch (TaskCanceledException)
            {
                // Игнорируем, если запрос был отменен
            }
            catch (Exception ex)
            {
                MessageBoxText.Text = $"Произошла ошибка: {ex.Message}";
            }
        }

        // Добавление в друзья
        private async void AddFriendButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            Console.WriteLine($"[DEBUG] button.Tag = {button.Tag} (тип: {button.Tag?.GetType()})");
            if (!int.TryParse(button.Tag?.ToString(), out var userId))
            {
                Console.WriteLine($"[DEBUG] sender: {sender?.GetType()}");
                Console.WriteLine($"[DEBUG] button.Tag = {button?.Tag} (тип: {button?.Tag?.GetType()})");
                Console.WriteLine("[ERROR] Невозможно получить userId из button.Tag!");
                return;
            }


            Console.WriteLine($"[CLIENT] Нажата кнопка: userId={userId}, currentUserId={currentUserId}");

            try
            {
                // 🔹 Проверка ID перед отправкой
                if (currentUserId <= 0 || userId <= 0)
                {
                    Console.WriteLine($"[CLIENT ERROR] Некорректные ID: currentUserId={currentUserId}, friendId={userId}");
                    MessageBoxText.Text = "Ошибка: некорректные ID пользователей.";
                    return;
                }

                // 🔹 Формируем JSON и логируем
                var jsonBody = JsonConvert.SerializeObject(new { UserId = currentUserId, FriendId = userId });
                Console.WriteLine($"[CLIENT] Отправляем запрос на сервер: {jsonBody}");

                var response = await HttpClientProvider.Client.PostAsync(
                    $"{HttpClientProvider.GetBaseUrl()}/api/Friends/send-request",
                    new StringContent(jsonBody, Encoding.UTF8, "application/json"));

                // 🔹 Логируем статус ответа
                Console.WriteLine($"[CLIENT] Ответ сервера: {response.StatusCode} - {response.ReasonPhrase}");

                if (!response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[CLIENT ERROR] Текст ответа сервера: {responseContent}");
                    MessageBoxText.Text = $"Ошибка добавления в друзья: {response.StatusCode} - {responseContent}";
                    return;
                }

                MessageBoxText.Text = "Вы добавили пользователя в друзья.";
                Console.WriteLine($"[CLIENT SUCCESS] Пользователь {userId} добавлен в друзья.");

                // Уведомляем MainWindow (например, через событие)
                FriendAdded?.Invoke(new Friend { UserId = userId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CLIENT ERROR] Исключение при добавлении в друзья: {ex.Message}");
                MessageBoxText.Text = $"Ошибка при добавлении в друзья: {ex.Message}";
            }
        }

        // Удаление друга
        private async void RemoveFriendButton_Click(object sender, RoutedEventArgs e)
        {
            var friendId = (int)((Button)sender).Tag;
            try
            {
                var response = await HttpClientProvider.Client.PostAsync(
                    $"{HttpClientProvider.GetBaseUrl()}/api/Friends/remove-friend",
                    new StringContent(JsonConvert.SerializeObject(new
                    {
                        UserId = currentUserId,
                        FriendId = friendId
                    }), Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                {
                    MessageBoxText.Text = $"Ошибка удаления друга: {response.StatusCode} - {response.ReasonPhrase}";
                    return;
                }

                MessageBoxText.Text = "Друг удален.";
            }
            catch (Exception ex)
            {
                MessageBoxText.Text = $"Ошибка при удалении друга: {ex.Message}";
            }
        }

        private void SearchResultsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Получаем выбранного пользователя из списка
            var selectedFriend = (Friend)SearchResultsListBox.SelectedItem;

            if (selectedFriend != null)
            {
                // Логика для обработки выбора пользователя
                MessageBoxText.Text = $"Вы выбрали {selectedFriend.FriendName}";
            }
        }
    }
}
