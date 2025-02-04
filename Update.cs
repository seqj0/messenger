using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json.Linq;

public class Update
{
    private const string GitHubApiUrl = "https://api.github.com/repos/seqj0/messenger/releases/latest";
    private readonly HttpClient _client;

    public Update()
    {
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Add("User-Agent", "YourAppName");
    }

    private string CurrentVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

    public async Task CheckForUpdatesAsync()
    {
        try
        {
            // Запрос на GitHub API
            var response = await _client.GetAsync(GitHubApiUrl);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var releaseInfo = JObject.Parse(content);

            string latestVersion = releaseInfo["tag_name"]?.ToString().TrimStart('v'); // Убираем "v", если есть
            string downloadUrl = releaseInfo["assets"]?[0]?["browser_download_url"]?.ToString();

            if (latestVersion == null || downloadUrl == null)
            {
                MessageBox.Show("Ошибка получения данных о релизе.", "Обновление");
                return;
            }

            // Сравнение версий
            if (Version.Parse(latestVersion) > Version.Parse(CurrentVersion))
            {
                if (MessageBox.Show($"Доступно обновление {latestVersion}. Скачать сейчас?", "Обновление", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    await DownloadUpdateAsync(downloadUrl);
                }
            }
            else
            {
                MessageBox.Show("Вы используете последнюю версию.", "Обновление");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка проверки обновлений: {ex.Message}", "Обновление");
        }
    }

    private async Task DownloadUpdateAsync(string url)
    {
        try
        {
            var downloadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "update.exe");

            using (var response = await _client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();

                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(downloadPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await stream.CopyToAsync(fileStream);
                }

                MessageBox.Show("Обновление загружено. Приложение будет закрыто для установки.", "Обновление");
                Process.Start(downloadPath);
                Application.Current.Shutdown();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка скачивания обновления: {ex.Message}", "Обновление");
        }
    }
}
