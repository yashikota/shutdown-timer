using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.Graphics;

namespace shutdown_timer
{
	/// <summary>
	/// An empty window that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			// 背景をMicaデザインに
			SystemBackdrop = new MicaBackdrop();

			// ウィンドウサイズの調整
			IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
			WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
			AppWindow appWindow = AppWindow.GetFromWindowId(windowId);

			appWindow.Resize(new SizeInt32(800, 600));
		}
	}
}
