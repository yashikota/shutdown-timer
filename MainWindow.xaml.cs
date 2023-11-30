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

			// �w�i��Mica�f�U�C����
			this.SystemBackdrop = new MicaBackdrop();

			// �E�B���h�E�T�C�Y�̒���
			this.AppWindow.Resize(new SizeInt32(800, 600));

			// �^�C�g���̕ύX
			this.Title = "Shutdown Timer";

			// �A�C�R���̕ύX
			this.AppWindow.SetIcon("Assets/timer.ico"); 
		}
	}
}
