using NAudio.Wave;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DotaReminder
{
	public class ReminderManager
	{
		Mp3FileReader _minorNotificationStream = new Mp3FileReader(new MemoryStream(Properties.Resources.minor_notification));
		Mp3FileReader _majorNotificationStream = new Mp3FileReader(new MemoryStream(Properties.Resources.major_notification));
		Mp3FileReader _criticalNotificationStream = new Mp3FileReader(new MemoryStream(Properties.Resources.critical_notification));
		WaveOut _soundPlayer = new WaveOut();

		public Dictionary<long, HashSet<Reminder>> CompletedReminders = new Dictionary<long, HashSet<Reminder>>();
		public List<(DateTime, Reminder)> ActiveReminders = new List<(DateTime, Reminder)>();

		public ObservableCollection<Reminder> AllReminders = new ObservableCollection<Reminder>();

		private int _currentGroupIndex = 0;
		private List<(int, List<Reminder>)> _reminderGroups = new List<(int, List<Reminder>)>();

		private MainWindow _window;

		public float SoundVolume = 0.4f;

		public ReminderManager(MainWindow mainWindow)
		{
			_window = mainWindow;

			_soundPlayer.PlaybackStopped += (sender, e) =>
			{
				_majorNotificationStream.Position = 0;
				_minorNotificationStream.Position = 0;
				_criticalNotificationStream.Position = 0;
			};

			_soundPlayer.Volume = SoundVolume;
			
			try
			{
				var jsonStr = File.ReadAllText("./reminders.json");

				var remindersList = JsonConvert.DeserializeObject<List<Reminder>>(jsonStr);

				if (remindersList != null)
				{
					AllReminders = new ObservableCollection<Reminder>(remindersList);
				}
			}
			catch (Exception e)
			{
				_window.AddNotification(new NotificationEvent($"Failed to load reminders: {e.Message}"));
			}

			BuildReminderGroups();
		}

		private void ReminderActivated(long matchID, Reminder reminder)
		{
			if (!CompletedReminders.ContainsKey(matchID))
			{
				CompletedReminders[matchID] = new HashSet<Reminder>();
			}

			if (CompletedReminders[matchID].Contains(reminder) && reminder.Type != ReminderType.OnMatchEnd)
			{
				return;
			}

			CompletedReminders[matchID].Add(reminder);

			var newReminder = (DateTime.Now, reminder);
			ActiveReminders.Add(newReminder);

			switch(reminder.Severity)
			{
				case ReminderSeverity.Minor:
					_soundPlayer.Volume = SoundVolume;
					_soundPlayer.Init(_minorNotificationStream);
					break;
				case ReminderSeverity.Major:
					_soundPlayer.Init(_majorNotificationStream);
					_soundPlayer.Volume = SoundVolume * 1.25f;
					break;
				case ReminderSeverity.Critical:
					_soundPlayer.Init(_criticalNotificationStream);
					_soundPlayer.Volume = SoundVolume * 1.5f;
					break;
			}

			_soundPlayer.Play();

			ReminderWindow reminderWindow = null;

			_window.Dispatcher.Invoke(() =>
			{
				reminderWindow = new ReminderWindow(reminder);
				reminderWindow.Show();

			});
			

			Task.Run(async () =>
			{
				await Task.Delay(5000);

				_window.Dispatcher.Invoke(() =>
				{
					reminderWindow?.Close();
				});
				ActiveReminders.Remove(newReminder);
			});
		}

		private List<Reminder> _empty = new List<Reminder>();
		public List<Reminder> GameTimeUpdated(long matchID, int gameTime)
		{
			if(_currentGroupIndex >= _reminderGroups.Count)
			{
				return _empty;
			}

			var retList = _empty;

			var group = _reminderGroups[_currentGroupIndex];

			if (gameTime >= group.Item1)
			{
				var diff = gameTime - group.Item1;
				if (diff < 10 && diff >= 0)
				{
					retList = new List<Reminder>();

					foreach (var reminder in group.Item2)
					{
						if(reminder.Type == ReminderType.InGameReminder)
						{
							retList.Add(reminder);
							ReminderActivated(matchID, reminder);
						}
					}
				}

				_currentGroupIndex++;
			}

			return retList;
		}

		public List<Reminder> GameEnded(long matchID)
		{
			var retList = new List<Reminder>();

			foreach(var reminder in AllReminders)
			{
				if(reminder.Type == ReminderType.OnMatchEnd)
				{
					retList.Add(reminder);
					ReminderActivated(matchID, reminder);
				}
			}

			return retList;
		}

		public void Reset()
		{
			CompletedReminders.Clear();
			ActiveReminders.Clear();
			_currentGroupIndex = 0;
		}

		public void SetReminders(ObservableCollection<Reminder> reminders)
		{
			AllReminders = reminders;

			AllReminders.CollectionChanged += (sender, e) =>
			{
				BuildReminderGroups();
			};

			BuildReminderGroups();
		}

		public void BuildReminderGroups()
		{
			List<(int, List<Reminder>)> groups = new();

			foreach (var reminder in AllReminders)
			{
				var currGroupIndex = groups.FindIndex(r => r.Item1 == reminder.ClockTime);

				List<Reminder> groupList = null;

				if (currGroupIndex == -1)
				{
					groupList = new List<Reminder>() { reminder };
					groups.Add((reminder.ClockTime, groupList));
				}
				else
				{
					groupList = groups[currGroupIndex].Item2;
					groupList.Add(reminder);
				}
			}

			groups.Sort((a, b) => a.Item1.CompareTo(b.Item1));

			_reminderGroups = groups;
			_currentGroupIndex = 0;
		}

		public void SaveReminders()
		{
			var jsonStr = JsonConvert.SerializeObject(AllReminders);
			File.WriteAllText("reminders.json", jsonStr);
		}
	}
}
