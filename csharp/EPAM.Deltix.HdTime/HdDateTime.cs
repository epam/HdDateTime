using System;

namespace EPAM.Deltix.HdTime
{
	/// <summary>
	/// <para/>Represents 64-bit structure holding high resolution UTC date/time, nanosecond interval relative to Unix epoch (00:00:00 UTC January 1, 1970).
	/// </summary>
	/// <remarks>
	/// <para/>Timestamp 'DateTime.MinValue' is 1678-01-01 00:00:00.000000000 UTC.
	/// <para/>Timestamp 'DateTime.MaxValue' is 2261-12-31 23:59:59.999999999 UTC.
	/// </remarks>
	[Serializable]
	public struct HdDateTime : IComparable, IComparable<HdDateTime>, IEquatable<HdDateTime>, System.IFormattable
	{
		#region Constants

		internal const long Min = -9214560000_000_000_000L;
		internal const long Max = 9214646400_000_000_000L - 1;
		internal const long NullValue = Int64.MinValue;  // Null value is not checked and is never returned by most methods

		/// <summary>
		/// <para/>Represents null/undefined <see cref="HdDateTime" /> This field is read-only.
		/// <para/>Most instance methods are not expected to behave correctly, but contain no checks due to performance considerations.
		/// TODO:
		/// </summary>
		static readonly HdDateTime Null = new HdDateTime(Max);

		/// <summary>
		/// <para/>Represents the largest possible value of <see cref="HdDateTime" /> (currently 9214646399999999999 nanoseconds). This field is read-only.
		/// <para/> 2261-12-31 23:59:59.999999999
		/// </summary>
		public static readonly HdDateTime MaxValue = new HdDateTime(Max);

		/// <summary>
		/// <para/>Represents the smallest possible value of <see cref="HdDateTime" /> (currently -9214560000000000000 nanoseconds). This field is read-only.
		/// <para/> 1678-01-01 00:00:00.000000000
		/// </summary>
		public static readonly HdDateTime MinValue = new HdDateTime(Min);

		public static readonly String DefaultFormat = "yyyy-MM-dd HH:mm:ss.fffffffff";

		#endregion

		private readonly Int64 _nanoseconds;

		#region Constructors
		/// <summary>
		/// <para/>Constructs instance of <see cref="HdDateTime" /> from <see cref="System.DateTime"/>.
		/// <para/>Ignores timezone information, does not convert local DateTime to UTC
		/// <para/><see cref="System.DateTime.MinValue"/> is converted to <see cref="HdDateTime.MinValue"/>
		/// <para/><see cref="System.DateTime.MaxValue"/> is converted to <see cref="HdDateTime.MaxValue"/>
		/// </summary>
		/// <param name="dateTime">.NET System.DateTime.</param>
		/// <exception cref="ArgumentOutOfRangeException"> The value of <paramref name="dateTime"/> is outside of the range representable by <see cref="HdDateTime"/>
		/// (less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/> after conversion).</exception>
		public HdDateTime(DateTime dateTime)
		{
			// TODO: Optimization opportunity
			_nanoseconds = dateTime == DateTime.MinValue ? Min : dateTime == DateTime.MaxValue ? Max
					: Convert.DateTime.FromTicks(dateTime.Ticks);
		}


		/// <summary>
		/// <para/>Constructs an instance of <see cref="HdDateTime" /> from <see cref="System.DateTime"/>,
		/// adding extra nanoseconds in order to obtain nanosecond precision.
		/// <para/>ignores timezone information, does not convert local DateTime to UTC
		/// <para/><see cref="System.DateTime.MinValue"/> is converted to <see cref="HdDateTime.MinValue"/> regardless of the value of <paramref name="extraNanos"/>
		/// <para/><see cref="System.DateTime.MaxValue"/> is converted to <see cref="HdDateTime.MaxValue"/> regardless of the value of <paramref name="extraNanos"/>
		/// <para/>therefore, if dateTime argument has one of these 2 values, extraNanos argument is ignored
		/// </summary>
		/// <param name="dateTime">.NET System.DateTime.</param>
		/// <param name="extraNanos">Extra nanoseconds to add. Should be in range [0..99]</param>
		/// <exception cref="ArgumentOutOfRangeException"> The value of <paramref name="dateTime"/> is outside of the range representable by <see cref="HdDateTime"/>
		/// (less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/> after conversion).
		/// </exception>
		/// <exception cref="OverflowException"> Adding <paramref name="extraNanos"/> caused arithmetic overflow</exception>
		public HdDateTime(DateTime dateTime, Byte extraNanos)
		{
			// TODO: Check for local time?
			_nanoseconds = DateTime.MinValue == dateTime ? Min : DateTime.MaxValue == dateTime	? Max
					: Convert.DateTime.FromTicks(dateTime.Ticks, extraNanos);
		}

		/// <summary>
		/// <para/>Constructs an instance of <see cref="HdDateTime" /> from nanoseconds.
		/// </summary>
		/// <param name="nanoseconds">Nanosecond timestamp, starting from Unix epoch (00:00:00 UTC January 1, 1970).</param>
		public HdDateTime(Int64 nanoseconds)
		{
			// TODO: Check range?
			_nanoseconds = nanoseconds;
		}

		/// <summary>
		/// <para/>Constructs instance of <see cref="HdDateTime" /> from year, month and day.
		/// </summary>
		/// <param name="year">The year (1678 through 2261).</param>
		/// <param name="month">The month (1 through 12).</param>
		/// <param name="day">The day (1 through the number of days in month).</param>
		/// <exception cref="ArgumentOutOfRangeException"> Any of the parameters is outside of the allowed range,
		/// or the resulting value is not representable as <see cref="HdDateTime"/>
		/// (less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/> after conversion).
		/// </exception>
		public HdDateTime(int year, int month, int day)
		{
			_nanoseconds = Convert.DateTime.From(year, month, day);
		}

		/// <summary>
		/// <para/>Constructs an instance of <see cref="HdDateTime" /> from year, month, day, hour, minute and second.
		/// </summary>
		/// <param name="year">The year (1678 through 2261).</param>
		/// <param name="month">The month (1 through 12).</param>
		/// <param name="day">The day (1 through the number of days in month).</param>
		/// <param name="hour">The hours (0 through 23).</param>
		/// <param name="minute"></param>
		/// <param name="second"></param>
		/// <exception cref="ArgumentOutOfRangeException"> Any of the parameters is outside of the allowed range,
		/// or the resulting value is not representable as <see cref="HdDateTime"/>
		/// (less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/> after conversion).
		/// </exception>
		public HdDateTime(int year, int month, int day, int hour, int minute, int second)
		{
			_nanoseconds = Convert.DateTime.From(year, month, day, hour, minute, second);
		}

		/// <summary>
		/// <para/>Constructs an instance of <see cref="HdDateTime" /> from year, month, day, hour, minute, second and millisecond.
		/// </summary>
		/// <param name="year">The year (1678 through 2261).</param>
		/// <param name="month">The month (1 through 12).</param>
		/// <param name="day">The day (1 through the number of days in month).</param>
		/// <param name="hour">The hours (0 through 23).</param>
		/// <param name="minute">The minutes (0 through 59).</param>
		/// <param name="second">The seconds (0 through 59).</param>
		/// <param name="nanosecond">The nanoseconds (0 through 999999999).</param>
		/// <exception cref="ArgumentOutOfRangeException"> Any of the parameters is outside of the allowed range,
		/// or the resulting value is not representable as <see cref="HdDateTime"/>
		/// (less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/> after conversion).
		/// </exception>
		public HdDateTime(int year, int month, int day, int hour, int minute, int second, int nanosecond)
		{
			// TODO: Test range checks?
			_nanoseconds = Convert.DateTime.From(year, month, day, hour, minute, second, nanosecond);
		}

		/// <summary>
		/// <para/>Constructs an instance of <see cref="HdDateTime" /> from Unix/Java nanosecond timestamp.
		/// <para/><see cref="Int64.MinValue"/> is converted to <see cref="MinValue"/>
		/// <para/><see cref="Int64.MaxValue"/> is converted to <see cref="MaxValue"/>
		/// </summary>
		/// <param name="nanoseconds">Timestamp with nanosecond resolution, relative to 1970-01-01 00:00:00 UTC</param>
		/// <exception cref="ArgumentOutOfRangeException"> The value of <paramref name="nanoseconds"/> is outside of the range representable by <see cref="HdDateTime"/>
		/// (less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>).
		/// </exception>
		public static HdDateTime FromEpochNanoseconds(Int64 nanoseconds) => new HdDateTime(Convert.DateTime.FromNanosWithMinMax(nanoseconds));

		/// <summary>
		/// <para/>Constructs an instance of <see cref="HdDateTime" /> from Unix/Java millisecond timestamp.
		/// <para/><see cref="Int64.MinValue"/> is converted to <see cref="MinValue"/>
		/// <para/><see cref="Int64.MaxValue"/> is converted to <see cref="MaxValue"/>
		/// </summary>
		/// <param name="milliseconds">Timestamp with millisecond resolution, relative to 1970-01-01 00:00:00 UTC</param>
		/// <exception cref="ArgumentOutOfRangeException"> The value of <paramref name="milliseconds"/> is outside of the range representable by <see cref="HdDateTime"/>
		/// (less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/> after conversion).
		/// </exception>
		public static HdDateTime FromEpochMilliseconds(Int64 milliseconds) => new HdDateTime(Convert.DateTime.FromMillisWithMinMax(milliseconds));

		[Obsolete("FromUnixMilliseconds is deprecated, please use FromEpochMilliseconds instead.")]
		public static HdDateTime FromUnixMilliseconds(Int64 milliseconds) => FromEpochMilliseconds(milliseconds);

		/// <summary>
		/// <para/>Constructs instance of <see cref="HdDateTime" /> from Unix/Java millisecond timestamp,
		/// adding extra nanoseconds in order to obtain nanosecond precision.
		/// <para/><see cref="Int64.MinValue"/> is converted to <see cref="MinValue"/> regardless of the value of <paramref name="extraNanos"/>
		/// <para/><see cref="Int64.MaxValue"/> is converted to <see cref="MaxValue"/> regardless of the value of <paramref name="extraNanos"/>
		/// </summary>
		/// <param name="milliseconds">Timestamp with millisecond resolution, relative to 1970-01-01 00:00:00 UTC</param>
		/// <param name="extraNanos">Extra nanoseconds to add. Usually expected to be in range [0..999999],
		/// but any value will be accepted as long as the addition does not cause arithmetic overflow.</param>
		/// <exception cref="ArgumentOutOfRangeException"> The value of <paramref name="milliseconds"/> is outside of the range representable by <see cref="HdDateTime"/>
		/// (less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/> after conversion).
		/// </exception>
		/// <exception cref="OverflowException"> Adding <paramref name="extraNanos"/> caused arithmetic overflow</exception>
		public static HdDateTime FromEpochMilliseconds(Int64 milliseconds, Int64 extraNanos)
		{
			return new HdDateTime(Convert.DateTime.FromMillisWithMinMax(milliseconds, extraNanos));
		}

		[Obsolete("FromUnixMilliseconds is deprecated, please use FromEpochMilliseconds instead.")]
		public static HdDateTime FromUnixMilliseconds(Int64 milliseconds, Int64 extraNanos)
		{
			return FromEpochMilliseconds(milliseconds, extraNanos);
		}

		/// <summary>
		/// <para/>Constructs an instance of <see cref="HdDateTime" /> from <see cref="System.DateTime"/> 100ns ticks
		/// <para/>Note that System.DateTime Ticks are relative to 0001-01-01 00:00:00 UTC or local time
		/// <para/>and HdDateTime.Nanoseconds are relative to 1970-01-01 00:00:00 UTC
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException"> The value of <paramref name="ticks"/> is outside of the range representable by <see cref="HdDateTime"/>
		/// (less than <see cref="HdDateTime.MinValue"/> or greater than <see cref="HdDateTime.MaxValue"/> after conversion).</exception>
		/// <param name="ticks">System.DateTime Ticks</param>
		public static HdDateTime FromTicks(Int64 ticks)
		{
			return new HdDateTime(Convert.DateTime.FromTicksWithMinMax(ticks));
		}

		public static HdDateTime Now => new HdDateTime(System.DateTime.UtcNow);
		public static HdDateTime Today => new HdDateTime(DateTime.UtcNow).Date;

		#endregion

		#region Conversion

		/// <summary>
		/// <para/>Gets the numeric value of this timestamp rounded down to milliseconds towards negative infinity,
		/// with 0 corresponding to the start of Unix epoch (00:00:00 UTC January 1, 1970).
		/// <para/><see cref="HdDateTime.MinValue"/> is converted to <see cref="Int64.MinValue"/>
		/// <para/><see cref="HdDateTime.MaxValue"/> is converted to <see cref="Int64.MaxValue"/>
		/// </summary>
		/// <remarks>
		/// <para/>This conversion loses precision. Use <see cref="UnixMillisecondsModulo" /> to retrieve the lost component
		/// ([0..999999] milliseconds), which can be later added back to restore the original precision
		/// </remarks>
		public Int64 EpochMilliseconds => Convert.DateTime.ToMillisWithMinMax(_nanoseconds);

		[Obsolete("UnixMilliseconds is deprecated, please use EpochMilliseconds instead.")]
		public Int64 UnixMilliseconds => EpochMilliseconds;


		/// <summary>
		/// <para/>Timestamp nanosecond component that is lost after converting to milliseconds
		/// (remainder after truncating to milliseconds towards negative infinity)
		/// <para/>Range: [0..999999]
		/// </summary>
		/// <remarks>
		/// <para/>See <see cref="UnixMilliseconds" />
		/// </remarks>
		internal Int64 EpochMillisecondsModulo => Convert.DateTime.ToMillisRemainder(_nanoseconds);

		[Obsolete("UnixMillisecondsModulo is deprecated, please use EpochMillisecondsModulo instead.")]
		internal Int64 UnixMillisecondsModulo => EpochMillisecondsModulo;

		/// <summary>
		/// <para/>The numeric value of this timestamp, as nanoseconds, starting from Unix epoch (00:00:00 UTC January 1, 1970).
		/// Can be negative if year &lt; 1970.
		/// </summary>
		public Int64 EpochNanoseconds => _nanoseconds;

		[Obsolete("Nanoseconds is deprecated, please use EpochNanoseconds instead.")]
		public Int64 Nanoseconds => EpochNanoseconds;


		/// <summary>
		/// <para/>Returns timestamp as .NET <see cref="System.DateTime"/>, rounding nanoseconds down to tick precision (100ns) towards negative infinity
		/// <para/><see cref="MinValue"/> is converted to <see cref="System.DateTime.MinValue"/>
		/// <para/><see cref="MaxValue"/> is converted to <see cref="System.DateTime.MaxValue"/>
		/// </summary>
		/// <remarks>
		/// <para/>This conversion loses precision. Use <see cref="TimestampModulo" /> to retrieve the lost component
		/// ([0..99] nanoseconds), which can be later added back to restore the original precision.
		/// </remarks>
		public DateTime DateTime
		{
			get
			{
				var nanos = _nanoseconds;
				// TODO: Optimization opportunity
				return nanos == Convert.DateTime.Min ? DateTime.MinValue : nanos == Convert.DateTime.Max ? DateTime.MaxValue
						: new DateTime(Convert.DateTime.ToTicksUnchecked(nanos), DateTimeKind.Utc);
			}
		}

		// DateTime without special case handling for MinValue and MaxValue
		internal DateTime RawDateTime => new DateTime(Convert.DateTime.ToTicksUnchecked(_nanoseconds), DateTimeKind.Utc);


		/// <summary>
		/// <para/>Timestamp nanosecond component that is lost after converting to 100ns ticks
		/// <para/>(remainder after truncating to ticks towards negative infinity)
		/// <para/>Range: [0..99]
		/// </summary>
		/// <remarks>
		/// <para/>See <see cref="DateTime" />
		/// </remarks>
		internal Byte TimestampModulo => Convert.DateTime.ToTicksRemainder(_nanoseconds);

		/// <summary>
		/// <para/>Gets the nanosecond of second component of the date represented by this instance.
		/// <para/>Range: [0..999999999]
		/// </summary>
		public int Nanosecond => Convert.DateTime.ExtractNanosecondOfSecond(_nanoseconds);

		/// <summary>
		/// <para/>Gets the microsecond of second component of the date represented by this instance.
		/// <para/>Range: [0..999999]
		/// </summary>
		public int Microsecond => Convert.DateTime.ExtractMicrosecondOfSecond(_nanoseconds);

		/// <summary>
		/// <para/>Gets the millisecond of second component of the date represented by this instance.
		/// <para/>Range: [0..999]
		/// </summary>
		public int Millisecond => Convert.DateTime.ExtractMillisecondOfSecond(_nanoseconds);

		/// <summary>
		/// <para/>Gets the second component of the date represented by this instance.
		/// <para/>Range: [0..59]
		/// </summary>
		public int Second => Convert.DateTime.ExtractSecondOfMinute(_nanoseconds);

		/// <summary>
		/// <para/>Gets the minute component of the date represented by this instance.
		/// <para/>Range: [0..59]
		/// </summary>
		public int Minute => Convert.DateTime.ExtractMinuteOfHour(_nanoseconds);

		/// <summary>
		/// Gets the hour component of the date represented by this instance.
		/// </summary>
		public int Hour => Convert.DateTime.ExtractHourOfDay(_nanoseconds);

		/// <summary>
		/// <para/>Gets the day component of the date represented by this instance.
		/// </summary>
		public int Day => ((DateTime)this).Day;

		/// <summary>
		/// Gets the month component of the date represented by this instance.
		/// </summary>
		public int Month => ((DateTime)this).Month;

		/// <summary>
		/// <para/>Gets the year component of the date represented by this instance.
		/// </summary>
		public int Year => ((DateTime)this).Year;

		/// <summary>
		/// <para/>Gets the day of the year represented by this instance.
		/// </summary>
		/// <returns>
		/// <para/>The day of the year, expressed as a value between 1 and 366.
		/// </returns>
		public int DayOfYear => ((DateTime)this).DayOfYear;

		/// <summary>
		/// <para/>Gets the day of the week represented by this instance.
		/// </summary>
		/// <returns>
		/// <para/>An enumerated constant that indicates the day of the week of this <see cref="HdDateTime"/> value.
		/// </returns>
		public DayOfWeek DayOfWeek => ((DateTime)this).DayOfWeek;

		/// <summary>
		/// <para/>Gets the time of day for this instance.
		/// </summary>
		/// <returns><para/>A time interval that represents the fraction of the day that has elapsed since
		///  midnight.</returns>
		public HdTimeSpan TimeOfDay => ((DateTime)this).TimeOfDay;

		/// <summary>
		/// <para/>Gets the date component of this instance.
		/// </summary>
		/// <returns>
		/// <para/>A new object with the same date as this instance, and the time value set to 00:00:00 (12:00:00 midnight).
		/// </returns>
		public HdDateTime Date => ((DateTime)this).Date;

		/// <summary>
		/// <para/>Converts <paramref name="hdDateTime" /> timestamp to .NET <see cref="System.DateTime"/>,
		/// rounding nanoseconds down to tick precision (100ns) towards negative infinity
		/// <para/><see cref="MinValue"/> is converted to <see cref="System.DateTime.MinValue"/>
		/// <para/><see cref="MaxValue"/> is converted to <see cref="System.DateTime.MaxValue"/>
		/// </summary>
		/// <remarks>
		/// <para/>This conversion loses precision. Use <see cref="TimestampModulo" /> to retrieve the lost component
		/// ([0..99] nanoseconds), which can be later added back to restore the original precision
		/// </remarks>
		public static explicit operator DateTime(HdDateTime hdDateTime)
		{
			return hdDateTime.DateTime;
		}

		/// <summary>
		/// <para/>Converts <see cref="System.DateTime"/> <paramref name="dateTime" /> timestamp to instance of <see cref="HdDateTime" />.
		/// <para/>ignores timezone information, does not convert local DateTime to UTC
		/// <para/><see cref="System.DateTime.MinValue"/> is converted to <see cref="HdDateTime.MinValue"/>
		/// <para/><see cref="System.DateTime.MaxValue"/> is converted to <see cref="HdDateTime.MaxValue"/>
		/// </summary>
		/// <param name="dateTime">Timestamp to convert.</param>
		/// <exception cref="ArgumentOutOfRangeException"> The value of <paramref name="dateTime"/> is outside of the range representable by <see cref="HdDateTime"/>
		/// (less than <see cref="HdDateTime.MinValue"/> or greater than <see cref="HdDateTime.MaxValue"/> after conversion).</exception>
		/// <returns>Instance of <see cref="HdDateTime"/> that contains nanoseconds, starting from Unix epoch (00:00:00 UTC on 1 January 1970).</returns>
		public static implicit operator HdDateTime(DateTime dateTime)
		{
			return new HdDateTime(dateTime);
		}

		#endregion

		#region Comparison
		/// <summary>
		/// <para/>Compares the values of <paramref name="left" /> and <paramref name="right" />.
		/// </summary>
		/// <param name="left">Left operand.</param>
		/// <param name="right">Right operand.</param>
		/// <returns>True, if the values of operands are equal; otherwise, false.</returns>
		public static Boolean operator ==(HdDateTime left, HdDateTime right)
		{
			return left._nanoseconds == right._nanoseconds;
		}


		/// <summary>
		/// <para/>Compares the values of <paramref name="left" /> and <paramref name="right" />.
		/// </summary>
		/// <param name="left">Left operand.</param>
		/// <param name="right">Right operand.</param>
		/// <returns>
		/// <para/>
		/// True, if the <paramref name="left"/> operand is greater than the <paramref name="right"/> operand; otherwise, false.
		/// </returns>
		public static Boolean operator >(HdDateTime left, HdDateTime right)
		{
			return left._nanoseconds > right._nanoseconds;
		}


		/// <summary>
		/// <para/>Compares the values of <paramref name="left" /> and <paramref name="right" />.
		/// </summary>
		/// <param name="left">Left operand.</param>
		/// <param name="right">Right operand.</param>
		/// <returns>True, if the <paramref name="left" /> operand is greater than or equal to the <paramref name="right" /> operand; otherwise, false.</returns>
		public static Boolean operator >=(HdDateTime left, HdDateTime right)
		{
			return left._nanoseconds >= right._nanoseconds;
		}


		/// <summary>
		/// <para/>Compares the values of <paramref name="left" /> and <paramref name="right" />.
		/// </summary>
		/// <param name="left">Left operand.</param>
		/// <param name="right">Right operand.</param>
		/// <returns>True, if the values of the operands are not equal; otherwise, false.</returns>
		public static Boolean operator !=(HdDateTime left, HdDateTime right)
		{
			return left._nanoseconds != right._nanoseconds;
		}


		/// <summary>
		/// <para/>Compares the values of <paramref name="left" /> and <paramref name="right" />.
		/// </summary>
		/// <param name="left">Left operand.</param>
		/// <param name="right">Right operand.</param>
		/// <returns>True, if the <paramref name="left" /> operand is less than the <paramref name="right" /> operand; otherwise, false.</returns>
		public static Boolean operator <(HdDateTime left, HdDateTime right)
		{
			return left._nanoseconds < right._nanoseconds;
		}


		/// <summary>
		/// <para/>Compares the values of <paramref name="left" /> and <paramref name="right" />.
		/// </summary>
		/// <param name="left">Left operand.</param>
		/// <param name="right">Right operand.</param>
		/// <returns>
		/// <para/>True, if the <paramref name="left" /> operand is less than or equal to the <paramref name="right" /> operand; otherwise, false.
		/// </returns>
		public static Boolean operator <=(HdDateTime left, HdDateTime right)
		{
			return left._nanoseconds <= right._nanoseconds;
		}

		/// <summary>
		/// <para/>Compares two <see cref="HdDateTime"/> values
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns>
		/// <para/>-1, if <paramref name="left" /> &lt; <paramref name="right" />
		/// <para/> 1, if <paramref name="left" /> &gt; <paramref name="right" />
		/// <para/> 0, if <paramref name="left" /> == <paramref name="right" />
		/// </returns>
		public static int Compare(HdDateTime left, HdDateTime right)
		{
			return left._nanoseconds > right._nanoseconds ? 1 : left._nanoseconds < right._nanoseconds ? -1 : 0;
		}

		#endregion

		#region Arithmetic and rounding

		HdDateTime RoundToU(Int64 resolution)
		{
			return new HdDateTime(Convert.TimeSpan.RoundTo(_nanoseconds, resolution));
		}

		/// <summary>
		/// Round to the specified resolution by truncating towards MinValue.
		/// </summary>
		/// <param name="resolution"></param>
		/// <returns>New instance of HdDateTime truncated to the specified resolution</returns>
		public HdDateTime RoundTo(HdTimeSpan resolution)
		{
			Int64 res = resolution._nanoseconds;
			if (res <= 0)
				throw new ArgumentException("resolution must be positive");

			return RoundToU(res);
		}

		/// <summary>
		/// Round to the specified resolution by truncating towards MinValue.
		/// </summary>
		/// <param name="resolution"></param>
		/// <returns>New instance of HdDateTime truncated to the specified resolution</returns>
		public HdDateTime RoundTo(Resolution resolution)
		{
			switch (resolution)
			{
				case Resolution.Day:
					return RoundToU(HdTimeSpan.NanosInDay);

				case Resolution.Hour:
					return RoundToU(HdTimeSpan.NanosInHour);

				case Resolution.Minute:
					return RoundToU(HdTimeSpan.NanosInMinute);

				case Resolution.Second:
					return RoundToU(HdTimeSpan.NanosInSecond);

				case Resolution.Millisecond:
					return RoundToU(HdTimeSpan.NanosInMillisecond);

				case Resolution.Microsecond:
					return RoundToU(HdTimeSpan.NanosInMicrosecond);

				case Resolution.Tick:
					return RoundToU(HdTimeSpan.NanosInTick);

				case Resolution.Nanosecond:
					return this;
			}

			throw new ArgumentException("Unsupported resolution: " + resolution);
		}

		/// <summary>
		/// <para/>Adds a specified time interval to a specified date and time, yielding a new HdDateTime
		/// </summary>
		/// <param name="left">The date and time value to add.</param>
		/// <param name="right">The time interval to add.</param>
		/// <returns>
		/// <para/>An object that is the sum of the values of <paramref name="left" /> and <paramref name="right" />.
		/// </returns>
		public static HdDateTime operator +(HdDateTime left, HdTimeSpan right)
		{
			return new HdDateTime(Util.AddToDt( left._nanoseconds, right._nanoseconds));
		}

		/// <summary>
		/// <para/>Adds a specified time interval to a specified date and time, yielding a new HdDateTime.
		/// </summary>
		/// <param name="other">The time interval to add.</param>
		/// <returns>
		/// <para/>An object that is the sum of the values of this HdDateTime object and <paramref name="other" />.
		/// </returns>
		public HdDateTime Add(HdTimeSpan other)
		{
			return new HdDateTime(Util.AddToDt(_nanoseconds, other._nanoseconds));
		}

		/// <summary>
		/// <para/>Adds a specified time interval to a specified date and time, yielding a new HdDateTime. No range/overflow checks performed.
		/// </summary>
		/// <param name="other">The time interval to add.</param>
		/// <returns>
		/// <para/>An object that is the sum of the values of this HdDateTime object and <paramref name="other" />.
		/// </returns>
		public HdDateTime AddUnchecked(HdTimeSpan other)
		{
			return new HdDateTime(unchecked(_nanoseconds + other._nanoseconds));
		}


		/// <summary>
		/// <para/>Subtracts a specified time interval from a specified date and time and returns new HdDateTime.
		/// </summary>
		/// <param name="left">The date and time value.</param>
		/// <param name="right">The time interval to subtract.</param>
		/// <returns>
		/// <para/>An object whose value is the value of <paramref name="left" /> minus the value of <paramref name="right" />.
		/// </returns>
		public static HdDateTime operator -(HdDateTime left, HdTimeSpan right)
		{
			return new HdDateTime(Util.SubtractToDt(left._nanoseconds, right._nanoseconds));
		}


		/// <summary>
		/// <para/>Subtracts a specified time interval from a specified date and time and returns new HdDateTime.
		/// </summary>
		/// <param name="other">The time interval to subtract from this instance's value.</param>
		/// <returns>
		/// <para/>An object whose value is the value of this HdDateTime object minus the value of <paramref name="other" />.
		/// </returns>
		public HdDateTime Subtract(HdTimeSpan other)
		{
			return new HdDateTime(Util.SubtractToDt(_nanoseconds, other._nanoseconds));
		}

		/// <summary>
		/// <para/>Subtracts a specified time interval from a specified date and time and returns new HdDateTime. No range/overflow checks performed.
		/// </summary>
		/// <param name="other">The time interval to subtract from this instance's value.</param>
		/// <returns>
		/// <para/>An object whose value is the value of this HdDateTime object minus the value of <paramref name="other" />.
		/// </returns>
		public HdDateTime SubtractUnchecked(HdTimeSpan other)
		{
			return new HdDateTime(unchecked(_nanoseconds - other._nanoseconds));
		}


		/// <summary>
		/// <para/>Subtracts <paramref name="other"/> value from this instance returning a new time interval.
		/// </summary>
		/// <param name="other">The date and time value to subtract from this instance's value.</param>
		/// <returns>
		/// <para/>The time interval between this <see cref="HdDateTime"/> instance and <paramref name="other" />
		/// </returns>
		public HdTimeSpan Subtract(HdDateTime other)
		{
			return new HdTimeSpan(Util.SubtractToTs(_nanoseconds, other._nanoseconds));
		}

		/// <summary>
		/// <para/>Subtracts <paramref name="other"/> value from this instance returning a new time interval. No overflow checks.
		/// </summary>
		/// <param name="other">The date and time value to subtract from this instance's value.</param>
		/// <returns>
		/// <para/>The time interval between this <see cref="HdDateTime"/> instance and <paramref name="other" />
		/// </returns>
		public HdTimeSpan SubtractUnchecked(HdDateTime other)
		{
			return new HdTimeSpan(unchecked(_nanoseconds - other._nanoseconds));
		}


		/// <summary>
		/// <para/>Subtracts right <see cref="HdDateTime"/> value from the left <see cref="HdDateTime"/> returning a new time interval.
		/// </summary>
		/// <param name="left">The date and time value to subtract (the minuend).</param>
		/// <param name="right">The date and time value to subtract (the subtrahend).</param>
		/// <returns>
		/// <para/>The time interval between <paramref name="left" /> and <paramref name="right" />;
		/// that is <paramref name="left" /> minus <paramref name="right" />.
		/// </returns>
		public static HdTimeSpan operator -(HdDateTime left, HdDateTime right)
		{
			return new HdTimeSpan(Util.SubtractToTs(left._nanoseconds, right._nanoseconds));
		}

		/// <summary>
		/// <para/>Returns a new <see cref="HdDateTime"/> that adds the specified amount of months to the value of this instance
		/// </summary>
		/// <param name="months">A number of months. Can be positive or negative.</param>
		/// <exception cref="ArgumentOutOfRangeException">The added value is beyond the allowed range,
		/// or the resulting value is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.</exception>
		/// <returns>
		/// <para/>New instance whose value is the sum of the date and time of this instance
		/// and the number of months represented by <paramref name="months" />
		/// </returns>
		public HdDateTime AddMonths(int months)
		{
			Byte remainder;
			Int64 dtTicks = new DateTime(Convert.DateTime.ToTicksUnchecked(_nanoseconds, out remainder))
				.AddMonths(months).Ticks;

			return new HdDateTime(Convert.DateTime.FromTicks(dtTicks, remainder));
		}

		/// <summary>
		/// <para/>Returns a new <see cref="HdDateTime"/> that adds the specified amount of years to the value of this instance
		/// </summary>
		/// <param name="years">A number of years. Can be positive or negative.</param>
		/// <exception cref="ArgumentOutOfRangeException">The added value is beyond the allowed range, or the resulting value is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.</exception>
		/// <returns>
		/// <para/>New instance whose value is the sum of the date and time of this instance
		/// and the number of years represented by <paramref name="years" />
		/// </returns>
		public HdDateTime AddYears(int years)
		{
			Byte remainder;
			Int64 dtTicks = new DateTime(Convert.DateTime.ToTicksUnchecked(_nanoseconds, out remainder))
				.AddYears(years).Ticks;

			return new HdDateTime(Convert.DateTime.FromTicks(dtTicks, remainder));
		}

		/// <summary>
		/// <para/>Returns a new <see cref="HdDateTime"/> that adds the specified amount of days to the value of this instance
		/// </summary>
		/// <param name="days">A number of days. Can be positive or negative.</param>
		/// <exception cref="ArgumentOutOfRangeException">The added value is beyond range representable as <see cref="HdTimeSpan"/></exception>
		/// <exception cref="OverflowException">The resulting <see cref="HdDateTime"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.</exception>
		/// <returns>
		/// <para/>New instance whose value is the sum of the date and time of this instance
		/// and the number of days represented by <paramref name="days" />
		/// </returns>
		public HdDateTime AddDays(Int64 days)
		{
			return Add(HdTimeSpan.FromDays(days));
		}

		/// <summary>
		/// <para/>Returns a new <see cref="HdDateTime"/> that adds the specified amount of hours to the value of this instance
		/// </summary>
		/// <param name="hours">A number of hours. Can be positive or negative.</param>
		/// <exception cref="ArgumentOutOfRangeException">The added value is beyond range representable as <see cref="HdTimeSpan"/></exception>
		/// <exception cref="OverflowException">The resulting <see cref="HdDateTime"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.</exception>
		/// <returns>
		/// <para/>New instance whose value is the sum of the date and time of this instance
		/// and the number of hours represented by <paramref name="hours" />
		/// </returns>
		public HdDateTime AddHours(Int64 hours)
		{
			return Add(HdTimeSpan.FromHours(hours));
		}

		/// <summary>
		/// <para/>Returns a new <see cref="HdDateTime"/> that adds the specified amount of minutes to the value of this instance
		/// </summary>
		/// <param name="minutes">A number of minutes. Can be positive or negative.</param>
		/// <exception cref="ArgumentOutOfRangeException">The added value is beyond range representable as <see cref="HdTimeSpan"/></exception>
		/// <exception cref="OverflowException">The resulting <see cref="HdDateTime"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.</exception>
		/// <returns>
		/// <para/>New instance whose value is the sum of the date and time of this instance
		/// and the number of minutes represented by <paramref name="minutes" />
		/// </returns>
		public HdDateTime AddMinutes(Int64 minutes)
		{
			return Add(HdTimeSpan.FromMinutes(minutes));
		}

		/// <summary>
		/// <para/>Returns a new <see cref="HdDateTime"/> that adds the specified amount of seconds to the value of this instance
		/// </summary>
		/// <param name="seconds">A number of seconds. Can be positive or negative.</param>
		/// <exception cref="ArgumentOutOfRangeException">The added value is beyond range representable as <see cref="HdTimeSpan"/></exception>
		/// <exception cref="OverflowException">The resulting <see cref="HdDateTime"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.</exception>
		/// <returns>
		/// <para/>New instance whose value is the sum of the date and time of this instance
		/// and the number of seconds represented by <paramref name="seconds" />
		/// </returns>
		public HdDateTime AddSeconds(Int64 seconds)
		{
			return Add(HdTimeSpan.FromSeconds(seconds));
		}

		/// <summary>
		/// <para/>Returns a new <see cref="HdDateTime"/> that adds the specified amount of milliseconds to the value of this instance
		/// </summary>
		/// <param name="milliseconds">A number of milliseconds. Can be positive or negative.</param>
		/// <exception cref="ArgumentOutOfRangeException">The added value is beyond range representable as <see cref="HdTimeSpan"/></exception>
		/// <exception cref="OverflowException">The resulting <see cref="HdDateTime"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.</exception>
		/// <returns>
		/// <para/>New instance whose value is the sum of the date and time of this instance
		/// and the number of milliseconds represented by <paramref name="milliseconds" />
		/// </returns>
		public HdDateTime AddMilliseconds(Int64 milliseconds)
		{
			return Add(HdTimeSpan.FromMilliseconds(milliseconds));
		}

		/// <summary>
		/// <para/>Returns a new <see cref="HdDateTime"/> that adds the specified amount of 100ns ticks to the value of this instance
		/// </summary>
		/// <param name="ticks">A number of 100ns .NET 'ticks'. Can be positive or negative.</param>
		/// <exception cref="ArgumentOutOfRangeException">The added value is beyond range representable as <see cref="HdTimeSpan"/></exception>
		/// <exception cref="OverflowException">The resulting <see cref="HdDateTime"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.</exception>
		/// <returns>
		/// <para/>New instance whose value is the sum of the date and time of this instance
		/// and the number of ticks represented by <paramref name="ticks" />
		/// </returns>
		public HdDateTime AddTicks(Int64 ticks)
		{
			return Add(HdTimeSpan.FromTicks(ticks));
		}

		/// <summary>
		/// <para/>Returns a new <see cref="HdDateTime"/> that adds the specified amount of nanoseconds to the value of this instance
		/// </summary>
		/// <param name="nanos">A number of nanoseconds. Can be positive or negative.</param>
		/// <exception cref="OverflowException">The resulting <see cref="HdDateTime"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.</exception>
		/// <returns>
		/// <para/>New instance whose value is the sum of the date and time of this instance
		/// and the number of nanoseconds represented by <paramref name="nanos" />
		/// </returns>
		public HdDateTime AddNanoseconds(Int64 nanos)
		{
			return new HdDateTime(Util.AddToDt(_nanoseconds, nanos));
		}

		/// <summary>
		/// <para/>Returns a new <see cref="HdDateTime"/> that adds the specified amount of days to the value of this instance
		/// </summary>
		/// <param name="days">A number of days. Can be positive or negative.</param>
		/// <exception cref="ArgumentOutOfRangeException">The added value is beyond range representable as <see cref="HdTimeSpan"/></exception>
		/// <exception cref="OverflowException">The resulting <see cref="HdDateTime"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.</exception>
		/// <returns>
		/// <para/>New instance whose value is the sum of the date and time of this instance
		/// and the number of days represented by <paramref name="days" />
		/// </returns>
		public HdDateTime AddDays(int days)
		{
			return Add(HdTimeSpan.FromDays(days));
		}

		/// <summary>
		/// <para/>Returns a new <see cref="HdDateTime"/> that adds the specified amount of hours to the value of this instance
		/// </summary>
		/// <param name="hours">A number of hours. Can be positive or negative.</param>
		/// <exception cref="ArgumentOutOfRangeException">The added value is beyond range representable as <see cref="HdTimeSpan"/></exception>
		/// <exception cref="OverflowException">The resulting <see cref="HdDateTime"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.</exception>
		/// <returns>
		/// <para/>New instance whose value is the sum of the date and time of this instance
		/// and the number of hours represented by <paramref name="hours" />
		/// </returns>
		public HdDateTime AddHours(int hours)
		{
			return Add(HdTimeSpan.FromHours(hours));
		}

		/// <summary>
		/// <para/>Returns a new <see cref="HdDateTime"/> that adds the specified amount of minutes to the value of this instance
		/// </summary>
		/// <param name="minutes">A number of minutes. Can be positive or negative.</param>
		/// <exception cref="ArgumentOutOfRangeException">The added value is beyond range representable as <see cref="HdTimeSpan"/></exception>
		/// <exception cref="OverflowException">The resulting <see cref="HdDateTime"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.</exception>
		/// <returns>
		/// <para/>New instance whose value is the sum of the date and time of this instance
		/// and the number of minutes represented by <paramref name="minutes" />
		/// </returns>
		public HdDateTime AddMinutes(int minutes)
		{
			return Add(HdTimeSpan.FromMinutes(minutes));
		}

		/// <summary>
		/// <para/>Returns a new <see cref="HdDateTime"/> that adds the specified amount of seconds to the value of this instance
		/// </summary>
		/// <param name="seconds">A number of seconds. Can be positive or negative.</param>
		/// <exception cref="OverflowException">The resulting <see cref="HdDateTime"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.</exception>
		/// <returns>
		/// <para/>New instance whose value is the sum of the date and time of this instance
		/// and the number of seconds represented by <paramref name="seconds" />
		/// </returns>
		public HdDateTime AddSeconds(int seconds)
		{
			return Add(HdTimeSpan.FromSeconds(seconds));
		}

		/// <summary>
		/// <para/>Returns a new <see cref="HdDateTime"/> that adds the specified amount of milliseconds to the value of this instance
		/// </summary>
		/// <param name="milliseconds">A number of milliseconds. Can be positive or negative.</param>
		/// <exception cref="OverflowException">The resulting <see cref="HdDateTime"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.</exception>
		/// <returns>
		/// <para/>New instance whose value is the sum of the date and time of this instance
		/// and the number of milliseconds represented by <paramref name="milliseconds" />
		/// </returns>
		public HdDateTime AddMilliseconds(int milliseconds)
		{
			return Add(HdTimeSpan.FromMilliseconds(milliseconds));
		}

		/// <summary>
		/// <para/>Returns a new <see cref="HdDateTime"/> that adds the specified amount of 100ns ticks to the value of this instance
		/// </summary>
		/// <param name="ticks">A number of 100ns .NET 'ticks'. Can be positive or negative.</param>
		/// <exception cref="OverflowException">The resulting <see cref="HdDateTime"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.</exception>
		/// <returns>
		/// <para/>New instance whose value is the sum of the date and time of this instance
		/// and the number of ticks represented by <paramref name="ticks" />
		/// </returns>
		public HdDateTime AddTicks(int ticks)
		{
			return Add(HdTimeSpan.FromTicks(ticks));
		}


		#endregion

		#region Helpers

		public int getDaysInMonth()
		{
			return DateTime.DaysInMonth(Year, Month);
		}

		public int getDaysInYear()
		{
			return 365 + (IsLeapYear() ? 1 : 0);
		}

		public bool IsLeapYear()
		{
			return DateTime.IsLeapYear(Year);
		}

		public bool IsLeapYear(int year)
		{
			return DateTime.IsLeapYear(year);
		}

		#endregion

		#region Parsing and Formatting

		/// <summary>
		///   Converts value of <see cref="HdDateTime" /> into its equivalent string representation using the specified format.
		/// </summary>
		/// <param name="format">A standard or custom <see cref="HdDateTime" /> format. (See Remarks)</param>
		/// <returns>String representation of the current instance.</returns>
		/// <remarks>
		///   <para>
		///     A date and time format, which can be the same as <see cref="System.DateTime" /> format. If you want to specify
		///     nanoseconds formatting, you can use "FFFFFFFFF" (9 Fs) or "fffffffff" (9 fs).
		///   </para>
		///   <para>
		///     Formats "d", "ddd", "dddd", "g", "gg", "K", "m", "M", "s", "t", "y", "yyy", "z", "zz", "zzz", duplicate upper case
		///     formats as "FFF FFFFFFFFF" are unsupported.
		///     Formats, which contains nanoseconds format not in the end of the format string are also unsupported.
		///   </para>
		/// </remarks>
		public String ToString(String format)
		{
			return Formatters.DateTime.Format(_nanoseconds, format ?? DefaultFormat);
		}

		/// <summary>
		///   Converts value of <see cref="HdDateTime" /> into its equivalent string representation and culture specific format
		///   information.
		/// </summary>
		/// <param name="provider">An object that supplies culture-specific formatting information.</param>
		/// <returns>String representation of the current instance.</returns>
		/// <remarks>
		///   A date and time format, which can be the same as <see cref="System.DateTime" /> format. If you want to specify
		///   nanoseconds formatting, you can use "FFFFFFFFF" (9 Fs) or "fffffffff" (9 fs).
		/// </remarks>
		public String ToString(IFormatProvider provider)
		{
			// TODO: ..
			return Formatters.DateTime.Format(_nanoseconds, DefaultFormat);
		}


		/// <summary>
		///   Converts value of <see cref="HdDateTime" /> into its equivalent string representation using the specified format and
		///   culture specific format information.
		/// </summary>
		/// <param name="format">A standart or custom <see cref="HdDateTime" /> format. (See Remarks)</param>
		/// <param name="provider">An object that supplies culture-specific formatting information.</param>
		/// <returns>String representation of the current instance.</returns>
		/// <remarks>
		///   <para>
		///     A date and time format string is the same as in <see cref="System.DateTime" /> fortmatting methods. If you want to specify
		///     nanoseconds precision, you can use "FFFFFFFFF" (9 Fs) or "fffffffff" (9 fs).
		///   </para>
		///   <para>
		///     Formats "d", "ddd", "dddd", "g", "gg", "K", "m", "M", "s", "t", "y", "yyy", "z", "zz", "zzz", duplicate upper case
		///     formats as "FFF FFFFFFFFF" are unsupported.
		///     Formats, which contains nanoseconds format field not at the end of the format string are also unsupported.
		///   </para>
		/// </remarks>
		public String ToString(String format, IFormatProvider provider)
		{
			return Formatters.DateTime.Format(_nanoseconds, format ?? DefaultFormat);
		}

		public static HdDateTime Parse(String source)
		{
			return new HdDateTime(Parsers.DateTime.Parse(source, DefaultFormat));
		}

		public static HdDateTime Parse(String source, String formatString)
		{
			return new HdDateTime(Parsers.DateTime.Parse(source, formatString));
		}

		#endregion

		#region IComparable Interface Implementation

		/// <summary>
		///   Compares the current instance with another instance <paramref name="other" />.
		/// </summary>
		/// <param name="other">An instance to compare with.</param>
		/// <returns>
		///   A value that indicates the relative order of the instances being compared. The return value has the following
		///   meanings:
		///   <list type="bullet">
		///     <item>
		///       <term>
		///         Value Meaning Less than zero: 
		///       </term>
		///       <description>this instance is less than the <paramref name="other" />.</description>
		///     </item>
		///     <item>
		///       <term>Zero: </term>
		///       <description> this instance is equal to <paramref name="other" />.</description>
		///     </item>
		///     <item>
		///       <term>Greater than zero: </term>
		///       <description>this instance is greater than <paramref name="other"/>.</description>
		///     </item>
		///   </list>
		/// </returns>
		public Int32 CompareTo(Object other)
		{
			if (other == null)
				return 1;

			if (other is HdDateTime)
				return CompareTo((HdDateTime)other);

			throw new ArgumentException("Argument must be HdDateTime.");
		}


		/// <summary>
		///   Compares the current instance with another instance <paramref name="other" />.
		/// </summary>
		/// <param name="other">An instance to compare with.</param>
		/// <returns>
		///   A value that indicates the relative order of the instances being compared. The return value has the following
		///   meanings:
		///   <list type="bullet">
		///     <item>
		///       <term>
		///         Value Meaning Less than zero: 
		///       </term>
		///       <description>this instance is less than the <paramref name="other" />.</description>
		///     </item>
		///     <item>
		///       <term>Zero: </term>
		///       <description> this instance is equal to <paramref name="other" />.</description>
		///     </item>
		///     <item>
		///       <term>Greater than zero: </term>
		///       <description>this instance is greater than <paramref name="other"/>.</description>
		///     </item>
		///   </list>
		/// </returns>
		public Int32 CompareTo(HdDateTime other)
		{
			return _nanoseconds.CompareTo(other._nanoseconds);
		}

		#endregion

		#region IEquatable<> Interface Implementation

		/// <summary>
		/// <para/>Indicates, whether the current instance is equal to <paramref name="other" />.
		/// </summary>
		/// <param name="other">Instance to compare with.</param>
		/// <returns>
		/// <para/>True, if the current instance is equal to <paramref name="other" />; otherwise, false.
		/// </returns>
		public Boolean Equals(HdDateTime other)
		{
			return _nanoseconds == other._nanoseconds;
		}

		#endregion

		#region Object Interface Implementation

		/// <summary>
		/// <para/>Indicates, whether the current instance is equal to <paramref name="other" />.
		/// </summary>
		/// <param name="other">Instance to compare with.</param>
		/// <returns>
		/// <para/>True, if the current instance is equal to <paramref name="other" />; otherwise, false.
		/// </returns>
		public override Boolean Equals(Object other)
		{
			return other is HdDateTime && Equals((HdDateTime) other);
		}


		/// <summary>
		/// <para/>Returns hash code for this instance.
		/// </summary>
		/// <returns>
		/// <para/>32-bit signed integer, that is hash code for this instance.
		/// </returns>
		public override Int32 GetHashCode()
		{
			unchecked
			{
				return _nanoseconds.GetHashCode() * 397;
			}
		}

		/// <summary>
		/// <para/>Converts value of <see cref="HdDateTime"/> into its equivalent string representation.
		/// </summary>
		/// <returns>
		/// <para/>String representation of the current instance.
		/// </returns>
		public override String ToString() => Formatters.DateTime.Format(_nanoseconds, DefaultFormat);

		#endregion
	}
}