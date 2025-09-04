using System;

namespace shutdown_timer.Models
{
    public enum AppTheme
    {
        Default,
        Light,
        Dark
    }

    public class AppSettings
    {
        public string Language { get; set; } = "ja";
        public AppTheme Theme { get; set; } = AppTheme.Default;
        public bool StartMinimized { get; init; }
        public bool RememberLastSettings { get; init; } = true;
        public bool ShowNotifications { get; init; } = true;
        public bool ShowWarningBeforeShutdown { get; init; } = true;
        public int WarningTimeSeconds { get; init; } = 30;
        public bool ConfirmOnExit { get; init; }
        public bool AutoSaveSchedule { get; set; } = true;
        public bool ShowCountdownInTitle { get; init; }

        // Last used settings (if RememberLastSettings is true)
        public TimerMode LastUsedMode { get; set; } = TimerMode.Duration;
        public int LastHours { get; set; }
        public int LastMinutes { get; set; } = 30;
        public int LastSeconds { get; set; }
        public TimeSpan LastSpecificTime { get; set; } = TimeSpan.Zero;
        public ActionType LastActionType { get; set; } = ActionType.Shutdown;
        public bool LastForceAction { get; set; }

        public AppSettings Clone()
        {
            return new AppSettings
            {
                Language = this.Language,
                Theme = this.Theme,
                StartMinimized = this.StartMinimized,
                RememberLastSettings = this.RememberLastSettings,
                ShowNotifications = this.ShowNotifications,
                ShowWarningBeforeShutdown = this.ShowWarningBeforeShutdown,
                WarningTimeSeconds = this.WarningTimeSeconds,
                ConfirmOnExit = this.ConfirmOnExit,
                AutoSaveSchedule = this.AutoSaveSchedule,
                ShowCountdownInTitle = this.ShowCountdownInTitle,
                LastUsedMode = this.LastUsedMode,
                LastHours = this.LastHours,
                LastMinutes = this.LastMinutes,
                LastSeconds = this.LastSeconds,
                LastSpecificTime = this.LastSpecificTime,
                LastActionType = this.LastActionType,
                LastForceAction = this.LastForceAction
            };
        }
    }
}
