using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace DotaReminder
{
	/// <summary>
	/// Interaction logic for ReminderWindow.xaml
	/// </summary>
	public partial class ReminderWindow : Window
	{
		public ReminderWindow(Reminder reminder)
		{
			InitializeComponent();

			switch (reminder.Severity)
			{
				case ReminderSeverity.Minor:
					contentGrid.Background = new SolidColorBrush(Colors.Green);
					reminderLabel.FontSize = 48;
					break;
				case ReminderSeverity.Major:
					contentGrid.Background = new SolidColorBrush(Colors.Yellow);
					reminderLabel.FontSize = 58;
					break;
				case ReminderSeverity.Critical:
					contentGrid.Background = new SolidColorBrush(Colors.Red);
					reminderLabel.FontSize = 68;
					break;
			}

			reminderLabel.Text = reminder.Message;

			var screens = Monitors.GetScreens();

			var mousePos = GetMousePosition();

			var screen = screens.FirstOrDefault(s => s.TopX <= mousePos.X && s.TopX + s.Width >= mousePos.X && s.TopY <= mousePos.Y && s.TopY + s.Height >= mousePos.Y);

			reminderLabel.SizeChanged += (s, e) =>
			{
				using (var graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero))
				{
					var pixelWidth = (int)(reminderLabel.ActualWidth * graphics.DpiX / 96.0);
					var pixelHeight = (int)(reminderLabel.ActualHeight * graphics.DpiY / 96.0);

					Top = screen.TopY + screen.Height * 0.25 - pixelHeight / 2;
					Left = screen.TopX + screen.Width / 2 - pixelWidth / 2;
				}

			};

			//Top = screen.TopY + screen.Height * 0.25;
			//Left = screen.TopX + screen.Width / 2;
			Show();
		}

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			//Set the window style to noactivate.
			var helper = new WindowInteropHelper(this);
			SetWindowLong(helper.Handle, GWL_EXSTYLE,
				GetWindowLong(helper.Handle, GWL_EXSTYLE) | WS_EX_NOACTIVATE | WS_EX_TRANSPARENT);
		}

		const int GWL_EXSTYLE = -20;
		const int WS_EX_NOACTIVATE = 0x08000000;
		const int WS_EX_TRANSPARENT = 0x00000020;

		[DllImport("user32.dll")]
		public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

		[DllImport("user32.dll")]
		public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool GetCursorPos(ref Win32Point pt);

		[StructLayout(LayoutKind.Sequential)]
		internal struct Win32Point
		{
			public Int32 X;
			public Int32 Y;
		};
		public static Point GetMousePosition()
		{
			var w32Mouse = new Win32Point();
			GetCursorPos(ref w32Mouse);

			return new Point(w32Mouse.X, w32Mouse.Y);
		}
	}
}
