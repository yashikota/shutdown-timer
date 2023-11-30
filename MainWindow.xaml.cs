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
			this.InitializeComponent();

			// 背景をMicaデザインに
			this.SystemBackdrop = new MicaBackdrop();

			// ウィンドウサイズの調整
			this.AppWindow.Resize(new SizeInt32(800, 600));

			// タイトルの変更
			this.Title = "Shutdown Timer";

			// アイコンの変更
			this.AppWindow.SetIcon("Assets/timer.ico"); 
		}
	}
}
