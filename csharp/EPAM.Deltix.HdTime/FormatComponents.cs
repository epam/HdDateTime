
namespace EPAM.Deltix.HdTime
{
	internal struct FormatComponents
	{
		public int sign;     // Sign mask (0 or -1)

		public int year;       // Year, always 4 digit integer
		public int month;      // Month of year, [1..12]
		public int day;        // Day of month. [1..31]
		public int hour;       // Hour of day [0..24]
		public int minute;     // Minute of hour [0..59]
		public int second;     // Seconds of minute [0..59]
		public int nanosecond; // Nanosecond of second [0..999999999]
	}
}
