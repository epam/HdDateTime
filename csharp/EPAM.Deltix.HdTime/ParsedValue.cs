
namespace EPAM.Deltix.HdTime
{
	internal struct ParsedValue
	{
		public long x;         // Complete value without sign
		public sbyte sign;     // Sign mask (0 or -1)

		public int year;       // Year, always 4 digit integer
		public int month;      // Month of year, [0..11] !!
		public int day;        // Day of month. [1..31]

		public long GetTs()
		{
			long x = this.x;
			long mask = this.sign;
			return x + mask ^ mask;
		}

		public long GetDt()
		{
			return Convert.DateTime.From(year, month, day) + x;
		}

		public void ResetTs()
		{
			x = 0;
			sign = 0;
		}

		public void ResetDt()
		{
			x = 0;
			sign = 0;
			year = month = day = 1;
		}
	}
}
