using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using shutdown_timer.Services;
using shutdown_timer.Models;

namespace shutdown_timer.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ShutdownService _shutdownService;
        private readonly LocalizationService _localizationService;
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly DispatcherQueueTimer _countdownTimer;

        private bool _isTimerActive;
        private DateTime _targetTime;
        private string _statusMessage;
        private string _countdownText = "";
        private string _targetTimeText = "";
        private bool _countdownVisible;

        public MainViewModel()
        {
            _shutdownService = new ShutdownService();
            _localizationService = LocalizationService.Instance;
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            _countdownTimer = _dispatcherQueue.CreateTimer();
            _countdownTimer.Interval = TimeSpan.FromSeconds(1);
            _countdownTimer.Tick += CountdownTimer_Tick;

            _statusMessage = _localizationService.GetString("Ready");
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsTimerActive
        {
            get => _isTimerActive;
            set => SetProperty(ref _isTimerActive, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public string CountdownText
        {
            get => _countdownText;
            set => SetProperty(ref _countdownText, value);
        }

        public string TargetTimeText
        {
            get => _targetTimeText;
            set => SetProperty(ref _targetTimeText, value);
        }

        public bool CountdownVisible
        {
            get => _countdownVisible;
            set => SetProperty(ref _countdownVisible, value);
        }

        public async Task<bool> StartShutdownAsync(ShutdownConfig config)
        {
            try
            {
                var success = await _shutdownService.ScheduleShutdownAsync(config);
                if (success)
                {
                    _targetTime = config.TargetTime;
                    IsTimerActive = true;
                    CountdownVisible = true;
                    StatusMessage = GetActionMessage(config.ActionType);
                    UpdateTargetTimeDisplay();
                    _countdownTimer.Start();
                }
                return success;
            }
            catch (Exception ex)
            {
                StatusMessage = _localizationService.GetString("Error", ex.Message);
                return false;
            }
        }

        public void RestoreTimerState(ShutdownConfig config)
        {
            // Restore timer state without re-scheduling shutdown command
            _targetTime = config.TargetTime;
            IsTimerActive = true;
            CountdownVisible = true;
            StatusMessage = GetActionMessage(config.ActionType);
            UpdateTargetTimeDisplay();
            _countdownTimer.Start();
        }

        public async Task<bool> CancelShutdownAsync()
        {
            try
            {
                var success = await _shutdownService.CancelShutdownAsync();
                if (success)
                {
                    IsTimerActive = false;
                    CountdownVisible = false;
                    StatusMessage = _localizationService.GetString("Cancelled");
                    _countdownTimer.Stop();
                }
                return success;
            }
            catch (Exception ex)
            {
                StatusMessage = _localizationService.GetString("Error", ex.Message);
                return false;
            }
        }

        private void CountdownTimer_Tick(object sender, object e)
        {
            var remainingTime = _targetTime - DateTime.Now;

            if (remainingTime.TotalSeconds <= 0)
            {
                _countdownTimer.Stop();
                CountdownVisible = false;
                IsTimerActive = false;
                StatusMessage = GetActionMessage(ActionType.Shutdown);
                return;
            }

            CountdownText = FormatTimeSpan(remainingTime);
        }

        private string FormatTimeSpan(TimeSpan timeSpan)
        {
            var isJapanese = _localizationService.CurrentLanguage == "ja";

            if (timeSpan.TotalDays >= 1)
            {
                if (isJapanese)
                    return $"{(int)timeSpan.TotalDays}日 {timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
                else
                    return $"{(int)timeSpan.TotalDays}d {timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            }
            else if (timeSpan.TotalHours >= 1)
            {
                return $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            }
            else
            {
                return $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            }
        }

        private void UpdateTargetTimeDisplay()
        {
            var targetTimeText = _targetTime.ToString("HH:mm");

            // Check if target time is tomorrow
            if (_targetTime.Date > DateTime.Now.Date)
            {
                TargetTimeText = $"明日 {targetTimeText}";
            }
            else
            {
                TargetTimeText = targetTimeText;
            }
        }

        private string GetActionMessage(ActionType actionType)
        {
            return actionType switch
            {
                ActionType.Shutdown => _localizationService.GetString("ExecutingShutdown"),
                ActionType.Restart => _localizationService.GetString("ExecutingRestart"),
                ActionType.Sleep => _localizationService.GetString("ExecutingSleep"),
                ActionType.Logoff => _localizationService.GetString("ExecutingLogoff"),
                _ => _localizationService.GetString("ExecutingShutdown")
            };
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
