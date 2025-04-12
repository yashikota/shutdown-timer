using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Graphics;
using System.Diagnostics;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;

namespace shutdown_timer
{
	public sealed partial class MainWindow : Window
	{
		private bool _isTimerStart = false;
		private DispatcherTimer _countdownTimer;
		private DateTime _targetShutdownTime;
		private const string SCHEDULE_FILE = "shutdown_schedule.json";

		private class ScheduleData
		{
			public DateTime TargetTime { get; set; }
			public bool IsActive { get; set; }
		}

		public MainWindow()
		{
			// Initialize the window
			InitializeComponent();

			// Set the window properties
			SystemBackdrop = new MicaBackdrop();
			AppWindow.Resize(new SizeInt32(500, 600));
			Title = "Shutdown Timer";
			AppWindow.SetIcon("Assets/timer.ico");

			// Initialize timer
			_countdownTimer = new DispatcherTimer();
			_countdownTimer.Interval = TimeSpan.FromSeconds(1);
			_countdownTimer.Tick += CountdownTimer_Tick;

			// Setup mode selection change handler
			ModeSelector.SelectionChanged += ModeSelector_SelectionChanged;

			// Load saved schedule
			LoadSchedule();

			// Handle window closing
			this.Closed += MainWindow_Closed;
		}

		private void MainWindow_Closed(object sender, WindowEventArgs args)
		{
			if (_isTimerStart)
			{
				SaveSchedule();
			}
			else
			{
				// Remove schedule file if exists
				if (File.Exists(SCHEDULE_FILE))
				{
					File.Delete(SCHEDULE_FILE);
				}
			}
		}

		private void SaveSchedule()
		{
			var scheduleData = new ScheduleData
			{
				TargetTime = _targetShutdownTime,
				IsActive = _isTimerStart
			};

			var jsonString = JsonSerializer.Serialize(scheduleData);
			File.WriteAllText(SCHEDULE_FILE, jsonString);
		}

		private void LoadSchedule()
		{
			try
			{
				if (File.Exists(SCHEDULE_FILE))
				{
					var jsonString = File.ReadAllText(SCHEDULE_FILE);
					var scheduleData = JsonSerializer.Deserialize<ScheduleData>(jsonString);

					if (scheduleData.IsActive && scheduleData.TargetTime > DateTime.Now)
					{
						_targetShutdownTime = scheduleData.TargetTime;
						_isTimerStart = true;
						ButtonIcon.Glyph = "\uE947"; // Stop icon
						ButtonText.Text = "Cancel Shutdown";
						ShutdownButton.Style = Application.Current.Resources["DangerButtonStyle"] as Style;
						MessageText.Text = "Shutdown scheduled";
						CountdownPanel.Visibility = Visibility.Visible;
						_countdownTimer.Start();
					}
					else
					{
						// Remove invalid schedule
						File.Delete(SCHEDULE_FILE);
					}
				}
			}
			catch (Exception ex)
			{
				MessageText.Text = "Failed to load schedule";
				if (File.Exists(SCHEDULE_FILE))
				{
					File.Delete(SCHEDULE_FILE);
				}
			}
		}

		private void ModeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var isSpecificTimeMode = ModeSelector.SelectedIndex == 0;
			ShutdownTimePicker.Visibility = isSpecificTimeMode ? Visibility.Visible : Visibility.Collapsed;
			DurationPanel.Visibility = isSpecificTimeMode ? Visibility.Collapsed : Visibility.Visible;
		}

		private void CountdownTimer_Tick(object sender, object e)
		{
			var remainingTime = _targetShutdownTime - DateTime.Now;
			if (remainingTime.TotalSeconds <= 0)
			{
				_countdownTimer.Stop();
				CountdownDisplay.Visibility = Visibility.Collapsed;
				return;
			}

			CountdownDisplay.Text = $"Shutdown in: {remainingTime.Hours:D2}:{remainingTime.Minutes:D2}:{remainingTime.Seconds:D2}";
		}

		private void ShutdownButtonClick(object sender, RoutedEventArgs e)
		{
			if (_isTimerStart)
			{
				CancelShutdown();
			}
			else
			{
				StartShutdown();
			}
		}

		private void StartShutdown()
		{
			int secondsUntilShutdown;

			if (ModeSelector.SelectedIndex == 0) // Specific time mode
			{
				var shutdownTime = ShutdownTimePicker.Time;
				var currentTime = DateTime.Now.TimeOfDay;
				secondsUntilShutdown = (int)(shutdownTime - currentTime).TotalSeconds;

				if (secondsUntilShutdown < 0)
				{
					secondsUntilShutdown += (int)TimeSpan.FromHours(24).TotalSeconds;
				}

				_targetShutdownTime = DateTime.Now.AddSeconds(secondsUntilShutdown);
			}
			else // Duration mode
			{
				var hours = (int)HoursInput.Value;
				var minutes = (int)MinutesInput.Value;
				secondsUntilShutdown = (hours * 3600) + (minutes * 60);
				_targetShutdownTime = DateTime.Now.AddSeconds(secondsUntilShutdown);
			}

			var psi = new ProcessStartInfo()
			{
				FileName = "shutdown.exe",
				Arguments = "/s /t " + secondsUntilShutdown,
				UseShellExecute = false,
				CreateNoWindow = true
			};

			Process.Start(psi);

			_isTimerStart = true;
			ButtonIcon.Glyph = "\uE947"; // Stop icon
			ButtonText.Text = "Cancel Shutdown";
			ShutdownButton.Style = Application.Current.Resources["DangerButtonStyle"] as Style;
			MessageText.Text = "Shutdown scheduled";
			CountdownPanel.Visibility = Visibility.Visible;
			_countdownTimer.Start();

			// Save schedule
			SaveSchedule();
		}

		private void CancelShutdown()
		{
			var psi = new ProcessStartInfo()
			{
				FileName = "shutdown.exe",
				Arguments = "/a",
				UseShellExecute = false,
				CreateNoWindow = true
			};

			Process.Start(psi);

			_isTimerStart = false;
			ButtonIcon.Glyph = "\uE916"; // Timer icon
			ButtonText.Text = "Start Timer";
			ShutdownButton.Style = Application.Current.Resources["AccentButtonStyle"] as Style;
			MessageText.Text = "Shutdown cancelled";
			CountdownPanel.Visibility = Visibility.Collapsed;
			_countdownTimer.Stop();

			// Remove schedule file
			if (File.Exists(SCHEDULE_FILE))
			{
				File.Delete(SCHEDULE_FILE);
			}
		}
	}
}
