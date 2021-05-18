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
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace EPAM.Deltix.HdTime
{
	using static Util;
	/// <summary>
	///   Utility class, which contains methods intended to convert timestamp into different formats.
	/// </summary>
	internal static class Convert {

		#region Constants
		// NOTE: The following identifiers are typically shortened for better readability in internal classes:
		//       Milliseconds/Nanoseconds -> Millis/Nanos, Divide/Modulo -> Div/Mod

		internal const uint TicksInSecond = 10000000;
		internal const uint TicksInMillisecond = TicksInSecond / 1000;
		internal const uint NanosInTick = 100;

		public const long MillisecondsInSecond = 1000;
		public const long MinutesInHour = 60;
		public const long SecondsInMinute = 60;
		public const long HoursInDay = 24;

		public const long TicksInMinute = TicksInSecond * SecondsInMinute;
		public const long TicksInHour = TicksInMinute * MinutesInHour;
		public const long TicksInDay = TicksInHour * HoursInDay;

		public const long NanosInMicrosecond = 1000;
		public const long NanosInMillisecond = 1000000;
		public const long NanosInSecond = NanosInMillisecond * MillisecondsInSecond;
		public const long NanosInMinute = NanosInSecond * SecondsInMinute;
		public const long NanosInHour = NanosInMinute * MinutesInHour;
		public const long NanosInDay = NanosInHour * HoursInDay;


		#endregion

		#region Helpers

		[MethodImpl(MethodImplOptions.NoInlining)]
		internal static ArgumentOutOfRangeException RangeException(string msg)
		{
			return new ArgumentOutOfRangeException(msg);
		}

		private static Int64 ToInt64MinMax(Int64 x)
		{
			return x < 0 ? Int64.MinValue : Int64.MaxValue; // TODO: Bit operations possible
		}

		private static bool IsInt64MinMax(Int64 x) => InRange(x, Int64.MaxValue, Int64.MinValue);

		// TODO: could be implemented better
		private static Int64 AssertInt64MinMax(Int64 x)
		{
			if (!IsInt64MinMax(x))
				ThrowOutOfRange();

			return x;
		}

		#endregion

		#region Range checks

		//[MethodImpl(MethodImplOptions.NoInlining)]
		internal static void ThrowOutOfRange(string msg) { throw new ArgumentOutOfRangeException(msg); }

		internal static void ThrowOutOfRange() { throw new ArgumentOutOfRangeException(); }

		private static void ThrowNanosOutOfRange() { throw RangeException("nanoseconds"); }

		private static void ThrowTicksOutOfRange() { throw RangeException("ticks"); }

		private static void ThrowMicrosOutOfRange() { throw RangeException("microseconds"); }

		private static void ThrowMillisOutOfRange() { throw RangeException("milliseconds"); }

		private static void ThrowSecondsOutOfRange() { throw RangeException("seconds"); }

		private static void ThrowMinutesOutOfRange() { throw RangeException("minutes"); }

		private static void ThrowHoursOutOfRange() { throw RangeException("hours"); }

		private static void ThrowDaysOutOfRange() { throw RangeException("days"); }

		internal static bool IsValidTimeOfDay(Int64 x) => InRange(x, 0, NanosInDay - 1);

		internal static bool IsValidNanosComponent(int x) => InRange(x, 0, NanosInSecond - 1);

		internal static bool IsValidMicrosComponent(int x) => InRange(x, 0, 999999);

		internal static bool IsValidMillisComponent(int x) => InRange(x, 0, 999);

		internal static bool IsValidSecondComponent(int x) => InRange(x, 0, 59);

		internal static bool IsValidMinuteComponent(int x) => InRange(x, 0, 59);

		internal static bool IsValidHourComponent(int x) => InRange(x, 0, 23);

		internal static uint CheckNanosComponent(int x)
		{
			if (!IsValidNanosComponent(x))
				ThrowNanosOutOfRange();

			return (UInt32)x;
		}

		#endregion

		internal static class DateTime
		{
			#region Constants
			internal const long Min = HdDateTime.Min;        // 1678-01-01 00:00:00.000000000
			internal const long Max = HdDateTime.Max;        // 2261-12-31 23:59:59.999999999

			// Max/Min values convertible to Int64 nanoseconds
			internal const Int64 MaxTicks = Max / NanosInTick;
			internal const Int64 MinTicks = Min / NanosInTick;
			internal const Int64 MinMicros = Min / NanosInMicrosecond;
			internal const Int64 MaxMicros = Max / NanosInMicrosecond;
			internal const Int64 MinMillis = Min / NanosInMillisecond;
			internal const Int64 MaxMillis = Max / NanosInMillisecond;
			internal const Int64 MinSeconds = Min / NanosInSecond;
			internal const Int64 MaxSeconds = Max / NanosInSecond;
			internal const Int64 MinMinutes = Min / NanosInMinute;
			internal const Int64 MaxMinutes = Max / NanosInMinute;
			internal const Int64 MinHours = Min / NanosInHour;
			internal const Int64 MaxHours = Max / NanosInHour;
			internal const Int64 MinDays = Min / NanosInDay;
			internal const Int64 MaxDays = Max / NanosInDay;

			internal const Int32 MinMinutes32 = (Int32)(Min / NanosInMinute);
			internal const Int32 MaxMinutes32 = (Int32)(Max / NanosInMinute);
			internal const Int32 MinHours32 = (Int32)(Min / NanosInHour);
			internal const Int32 MaxHours32 = (Int32)(Max / NanosInHour);
			internal const Int32 MinDays32 = (Int32)(Min / NanosInDay);
			internal const Int32 MaxDays32 = (Int32)(Max / NanosInDay);

			// DateTime(1970, 1, 1, 0, 0, 0, Utc).Ticks == 621355968000000000
			internal const Int64 Gmt1970Ticks = (NanosInDay / NanosInTick) * DateTimeDaysTo1970;

			internal const Int64 DateTimeMinValueTicks = 0;
			internal const Int64 DateTimeMaxValueTicks = 3155378975999999999; // Maximum representable by .NET DateTime
			internal const Int64 DateTimeDaysTo1970 = 719162;

			// Constants for unsigned division are not used anymore after unsigned division by constant
			// is found to be poorly supported by .NET JIT optimizer
			//private const UInt64 Div100OffsetBefore = (1UL << 63) / 100 * 100;
			//private const UInt64 Div1000000OffsetBefore = (1UL << 63) / 1000000 * 1000000;
			//private const Int64 Div100OffsetAfter = -(Int64)((1UL << 63) / 100);
			//private const Int64 Div1000000OffsetAfter = -(Int64)((1UL << 63) / 1000000);

			#endregion

			#region Utility/Helper methods

			// Divide with rounding towards negative infinity(not towards 0)
			static Int64 Div(Int64 x, Int64 to)
			{
				Int64 sign = x >> 63;
				return (x - sign) / to + sign;
			}

			static Int64 Mod(Int64 x, Int64 to)
			{
				// .NET only seems to optimize signed division by constant but not unsigned.
				// This "branch-free" solution seems to be faster than the comparison-based
				Int64 sign = x >> 63;
				return x - (x - sign) / to * to + (sign & to);
			}

			internal static Byte Mod100(Int64 x) => (Byte)Mod(x, 100);

			// Divide by 100 rounding down to negative infinity
			internal static Int64 Div100(Int64 x) => Div(x, 100);

			internal static Int64 DivMod100(Int64 x, out Int64 modulo)
			{
				Int64 result = Div100(x);
				modulo = x - result * 100U;
				return result;
			}

			internal static Int64 DivMod100(Int64 x, out Byte modulo)
			{
				Int64 result = Div100(x);
				modulo = (Byte)(x - result * 100U);
				return result;
			}

			internal static UInt32 Mod1000000(Int64 x) => (UInt32)Mod(x, 1000000);

			internal static Int64 Div1000000(Int64 x) => Div(x, 1000000);

			internal static Int64 ModDiv(Int64 x, Int64 divider1, Int64 divider2) => Mod(x, divider1) / divider2;

			internal static Int64 RoundTo(Int64 dt, Int64 unitSizeNs)
			{
				return Div(dt, unitSizeNs) * unitSizeNs;
			}

			#endregion

			#region Range checks

			internal static bool IsValidNanos(Int64 x) => InRange(x, Min, Max);

			// TODO: !InRange(x, Max, Min); 
			static bool IsFiniteNanos(Int64 x) => InRange(x, Min + 1, Max - 1);

			// Returns true if given finite Ticks value fits into HdDateTime/HdTimeSpan
			internal static bool IsValidTicks(Int64 x) => InRange(x, MinTicks, MaxTicks);

			internal static bool IsValidMicros(Int64 x) => InRange(x, MinMicros, MaxMicros);

			internal static bool IsValidMillis(Int64 x) => InRange(x, MinMillis, MaxMillis);

			internal static bool IsValidSeconds(Int64 x) => InRange(x, MinSeconds, MaxSeconds);

			internal static bool IsValidMinutes(Int64 x) => InRange(x, MinMinutes, MaxMinutes);

			internal static bool IsValidHours(Int64 x) => InRange(x, MinHours, MaxHours);

			internal static bool IsValidDays(int x) => InRange(x, MinDays, MaxDays);

			// Throws if given finite Ticks value does not fit into HdDateTime/HdTimeSpan
			internal static Int64 CheckTicks(Int64 ticks)
			{
				if (!IsValidTicks(ticks))
					ThrowTicksOutOfRange();

				return ticks;
			}

			internal static Int64 CheckMicros(Int64 microseconds)
			{
				if (!IsValidMillis(microseconds))
					ThrowMicrosOutOfRange();

				return microseconds;
			}

			internal static Int64 CheckMillis(Int64 milliseconds)
			{
				if (!IsValidMillis(milliseconds))
					ThrowMillisOutOfRange();

				return milliseconds;
			}

			internal static Int64 CheckSeconds(Int64 seconds)
			{
				if (!IsValidSeconds(seconds))
					ThrowSecondsOutOfRange();

				return seconds;
			}

			internal static Int64 CheckMinutes(Int64 minutes)
			{
				if (!IsValidMinutes(minutes))
					ThrowMinutesOutOfRange();

				return minutes;
			}

			internal static Int64 CheckHours(Int64 hours)
			{
				if (!IsValidHours(hours))
					ThrowHoursOutOfRange();

				return hours;
			}

			internal static Int64 CheckDays(int days)
			{
				if (!IsValidDays(days))
					ThrowDaysOutOfRange();

				return days;
			}

			#endregion

			#region Unchecked conversions

			internal static Int64 MillisToTicksUnchecked(Int64 x)
			{
				return x * TicksInMillisecond + Gmt1970Ticks;
			}

			internal static Int64 ToTicksUnchecked(Int64 x)
			{
				return Div100(x) + Gmt1970Ticks;
			}

			internal static Int64 ToTicksUnchecked(Int64 x, out Int64 nanosRemainder)
			{
				return DivMod100(x, out nanosRemainder) + Gmt1970Ticks;
			}

			internal static Int64 ToTicksUnchecked(Int64 x, out Byte nanosRemainder)
			{
				return DivMod100(x, out nanosRemainder) + Gmt1970Ticks;
			}

			internal static Int64 TicksToMillisUnchecked(Int64 x)
			{
				return (x / TicksInMillisecond) + -Gmt1970Ticks / TicksInMillisecond;
			}

			internal static Int64 TicksToMillisUnchecked(Int64 x, out Int64 nanosRemainder)
			{
				Int64 result = x / TicksInMillisecond;
				nanosRemainder = (x - result * TicksInMillisecond) * 100;
				return result + -Gmt1970Ticks / TicksInMillisecond;
			}

			#endregion

			#region Conversions with overflow checks

			// TODO: Delete unused
			//internal static Int64 FromTicks(Int64 ticks, Int64 extraNanos)
			//{
			//	// TODO: Better/faster check possible, but this will do for now
			//	// (the cost of checked Add is small in comparison to the multiplication and range check)
			//	return AddDtSpecial(CheckTicks(ticks + -Gmt1970Ticks) * NanosInTick, extraNanos);
			//}
			// Add extraNanos that is guaranteed to only be positive and less than int, for internal use only
			//internal static Int64 FromTicksSpecial(Int64 ticks, Int64 extraNanos)
			//{
			//	return AddSpecial(CheckTicks(ticks + -Gmt1970Ticks) * NanosInTick, extraNanos);
			//}

			private static long AddDtSpecial(Int64 a, Int64 b)
			{
				Int64 x = unchecked(a + b);
				if (x >= Max)
					ThrowNanosOutOfRange();

				return x;
			}

			// Convert to nanoseconds with range check
			internal static Int64 FromDays(int days) => CheckDays(days) * NanosInDay;

			//internal static Int64 FromDays(Int64 days) => CheckDays(days) * NanosInDay;

			internal static Int64 FromHours(Int64 hours) => CheckHours(hours) * NanosInHour;

			internal static Int64 FromMinutes(Int64 minutes) => CheckMinutes(minutes) * NanosInMinute;

			internal static Int64 FromSeconds(Int64 seconds) => CheckSeconds(seconds) * NanosInSecond;

			internal static Int64 FromMillis(Int64 milliseconds) => CheckMillis(milliseconds) * NanosInMillisecond;

			internal static Int64 FromMicros(Int64 microseconds) => CheckMicros(microseconds) * NanosInMicrosecond;

			internal static Int64 FromTicks(Int64 ticks) => CheckTicks(ticks + -Gmt1970Ticks) * NanosInTick;

			internal static Int64 FromTicks(Int64 ticks, Byte extraNanos) => CheckTicks(ticks + -Gmt1970Ticks) * NanosInTick + extraNanos;

			internal static Int64 From(int year, int month, int day)
			{
				return FromTicks(new System.DateTime(year, month, day).Ticks);
			}

			internal static Int64 From(int year, int month, int day, int hour, int minute, int second)
			{
				return FromTicks(new System.DateTime(year, month, day, hour, minute, second).Ticks);
			}

			internal static Int64 From(int year, int month, int day, int hour, int minute, int second, int nanoseconds)
			{
				// TODO: Range check wrong?
				return FromTicks(new System.DateTime(year, month, day, hour, minute, second).Ticks) + CheckNanosComponent(nanoseconds);
			}

			#endregion

			#region Component extraction

			internal static int ExtractHourOfDay(Int64 x)
			{
				return (int)ModDiv(x, NanosInDay, NanosInHour);
			}

			internal static int ExtractMinuteOfHour(Int64 x)
			{
				return (int)ModDiv(x, NanosInHour, NanosInMinute);
			}

			internal static int ExtractSecondOfMinute(Int64 x)
			{
				return (int)ModDiv(x, NanosInMinute, NanosInSecond);
			}

			internal static int ExtractMillisecondOfSecond(Int64 x)
			{
				return (int)ModDiv(x, NanosInSecond, NanosInMillisecond);
			}

			internal static int ExtractMicrosecondOfSecond(Int64 x)
			{
				return (int)ModDiv(x, NanosInSecond, NanosInMicrosecond);
			}

			internal static int ExtractNanosecondOfSecond(Int64 x)
			{
				return (int)Mod(x, NanosInSecond);
			}

			#endregion

			#region Min/Max - preserving conversions

			private static Int64 ToMinMax(Int64 x) => x < 0 ? Min : Max; // TODO: Bit operations possible

			internal static Byte ToTicksRemainder(Int64 x) => Mod100(x);

			internal static Int64 ToMillisWithMinMax(Int64 x) => IsValidNanos(x) ? Div1000000(x) : ToInt64MinMax(x);

			internal static Int64 ToMillisRemainder(Int64 x) => Mod1000000(x);

			//internal static Int64 MillisToNanosUnchecked(Int64 x)
			//{
			//	return IsMinMax(x) ? x : x * NanosInMillisecond;
			//}

			//internal static Int64 MillisToNanosUnchecked(Int64 x, Int64 extraNanos)
			//{
			//	return IsMinMax(x) ? x : x * NanosInMillisecond + extraNanos;
			//}

			//internal static Int64 TicksToNanos(Int64 x)
			//{
			//	return IsMinMax(x) ? x : (x + -Gmt1970Ticks) * 100U;
			//}

			//internal static Int64 TicksToNanos(Int64 x, byte extraNanos)
			//{
			//	return IsMinMax(x) ? x : (x + -Gmt1970Ticks) * 100U + extraNanos;
			//}

			internal static Int64 FromMillisWithMinMax(Int64 millis)
			{
				return IsValidMillis(millis) ? millis * NanosInMillisecond : ToMinMax(AssertInt64MinMax(millis));
			}

			internal static Int64 FromMillisWithMinMax(Int64 millis, Int64 extraNanos)
			{
				// TODO: Optimization opportunity: Add()
				return IsValidMillis(millis) ? AddToDt(millis * NanosInMillisecond, extraNanos) : ToMinMax(AssertInt64MinMax(millis));
			}

			internal static Int64 FromTicksWithMinMax(Int64 ticks)
			{
				if (IsValidTicks(ticks + -Gmt1970Ticks))
					return (ticks + -Gmt1970Ticks) * NanosInTick;

				if (0 == ticks)
					return Min;

				if (DateTimeMaxValueTicks == ticks)
					return Max;

				throw RangeException("ticks");
			}

			internal static Int64 FromNanosWithMinMax(Int64 nanos)
			{
				return IsValidNanos(nanos) ? nanos : ToMinMax(AssertInt64MinMax(nanos));
			}

			internal static FormatComponents ToComponents(Int64 dt)
			{
				FormatComponents components;
				components.sign = 0;
				long sign = dt >> 63;
				long day = (dt - sign) / NanosInDay + sign;
				dt = dt - day * NanosInDay;
				long old = dt;
				dt /= NanosInMinute;
				long sec = old - dt * NanosInMinute;
				old = dt;
				dt /= MinutesInHour;
				long secOld = sec;
				sec /= NanosInSecond;
				components.minute = (int)(old - dt * MinutesInHour);
				components.nanosecond = (int)(secOld - sec * NanosInSecond);
				components.second = (int)sec;
				components.hour = (int)dt;

				// TODO: Optimize, remove dependency from DateTime
				var t = new System.DateTime(day * TicksInDay + Gmt1970Ticks);
				components.day = t.Day;
				components.month = t.Month;
				components.year = t.Year;
				return components;
			}

			#endregion
		}

		internal static class TimeSpan
		{
			#region Constants
			internal const long Min = HdTimeSpan.Min;
			internal const long Max = HdTimeSpan.Max;

			// Max/Min values convertible to Int64 nanoseconds
			internal const Int64 MaxTicks = Max / NanosInTick;
			internal const Int64 MinTicks = Min / NanosInTick;
			internal const Int64 MinMicros = Min / NanosInMicrosecond;
			internal const Int64 MaxMicros = Max / NanosInMicrosecond;
			internal const Int64 MinMillis = Min / NanosInMillisecond;
			internal const Int64 MaxMillis = Max / NanosInMillisecond;
			internal const Int64 MinSeconds = Min / NanosInSecond;
			internal const Int64 MaxSeconds = Max / NanosInSecond;
			internal const Int64 MinMinutes = Min / NanosInMinute;
			internal const Int64 MaxMinutes = Max / NanosInMinute;
			internal const Int64 MinHours = Min / NanosInHour;
			internal const Int64 MaxHours = Max / NanosInHour;
			internal const Int64 MinDays = Min / NanosInDay;
			internal const Int64 MaxDays = Max / NanosInDay;
			internal const Int32 MinMinutes32 = (Int32)(Min / NanosInMinute);
			internal const Int32 MaxMinutes32 = (Int32)(Max / NanosInMinute);
			internal const Int32 MinHours32 = (Int32)(Min / NanosInHour);
			internal const Int32 MaxHours32 = (Int32)(Max / NanosInHour);
			internal const Int32 MinDays32 = (Int32)(Min / NanosInDay);
			internal const Int32 MaxDays32 = (Int32)(Max / NanosInDay);

			// Max/min ranges for seconds and millis that can be converted to .NET TimeSpan Ticks
			internal const Int64 MaxTickSeconds = Int64.MaxValue / TicksInSecond;
			internal const Int64 MinTickSeconds = Int64.MinValue / TicksInSecond;
			internal const Int64 MaxTickMillis = Int64.MaxValue / TicksInMillisecond;
			internal const Int64 MinTickMillis = Int64.MinValue / TicksInMillisecond;
			#endregion

			#region Utility/Helper methods

			internal static Int64 Mod100(Int64 x) => x % 100;

			internal static Int64 Div100(Int64 x) => x / 100;

			internal static Int64 Mod1000000(Int64 x) => x % 1000000;

			internal static Int64 Div1000000(Int64 x) => x / 10000000;

			internal static Int64 RoundTo(Int64 ts, Int64 unitSizeNs)
			{
				return (ts / unitSizeNs) * unitSizeNs;
			}

			#endregion

			#region Range checks

			// Valid .NET TimeSpan millis?
			internal static bool IsValidDotNetMillis(Int64 x)
			{
				return InRange(x, MinTickMillis, MaxTickMillis);
			}

			// Valid .NET TimeSpan seconds?
			internal static bool IsValidDotNetSeconds(Int64 x)
			{
				return InRange(x, MinTickSeconds, MaxTickSeconds);
			}


			static bool IsValidNanos(Int64 x) => InRange(x, Min, Max);

			// TODO: !InRange(x, Max, Min); 
			static bool IsFiniteNanos(Int64 x) => InRange(x, Min + 1, Max - 1);

			static bool IsValidTicks(Int64 x) => InRange(x, MinTicks, MaxTicks);

			static bool IsValidMicros(Int64 x) => InRange(x, MinMicros, MaxMicros);

			static bool IsValidMillis(Int64 x) => InRange(x, MinMillis, MaxMillis);

			static bool IsValidSeconds(Int64 x) => InRange(x, MinSeconds, MaxSeconds);

			static bool IsValidMinutes(Int64 x) => InRange(x, MinMinutes, MaxMinutes);

			static bool IsValidHours(Int64 x) => InRange(x, MinHours, MaxHours);

			static bool IsValidDays(Int64 x) => InRange(x, MinDays, MaxDays);

			static bool IsValidDays(int x) => InRange(x, MinDays, MaxDays);

			// Throws if given finite Ticks value does not fit into HdDateTime/HdTimeSpan
			internal static Int64 CheckTicks(Int64 ticks)
			{
				if (!IsValidTicks(ticks))
					ThrowTicksOutOfRange();

				return ticks;
			}

			internal static Int64 CheckMicros(Int64 microseconds)
			{
				if (!IsValidMillis(microseconds))
					ThrowMicrosOutOfRange();

				return microseconds;
			}

			internal static Int64 CheckMillis(Int64 milliseconds)
			{
				if (!IsValidMillis(milliseconds))
					ThrowMillisOutOfRange();

				return milliseconds;
			}

			internal static Int64 CheckSeconds(Int64 seconds)
			{
				if (!IsValidSeconds(seconds))
					ThrowSecondsOutOfRange();

				return seconds;
			}

			internal static Int64 CheckMinutes(Int64 minutes)
			{
				if (!IsValidMinutes(minutes))
					ThrowMinutesOutOfRange();

				return minutes;
			}

			internal static Int64 CheckHours(Int64 hours)
			{
				if (!IsValidHours(hours))
					ThrowHoursOutOfRange();

				return hours;
			}

			internal static int CheckDays(int days)
			{
				if (!IsValidDays(days))
					ThrowDaysOutOfRange();

				return days;
			}

			internal static Int64 CheckDays(Int64 days)
			{
				if (!IsValidDays(days))
					ThrowDaysOutOfRange();

				return days;
			}

			internal static Int64 CheckDotNetMillis(Int64 milliseconds)
			{
				if (!IsValidDotNetMillis(milliseconds))
					ThrowMillisOutOfRange();

				return milliseconds;
			}

			internal static Int64 CheckDotNetSeconds(Int64 seconds)
			{
				if (!IsValidDotNetSeconds(seconds))
					ThrowSecondsOutOfRange();

				return seconds;
			}
			#endregion

			#region Unchecked operations with .NET Ticks

			internal static Int64 MillisToTicksUnchecked(Int64 x) => x * TicksInMillisecond;

			internal static Int64 TicksToMillisUnchecked(Int64 x) => x / TicksInMillisecond;

			#endregion

			#region Conversions with overflow checks

			// Convert to nanoseconds with range check
			internal static Int64 FromDays(int days) => CheckDays(days) * NanosInDay;

			internal static Int64 FromDays(Int64 days) => CheckDays(days) * NanosInDay;

			internal static Int64 FromHours(Int64 hours) => CheckHours(hours) * NanosInHour;

			internal static Int64 FromMinutes(Int64 minutes) => CheckMinutes(minutes) * NanosInMinute;

			internal static Int64 FromSeconds(Int64 seconds) => CheckSeconds(seconds) * NanosInSecond;

			internal static Int64 FromMillis(Int64 milliseconds) => CheckMillis(milliseconds) * NanosInMillisecond;

			internal static Int64 FromMicros(Int64 microseconds) => CheckMicros(microseconds) * NanosInMicrosecond;

			internal static Int64 FromTicks(Int64 ticks) => CheckTicks(ticks) * NanosInTick;

			internal static Int64 From(int hours, int minutes, int seconds)
			{
				return FromSeconds(hours * 3600L + minutes * 60L + seconds);
			}

			internal static Int64 From(int days, int hours, int minutes, int seconds)
			{
				return FromSeconds(days * 86400L + hours * 3600L + minutes * 60L + seconds);
			}

			internal static Int64 From(int days, int hours, int minutes, int seconds, int nanoseconds)
			{
				return AddToTs(From(days, hours, minutes, seconds), nanoseconds);
			}

			#endregion

			#region Min/Max - preserving conversions

			private static Int64 ToMinMax(Int64 x) => x < 0 ? Min : Max; // TODO: Bit operations possible

			internal static Int64 ToTicksWithMinMax(Int64 x) => IsFiniteNanos(x) ? Div100(x) : ToInt64MinMax(x);

			internal static Int64 ToTicksRemainder(Int64 x) => Mod100(x);

			internal static Int64 ToMillisWithMinMax(Int64 x) => IsFiniteNanos(x) ? Div1000000(x) : ToInt64MinMax(x);

			internal static Int64 ToMillisRemainder(Int64 x) => Mod1000000(x);

			internal static Int64 FromTicksWithMinMaxUnchecked(Int64 x) => IsValidTicks(x) ? x * NanosInTick : ToMinMax(x);

			internal static Int64 FromTicksWithMinMaxUnchecked(Int64 x, Int64 extraNanos) => IsValidTicks(x) ? x * NanosInTick + extraNanos : ToMinMax(x);

			internal static Int64 FromMillisWithMinMaxUnchecked(Int64 x) => IsValidMillis(x) ? x * NanosInMillisecond : ToMinMax(x);

			internal static Int64 FromMillisWithMinMaxUnchecked(Int64 x, Int64 extraNanos)
			{
				return IsValidMillis(x) ? x * NanosInMillisecond + extraNanos : ToMinMax(x);
			}

			internal static FormatComponents ToComponents(long ts)
			{
				FormatComponents components;
				long sign = ts >> 63;
				components.year = components.month = 0;
				components.sign = (int)sign;
				ts = (ts + sign) ^ sign;
				long old = ts;
				ts /= NanosInMinute;
				long sec = old - ts * NanosInMinute;
				old = ts;
				ts /= MinutesInHour;
				long secOld = sec;
				sec /= NanosInSecond;
				components.minute = (int)(old - ts * MinutesInHour);
				components.nanosecond = (int)(secOld - sec * NanosInSecond);
				components.second = (int)sec;
				old = ts;
				ts /= HoursInDay;
				components.hour = (int)(old - ts * HoursInDay);
				components.day = (int)ts;
				return components;
			}

			internal static Int64 TicksToMillis(Int64 x)
			{
				return IsValidTicks(x) ? x / (Int64)TicksInMillisecond : ToInt64MinMax(x);
			}

			// Checks for min/max and also checks allowable range
			internal static Int64 MillisToTicksChecked(Int64 milliseconds)
			{
				return IsValidDotNetMillis(milliseconds) ? milliseconds * TicksInMillisecond : ToMinMax(AssertInt64MinMax(milliseconds));
			}

			internal static Int64 FromTicksWithMinMax(Int64 ticks)
			{
				return IsValidTicks(ticks) ? ticks * NanosInTick : ToMinMax(AssertInt64MinMax(ticks));
			}


			internal static Int64 FromMillisWithMinMax(Int64 millis)
			{
				return IsValidMillis(millis) ? millis * NanosInMillisecond : ToMinMax(AssertInt64MinMax(millis));
			}

			internal static Int64 FromMillisWithMinMax(Int64 millis, Int64 extraNanos)
			{
				// TODO: Optimization opportunity: Add()
				return IsValidMillis(millis) ? AddToTs(millis * NanosInMillisecond, extraNanos) : ToMinMax(AssertInt64MinMax(millis));
			}

			#endregion
		}

		// trash to be removed:

		//internal static Int64 DateTimeTicksToNanosUnchecked(Int64 x)
		//{
		//	return (x + -Gmt1970Ticks) * NanosInTick;
		//}

		//internal static Int64 DateTimeTicksToNanosUnchecked(Int64 x, Int64 extraNanos)
		//{
		//	return (x + -Gmt1970Ticks) * NanosTick + extraNanos;
		//}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void RangeCheck(bool condition, string msg)
		{
			if (!condition)
				ThrowOutOfRange(msg);
		}


		//internal static Int64 CheckDateTimeTicks(Int64 ticks)
		//{
		//	if (!IsValidDotNetDateTimeTicks(ticks))
		//		throw new ArgumentOutOfRangeException("ticks");
		//	return ticks;
		//}

		//internal static Int64 CheckTimeSpanTicks(Int64 ticks)
		//{
		//	if (!IsValidDotNetTimeSpanTicks(ticks))
		//		throw new ArgumentOutOfRangeException("ticks");
		//	return ticks;
		//}
	}
}