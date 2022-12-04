using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBeaver.Collections
{
	/// <summary>
	/// An object that stores information for a log entry
	/// </summary>
	public struct LogInfo
	{
		public LogType Type { get; }
		public string Message { get; }
		public DateTime Timestamp { get; }

		public LogInfo(LogType type, string message)
		{
			Type = type;
			Message = message;
			Timestamp = DateTime.Now;
		}
	}
}
