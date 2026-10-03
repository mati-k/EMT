using EMT.Helpers;
using EMT.Models;
using Serilog;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace EMT.Services
{
    public class ConfigService : IConfigService
    {
        public ConfigData ConfigData { get; private set; } = new();

        public ConfigService()
        {
            if (TryRead(AppPaths.ConfigFile) is ConfigData config)
            {
                ConfigData = config;
                return;
            }

            foreach (string legacyFile in AppPaths.LegacyConfigFiles)
            {
                if (TryRead(legacyFile) is ConfigData legacyConfig)
                {
                    ConfigData = legacyConfig;
                    return;
                }
            }
        }

        private static ConfigData? TryRead(string path)
        {
            if (!File.Exists(path))
                return null;

            try
            {
                using var fs = File.OpenRead(path);
                return JsonSerializer.Deserialize<ConfigData>(fs);
            }
            catch (Exception e)
            {
                Log.Warning(e, "Couldn't read config {Path}", path);
                return null;
            }
        }

        public async Task SaveConfig(ConfigData config)
        {
            Directory.CreateDirectory(AppPaths.DataFolder);
            using (var fs = File.Create(AppPaths.ConfigFile))
            {
                await JsonSerializer.SerializeAsync(fs, config, new JsonSerializerOptions { WriteIndented = true });
            }

            ConfigData = config;
        }
    }
}
