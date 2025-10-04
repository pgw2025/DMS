using DMS.Core.Models;
using Newtonsoft.Json;

namespace DMS.Application.Configurations
{


    public class AppSettings
    {
        private static readonly string SettingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigJsonFileName);

        private const string ConfigJsonFileName = "dms_config.json";


        public class Database
        {
            public string DbType { get; set; } = "MySql";
            public string Server { get; set; } = "127.0.0.1";
            public int Port { get; set; } = 3306;
            public string UserId { get; set; } = "root";
            public string Password { get; set; } = "Pgw15221236646";
            public string DbName { get; set; } = "dms_test";
        }


        public Database Db { get; set; } = new Database();

        public Variable VariableImportTemplate = new Variable()
        {
            IsActive = true,
            PollingInterval = 30000,
            IsHistoryEnabled = true,
            HistoryDeadband = 0
        };

        public string Theme { get; set; } = "跟随系统";
        public bool MinimizeToTrayOnClose { get; set; } = true;

        

        public void Load()
        {
            if (File.Exists(SettingsFilePath))
            {
                string json = File.ReadAllText(SettingsFilePath);
                AppSettings? appSettings = JsonConvert.DeserializeObject<AppSettings>(json);
                if (appSettings != null)
                {
                    this.Db= appSettings.Db;
                    this.VariableImportTemplate = appSettings.VariableImportTemplate;
                    this.Theme=appSettings.Theme;
                    this.MinimizeToTrayOnClose=appSettings.MinimizeToTrayOnClose;
                }
                else
                {
                    throw new InvalidOperationException("加载配置文件出现了错误。");
                }
            }
        }

        public void Save()
        {
                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(SettingsFilePath, json);
        }

        public string ToConnectionString()
        {
            return $"server={Db.Server};port={Db.Port};user={Db.UserId};password={Db.Password};database={Db.DbName};";
        }
    }
}
