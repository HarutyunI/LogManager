using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogManager
{
	public class LogMerger
	{
		private ConcurrentBag<LogEntry> _logs;
		private DateTime? _startTime = null;
		private DateTime? _endTime = null;

		public async Task RunAsync()
		{
			while (true)
			{
				try
				{
					Reset();
					Output("Input folder path");
					string folderPath = Input();

					folderPath = Path.GetFullPath(folderPath);
					if (!Directory.Exists(folderPath))
					{
						Output("directory doesn't exist");
						continue;
					}

					Output("full path: {0}", folderPath);

					var files = Directory.GetFiles(folderPath, "*.log.*", SearchOption.TopDirectoryOnly);

					Task[] tasks = new Task[files.Length];
					for (int i = 0; i < files.Length; i++)
					{
						int index = i;
						tasks[index] = Task.Run(() => ReadFile(files[index]));
					}

					await Task.WhenAll(tasks);
					string exportPath = Path.Combine(folderPath, $"MergedLog_{GetCurrentTimeString().Replace(':', '-')}.log");
					ExportResult(exportPath);
				}
				catch (Exception ex)
				{
					Output(ex.Message);
					Output(ex.StackTrace);
					Console.WriteLine();
					continue;
				}

				Console.WriteLine();
			}
		}

		private void ReadFile(string file)
		{
			string line;
			Output("{0} Started reading file {1}", GetCurrentTimeString(), file);
			using (var reader = new StreamReader(file))
			{
				StringBuilder pendingLine = new StringBuilder();
				int lineNumber = 0;
				while ((line = reader.ReadLine()) != null)
				{
					if (!string.IsNullOrWhiteSpace(line))
					{
						if (IsNewlogLine(line))
						{
							if (pendingLine.Length > 0)
							{
								_logs.Add(new LogEntry(pendingLine.ToString(), lineNumber++));
								pendingLine.Clear();
							}

							pendingLine.Append(line);
						}
						else
						{
							pendingLine.AppendLine();
							pendingLine.Append(line);
						}
					}
				}

				if (pendingLine.Length > 0)
				{
					_logs.Add(new LogEntry(pendingLine.ToString(), lineNumber++));
					pendingLine.Clear();
				}
			}

			Output("{0} Finished reading file {1}", GetCurrentTimeString(), file);
		}

		private void ExportResult(string exportPath)
		{
			Output("{0} started exporting result", GetCurrentTimeString());
			var logs = _logs.AsEnumerable();

			if (_startTime.HasValue)
			{
				logs = logs.Where(l => l.Time > _startTime.Value);
			}

			if (_endTime.HasValue)
			{
				logs = logs.Where(l => l.Time < _endTime.Value);
			}

			var orderedLogs = logs.OrderBy(l => l.Time).ThenBy(l => l.LineNumber).ToArray();

			Output("{0} Started exporting file", GetCurrentTimeString());
			using (var writer = new StreamWriter(exportPath, false, Encoding.UTF8))
			{
				foreach (var log in orderedLogs)
				{
					writer.WriteLine(log.LogValue);
				}
			}

			Output("{0} Finished exporting file", GetCurrentTimeString());
		}

		private void Reset()
		{
			_logs = new ConcurrentBag<LogEntry>();
			_startTime = null;
			_endTime = null;
			Output("input start time (dd-MM-yyyy hh:mm). leave empty for not filtering");
			string startDateString = Input();
			if (!string.IsNullOrWhiteSpace(startDateString))
			{
				_startTime = startDateString.ToDateTime();
			}

			Output("input end time (dd-MM-yyyy hh:mm). leave empty for not filtering");
			string endDateString = Input();
			if (!string.IsNullOrWhiteSpace(endDateString))
			{
				_endTime = endDateString.ToDateTime();
			}
		}

		private void Output(string s, params object[] args) => Console.WriteLine(s, args);

		private string Input() => Console.ReadLine();

		private string GetCurrentTimeString() => DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss.fff");

		private bool IsNewlogLine(string line)
		{
			var logLevels = new string[] { "DEBUG ", "INFO ", "WARN ", "ERROR ", "FATAL " };

			foreach (string logLevel in logLevels)
			{
				if (line.StartsWith(logLevel, StringComparison.InvariantCultureIgnoreCase))
				{
					var lineLog = line.Substring(logLevel.Length).TrimStart();
					if (lineLog.Length > 0 && char.IsDigit(lineLog[0]))
					{
						return true;
					}
				}
			}

			return false;
		}
	}
}
