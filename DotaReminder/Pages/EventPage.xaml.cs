using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DotaReminder.Pages
{
	/// <summary>
	/// Interaction logic for EventPage.xaml
	/// </summary>
	public partial class EventPage : Page
	{
		private MainWindow _window;

		public EventPage(MainWindow window)
		{
			_window = window;
			InitializeComponent();

			eventsGrid.ItemsSource = _window.NotificationEvents;

			_window.NotificationEvents.CollectionChanged += RefreshItems;
		}

		~EventPage()
		{
			_window.NotificationEvents.CollectionChanged -= RefreshItems;
		}

		private void RefreshItems(object? sender, NotifyCollectionChangedEventArgs e)
		{
			_window.Dispatcher.Invoke(() =>
			{
				eventsGrid.Items.Refresh();
			});
		}
	}
}
