using System;
using System.Collections.Generic;
using System.Text;

namespace LogManager
{
	public static class Extensions
	{
		public static DateTime ToDateTime(this string dateString)
		{
			var values = dateString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			string[] datePart;
			string[] timePart;

			if (values.Length == 1)
			{
				if (values[0].Contains(':'))
				{
					timePart = values[0].Split(new char[] { ':' });
					return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, int.Parse(timePart[0]), int.Parse(timePart[1]), 0);
				}
				else if (values[0].Contains('-'))
				{
					datePart = values[0].Split(new char[] { '-' });
					return new DateTime(int.Parse(datePart[2]), int.Parse(datePart[1]), int.Parse(datePart[0]));
				}
			}
			else if (values.Length == 2)
			{
				datePart = values[1].Split(new char[] { '-' });
				timePart = values[2].Split(new char[] { ':' });
				return new DateTime(int.Parse(datePart[2]), int.Parse(datePart[1]), int.Parse(datePart[0]), int.Parse(timePart[0]), int.Parse(timePart[1]), 0);
			}

			throw new Exception($"datetime format for '{dateString}' is invalid");
		}
	}
}
