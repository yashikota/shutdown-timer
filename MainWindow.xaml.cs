using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.Graphics;
using System;
using System.Threading.Tasks;
using shutdown_timer.ViewModels;
using shutdown_timer.Models;
using shutdown_timer.Services;

namespace shutdown_timer
{
	public sealed partial class MainWindow : Window
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

            SetupWindow();
            InitializeUI();
            SetupEventHandlers();
            LoadSavedSchedule();
        }

        private void SetupWindow()
        {
			SystemBackdrop = new MicaBackdrop();
            AppWindow.Resize(new SizeInt32(700, 900));
			AppWindow.SetIcon("Assets/timer.ico");
            Title = _localization.GetString("AppTitle");

            // Apply saved theme
            var settings = _settingsService.CurrentSettings;
            ApplyTheme(settings.Theme);
        }

        private void InitializeUI()
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
            StatusText.Text = _localization.GetString("Ready");



            // Localize action options
            ((RadioButton)ActionTypeRadio.Items[0]).Content = _localization.GetString("Shutdown");
            ((RadioButton)ActionTypeRadio.Items[1]).Content = _localization.GetString("Restart");
            ((RadioButton)ActionTypeRadio.Items[2]).Content = _localization.GetString("Sleep");
            ((RadioButton)ActionTypeRadio.Items[3]).Content = _localization.GetString("Logoff");

            ForceShutdownCheck.Content = _localization.GetString("ForceShutdown");

            // Update preview texts
            UpdateDurationPreview();
            UpdateTimePreview();
        }

        private void SetupEventHandlers()
        {
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private async void LoadSavedSchedule()
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

                    // Start the timer with saved config
                    await _viewModel.StartShutdownAsync(savedConfig);
                    UpdateUIForActiveTimer();
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = _localization.GetString("Error", ex.Message);
            }
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(_viewModel.StatusMessage):
                    StatusText.Text = _viewModel.StatusMessage;
                    break;
                case nameof(_viewModel.CountdownText):
                    CountdownDisplay.Text = _viewModel.CountdownText;
                    break;
                case nameof(_viewModel.CountdownVisible):
                    CountdownPanel.Visibility = _viewModel.CountdownVisible ? Visibility.Visible : Visibility.Collapsed;
                    break;
                case nameof(_viewModel.IsTimerActive):
                    UpdateUIForTimerState();
                    break;
            }
        }

        private void UpdateUIForTimerState()
        {
            if (_viewModel.IsTimerActive)
            {
                UpdateUIForActiveTimer();
            }
            else
            {
                UpdateUIForInactiveTimer();
            }
        }

        private void UpdateUIForActiveTimer()
        {
            StartButton.IsEnabled = false;
            CancelButton.IsEnabled = true;
            StartButtonIcon.Glyph = "\uE769"; // Pause icon
            StartButtonText.Text = _localization.GetString("Cancel");

            // Disable input controls
            ModeSelectorBar.IsEnabled = false;
            ActionTypeRadio.IsEnabled = false;
            ForceShutdownCheck.IsEnabled = false;
        }

        private void UpdateUIForInactiveTimer()
        {
            StartButton.IsEnabled = true;
            CancelButton.IsEnabled = false;
            StartButtonIcon.Glyph = "\uE768"; // Play icon
            StartButtonText.Text = _localization.GetString("Start");

            // Enable input controls
            ModeSelectorBar.IsEnabled = true;
            ActionTypeRadio.IsEnabled = true;
            ForceShutdownCheck.IsEnabled = true;
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
                DurationPreview.Text = _localization.GetString("SetTime");
				return;
			}

            var duration = TimeSpan.FromSeconds(totalSeconds);
            var targetTime = DateTime.Now.Add(duration);

            var durationText = FormatDuration(duration);
            var timeText = $"({targetTime:HH:mm})";

            if (_localization.CurrentLanguage == "ja")
            {
                DurationPreview.Text = $"{durationText}後 {timeText} にシャットダウンします";
            }
            else
            {
                DurationPreview.Text = _localization.GetString("WillShutdownIn", $"{durationText} {timeText}");
            }
        }

                private void UpdateTimePreview()
        {
            if (TimePreview == null || ShutdownTimePicker?.SelectedTime == null) return;

            var selectedTime = ShutdownTimePicker.SelectedTime.Value;
            var targetDateTime = DateTime.Today.Add(selectedTime);

            if (targetDateTime <= DateTime.Now)
            {
                targetDateTime = targetDateTime.AddDays(1);
                var timeText = $"{_localization.GetString("Tomorrow")} {selectedTime:hh\\:mm}";
                TimePreview.Text = _localization.GetString("WillShutdownAt", timeText);
            }
            else
            {
                var timeText = $"{_localization.GetString("Today")} {selectedTime:hh\\:mm}";
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
                    if (minutes > 0)
                    {
                        return $"{hours}時間{minutes}分";
                    }
                    else
                    {
                        return $"{hours}時間";
                    }
                }
                else
                {
                    var hourUnit = hours == 1 ? _localization.GetString("HourUnit") : _localization.GetString("HoursUnit");
                    var minuteUnit = minutes == 1 ? _localization.GetString("MinuteUnit") : _localization.GetString("MinutesUnit");

                    if (minutes > 0)
                    {
                        return $"{hours} {hourUnit} {minutes} {minuteUnit}";
                    }
                    else
                    {
                        return $"{hours} {hourUnit}";
                    }
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

                if (duration.TotalSeconds == 0)
                {
                    throw new InvalidOperationException("時間を設定してください");
                }

                return ShutdownConfig.CreateFromDuration(duration, actionType, forceAction);
            }
            else // Time mode
            {
                if (ShutdownTimePicker.SelectedTime == null)
                {
                    throw new InvalidOperationException("時刻を設定してください");
                }

                return ShutdownConfig.CreateFromTime(ShutdownTimePicker.SelectedTime.Value, actionType, forceAction);
            }
        }

        private async void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsDialog = new Dialogs.SettingsDialog();
            settingsDialog.XamlRoot = this.Content.XamlRoot;

            var result = await settingsDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // Settings were saved, apply changes
                ApplySettings(settingsDialog.Settings);
            }
        }

        private async void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            var aboutDialog = new Dialogs.AboutDialog();
            aboutDialog.XamlRoot = this.Content.XamlRoot;

            await aboutDialog.ShowAsync();
        }

        private void ApplySettings(AppSettings settings)
        {
            // Apply language change
            if (settings.Language != _localization.CurrentLanguage)
            {
                _localization.SetLanguage(settings.Language);
                InitializeUI(); // Re-initialize UI with new language
            }

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
                AppTheme.Default => ElementTheme.Default,
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
            if (ShutdownTimePicker != null)
            {
                // Store current value
                var currentTime = ShutdownTimePicker.SelectedTime;

                // Apply theme to TimePicker
                ShutdownTimePicker.RequestedTheme = theme;

                // Force refresh by temporarily changing visibility
                ShutdownTimePicker.Visibility = Visibility.Collapsed;
                ShutdownTimePicker.Visibility = Visibility.Visible;

                // Restore value
                ShutdownTimePicker.SelectedTime = currentTime;
            }

            // Also refresh NumberBox controls
            RefreshNumberBoxTheme(theme);
        }

                        private void RefreshNumberBoxTheme(ElementTheme theme)
        {
            // Store current values
            var hoursValue = HoursInput?.Value ?? 0;
            var minutesValue = MinutesInput?.Value ?? 0;
            var secondsValue = SecondsInput?.Value ?? 0;

            // Store headers
            var hoursHeader = HoursInput?.Header;
            var minutesHeader = MinutesInput?.Header;
            var secondsHeader = SecondsInput?.Header;

            // Use dispatcher to recreate NumberBoxes with proper theme
            this.DispatcherQueue.TryEnqueue(() =>
            {
                // Recreate HoursInput
                if (HoursInput != null)
                {
                    var parent = HoursInput.Parent as Panel;
                    var index = parent?.Children.IndexOf(HoursInput) ?? -1;
                    if (parent != null && index >= 0)
                    {
                        parent.Children.RemoveAt(index);
                        var newHoursInput = new NumberBox
                        {
                            Name = "HoursInput",
                            Header = hoursHeader,
                            Value = hoursValue,
                            Minimum = 0,
                            Maximum = 23,
                            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact,
                            RequestedTheme = theme,
                            Margin = new Thickness(0, 0, 8, 0)
                        };
                        Grid.SetColumn(newHoursInput, 0);
                        newHoursInput.ValueChanged += TimeInput_ValueChanged;
                        parent.Children.Insert(index, newHoursInput);
                        HoursInput = newHoursInput;
                    }
                }

                // Recreate MinutesInput
                if (MinutesInput != null)
                {
                    var parent = MinutesInput.Parent as Panel;
                    var index = parent?.Children.IndexOf(MinutesInput) ?? -1;
                    if (parent != null && index >= 0)
                    {
                        parent.Children.RemoveAt(index);
                        var newMinutesInput = new NumberBox
                        {
                            Name = "MinutesInput",
                            Header = minutesHeader,
                            Value = minutesValue,
                            Minimum = 0,
                            Maximum = 59,
                            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact,
                            RequestedTheme = theme,
                            Margin = new Thickness(8, 0, 8, 0)
                        };
                        Grid.SetColumn(newMinutesInput, 1);
                        newMinutesInput.ValueChanged += TimeInput_ValueChanged;
                        parent.Children.Insert(index, newMinutesInput);
                        MinutesInput = newMinutesInput;
                    }
                }

                // Recreate SecondsInput
                if (SecondsInput != null)
                {
                    var parent = SecondsInput.Parent as Panel;
                    var index = parent?.Children.IndexOf(SecondsInput) ?? -1;
                    if (parent != null && index >= 0)
                    {
                        parent.Children.RemoveAt(index);
                        var newSecondsInput = new NumberBox
                        {
                            Name = "SecondsInput",
                            Header = secondsHeader,
                            Value = secondsValue,
                            Minimum = 0,
                            Maximum = 59,
                            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact,
                            RequestedTheme = theme,
                            Margin = new Thickness(8, 0, 0, 0)
                        };
                        Grid.SetColumn(newSecondsInput, 2);
                        newSecondsInput.ValueChanged += TimeInput_ValueChanged;
                        parent.Children.Insert(index, newSecondsInput);
                        SecondsInput = newSecondsInput;
                    }
                }
            });
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
