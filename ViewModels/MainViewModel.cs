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
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly DispatcherQueueTimer _countdownTimer;

        private bool _isTimerActive;
        private DateTime _targetTime;
        private string _statusMessage = "準備完了";
        private string _countdownText = "";
        private bool _countdownVisible;

        public MainViewModel()
        {
            _shutdownService = new ShutdownService();
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            _countdownTimer = _dispatcherQueue.CreateTimer();
            _countdownTimer.Interval = TimeSpan.FromSeconds(1);
            _countdownTimer.Tick += CountdownTimer_Tick;
        }

        public event PropertyChangedEventHandler PropertyChanged;

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
                    _countdownTimer.Start();
                }
                return success;
            }
            catch (Exception ex)
            {
                StatusMessage = $"エラー: {ex.Message}";
                return false;
            }
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
                    StatusMessage = "キャンセルしました";
                    _countdownTimer.Stop();
                }
                return success;
            }
            catch (Exception ex)
            {
                StatusMessage = $"エラー: {ex.Message}";
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
                StatusMessage = "実行中...";
                return;
            }

            CountdownText = FormatTimeSpan(remainingTime);
        }

        private string FormatTimeSpan(TimeSpan timeSpan)
        {
            if (timeSpan.TotalDays >= 1)
            {
                return $"{(int)timeSpan.TotalDays}日 {timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
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

        private string GetActionMessage(ActionType actionType)
        {
            return actionType switch
            {
                ActionType.Shutdown => "シャットダウンを実行します",
                ActionType.Restart => "再起動を実行します",
                ActionType.Sleep => "スリープを実行します",
                ActionType.Logoff => "ログオフを実行します",
                _ => "アクションを実行します"
            };
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
