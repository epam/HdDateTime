/*
  Copyright 2021 EPAM Systems, Inc

  See the NOTICE file distributed with this work for additional information
  regarding copyright ownership. Licensed under the Apache License,
  Version 2.0 (the "License"); you may not use this file except in compliance
  with the License.  You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
  WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the
  License for the specific language governing permissions and limitations under
  the License.
 */

using System;

using Convert = EPAM.Deltix.HdTime.Convert;

namespace EPAM.Deltix.HdTime
{
	/// <summary>
	///   <para>Represents high definition time interval, which stores nanoseconds.</para>
	/// </summary>
	public struct HdTimeSpan : IComparable, IComparable<HdTimeSpan>, IEquatable<HdTimeSpan>
	{
		#region Constants

		internal const long Min		= -Int64.MaxValue;
		internal const long Max		= Int64.MaxValue;
		internal const long NullValue	= Int64.MinValue;	// Null value is not checked and is never returned by most methods

		/// <summary>
		///   Represents the largest possible value of <see cref="HdTimeSpan" />. This field is read-only.
		/// </summary>
		public static readonly HdTimeSpan MaxValue = new HdTimeSpan(Max);

		/// <summary>
		///   Represents the smallest(negative) possible value of <see cref="HdTimeSpan" />. This field is read-only.
		/// </summary>
		public static readonly HdTimeSpan MinValue = new HdTimeSpan(Min);

		/// <summary>
		/// Represents null/undefined <see cref="HdTimeSpan" />. This field is read-only.
		/// <para/>Most instance methods are not expected to behave correctly, but contain no checks due to performance considerations.
		/// TODO:
		/// </summary>
		internal static readonly HdTimeSpan Null = new HdTimeSpan(NullValue);

		/// <summary>
		///   Represents the zero <see cref="HdTimeSpan" /> value. This field is read-only.
		/// </summary>
		public static readonly HdTimeSpan Zero = new HdTimeSpan(0L);

		/// <summary>
		///   Represents the number of hours in 1 day.
		/// </summary>
		public const long HoursInDay = 24;

		/// <summary>
		///   Represents the number of minutes in 1 hour.
		/// </summary>
		public const long MinutesInHour = 60;

		/// <summary>
		///   Represents the number of minutes in 1 day.
		/// </summary>
		public const long MinutesInDay = SecondsInHour * HoursInDay;

		/// <summary>
		///   Represents the number of seconds in 1 minute.
		/// </summary>
		public const long SecondsInMinute = 60;

		/// <summary>
		///   Represents the number of seconds in 1 hour.
		/// </summary>
		public const long SecondsInHour = SecondsInMinute * MinutesInHour;

		/// <summary>
		///   Represents the number of seconds in 1 day.
		/// </summary>
		public const long SecondsInDay = SecondsInHour * HoursInDay;

		/// <summary>
		///   Represents the number of milliseconds in 1 second.
		/// </summary>
		public const long MillisecondsInSecond = 1000;

		/// <summary>
		///   Represents the number of milliseconds in 1 minute.
		/// </summary>
		public const long MillisecondsInMinute = MillisecondsInSecond * SecondsInMinute;

		/// <summary>
		///   Represents the number of milliseconds in 1 hour.
		/// </summary>
		public const long MillisecondsInHour = MillisecondsInMinute * MinutesInHour;

		/// <summary>
		///   Represents the number of milliseconds in 1 day.
		/// </summary>
		public const long MillisecondsInDay = MillisecondsInHour * HoursInDay;

		/// <summary>
		///   Represents the number of microseconds in 1 millisecond.
		/// </summary>
		public const long MicrosecondsInMillisecond = 1000;

		/// <summary>
		///   Represents the number of microseconds in 1 second.
		/// </summary>
		public const long MicrosecondsInSecond = MicrosecondsInMillisecond * MillisecondsInSecond;

		/// <summary>
		///   Represents the number of microseconds in 1 minute.
		/// </summary>
		public const long MicrosecondsInMinute = MicrosecondsInSecond * SecondsInMinute;

		/// <summary>
		///   Represents the number of microseconds in 1 hour.
		/// </summary>
		public const long MicrosecondsInHour = MicrosecondsInMinute * MinutesInHour;

		/// <summary>
		///   Represents the number of microseconds in 1 day.
		/// </summary>
		public const long MicrosecondsInDay = MicrosecondsInHour * HoursInDay;

		/// <summary>
		///   Represents the number of nanoseconds in 1 .NET tick.
		/// </summary>
		public const long NanosInTick = 100;

		/// <summary>
		///   Represents the number of nanoseconds in 1 microsecond.
		/// </summary>
		public const long NanosInMicrosecond = 1000;

		/// <summary>
		///   Represents the number of nanoseconds in 1 millisecond.
		/// </summary>
		public const long NanosInMillisecond = NanosInMicrosecond * MicrosecondsInMillisecond;

		/// <summary>
		///   Represents the number of nanoseconds in 1 second.
		/// </summary>
		public const long NanosInSecond = NanosInMillisecond * MillisecondsInSecond;

		/// <summary>
		///   Represents the number of nanoseconds in 1 minute.
		/// </summary>
		public const long NanosInMinute = NanosInSecond * SecondsInMinute;

		/// <summary>
		///   Represents the number of nanoseconds in 1 hour.
		/// </summary>
		public const long NanosInHour = NanosInMinute * MinutesInHour;

		/// <summary>
		///   Represents the number of nanoseconds in 1 day.
		/// </summary>
		public const long NanosInDay = NanosInHour * HoursInDay;

		public static readonly String DefaultFormat = "d HH:mm:ss.fffffffff";

		#endregion

		internal readonly Int64 _nanoseconds;

		#region Constructors

		/// <summary>
		/// <para/>Constructs <see cref="HdTimeSpan" /> instance from a nanosecond time interval.
		/// </summary>
		/// <param name="nanoseconds"> Time interval, specified in nanoseconds.</param>
		public HdTimeSpan(Int64 nanoseconds)
		{
			_nanoseconds = nanoseconds;
		}


		/// <summary>
		/// <para/>Constructs <see cref="HdTimeSpan" /> instance from a time interval.
		/// </summary>
		/// <param name="timeSpan"> Time interval</param>
		public HdTimeSpan(TimeSpan timeSpan)
		{
			_nanoseconds = Convert.TimeSpan.FromTicksWithMinMax(timeSpan.Ticks);
		}

		/// <summary>
		/// <para/>Constructs <see cref="HdTimeSpan" /> instance from a time interval specified as integer hours, minutes and seconds.
		/// </summary>
		/// <param name="hours"></param>
		/// <param name="minutes"></param>
		/// <param name="seconds"></param>
		public HdTimeSpan(int hours, int minutes, int seconds)
		{
			_nanoseconds = Convert.TimeSpan.From(hours, minutes, seconds);
		}

		/// <summary>
		/// <para/>Constructs <see cref="HdTimeSpan" /> instance from a time interval specified as integer days, hours, minutes and seconds.
		/// </summary>
		/// <param name="days"></param>
		/// <param name="hours"></param>
		/// <param name="minutes"></param>
		/// <param name="seconds"></param>
		public HdTimeSpan(int days, int hours, int minutes, int seconds)
		{
			_nanoseconds = Convert.TimeSpan.From(days, hours, minutes, seconds);
		}

		/// <summary>
		/// <para/>Constructs <see cref="HdTimeSpan" /> instance from a time interval specified as integer days, hours, minutes, seconds and nanoseconds.
		/// </summary>
		/// <param name="days"></param>
		/// <param name="hours"></param>
		/// <param name="minutes"></param>
		/// <param name="seconds"></param>
		/// <param name="nanoseconds"></param>
		public HdTimeSpan(int days, int hours, int minutes, int seconds, int nanoseconds)
		{
			_nanoseconds = Convert.TimeSpan.From(days, hours, minutes, seconds, nanoseconds);
		}


		/// <summary>
		/// <para/>Constructs <see cref="HdTimeSpan"/> instance from a 32-bit integer time interval specified in days.
		/// </summary>
		/// <param name="days">Time interval in days.</param>
		/// <returns>A new <see cref="HdTimeSpan"/> instance.</returns>
		public static HdTimeSpan FromDays(int days)
		{
			return new HdTimeSpan(Convert.TimeSpan.FromDays(days));
		}

		/// <summary>
		/// <para/>Constructs <see cref="HdTimeSpan"/> instance from a 32-bit integer time interval specified in hours.
		/// </summary>
		/// <param name="hours">Time interval in hours.</param>
		/// <returns>A new <see cref="HdTimeSpan"/> instance.</returns>
		public static HdTimeSpan FromHours(int hours)
		{
			return new HdTimeSpan(Convert.TimeSpan.FromHours(hours));
		}

		/// <summary>
		/// <para/>Constructs <see cref="HdTimeSpan"/> instance from a 32-bit integer time interval specified in minutes.
		/// </summary>
		/// <param name="minutes">Time interval in minutes.</param>
		/// <returns>A new <see cref="HdTimeSpan"/> instance.</returns>
		public static HdTimeSpan FromMinutes(int minutes)
		{
			return new HdTimeSpan(Convert.TimeSpan.FromMinutes(minutes));
		}

		/// <summary>
		/// <para/>Constructs <see cref="HdTimeSpan"/> instance from a 32-bit integer time interval specified in seconds.
		/// </summary>
		/// <param name="seconds">Time interval in seconds.</param>
		/// <returns>A new <see cref="HdTimeSpan"/> instance.</returns>
		public static HdTimeSpan FromSeconds(int seconds)
		{
			// No need to check range if creating fron Int32 seconds
			return new HdTimeSpan((Int64)seconds * Convert.NanosInSecond);
		}

		/// <summary>
		/// <para/>Constructs <see cref="HdTimeSpan"/> instance from a 32-bit integer time interval specified in milliseconds.
		/// </summary>
		/// <param name="milliseconds">Time interval in milliseconds.</param>
		/// <returns>A new <see cref="HdTimeSpan"/> instance.</returns>
		public static HdTimeSpan FromMilliseconds(int milliseconds)
		{
			// No need to check range if creating fron Int32 millis
			return new HdTimeSpan((Int64)milliseconds * Convert.NanosInMillisecond);
		}

		/// <summary>
		/// <para/>Constructs <see cref="HdTimeSpan"/> instance from a 32-bit integer time interval specified in microseconds.
		/// </summary>
		/// <param name="millis">Time interval in microseconds.</param>
		/// <returns>A new <see cref="HdTimeSpan"/> instance.</returns>
		public static HdTimeSpan FromMicroseconds(int microseconds)
		{
			// No need to check range if creating fron Int32 microseconds
			return new HdTimeSpan((Int64)microseconds * Convert.NanosInMicrosecond);
		}

		/// <summary>
		/// <para/>Constructs <see cref="HdTimeSpan"/> instance from a 32-bit integer time interval specified in .NET 100ns ticks.
		/// </summary>
		/// <param name="ticks">Time interval in .NET 100-nanosecond ticks.</param>
		/// <returns>A new <see cref="HdTimeSpan"/> instance.</returns>
		public static HdTimeSpan FromTicks(int ticks)
		{
			// No need to check range if creating fron Int32 ticks
			return new HdTimeSpan((Int64)ticks * Convert.NanosInTick);
		}

		/// <summary>
		/// <para/>Constructs <see cref="HdTimeSpan"/> instance from a 64-bit integer time interval specified in days.
		/// </summary>
		/// <param name="days">Time interval in days.</param>
		/// <exception cref="ArgumentOutOfRangeException">The resulting <see cref="HdTimeSpan"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.</exception>
		/// <returns>A new <see cref="HdTimeSpan"/> instance.</returns>
		public static HdTimeSpan FromDays(Int64 days)
		{
			return new HdTimeSpan(Convert.TimeSpan.FromDays(days));
		}

		/// <summary>
		/// <para/>Constructs <see cref="HdTimeSpan"/> instance from a 64-bit integer time interval specified in hours.
		/// </summary>
		/// <param name="hours">Time interval in hours.</param>
		/// <exception cref="ArgumentOutOfRangeException">The resulting <see cref="HdTimeSpan"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.</exception>
		/// <returns>A new <see cref="HdTimeSpan"/> instance.</returns>
		public static HdTimeSpan FromHours(Int64 hours)
		{
			return new HdTimeSpan(Convert.TimeSpan.FromHours(hours));
		}

		/// <summary>
		/// <para/>Constructs <see cref="HdTimeSpan"/> instance from a 64-bit integer time interval specified in minutes.
		/// </summary>
		/// <param name="minutes">Time interval in minutes.</param>
		/// <exception cref="ArgumentOutOfRangeException">The resulting <see cref="HdTimeSpan"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.</exception>
		/// <returns>A new <see cref="HdTimeSpan"/> instance.</returns>
		public static HdTimeSpan FromMinutes(Int64 minutes)
		{
			return new HdTimeSpan(Convert.TimeSpan.FromMinutes(minutes));
		}

		/// <summary>
		/// <para/>Constructs <see cref="HdTimeSpan"/> instance from a 64-bit integer time interval specified in seconds.
		/// </summary>
		/// <param name="seconds">Time interval in seconds</param>
		/// <exception cref="ArgumentOutOfRangeException">The resulting <see cref="HdTimeSpan"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.</exception>
		/// <returns>A new <see cref="HdTimeSpan"/> instance.</returns>
		public static HdTimeSpan FromSeconds(Int64 seconds)
		{
			return new HdTimeSpan(Convert.TimeSpan.FromSeconds(seconds));
		}

		/// <summary>
		/// <para/>Constructs <see cref="HdTimeSpan"/> instance from a 64-bit integer time interval specified in milliseconds.
		/// </summary>
		/// <param name="milliseconds">Time interval in milliseconds.</param>
		/// <exception cref="ArgumentOutOfRangeException">The resulting <see cref="HdTimeSpan"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.</exception>
		/// <returns>A new <see cref="HdTimeSpan"/> instance.</returns>
		public static HdTimeSpan FromMilliseconds(Int64 milliseconds)
		{
			// TODO: Discuss: Do we need to handle Min/Max constants here when creating from millis value?
			return new HdTimeSpan(Convert.TimeSpan.FromMillis(milliseconds));
		}

		/// <summary>
		/// <para/>Constructs <see cref="HdTimeSpan"/> instance from a 64-bit integer time interval specified in microseconds.
		/// </summary>
		/// <param name="millis">Time interval in microseconds.</param>
		/// <returns>A new <see cref="HdTimeSpan"/> instance.</returns>
		public static HdTimeSpan FromMicroseconds(Int64 microseconds)
		{
			return new HdTimeSpan(Convert.TimeSpan.FromMicros(microseconds));
		}

		/// <summary>
		/// <para/>Constructs <see cref="HdTimeSpan"/> instance from a 64-bit integer time interval specified in .NET 100ns ticks.
		/// </summary>
		/// <param name="ticks">Time interval in .NET 100-nanosecond ticks.</param>
		/// <exception cref="ArgumentOutOfRangeException">The resulting <see cref="HdTimeSpan"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.
		/// </exception>
		/// <returns><para/>A new <see cref="HdTimeSpan"/> instance.</returns>
		/// <remarks><para/>If MinValue/MaxValue translation is desired, use constructor or typecast from System.TimeSpan struct</remarks>
		public static HdTimeSpan FromTicks(Int64 ticks) => new HdTimeSpan(Convert.TimeSpan.FromTicks(ticks));

		/// <summary>
		/// <para/>Constructs <see cref="HdTimeSpan"/> instance from a 64-bit integer time interval specified in nanoseconds.
		/// </summary>
		/// <param name="nanos">Time interval in nanoseconds</param>
		/// <returns>A new <see cref="HdTimeSpan"/> instance.</returns>
		public static HdTimeSpan FromNanoseconds(Int64 nanos) => new HdTimeSpan(nanos);


		#endregion

		#region Conversion

		/// <summary>
		/// <para/>Gets the nanosecond of second component of the time interval represented by this <see cref="HdTimeSpan"/> structure
		/// <para/>The return value ranges from -999999999 to 999999999
		/// <para/>Use <see cref="TotalNanoseconds" /> to get the complete time interval in nanoseconds.
		/// </summary>
		public int Nanoseconds => (int)(_nanoseconds % NanosInSecond);

		/// <summary>
		/// <para/>Gets the microseconds of second component of the time interval represented by this <see cref="HdTimeSpan"/> structure
		/// <para/>The return value ranges from -999999 to 999999
		/// </summary>
		public int Microseconds => (int)((_nanoseconds / NanosInMicrosecond) % 1000000);

		/// <summary>
		/// <para/>Gets the milliseconds component of the time interval represented by this <see cref="HdTimeSpan"/> structure
		/// <para/>The return value ranges from -999 to 999
		/// </summary>
		public int Milliseconds => (int)((_nanoseconds / NanosInMillisecond) % 1000);

		/// <summary>
		/// <para/>Gets the seconds component of the time interval represented by this <see cref="HdTimeSpan"/> structure
		/// <para/>The return value ranges from -59 to 59
		/// </summary>
		public int Seconds => (int)((_nanoseconds / NanosInSecond) % 60);

		/// <summary>
		/// <para/>Gets the minutes component of the time interval represented by this <see cref="HdTimeSpan"/> structure
		/// <para/>The return value ranges from -59 to 59
		/// </summary>
		public int Minutes => ((int)(_nanoseconds / NanosInMinute) % 60);

		/// <summary>
		/// <para/>Gets the hours component of the time interval represented by this <see cref="HdTimeSpan"/> structure
		/// <para/>The return value ranges from -23 to 23
		/// </summary>
		public int Hours => ((int)(_nanoseconds / NanosInHour) % 24);

		/// <summary>
		/// <para/>Gets the days component of the time interval represented by this <see cref="HdTimeSpan"/> structure
		/// <para/>The return value can be positive or negative.
		/// </summary>
		public int Days => (int)(_nanoseconds / NanosInDay);

		/// <summary>
		/// Gets the value of this time interval expressed in <see cref="Int64"/> nanoseconds.
		/// </summary>
		public Int64 TotalNanoseconds => _nanoseconds;

		/// <summary>
		/// Gets the value of this time interval expressed in whole and fractional milliseconds.
		/// </summary>
		public double TotalMicroseconds => _nanoseconds * MicrosecondsPerNanosecond;

		/// <summary>
		/// Gets the value of this time interval expressed in whole and fractional milliseconds.
		/// </summary>
		public double TotalMilliseconds => _nanoseconds * MillisecondsPerNanosecond;

		/// <summary>
		/// Gets the value of this time interval expressed in whole and fractional seconds.
		/// </summary>
		public double TotalSeconds => _nanoseconds * SecondsPerNanosecond;

		/// <summary>
		/// Gets the value of this time interval expressed in whole and fractional minutes.
		/// </summary>
		public double TotalMinutes => _nanoseconds / (double)NanosInMinute;

		/// <summary>
		/// Gets the value of this time interval expressed in whole and fractional hours.
		/// </summary>
		public double TotalHours => _nanoseconds / (double)NanosInHour;

		/// <summary>
		///  Gets the value of this time interval expressed in whole and fractional days.
		/// </summary>
		public double TotalDays => _nanoseconds / (double)NanosInDay;

		/// <summary>
		/// <para/>Converts to standard .NET <see cref="System.TimeSpan"/>, rounding the value towards 0.
		/// </summary>
		/// <remarks>
		/// <para/>Loses time interval component, which is less than 1 tick (1 tick == 100 nanoseconds).
		/// <para/>Use <see cref="TimeSpanModulo" /> to retrieve the lost component.
		/// </remarks>
		public TimeSpan TimeSpan => new TimeSpan(Convert.TimeSpan.ToTicksWithMinMax(_nanoseconds));

		/// <summary>
		/// <para/>Returns time interval nanosecond component which is lost after converting to <see cref="System.TimeSpan"/>.
		/// <para/>[0..99] for positive time interval, [-99..0] for negative time interval
		/// </summary>
		/// <remarks>
		///   See <see cref="TimeSpan" />
		/// </remarks>
		public SByte TimeSpanModulo => (SByte)Convert.TimeSpan.ToTicksRemainder(_nanoseconds);

		/// <summary>
		/// Converts <paramref name="hdTimeSpan" /> time interval to <see cref="System.TimeSpan"/>.
		/// </summary>
		/// <param name="hdTimeSpan">Time interval to convert.</param>
		/// <returns>Time interval component, which contains ticks.</returns>
		/// <remarks>
		/// Loses time interval component, which is less than 1 tick (1 tick == 100 nanoseconds).
		/// <para/>Use <see cref="TimeSpanModulo" /> to retrieve the lost component.
		/// </remarks>
		public static explicit operator TimeSpan(HdTimeSpan hdTimeSpan) => hdTimeSpan.TimeSpan;

		/// <summary>
		/// <para/>Converts <paramref name="timeSpan" /> time interval to the high definition time interval <see cref="HdDateTime" />.
		/// </summary>
		/// <param name="timeSpan">Time interval to convert.</param>
		/// <returns>Time interval, which contains nanoseconds.</returns>
		public static implicit operator HdTimeSpan(TimeSpan timeSpan) => new HdTimeSpan(timeSpan);

		#endregion

		#region Comparison

		/// <summary>
		///   Compares the values of <paramref name="left" /> and <paramref name="right" />.
		/// </summary>
		/// <param name="left">Left operand.</param>
		/// <param name="right">Right operand.</param>
		/// <returns>True, if the values of operands are equal; otherwise, false.</returns>
		public static Boolean operator ==(HdTimeSpan left, HdTimeSpan right) => left._nanoseconds == right._nanoseconds;


		/// <summary>
		///   Compares the values of <paramref name="left" /> and <paramref name="right" />.
		/// </summary>
		/// <param name="left">Left operand.</param>
		/// <param name="right">Right operand.</param>
		/// <returns>True, if the left operand is more than the right operand; otherwise, false.</returns>
		public static Boolean operator >(HdTimeSpan left, HdTimeSpan right) => left._nanoseconds > right._nanoseconds;


		/// <summary>
		///   Compares the values of <paramref name="left" /> and <paramref name="right" />.
		/// </summary>
		/// <param name="left">Left operand.</param>
		/// <param name="right">Right operand.</param>
		/// <returns>True, if the left operand is more or equal to the right operand; otherwise, false.</returns>
		public static Boolean operator >=(HdTimeSpan left, HdTimeSpan right) => left._nanoseconds >= right._nanoseconds;


		/// <summary>
		///   Compares the values of <paramref name="left" /> and <paramref name="right" />.
		/// </summary>
		/// <param name="left">Left operand.</param>
		/// <param name="right">Right operand.</param>
		/// <returns>True, if the values of operands are not equal; otherwise, false.</returns>
		public static Boolean operator !=(HdTimeSpan left, HdTimeSpan right) => left._nanoseconds != right._nanoseconds;


		/// <summary>
		///   Compares the values of <paramref name="left" /> and <paramref name="right" />.
		/// </summary>
		/// <param name="left">Left operand.</param>
		/// <param name="right">Right operand.</param>
		/// <returns>True, if the left operand is less than the right operand; otherwise, false.</returns>
		public static Boolean operator <(HdTimeSpan left, HdTimeSpan right) => left._nanoseconds < right._nanoseconds;


		/// <summary>
		///   Compares the values of <paramref name="left" /> and <paramref name="right" />.
		/// </summary>
		/// <param name="left">Left operand.</param>
		/// <param name="right">Right operand.</param>
		/// <returns>True, if the left operand is less or equal to the right operand; otherwise, false.</returns>
		public static Boolean operator <=(HdTimeSpan left, HdTimeSpan right) => left._nanoseconds <= right._nanoseconds;

		/// <summary>
		/// <para/>Compares two <see cref="HdTimeSpan"/> values
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns>
		/// <para/>-1, if <paramref name="left" /> &lt; <paramref name="right" />
		/// <para/>1, if <paramref name="left" /> &gt; <paramref name="right" />
		/// <para/>0, if <paramref name="left" /> == <paramref name="right" />
		/// </returns>
		public static int Compare(HdTimeSpan left, HdTimeSpan right) => left._nanoseconds.CompareTo(right._nanoseconds);

		public Boolean IsZero() => 0 == _nanoseconds;

		public Boolean IsPositive() => _nanoseconds > 0;

		public Boolean IsNegative() => _nanoseconds < 0;

		#endregion

		#region Arithmetic and rounding

		HdTimeSpan RoundToU(Int64 resolution)
		{
			return new HdTimeSpan(Convert.TimeSpan.RoundTo(_nanoseconds, resolution));
		}

		/// <summary>
		/// Round to the specified resolution by truncating towards 0.
		/// </summary>
		/// <param name="resolution"></param>
		/// <returns>New instance of HdTimeSpan truncated to the specified resolution</returns>
		public HdTimeSpan RoundTo(HdTimeSpan resolution)
		{
			Int64 res = resolution._nanoseconds;
			if (res <= 0)
				throw new ArgumentException("resolution must be positive");

			return RoundToU(res);
		}

		/// <summary>
		/// Round to the specified resolution by truncating towards 0.
		/// </summary>
		/// <param name="resolution"></param>
		/// <returns>New instance of HdTimeSpan truncated to the specified resolution</returns>
		public HdTimeSpan RoundTo(Resolution resolution)
		{
			switch (resolution)
			{
				case Resolution.Day:
					return RoundToU(NanosInDay);

				case Resolution.Hour:
					return RoundToU(NanosInHour);

				case Resolution.Minute:
					return RoundToU(NanosInMinute);

				case Resolution.Second:
					return RoundToU(NanosInSecond);

				case Resolution.Millisecond:
					return RoundToU(NanosInMillisecond);

				case Resolution.Microsecond:
					return RoundToU(NanosInMicrosecond);

				case Resolution.Tick:
					return RoundToU(NanosInTick);

				case Resolution.Nanosecond:
					return this;
			}

			throw new ArgumentException("Unsupported resolution: " + resolution);
		}

		/// <summary>
		///   Returns the specified time interval.
		/// </summary>
		/// <param name="timeSpan">The time interval to return.</param>
		/// <returns>
		///   The specified time interval.
		/// </returns>
		public static HdTimeSpan operator +(HdTimeSpan timeSpan) => timeSpan;

		/// <summary>
		///   Returns a time interval, whose value is the negated value of the specified instance. Correctly handles the negation of MinValue/MaxValue
		/// </summary>
		/// <param name="timeSpan">The time interval to be negated.</param>
		/// <returns>
		///   A time interval that has the same numeric value as <paramref name="timeSpan"/>, but the opposite sign.
		/// </returns>
		public static HdTimeSpan operator -(HdTimeSpan timeSpan) => new HdTimeSpan(Util.Negate(timeSpan._nanoseconds));

		/// <summary>
		///   Returns a time interval, whose value is the negated value of this instance. Correctly handles negation of MinValue/MaxValue
		/// </summary>
		/// <returns>
		///   A new time interval that has the same numeric value as this instance, but the opposite sign.
		/// </returns>
		public HdTimeSpan Negate() => new HdTimeSpan(Util.Negate(_nanoseconds));

		/// <summary>
		///   Returns a time interval, whose value is whose value is the absolute value of this instance.
		/// </summary>
		/// <returns>
		///   A new time interval that has the absolute value of this instance.
		/// </returns>
		public HdTimeSpan Duration() => new HdTimeSpan(Util.Abs(_nanoseconds));

		/// <summary>
		///   Adds two specified time intervals with signed overflow check.
		/// </summary>
		/// <param name="left">The time interval to add.</param>
		/// <param name="right">The time interval to add.</param>
		/// <returns>A new time interval that is the sum of the values of <paramref name="left" /> and <paramref name="right" />.</returns>
		public static HdTimeSpan operator +(HdTimeSpan left, HdTimeSpan right)
		{
			return new HdTimeSpan(Util.AddToTs(left._nanoseconds, right._nanoseconds));
		}

		/// <summary>
		///   Adds the specified time interval with signed overflow check.
		/// </summary>
		/// <param name="other">The time interval to add.</param>
		/// <returns>A new time interval whose value is the result of the value of this instance plus
		///    the value of <paramref name="other" />.</returns>
		public HdTimeSpan Add(HdTimeSpan other)
		{
			return new HdTimeSpan(Util.AddToTs(_nanoseconds, other._nanoseconds));
		}

		/// <summary>
		///   Adds the specified time interval without overflow check.
		/// </summary>
		/// <param name="other">The time interval to add.</param>
		/// <returns>A new time interval whose value is the result of the value of this instance plus
		///    the value of <paramref name="other" />.</returns>
		public HdTimeSpan AddUnchecked(HdTimeSpan other) => new HdTimeSpan(_nanoseconds + other._nanoseconds);

		/// <summary>
		///   Subtracts two specified time intervals with signed overflow check.
		/// </summary>
		/// <param name="left">The time interval to substract (the minuend).</param>
		/// <param name="right">The time interval to substract (the subtrahend).</param>
		/// <returns>
		///   A new time interval which is <paramref name="left" /> minus <paramref name="right" />.
		/// </returns>
		public static HdTimeSpan operator -(HdTimeSpan left, HdTimeSpan right)
		{
			return new HdTimeSpan(Util.SubtractToTs(left._nanoseconds, right._nanoseconds));
		}

		/// <summary>
		///   Subtracts two specified time intervals with signed overflow check.
		/// </summary>
		/// <param name="other">The time interval to be subtracted (the subtrahend).</param>
		/// <returns>
		///   A new time interval whose value is the result of the value of this instance minus
		///   the value of <paramref name="other" />.
		/// </returns>
		public HdTimeSpan Subtract(HdTimeSpan other)
		{
			return new HdTimeSpan(Util.SubtractToTs(_nanoseconds, other._nanoseconds));
		}

		/// <summary>
		///   Subtracts two specified time intervals without overflow check.
		/// </summary>
		/// <param name="other">The time interval to be subtracted (the subtrahend).</param>
		/// <returns>
		///   A new time interval whose value is the result of the value of this instance minus
		///   the value of <paramref name="other" />.
		/// </returns>
		public HdTimeSpan SubtractUnchecked(HdTimeSpan other) => new HdTimeSpan(_nanoseconds - other._nanoseconds);

		/// <summary>
		/// <para/>Returns a new <see cref="HdTimeSpan"/> that adds the specified amount of days to the value of this instance
		/// </summary>
		/// <param name="days">A number of days. Can be positive or negative.</param>
		/// <exception cref="ArgumentOutOfRangeException">The added value is beyond range representable as <see cref="HdTimeSpan"/></exception>
		/// <exception cref="OverflowException">The resulting <see cref="HdTimeSpan"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.</exception>
		/// <returns>
		/// <para/>New instance whose value is the sum of the date and time of this instance
		/// and the number of days represented by <paramref name="days" />
		/// </returns>
		public HdTimeSpan AddDays(Int64 days)
		{
			return Add(HdTimeSpan.FromDays(days));
		}

		/// <summary>
		/// <para/>Returns a new <see cref="HdTimeSpan"/> that adds the specified amount of hours to the value of this instance
		/// </summary>
		/// <param name="hours">A number of hours. Can be positive or negative.</param>
		/// <exception cref="ArgumentOutOfRangeException">The added value is beyond range representable as <see cref="HdTimeSpan"/></exception>
		/// <exception cref="OverflowException">The resulting <see cref="HdTimeSpan"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.</exception>
		/// <returns>
		/// <para/>New instance whose value is the sum of the date and time of this instance
		/// and the number of hours represented by <paramref name="hours" />
		/// </returns>
		public HdTimeSpan AddHours(Int64 hours)
		{
			return Add(HdTimeSpan.FromHours(hours));
		}

		/// <summary>
		/// <para/>Returns a new <see cref="HdTimeSpan"/> that adds the specified amount of minutes to the value of this instance
		/// </summary>
		/// <param name="minutes">A number of minutes. Can be positive or negative.</param>
		/// <exception cref="ArgumentOutOfRangeException">The added value is beyond range representable as <see cref="HdTimeSpan"/></exception>
		/// <exception cref="OverflowException">The resulting <see cref="HdTimeSpan"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.</exception>
		/// <returns>
		/// <para/>New instance whose value is the sum of the date and time of this instance
		/// and the number of minutes represented by <paramref name="minutes" />
		/// </returns>
		public HdTimeSpan AddMinutes(Int64 minutes)
		{
			return Add(HdTimeSpan.FromMinutes(minutes));
		}

		/// <summary>
		/// <para/>Returns a new <see cref="HdTimeSpan"/> that adds the specified amount of seconds to the value of this instance
		/// </summary>
		/// <param name="seconds">A number of seconds. Can be positive or negative.</param>
		/// <exception cref="ArgumentOutOfRangeException">The added value is beyond range representable as <see cref="HdTimeSpan"/></exception>
		/// <exception cref="OverflowException">The resulting <see cref="HdTimeSpan"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.</exception>
		/// <returns>
		/// <para/>New instance whose value is the sum of the date and time of this instance
		/// and the number of seconds represented by <paramref name="seconds" />
		/// </returns>
		public HdTimeSpan AddSeconds(Int64 seconds)
		{
			return Add(HdTimeSpan.FromSeconds(seconds));
		}

		/// <summary>
		/// <para/>Returns a new <see cref="HdTimeSpan"/> that adds the specified amount of milliseconds to the value of this instance
		/// </summary>
		/// <param name="milliseconds">A number of milliseconds. Can be positive or negative.</param>
		/// <exception cref="ArgumentOutOfRangeException">The added value is beyond range representable as <see cref="HdTimeSpan"/></exception>
		/// <exception cref="OverflowException">The resulting <see cref="HdTimeSpan"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.</exception>
		/// <returns>
		/// <para/>New instance whose value is the sum of the date and time of this instance
		/// and the number of milliseconds represented by <paramref name="milliseconds" />
		/// </returns>
		public HdTimeSpan AddMilliseconds(Int64 milliseconds)
		{
			return Add(HdTimeSpan.FromMilliseconds(milliseconds));
		}

		/// <summary>
		/// <para/>Returns a new <see cref="HdTimeSpan"/> that adds the specified amount of 100ns ticks to the value of this instance
		/// </summary>
		/// <param name="ticks">A number of 100ns .NET 'ticks'. Can be positive or negative.</param>
		/// <exception cref="ArgumentOutOfRangeException">The added value is beyond range representable as <see cref="HdTimeSpan"/></exception>
		/// <exception cref="OverflowException">The resulting <see cref="HdTimeSpan"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.</exception>
		/// <returns>
		/// <para/>New instance whose value is the sum of the date and time of this instance
		/// and the number of ticks represented by <paramref name="ticks" />
		/// </returns>
		public HdTimeSpan AddTicks(Int64 ticks)
		{
			return Add(HdTimeSpan.FromTicks(ticks));
		}

		/// <summary>
		/// <para/>Returns a new <see cref="HdTimeSpan"/> that adds the specified amount of nanoseconds to the value of this instance
		/// </summary>
		/// <param name="nanos">A number of nanoseconds. Can be positive or negative.</param>
		/// <exception cref="OverflowException">The resulting <see cref="HdTimeSpan"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.</exception>
		/// <returns>
		/// <para/>New instance whose value is the sum of the date and time of this instance
		/// and the number of nanoseconds represented by <paramref name="nanos" />
		/// </returns>
		public HdTimeSpan AddNanoseconds(Int64 nanos)
		{
			return new HdTimeSpan(Util.AddToDt(_nanoseconds, nanos));
		}

		/// <summary>
		/// <para/>Returns a new <see cref="HdTimeSpan"/> that adds the specified amount of days to the value of this instance
		/// </summary>
		/// <param name="days">A number of days. Can be positive or negative.</param>
		/// <exception cref="ArgumentOutOfRangeException">The added value is beyond range representable as <see cref="HdTimeSpan"/></exception>
		/// <exception cref="OverflowException">The resulting <see cref="HdTimeSpan"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.</exception>
		/// <returns>
		/// <para/>New instance whose value is the sum of the date and time of this instance
		/// and the number of days represented by <paramref name="days" />
		/// </returns>
		public HdTimeSpan AddDays(int days)
		{
			return Add(HdTimeSpan.FromDays(days));
		}

		/// <summary>
		/// <para/>Returns a new <see cref="HdTimeSpan"/> that adds the specified amount of hours to the value of this instance
		/// </summary>
		/// <param name="hours">A number of hours. Can be positive or negative.</param>
		/// <exception cref="ArgumentOutOfRangeException">The added value is beyond range representable as <see cref="HdTimeSpan"/></exception>
		/// <exception cref="OverflowException">The resulting <see cref="HdTimeSpan"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.</exception>
		/// <returns>
		/// <para/>New instance whose value is the sum of the date and time of this instance
		/// and the number of hours represented by <paramref name="hours" />
		/// </returns>
		public HdTimeSpan AddHours(int hours)
		{
			return Add(HdTimeSpan.FromHours(hours));
		}

		/// <summary>
		/// <para/>Returns a new <see cref="HdTimeSpan"/> that adds the specified amount of minutes to the value of this instance
		/// </summary>
		/// <param name="minutes">A number of minutes. Can be positive or negative.</param>
		/// <exception cref="ArgumentOutOfRangeException">The added value is beyond range representable as <see cref="HdTimeSpan"/></exception>
		/// <exception cref="OverflowException">The resulting <see cref="HdTimeSpan"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.</exception>
		/// <returns>
		/// <para/>New instance whose value is the sum of the date and time of this instance
		/// and the number of minutes represented by <paramref name="minutes" />
		/// </returns>
		public HdTimeSpan AddMinutes(int minutes)
		{
			return Add(HdTimeSpan.FromMinutes(minutes));
		}

		/// <summary>
		/// <para/>Returns a new <see cref="HdTimeSpan"/> that adds the specified amount of seconds to the value of this instance
		/// </summary>
		/// <param name="seconds">A number of seconds. Can be positive or negative.</param>
		/// <exception cref="OverflowException">The resulting <see cref="HdTimeSpan"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.</exception>
		/// <returns>
		/// <para/>New instance whose value is the sum of the date and time of this instance
		/// and the number of seconds represented by <paramref name="seconds" />
		/// </returns>
		public HdTimeSpan AddSeconds(int seconds)
		{
			return Add(HdTimeSpan.FromSeconds(seconds));
		}

		/// <summary>
		/// <para/>Returns a new <see cref="HdTimeSpan"/> that adds the specified amount of milliseconds to the value of this instance
		/// </summary>
		/// <param name="milliseconds">A number of milliseconds. Can be positive or negative.</param>
		/// <exception cref="OverflowException">The resulting <see cref="HdTimeSpan"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.</exception>
		/// <returns>
		/// <para/>New instance whose value is the sum of the date and time of this instance
		/// and the number of milliseconds represented by <paramref name="milliseconds" />
		/// </returns>
		public HdTimeSpan AddMilliseconds(int milliseconds)
		{
			return Add(HdTimeSpan.FromMilliseconds(milliseconds));
		}

		/// <summary>
		/// <para/>Returns a new <see cref="HdTimeSpan"/> that adds the specified amount of 100ns ticks to the value of this instance
		/// </summary>
		/// <param name="ticks">A number of 100ns .NET 'ticks'. Can be positive or negative.</param>
		/// <exception cref="OverflowException">The resulting <see cref="HdTimeSpan"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.</exception>
		/// <returns>
		/// <para/>New instance whose value is the sum of the date and time of this instance
		/// and the number of ticks represented by <paramref name="ticks" />
		/// </returns>
		public HdTimeSpan AddTicks(int ticks)
		{
			return Add(HdTimeSpan.FromTicks(ticks));
		}

		#endregion

		#region Parsing and Formatting

		public String ToString(String format)
		{
			return Formatters.TimeSpan.Format(_nanoseconds, format ?? DefaultFormat);
		}

		public String ToString(IFormatProvider provider)
		{
			return Formatters.TimeSpan.Format(_nanoseconds, DefaultFormat);
		}

		public String ToString(String format, IFormatProvider provider)
		{
			return Formatters.TimeSpan.Format(_nanoseconds, format ?? DefaultFormat);
		}

		public static HdTimeSpan Parse(String source)
		{
			return new HdTimeSpan(Parsers.TimeSpan.Parse(source, DefaultFormat));
		}

		public static HdTimeSpan Parse(String source, String formatString)
		{
			return new HdTimeSpan(Parsers.TimeSpan.Parse(source, formatString));
		}

		#endregion

		#region Object Interface Implementation

		/// <summary>
		///   Indicates, whether the current instance is equal to <paramref name="other" />.
		/// </summary>
		/// <param name="other">Instance to compare with.</param>
		/// <returns>True, if the current instance is equal to <paramref name="other" />; otherwise, false.</returns>
		public override Boolean Equals(Object other)
		{
			return other is HdTimeSpan && Equals((HdTimeSpan)other);
		}


		/// <summary>
		///   Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		///   A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		public override Int32 GetHashCode()
		{
			unchecked
			{
				return _nanoseconds.GetHashCode() * 397;
			}
		}

		/// <summary>
		///   Converts value of <see cref="HdTimeSpan" /> into its equivalent string representation.
		/// </summary>
		/// <returns>String representation of the current instance.</returns>
		public override String ToString()
		{
			return Formatters.TimeSpan.Format(_nanoseconds, DefaultFormat);
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
		///       <description>this instance is greater than <paramref name="other" />.</description>
		///     </item>
		///   </list>
		/// </returns>
		public Int32 CompareTo(Object other)
		{
			if (other == null)
				return 1;
			if (!(other is HdTimeSpan))
				throw new ArgumentException("Argument must be HdTimeSpan");

			HdTimeSpan otherInterval = (HdTimeSpan)other;
			return _nanoseconds.CompareTo(otherInterval._nanoseconds);
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
		///       <description>this instance is greater than <paramref name="other" />.</description>
		///     </item>
		///   </list>
		/// </returns>
		public Int32 CompareTo(HdTimeSpan other)
		{
			return _nanoseconds.CompareTo(other._nanoseconds);
		}

		// TODO:
		//public Int32 CompareTo(TimeSpan other)
		//{
		//	return _nanoseconds.CompareTo(Convert.TimeSpanTicksToNanos(other.Ticks));
		//}

		#endregion

		#region IEquatable<> Interface Implementation

		/// <summary>
		///   Indicates, whether the current instance is equal to <paramref name="other" />.
		/// </summary>
		/// <param name="other">Instance to compare with.</param>
		/// <returns>True, if the current instance is equal to <paramref name="other" />; otherwise, false.</returns>
		public Boolean Equals(HdTimeSpan other)
		{
			return _nanoseconds == other._nanoseconds;
		}

		#endregion

		#region Private constants
		private const double MicrosecondsPerNanosecond = 1.0 / NanosInMicrosecond;
		private const double MillisecondsPerNanosecond = 1.0 / NanosInMillisecond;
		private const double SecondsPerNanosecond = 1.0 / NanosInSecond;
		#endregion
	}
}