using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SHOOTER_MESSANGER
{
    public partial class AuthWindow : Window
    {
        private bool _isRegisterMode = false;
        private const string AuthStatusFile = "auth_status.txt";

        public AuthWindow()
        {
            try
            {
                InitializeComponent();

                // Проверка авторизации
                if (IsUserLoggedIn())
                {
                    int userId = GetSavedUserId(); // Проблемный вызов
                    var mainWindow = new MainWindow(userId);
                    mainWindow.Show();
                    this.Close();
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка в конструкторе: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private int GetSavedUserId()
        {
            string filePath = "user_id.txt";

            // Проверяем, существует ли файл
            if (!File.Exists(filePath))
            {
                throw new Exception("Файл user_id.txt не найден.");
            }

            // Читаем содержимое файла
            string idContent = File.ReadAllText(filePath).Trim();

            // Пробуем преобразовать содержимое в целое число
            if (int.TryParse(idContent, out int userId))
            {
                return userId;
            }

            // Если преобразование не удалось, выбрасываем исключение
            throw new Exception("Содержимое user_id.txt некорректно.");
        }

        private void SetUserId(int userId)
        {
            string filePath = "user_id.txt";
            File.WriteAllText(filePath, userId.ToString());
        }

        private void SwitchModeButton_Click(object sender, RoutedEventArgs e)
        {
            // Переключаем режим
            _isRegisterMode = !_isRegisterMode;

            if (_isRegisterMode)
            {
                // Режим регистрации
                TitleText.Text = "Регистрация";
                PrimaryButton.Content = "Зарегистрироваться";
                SwitchModeButton.Content = "У меня уже есть аккаунт";

                ConfirmPasswordBox.Visibility = Visibility.Visible;
                ConfirmPasswordLabel.Visibility = Visibility.Visible;
            }
            else
            {
                // Режим входа
                TitleText.Text = "Вход";
                PrimaryButton.Content = "Войти";
                SwitchModeButton.Content = "Создать аккаунт";

                ConfirmPasswordBox.Visibility = Visibility.Collapsed;
                ConfirmPasswordLabel.Visibility = Visibility.Collapsed;
            }
        }

        private async void PrimaryButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameBox.Text;
            string password = PasswordBox.Password;

            if (_isRegisterMode)
            {
                // Регистрация
                string confirmPassword = ConfirmPasswordBox.Password;

                if (password != confirmPassword)
                {
                    MessageBox.Show("Пароли не совпадают!");
                    return;
                }

                // Логика для регистрации
                var response = await RegisterUser(username, password);
                if (response.IsSuccessStatusCode)
                {
                    _isRegisterMode = false; // После успешной регистрации переключаемся на вход
                    SwitchModeButton_Click(null, null); // Обновляем интерфейс
                }
                else
                {
                    MessageBox.Show("Ошибка регистрации.");
                }
            }
            else
            {
                // Вход
                var response = await LoginUser(username, password);

                // Получаем ответ от сервера
                var responseMessage = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseMessage);
                    int userId = loginResponse.UserId; // Получаем ID пользователя

                    SetUserId(userId); // Сохраняем userId

                    MessageBox.Show("Вход выполнен успешно!",
                                    "Успех",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);

                    SetUserLoggedIn();

                    // Переход к главному окну
                    var mainWindow = new MainWindow(userId);
                    mainWindow.Show();
                    this.Close();
                }
                else // Если ответ не является JSON, отобразим ошибку
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
        }

        private async Task<HttpResponseMessage> RegisterUser(string username, string password)
        {
            var user = new { 
                Username = username, 
                Password = password 
            };
            var content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
            return await HttpClientProvider.Client.PostAsync($"{HttpClientProvider.GetBaseUrl()}/register", content);
        }

        private async Task<HttpResponseMessage> LoginUser(string username, string password)
        {
            var user = new { 
                Username = username, 
                Password = password 
            };
            var content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
            return await HttpClientProvider.Client.PostAsync($"{HttpClientProvider.GetBaseUrl()}/login", content);
        }

        private bool IsUserLoggedIn()
        {
            // Проверяем, существует ли файл и содержит ли он "true"
            if (File.Exists(AuthStatusFile))
            {
                string status = File.ReadAllText(AuthStatusFile);
                return status.Trim().ToLower() == "true";
            }
            return false;
        }

        private void SetUserLoggedIn()
        {
            // Сохраняем статус авторизации
            File.WriteAllText(AuthStatusFile, "true");
        }
    }
}
