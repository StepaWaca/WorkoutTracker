using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace WorkoutTracker
{
    public class DatabaseConfig
    {
        public string Server { get; set; } = ".\\SQLEXPRESS";
        public string DatabaseName { get; set; } = "WorkoutTrackerDB";
        public bool UseWindowsAuthentication { get; set; } = true;
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";

        public string GetConnectionString()
        {
            if (UseWindowsAuthentication)
            {
                return $"Server={Server};Database={DatabaseName};Trusted_Connection=True;TrustServerCertificate=True;";
            }
            return $"Server={Server};Database={DatabaseName};User Id={Username};Password={Password};TrustServerCertificate=True;";
        }

        public static DatabaseConfig LoadFromFile(string filePath = "appsettings.json")
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                var config = JsonConvert.DeserializeObject<DatabaseConfig>(json);
                if (config != null) return config;
            }
            var config2 = new DatabaseConfig();
            string json2 = JsonConvert.SerializeObject(config2, Formatting.Indented);
            File.WriteAllText(filePath, json2);
            return config2;
        }
    }
}