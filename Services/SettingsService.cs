using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using shutdown_timer.Models;

namespace shutdown_timer.Services
{
    public class SettingsService
    {
        private static SettingsService? _instance;
        private static readonly object _lock = new object();

        public static SettingsService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new SettingsService();
                        }
                    }
                }
                return _instance;
            }
        }

        private const string SettingsFile = "app_settings.json";
        private AppSettings _currentSettings;

        private SettingsService()
        {
            _currentSettings = LoadSettingsSync();
        }

        private static AppSettings LoadSettingsSync()
        {
            try
            {
                if (File.Exists(SettingsFile))
                {
                    var jsonString = File.ReadAllText(SettingsFile);
                    var settings = JsonSerializer.Deserialize<AppSettings>(jsonString);
                    return settings ?? new AppSettings();
                }
            }
            catch
            {
                // If there's any error loading settings, return default settings
            }

            return new AppSettings();
        }

        public async Task SaveSettingsAsync(AppSettings settings)
        {
            try
            {
                var jsonString = JsonSerializer.Serialize(settings, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                await File.WriteAllTextAsync(SettingsFile, jsonString);
                _currentSettings = settings.Clone();
            }
            catch
            {
                // Ignore save errors - not critical
            }
        }

        public AppSettings CurrentSettings => _currentSettings.Clone();

        public async Task UpdateLastUsedSettingsAsync(TimerMode mode, int hours, int minutes, int seconds,
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

                await SaveSettingsAsync(_currentSettings);
            }
        }



        public event EventHandler<AppSettings>? SettingsChanged;

        protected virtual void OnSettingsChanged(AppSettings settings)
        {
            SettingsChanged?.Invoke(this, settings);
        }
    }
}
