using System;
using System.Net.Http;
using static System.Net.WebRequestMethods;

namespace SHOOTER_MESSANGER
{
    public static class HttpClientProvider
    {
        public static readonly HttpClient Client = new HttpClient();
        public static bool IsSecure { get; } = false;
        public static string GetBaseUrl()
        {
            // Внешний IP: 195.46.162.142
            // Локальный IP: 192.168.1.34
            return "http://192.168.1.34:5000";
        }
    }
}
