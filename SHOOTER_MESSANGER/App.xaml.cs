using System.Reflection;
using System.Windows;

namespace SHOOTER_MESSANGER
{
    public partial class App : Application
    {
        public static string CurrentVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();
    }
}
