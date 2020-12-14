using System;

namespace LogManager
{
	public class LogEntry
	{
		public LogEntry(string logLine, int lineNumber)
		{
			LogValue = logLine;
			LineNumber = lineNumber;

			var values = logLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			var datePart = values[1].Split(new char[] { '-' });
			var timePart = values[2].Split(new char[] { ':' });
			Time = new DateTime(int.Parse(datePart[2]), int.Parse(datePart[1]), int.Parse(datePart[0]), int.Parse(timePart[0]), int.Parse(timePart[1]), int.Parse(timePart[2]));
		}

		public DateTime Time { get; private set; }

		public string LogValue { get; set; }

		public int LineNumber { get; set; }
	}
}
