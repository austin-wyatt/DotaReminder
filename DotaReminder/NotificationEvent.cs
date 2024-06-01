using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotaReminder
{
	public class NotificationEvent
	{
		public DateTime Timestamp { get; set; }
		public string Message { get; set; } = "";

		public NotificationEvent(DateTime timestamp, string message)
		{
			Timestamp = timestamp;
			Message = message;
		}

		public NotificationEvent(string message)
		{
			Timestamp = DateTime.Now;
			Message = message;
		}
	}
}
