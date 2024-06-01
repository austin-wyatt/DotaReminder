using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotaReminder
{
	public enum ReminderSeverity
	{
		Minor,
		Major,
		Critical
	}

	public enum ReminderType
	{
		InGameReminder,
		OnMatchEnd
	}

	public class Reminder : INotifyPropertyChanged
	{
		private int _clockTime;
		private string _message;
		private ReminderSeverity _severity;
		private ReminderType _type;

		public int ClockTime
		{
			get => _clockTime;
			set
			{
				_clockTime = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ClockTime)));
			}
		}
		public string Message
		{
			get => _message;
			set
			{
				_message = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
			}
		}
		public ReminderSeverity Severity 
		{
			get => _severity;
			set
			{
				_severity = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Severity)));
			}
		}
		public ReminderType Type
		{
			get => _type;
			set
			{
				_type = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Type)));
			}
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		public Reminder()
		{
			_message = "";
		}

		public override bool Equals(object? obj)
		{
			return obj is Reminder reminder &&
				   ClockTime == reminder.ClockTime &&
				   Message == reminder.Message &&
				   Severity == reminder.Severity;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(ClockTime, Message, Severity);
		}
	}
}
