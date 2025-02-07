using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;

namespace SHOOTER_MESSANGER
{
    public partial class LoginWindow : Window
    {
        public static readonly HttpClient httpClient = new HttpClient();

        public LoginWindow()
        {
            InitializeComponent();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameBox.Text.Trim();
            var password = PasswordBox.Password.Trim();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Имя пользователя и пароль не могут быть пустыми.",
                                 "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Создание JSON с учетными данными
                var user = new { Username = username, Password = password };
                var json = JsonConvert.SerializeObject(user);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Отправка POST-запроса на сервер
                var response = await httpClient.PostAsync($"{HttpClientProvider.GetBaseUrl()}/login", content);

                // Получаем ответ от сервера
                var responseMessage = await response.Content.ReadAsStringAsync();

                // Если ответ не является JSON, отобразим ошибку
                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseMessage);
                    int userId = loginResponse.UserId; // Получаем ID пользователя

                    MessageBox.Show("Вход выполнен успешно!",
                                    "Успех", 
                                    MessageBoxButton.OK, 
                                    MessageBoxImage.Information);

                    // Переход к главному окну
                    var mainWindow = new MainWindow(userId); // Передаем ID пользователя
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    // Обработка ошибок
                    if ((int)response.StatusCode == 401)
                    {
                        MessageBox.Show("Неверный логин или пароль.",
                                        "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        MessageBox.Show($"Ошибка входа: {responseMessage}",
                                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Сервер недоступен. Проверьте соединение.\n{ex.Message}",
                                "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Неизвестная ошибка: {ex.Message}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenRegisterWindow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Открытие окна регистрации
                var registerWindow = new RegisterWindow();
                registerWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии окна регистрации: {ex.Message}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
