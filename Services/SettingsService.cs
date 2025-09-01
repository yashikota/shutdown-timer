using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using shutdown_timer.Models;

namespace shutdown_timer.Services
{
    public class SettingsService
    {
        private static SettingsService _instance;
        public static SettingsService Instance => _instance ??= new SettingsService();

        private const string SETTINGS_FILE = "app_settings.json";
        private AppSettings _currentSettings;

        private SettingsService()
        {
            _currentSettings = LoadSettings();
        }

        public AppSettings LoadSettings()
        {
            try
            {
                if (File.Exists(SETTINGS_FILE))
                {
                    var jsonString = File.ReadAllText(SETTINGS_FILE);
                    var settings = JsonSerializer.Deserialize<AppSettings>(jsonString);
                    return settings ?? new AppSettings();
                }
            }
            catch (Exception)
            {
                // If there's any error loading settings, return default settings
            }

            return new AppSettings();
        }

        public async Task<AppSettings> LoadSettingsAsync()
        {
            try
            {
                if (File.Exists(SETTINGS_FILE))
                {
                    var jsonString = await File.ReadAllTextAsync(SETTINGS_FILE);
                    var settings = JsonSerializer.Deserialize<AppSettings>(jsonString);
                    return settings ?? new AppSettings();
                }
            }
            catch (Exception)
            {
                // If there's any error loading settings, return default settings
            }

            return new AppSettings();
        }

        public void SaveSettings(AppSettings settings)
        {
            try
            {
                var jsonString = JsonSerializer.Serialize(settings, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(SETTINGS_FILE, jsonString);
                _currentSettings = settings.Clone();
            }
            catch (Exception)
            {
                // Ignore save errors - not critical
            }
        }

        public async Task SaveSettingsAsync(AppSettings settings)
        {
            try
            {
                var jsonString = JsonSerializer.Serialize(settings, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                await File.WriteAllTextAsync(SETTINGS_FILE, jsonString);
                _currentSettings = settings.Clone();
            }
            catch (Exception)
            {
                // Ignore save errors - not critical
            }
        }

        public AppSettings CurrentSettings => _currentSettings.Clone();

        public void UpdateLastUsedSettings(TimerMode mode, int hours, int minutes, int seconds,
            TimeSpan specificTime, ActionType actionType, bool forceAction)
        {
            if (_currentSettings.RememberLastSettings)
            {
                _currentSettings.LastUsedMode = mode;
                _currentSettings.LastHours = hours;
                _currentSettings.LastMinutes = minutes;
                _currentSettings.LastSeconds = seconds;
                _currentSettings.LastSpecificTime = specificTime;
                _currentSettings.LastActionType = actionType;
                _currentSettings.LastForceAction = forceAction;

                SaveSettings(_currentSettings);
            }
        }

        public void ResetSettings()
        {
            _currentSettings = new AppSettings();
            SaveSettings(_currentSettings);
        }

        public event EventHandler<AppSettings> SettingsChanged;

        protected virtual void OnSettingsChanged(AppSettings settings)
        {
            SettingsChanged?.Invoke(this, settings);
        }
    }
}
