using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Windows.Graphics;
using System.Diagnostics;
using System;

namespace shutdown_timer
{
	public sealed partial class MainWindow : Window
	{
		bool isTimerStart = false;

		public MainWindow()
		{
			// Initialize the window
			this.InitializeComponent();

			// Set the window properties
			this.SystemBackdrop = new MicaBackdrop();
			this.AppWindow.Resize(new SizeInt32(800, 600));
			this.Title = "Shutdown Timer";
			this.AppWindow.SetIcon("Assets/timer.ico");
		}

		private void ShutdownButtonClick(object sender, RoutedEventArgs e)
		{
			if (isTimerStart)
			{
				isTimerStart = false;
				MessageText.Text = "Cancel Shutdown";

				var psi = new ProcessStartInfo()
				{
					FileName = "shutdown.exe",
					Arguments = "/a",
					UseShellExecute = false,
					CreateNoWindow = true
				};

				Process.Start(psi);
			}
			else
			{
				isTimerStart = true;
				TimeSpan shutdownTime = ShutdownTimePicker.Time;
				TimeSpan currentTime = DateTime.Now.TimeOfDay;

				int diffTime = (int)(shutdownTime - currentTime).TotalSeconds;
				if (diffTime < 0)
				{
					diffTime += (int)TimeSpan.FromHours(24).TotalSeconds;
				}
				MessageText.Text = diffTime.ToString();

				var psi = new ProcessStartInfo()
				{
					FileName = "shutdown.exe",
					Arguments = "/s /t " + diffTime,
					UseShellExecute = false,
					CreateNoWindow = true
				};

				Process.Start(psi);
			}
		}
	}
}
