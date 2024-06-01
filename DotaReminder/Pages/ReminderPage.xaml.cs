using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
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
	/// Interaction logic for ReminderPage.xaml
	/// </summary>
	public partial class ReminderPage : Page
	{
		private ReminderManager _manager;
		private MainWindow _window;

		public int SelectedIndex { get; set; }

		private List<Reminder> _currReminders = new List<Reminder>();


		public ReminderPage(ReminderManager reminderManager, MainWindow mainWindow)
		{
			_manager = reminderManager;
			_window = mainWindow;

			InitializeComponent();

			remindersGrid.ItemsSource = _manager.AllReminders;

			_manager.AllReminders.CollectionChanged += AllReminders_CollectionChanged;

			remindersGrid.Unloaded += DataGrid_Unloaded;
		}

		~ReminderPage()
		{
		}

		private void AllReminders_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			foreach (var reminder in _currReminders)
			{
				reminder.PropertyChanged -= ReminderPropertyChanged;
			}

			_currReminders = _manager.AllReminders.ToList();

			foreach (var reminder in _currReminders)
			{
				reminder.PropertyChanged += ReminderPropertyChanged;
			}
		}

		private void ReminderPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			_manager.BuildReminderGroups();
			saveReminders.Content = "Save Reminders*";
		}

		void DataGrid_Unloaded(object sender, RoutedEventArgs e)
		{
			var grid = (DataGrid)sender;
			grid.CommitEdit(DataGridEditingUnit.Row, true);
		}

		private void SaveReminders_Click(object sender, RoutedEventArgs e)
		{
			_manager.SaveReminders();
			saveReminders.Content = "Save Reminders";
		}
	}
}
