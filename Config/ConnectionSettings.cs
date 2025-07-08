using System;
using System.IO;
using Newtonsoft.Json;

namespace PMSWPF.Config
{
    public class ConnectionSettings
    {
        public string DbType { get; set; } = "MySql";
        public string Server { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 3306;
        public string UserId { get; set; } = "root";
        public string Password { get; set; } = "Pgw15221236646";
        public string Database { get; set; } = "pmswpf";
        public bool EnableS7Service { get; set; } = true;
        public bool EnableMqttService { get; set; } = true;
        public bool EnableOpcUaService { get; set; } = true;

        private static readonly string SettingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "connectionSettings.json");

        public static ConnectionSettings Load()
        {
            if (File.Exists(SettingsFilePath))
            {
                string json = File.ReadAllText(SettingsFilePath);
                return JsonConvert.DeserializeObject<ConnectionSettings>(json);
            }
            return new ConnectionSettings(); // Return default settings if file doesn't exist
        }

        public void Save()
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(SettingsFilePath, json);
        }

        public string ToConnectionString()
        {
            // This example is for MySQL. You'll need to adjust for other database types.
            return $"server={Server};port={Port};user={UserId};password={Password};database={Database};";
        }
    }
}
