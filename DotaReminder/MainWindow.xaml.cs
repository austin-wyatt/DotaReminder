using Dota2GSI;
using Dota2GSI.Nodes;
using DotaReminder.Pages;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DotaReminder
{
	public enum PageType
	{
		Events, Reminders 
	}

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public ObservableCollection<NotificationEvent> NotificationEvents { get; set; } = new ObservableCollection<NotificationEvent>();
		public GameStateListener GameStateListener { get; set; }

		public ReminderManager ReminderManager { get; set; }

		private HashSet<DOTA_GameState> _gameInProgressStates = new HashSet<DOTA_GameState>()
		{
			DOTA_GameState.DOTA_GAMERULES_STATE_GAME_IN_PROGRESS,
			DOTA_GameState.DOTA_GAMERULES_STATE_PRE_GAME,
			DOTA_GameState.DOTA_GAMERULES_STATE_HERO_SELECTION,
			DOTA_GameState.DOTA_GAMERULES_STATE_STRATEGY_TIME,
			DOTA_GameState.DOTA_GAMERULES_STATE_TEAM_SHOWCASE,
			DOTA_GameState.DOTA_GAMERULES_STATE_WAIT_FOR_MAP_TO_LOAD,
			DOTA_GameState.DOTA_GAMERULES_STATE_WAIT_FOR_PLAYERS_TO_LOAD,
		};

		private DOTA_GameState _previousGameState = DOTA_GameState.Undefined;

		public PageType CurrentPage { get; private set; }

		public MainWindow()
		{
			ReminderManager = new ReminderManager(this);

			Icon = Bitmap2BitmapImage(Properties.Resources.icon);

			InitializeComponent();

			SetPage(PageType.Events);

			GameStateListener = new GameStateListener(3000);

			if (!GameStateListener.GenerateGSIConfigFile("DotaReminder"))
			{
				AddNotification(new NotificationEvent("Error generating GSI config file"));
			}

			GameStateListener.NewGameState += (game_state) =>
			{
				if(GameStateListener.Running == false)
				{
					return;
				}

				Trace.WriteLine(game_state.Map.ClockTime);

				Dispatch(() => {
					stateLabel.Text = game_state.Map.GameState == DOTA_GameState.Undefined ? "" : game_state.Map.GameState.ToString();
				});

				if (_gameInProgressStates.Contains(game_state.Map.GameState))
				{
					Dispatch(() => statusLabel.Text = "Connected to: " + game_state.Map.MatchID);
					if (!_gameInProgressStates.Contains(_previousGameState))
					{
						AddNotification(new NotificationEvent("Connected to: " + game_state.Map.MatchID));
					}

					var activatedReminders = ReminderManager.GameTimeUpdated(game_state.Map.MatchID, game_state.Map.ClockTime);

					if(activatedReminders.Count > 0)
					{
						AddNotification(new NotificationEvent($"{activatedReminders.Count} Reminder(s) activated at game time {activatedReminders[0].ClockTime}"));
					}
				}
				else
				{
					if (_gameInProgressStates.Contains(_previousGameState))
					{
						var activatedReminders = ReminderManager.GameEnded(game_state.Map.MatchID);

						if (activatedReminders.Count > 0)
						{
							AddNotification(new NotificationEvent($"{activatedReminders.Count} Reminder(s) activated after game."));
						}

						Dispatch(() => statusLabel.Text = "Waiting for game");

						AddNotification(new NotificationEvent("Disconnected"));
						ReminderManager.Reset();
					}
				}

				_previousGameState = game_state.Map.GameState;
			};

			if (!GameStateListener.Start())
			{
				statusLabel.Text = "Error starting listener";
				AddNotification(new NotificationEvent("Error starting listener"));
			}
			else
			{
				statusLabel.Text = "Waiting for game";
				AddNotification(new NotificationEvent("Waiting for game"));
			}
		}

		public void SetPage(PageType page)
		{
			CurrentPage = page;
			switch (page)
			{
				case PageType.Events:
					pageLabel.Text = "Events";
					pageContent.NavigationService.Navigate(new EventPage(this));
					break;
				case PageType.Reminders:
					pageLabel.Text = "Reminders";
					pageContent.NavigationService.Navigate(new ReminderPage(ReminderManager, this));
					break;
			}
		}

		private void Dispatch(Action action)
		{
			Dispatcher.Invoke(action);
		}

		private void GSIListenerStart_Click(object sender, RoutedEventArgs e)
		{
			if(!GameStateListener.Running)
			{
				if(!GameStateListener.Start())
				{
					statusLabel.Text = "Error starting listener";
				}
				else
				{
					statusLabel.Text = "Waiting for game";
				}
			}
		}

		private void GSIListenerStop_Click(object sender, RoutedEventArgs e)
		{
			if(GameStateListener.Running)
			{
				GameStateListener.Stop();
				statusLabel.Text = "Disconnected";
			}
		}

		private void EventsView_Click(object sender, RoutedEventArgs e)
		{
			SetPage(PageType.Events);
		}


		private void Reminders_Click(object sender, RoutedEventArgs e)
		{
			SetPage(PageType.Reminders);
		}

		public void AddNotification(NotificationEvent e){
			Dispatch(() => NotificationEvents.Add(e));
		}


		[System.Runtime.InteropServices.DllImport("gdi32.dll")]
		public static extern bool DeleteObject(IntPtr hObject);


		//Copied from: https://stackoverflow.com/a/6484754
		private ImageSource Bitmap2BitmapImage(Bitmap bitmap)
		{
			IntPtr hBitmap = bitmap.GetHbitmap();
			ImageSource retval;

			try
			{
				retval = Imaging.CreateBitmapSourceFromHBitmap(
							 hBitmap,
							 IntPtr.Zero,
							 Int32Rect.Empty,
							 BitmapSizeOptions.FromEmptyOptions());
			}
			finally
			{
				DeleteObject(hBitmap);
			}

			return retval;
		}
	}
}
