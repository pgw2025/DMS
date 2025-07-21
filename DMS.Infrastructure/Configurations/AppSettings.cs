using DMS.Core.Models;
using Newtonsoft.Json;

namespace DMS.Infrastructure.Configurations
{
    public class DatabaseSettings
    {
        public string DbType { get; set; } = "MySql";
        public string Server { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 3306;
        public string UserId { get; set; } = "root";
        public string Password { get; set; } = "Pgw15221236646";
        public string Database { get; set; } = "pmswpf";
    }

    public class AppSettings
    {
        public DatabaseSettings Database { get; set; } = new DatabaseSettings();
        public string Theme { get; set; } = "跟随系统";
        public bool EnableS7Service { get; set; } = true;
        public bool EnableMqttService { get; set; } = true;
        public bool EnableOpcUaService { get; set; } = true;
        public bool MinimizeToTrayOnClose { get; set; } = true;
        public List<MenuBean> Menus { get; set; } = new List<MenuBean>();

        private static readonly string SettingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appSettings.json");

        public static AppSettings Load()
        {
            if (File.Exists(SettingsFilePath))
            {
                string json = File.ReadAllText(SettingsFilePath);
                return JsonConvert.DeserializeObject<AppSettings>(json);
            }
            return new AppSettings();
        }

        public void Save()
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(SettingsFilePath, json);
        }

        public string ToConnectionString()
        {
            return $"server={Database.Server};port={Database.Port};user={Database.UserId};password={Database.Password};database={Database.Database};";
        }
    }
}
