using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Graphics;
using System;
using System.Threading.Tasks;
using shutdown_timer.ViewModels;
using shutdown_timer.Models;
using shutdown_timer.Services;

namespace shutdown_timer
{
	public sealed partial class MainWindow
    {
        private readonly MainViewModel _viewModel;
        private readonly LocalizationService _localization;
        private readonly SettingsService _settingsService;

		public MainWindow()
		{
			InitializeComponent();

            _viewModel = new MainViewModel();
            _localization = LocalizationService.Instance;
            _settingsService = SettingsService.Instance;

            // Apply saved language setting before UI initialization
            var settings = _settingsService.CurrentSettings;
            _localization.SetLanguage(settings.Language);

            SetupWindow();
            InitializeUi();
            SetupEventHandlers();

            // Load saved schedule after window is fully initialized
            this.Activated += MainWindow_Activated;
        }

        private async void MainWindow_Activated(object sender, WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState == WindowActivationState.Deactivated) return;
            // Only load once when first activated
            this.Activated -= MainWindow_Activated;
            await LoadSavedScheduleAsync();
        }

        private void SetupWindow()
        {
			SystemBackdrop = new MicaBackdrop();
            AppWindow.Resize(new SizeInt32(700, 850));
			AppWindow.SetIcon("Assets/timer.ico");
            Title = _localization.GetString("AppTitle");

            // Apply saved theme
            var settings = _settingsService.CurrentSettings;
            ApplyTheme(settings.Theme);
        }

        private void InitializeUi()
        {
            // Localize UI elements
            HeaderText.Text = _localization.GetString("AppTitle");
            DurationSelectorItem.Text = _localization.GetString("Duration");
            SpecificTimeSelectorItem.Text = _localization.GetString("SpecificTime");
            HoursInput.Header = _localization.GetString("Hours");
            MinutesInput.Header = _localization.GetString("Minutes");
            SecondsInput.Header = _localization.GetString("Seconds");
            ShutdownTimePicker.Header = _localization.GetString("ShutdownAt");
            StartButtonText.Text = _localization.GetString("Start");
            CancelButtonText.Text = _localization.GetString("Cancel");
            StatusText.Text = _localization.GetString("Ready");
            ActionOptionsHeader.Text = _localization.GetString("ShutdownOptions");



            // Localize action options
            ((RadioButton)ActionTypeRadio.Items[0]).Content = _localization.GetString("Shutdown");
            ((RadioButton)ActionTypeRadio.Items[1]).Content = _localization.GetString("Restart");

            ForceShutdownCheck.Content = _localization.GetString("ForceShutdown");

            // Update preview texts
            UpdateDurationPreview();
            UpdateTimePreview();
        }

        private void SetupEventHandlers()
        {
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private async Task LoadSavedScheduleAsync()
        {
            try
            {
                var shutdownService = new ShutdownService();
                var savedConfig = await shutdownService.LoadScheduleAsync();

                if (savedConfig != null)
                {
                    // Restore UI state based on saved config
                    if (savedConfig.Mode == TimerMode.Duration)
                    {
                        ModeSelectorBar.SelectedItem = DurationSelectorItem;
                        DurationModeContent.Visibility = Visibility.Visible;
                        SpecificTimeModeContent.Visibility = Visibility.Collapsed;
                        var duration = savedConfig.Duration;
                        HoursInput.Value = duration.Hours;
                        MinutesInput.Value = duration.Minutes;
                        SecondsInput.Value = duration.Seconds;
                    }
                    else
                    {
                        ModeSelectorBar.SelectedItem = SpecificTimeSelectorItem;
                        DurationModeContent.Visibility = Visibility.Collapsed;
                        SpecificTimeModeContent.Visibility = Visibility.Visible;
                        ShutdownTimePicker.SelectedTime = savedConfig.SpecificTime;
                    }

                    ActionTypeRadio.SelectedIndex = (int)savedConfig.ActionType;
                    ForceShutdownCheck.IsChecked = savedConfig.ForceAction;

                    // Check if the scheduled time is still in the future
                    if (savedConfig.TargetTime > DateTime.Now)
                    {
                        // Only restore the timer state without re-scheduling shutdown
                        _viewModel.RestoreTimerState(savedConfig);
                        UpdateUiForActiveTimer();
                    }
                    else
                    {
                        // Schedule has expired, clean up
                        await ShutdownService.CancelShutdownAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = _localization.GetString("Error", ex.Message);

                // If there's an error (like exit code 1190), try to cancel any existing shutdown
                try
                {
                    var shutdownService = new ShutdownService();
                    await ShutdownService.CancelShutdownAsync();
                }
                catch
                {
                    // Ignore cancel errors
                }
            }
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(_viewModel.StatusMessage):
                    StatusText.Text = _viewModel.StatusMessage;
                    break;
                case nameof(_viewModel.CountdownText):
                    CountdownDisplay.Text = _viewModel.CountdownText;
                    break;
                case nameof(_viewModel.TargetTimeText):
                    TargetTimeDisplay.Text = _viewModel.TargetTimeText;
                    break;
                case nameof(_viewModel.CountdownVisible):
                    CountdownPanel.Visibility = _viewModel.CountdownVisible ? Visibility.Visible : Visibility.Collapsed;
                    break;
                case nameof(_viewModel.IsTimerActive):
                    UpdateUiForTimerState();
                    break;
            }
        }

        private void UpdateUiForTimerState()
        {
            if (_viewModel.IsTimerActive)
            {
                UpdateUiForActiveTimer();
            }
            else
            {
                UpdateUiForInactiveTimer();
            }
        }

        private void UpdateUiForActiveTimer()
        {
            StartButton.IsEnabled = false;
            CancelButton.IsEnabled = true;
            StartButtonIcon.Glyph = "\uE768"; // Keep Play icon but disabled
            StartButtonText.Text = _localization.GetString("Start"); // Keep original text

            // Disable input controls
            ModeSelectorBar.IsEnabled = false;
            ActionTypeRadio.IsEnabled = false;
            ForceShutdownCheck.IsEnabled = false;

            // Disable time input controls
            HoursInput.IsEnabled = false;
            MinutesInput.IsEnabled = false;
            SecondsInput.IsEnabled = false;
            ShutdownTimePicker.IsEnabled = false;
        }

        private void UpdateUiForInactiveTimer()
        {
            StartButton.IsEnabled = true;
            CancelButton.IsEnabled = false;
            StartButtonIcon.Glyph = "\uE768"; // Play icon
            StartButtonText.Text = _localization.GetString("Start");

            // Enable input controls
            ModeSelectorBar.IsEnabled = true;
            ActionTypeRadio.IsEnabled = true;
            ForceShutdownCheck.IsEnabled = true;

            // Enable time input controls
            HoursInput.IsEnabled = true;
            MinutesInput.IsEnabled = true;
            SecondsInput.IsEnabled = true;
            ShutdownTimePicker.IsEnabled = true;
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
                var config = CreateShutdownConfig();
                await _viewModel.StartShutdownAsync(config);
            }
            catch (Exception ex)
            {
                StatusText.Text = _localization.GetString("Error", ex.Message);
            }
        }

        private async void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _viewModel.CancelShutdownAsync();
            }
            catch (Exception ex)
            {
                StatusText.Text = _localization.GetString("Error", ex.Message);
            }
        }



        private void TimeInput_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            UpdateDurationPreview();
        }

        private void ShutdownTimePicker_SelectedTimeChanged(TimePicker sender, TimePickerSelectedValueChangedEventArgs args)
        {
            UpdateTimePreview();
        }

                private void ModeSelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs e)
        {
            var isDurationMode = ModeSelectorBar.SelectedItem == DurationSelectorItem;

            DurationModeContent.Visibility = isDurationMode ? Visibility.Visible : Visibility.Collapsed;
            SpecificTimeModeContent.Visibility = isDurationMode ? Visibility.Collapsed : Visibility.Visible;

            if (isDurationMode)
            {
                UpdateDurationPreview();
            }
            else
            {
                UpdateTimePreview();
            }
        }

                private void UpdateDurationPreview()
        {
            if (DurationPreview == null) return;

            var hours = (int)(HoursInput?.Value ?? 0);
            var minutes = (int)(MinutesInput?.Value ?? 0);
            var seconds = (int)(SecondsInput?.Value ?? 0);

            var totalSeconds = hours * 3600 + minutes * 60 + seconds;
            if (totalSeconds == 0)
            {
                // Show immediate shutdown message for 0:0:0
                DurationPreview.Text = _localization.CurrentLanguage == "ja" ? "即座にシャットダウンします" : "Will shutdown immediately";
				return;
			}

            var duration = TimeSpan.FromSeconds(totalSeconds);
            var targetTime = DateTime.Now.Add(duration);

            var durationText = FormatDuration(duration);
            var timeText = $"({targetTime:HH:mm})";

            DurationPreview.Text = _localization.CurrentLanguage == "ja" ? $"{durationText}後 {timeText} にシャットダウンします" : _localization.GetString("WillShutdownIn", $"{durationText} {timeText}");
        }

                private void UpdateTimePreview()
        {
            if (TimePreview == null || ShutdownTimePicker?.SelectedTime == null || _localization == null) return;

            var selectedTime = ShutdownTimePicker.SelectedTime.Value;
            var targetDateTime = DateTime.Today.Add(selectedTime);

            if (targetDateTime <= DateTime.Now)
            {
                var timeText = $"{_localization.GetString("Tomorrow")} {selectedTime:h\\:mm}";
                TimePreview.Text = _localization.GetString("WillShutdownAt", timeText);
            }
            else
            {
                var timeText = $"{_localization.GetString("Today")} {selectedTime:h\\:mm}";
                TimePreview.Text = _localization.GetString("WillShutdownAt", timeText);
            }
        }

                private string FormatDuration(TimeSpan duration)
        {
            var isJapanese = _localization.CurrentLanguage == "ja";

            if (duration.TotalHours >= 1)
            {
                var hours = (int)duration.TotalHours;
                var minutes = duration.Minutes;

                if (isJapanese)
                {
                    return minutes > 0 ? $"{hours}時間{minutes}分" : $"{hours}時間";
                }
                else
                {
                    var hourUnit = hours == 1 ? _localization.GetString("HourUnit") : _localization.GetString("HoursUnit");
                    var minuteUnit = minutes == 1 ? _localization.GetString("MinuteUnit") : _localization.GetString("MinutesUnit");

                    return minutes > 0 ? $"{hours} {hourUnit} {minutes} {minuteUnit}" : $"{hours} {hourUnit}";
                }
            }
            else if (duration.TotalMinutes >= 1)
            {
                var minutes = (int)duration.TotalMinutes;

                if (isJapanese)
                {
                    return $"{minutes}分";
                }
                else
                {
                    var minuteUnit = minutes == 1 ? _localization.GetString("MinuteUnit") : _localization.GetString("MinutesUnit");
                    return $"{minutes} {minuteUnit}";
                }
			}
			else
			{
                var seconds = duration.Seconds;

                if (isJapanese)
                {
                    return $"{seconds}秒";
                }
                else
                {
                    var secondUnit = seconds == 1 ? _localization.GetString("SecondUnit") : _localization.GetString("SecondsUnit");
                    return $"{seconds} {secondUnit}";
                }
            }
        }

        private ShutdownConfig CreateShutdownConfig()
        {
            var actionType = (ActionType)ActionTypeRadio.SelectedIndex;
            var forceAction = ForceShutdownCheck.IsChecked ?? false;

            if (ModeSelectorBar.SelectedItem == DurationSelectorItem) // Duration mode
            {
                var hours = (int)(HoursInput.Value);
                var minutes = (int)(MinutesInput.Value);
                var seconds = (int)(SecondsInput.Value);
                var duration = new TimeSpan(hours, minutes, seconds);

                // Allow 0 seconds for immediate shutdown
                return ShutdownConfig.CreateFromDuration(duration, actionType, forceAction);
            }
            else // Time mode
            {
                return ShutdownTimePicker.SelectedTime == null ? throw new InvalidOperationException(_localization.GetString("SetTime")) : ShutdownConfig.CreateFromTime(ShutdownTimePicker.SelectedTime.Value, actionType, forceAction);
            }
        }

        private async void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsDialog = new Dialogs.SettingsDialog(OnSettingsChanged)
            {
                XamlRoot = this.Content.XamlRoot
            };

            await settingsDialog.ShowAsync();
        }

        private void OnSettingsChanged(AppSettings settings)
        {
            // Apply settings immediately when changed
            ApplySettings(settings);
        }

        private async void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            var aboutDialog = new Dialogs.AboutDialog
            {
                XamlRoot = this.Content.XamlRoot
            };

            await aboutDialog.ShowAsync();
        }

        private void ApplySettings(AppSettings settings)
        {
            // Always apply language change (force update)
            _localization.SetLanguage(settings.Language);
            InitializeUi(); // Re-initialize UI with new language

            // Apply theme change
            ApplyTheme(settings.Theme);

            // Update title if countdown in title is enabled
            if (settings.ShowCountdownInTitle && _viewModel.IsTimerActive)
            {
                UpdateTitleWithCountdown();
            }
            else
            {
                Title = _localization.GetString("AppTitle");
            }
        }

        private void ApplyTheme(AppTheme theme)
        {
            var elementTheme = theme switch
            {
                AppTheme.Light => ElementTheme.Light,
                AppTheme.Dark => ElementTheme.Dark,
                _ => ElementTheme.Default
            };

            if (Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = elementTheme;
            }

            // Force refresh TimePicker to apply theme correctly
            RefreshTimePickerTheme(elementTheme);
        }

                private void RefreshTimePickerTheme(ElementTheme theme)
        {
            // Recreate TimePicker with proper theme
            this.DispatcherQueue.TryEnqueue(() =>
            {
                RecreateTimePicker(theme);
            });

            // Also refresh NumberBox controls
            RefreshNumberBoxTheme(theme);
        }

        private void RecreateTimePicker(ElementTheme theme)
        {
            if (ShutdownTimePicker?.Parent is not Panel parent) return;

            // Store current values and position
            var currentTime = ShutdownTimePicker.SelectedTime;
            var currentHeader = ShutdownTimePicker.Header;
            var currentMargin = ShutdownTimePicker.Margin;
            var currentName = ShutdownTimePicker.Name;
            var currentHorizontalAlignment = ShutdownTimePicker.HorizontalAlignment;
            var currentClockIdentifier = ShutdownTimePicker.ClockIdentifier;
            var currentIndex = parent.Children.IndexOf(ShutdownTimePicker);

            // Remove old control
            parent.Children.Remove(ShutdownTimePicker);

            // Create new control with proper theme
            var newTimePicker = new TimePicker
            {
                Name = currentName,
                Header = currentHeader,
                SelectedTime = currentTime,
                RequestedTheme = theme,
                Margin = currentMargin,
                HorizontalAlignment = currentHorizontalAlignment,
                ClockIdentifier = currentClockIdentifier
            };

            // Add event handler
            newTimePicker.SelectedTimeChanged += ShutdownTimePicker_SelectedTimeChanged;

            // Insert at original position
            if (currentIndex >= 0 && currentIndex < parent.Children.Count)
            {
                parent.Children.Insert(currentIndex, newTimePicker);
            }
            else
            {
                parent.Children.Add(newTimePicker);
            }

            // Update reference
            ShutdownTimePicker = newTimePicker;
        }

        private void RefreshNumberBoxTheme(ElementTheme theme)
        {
            // Recreate NumberBox controls with proper theme
            this.DispatcherQueue.TryEnqueue(() =>
            {
                RecreateNumberBox(ref HoursInput, "HoursInput", 0, 23, 0, theme);
                RecreateNumberBox(ref MinutesInput, "MinutesInput", 0, 59, 1, theme);
                RecreateNumberBox(ref SecondsInput, "SecondsInput", 0, 59, 2, theme);
            });
        }

        private void RecreateNumberBox(ref NumberBox numberBox, string name, int min, int max, int gridColumn, ElementTheme theme)
        {
            if (numberBox.Parent is not Panel parent) return;
            // Store current values
            var currentValue = numberBox.Value;
            var currentHeader = numberBox.Header;
            var currentMargin = numberBox.Margin;

            // Remove old control
            parent.Children.Remove(numberBox);

            // Create new control with proper theme
            var newNumberBox = new NumberBox
            {
                Name = name,
                Header = currentHeader,
                Value = currentValue,
                Minimum = min,
                Maximum = max,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact,
                RequestedTheme = theme,
                Margin = currentMargin
            };

            // Set grid column
            Grid.SetColumn(newNumberBox, gridColumn);

            // Add event handler
            newNumberBox.ValueChanged += TimeInput_ValueChanged;

            // Add to parent
            parent.Children.Add(newNumberBox);

            // Update reference
            numberBox = newNumberBox;
        }

        private void UpdateTitleWithCountdown()
        {
            if (!string.IsNullOrEmpty(_viewModel.CountdownText))
            {
                Title = $"{_localization.GetString("AppTitle")} - {_viewModel.CountdownText}";
			}
		}
	}
}
