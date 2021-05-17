// =============================================================================
// Copyright 2021 EPAM Systems, Inc
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership. Licensed under the Apache License,
// Version 2.0 (the "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the
// License for the specific language governing permissions and limitations under
// =============================================================================
using System;
using Convert = EPAM.Deltix.HdTime.Convert;

namespace EPAM.Deltix.HdTime
{
	/// <summary>
	///   Utility class, which contains constants, used in converting timestamp to/from different formats. 
 	/// </summary>
	internal static class DateConversionConstants
	{
		internal const uint TicksPerMillisecond = Convert.TicksInMillisecond;
		internal static readonly DateTime Gmt1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		internal const Int64 Gmt1970Ticks = Convert.DateTime.Gmt1970Ticks;
	}
	
	/// <summary>
	///   Public utility class which contains methods intended to convert Java timestamps to/from .NET formats.
	/// </summary>
	/// 

	public static class DateConverter
	{
		internal static Int64 DateTimeMillisToTicksChecked(Int64 milliseconds)
		{
			// TODO:
			//if (!Convert.IsValidDotNetDateTimeMilliSeconds(milliseconds))
			//	throw new ArgumentOutOfRangeException("milliseconds");
			return Convert.DateTime.MillisToTicksUnchecked(milliseconds);
		}

		internal static Int64 TimeSpanMillisToTicksChecked(Int64 milliseconds)
		{
			if (!Convert.TimeSpan.IsValidDotNetMillis(milliseconds))
				throw new ArgumentOutOfRangeException("milliseconds");

			return Convert.TimeSpan.MillisToTicksUnchecked(milliseconds);
		}

		/// <summary>
		/// <para/>Converts <see cref="Int64" /> Unix/Java millisecond UTC timestamp to <see cref="System.DateTime" />
		/// </summary>
		/// <param name="milliseconds">Timestamp in milliseconds, starting from January 1, 1970.</param>
		/// <returns>
		///		<para/>Converted <paramref name="milliseconds" />, represented as <see cref="System.DateTime" />.
		///		<para/><see cref="DateTime.MinValue" />, if <paramref name="milliseconds" /> == <see cref="Int64.MinValue" />.
		///		<para/><see cref="DateTime.MaxValue" />, if <paramref name="milliseconds" /> == <see cref="Int64.MaxValue" />.
		/// </returns>
		/// <exception cref="ArgumentOutOfRangeException">
		///   Thrown, when <paramref name="milliseconds" /> is out of available range for <see cref="System.DateTime" />.
		/// </exception>
		public static DateTime FromEpochMilliseconds(Int64 milliseconds)
		{
			return milliseconds == Int64.MinValue ? DateTime.MinValue : milliseconds == Int64.MaxValue ? DateTime.MaxValue
					: new DateTime(DateTimeMillisToTicksChecked(milliseconds), DateTimeKind.Utc);
			// TODO: May silently overflow for certain big values
		}

		[Obsolete("Use FromEpochMilliseconds instead")]
		public static DateTime FromJavaMilliseconds(Int64 milliseconds) => FromEpochMilliseconds(milliseconds);


		/// <summary>
		/// <para/>Converts <see cref="Int64" /> millisecond time offset to <see cref="System.TimeSpan" />
		/// </summary>
		/// <param name="offset">Time offset in milliseconds</param>
		/// <returns>
		/// <para/>Converted <paramref name="offset" />, represented as <see cref="System.TimeSpan" />.
		/// </returns>

		public static TimeSpan FromMilliseconds(Int64 offset)
		{
			return new TimeSpan(Convert.TimeSpan.MillisToTicksChecked(offset));
		}

		[Obsolete("Use FromMilliseconds instead")]
		public static TimeSpan FromJavaOffset(Int64 offset) => FromMilliseconds(offset);


		/// <summary>
		/// <para/>Converts timestamp, represented by <paramref name="timestamp" /> from <see cref="System.DateTime" /> to <see cref="Int64" />.
		/// </summary>
		/// <param name="timestamp">Timestamp to convert</param>
		///
		/// <returns>
		/// <para>Converted <paramref name="timestamp" />, represented as <see cref="Int64" />, which contains only milliseconds
		/// component of the <paramref name="timestamp" />, starting from January 1, 1970 UTC.
		/// </para>
		/// <para/><see cref="Int64.MinValue" />, if <paramref name="timestamp" /> == <see cref="DateTime.MinValue" />.
		/// <para/><see cref="Int64.MaxValue" />, if <paramref name="timestamp" /> == <see cref="DateTime.MaxValue" />.
		/// </returns>
		/// 
		/// <remarks>
		///   Loses timestamp component, that is less than 1 millisecond. Use <see cref="ToJavaMilliseconds(System.DateTime, out System.Int64)" /> to
		///   convert timestamp with no loss.
		/// </remarks>
		public static Int64 ToEpochMilliseconds(DateTime timestamp)
		{
			return timestamp == DateTime.MinValue ? Int64.MinValue : timestamp == DateTime.MaxValue	? Int64.MaxValue
					: Convert.DateTime.TicksToMillisUnchecked(timestamp.Ticks);
		}

		[Obsolete("Use ToEpochMilliseconds instead")]
		public static Int64 ToJavaMilliseconds(DateTime timestamp) => ToEpochMilliseconds(timestamp);

		/// <summary>
		/// <para/>Converts timestamp, represented my <paramref name="timestamp" /> from <see cref="HdDateTime" /> to <see cref="Int64" />.
		/// </summary>
		/// <param name="timestamp">Timestamp to convert</param>
		/// <returns>
		///   <para>
		///     Converted <paramref name="timestamp" />, represented as <see cref="Int64" />, which contains only millisecond
		///     component of the <paramref name="timestamp" /> with 0 corresponding to January 1, 1970 UTC
		///   </para>
		///   <para/><see cref="Int64.MinValue" />, if <paramref name="timestamp" /> == <see cref="HdDateTime.MinValue" />.
		///   <para/><see cref="Int64.MaxValue" />, if <paramref name="timestamp" /> == <see cref="HdDateTime.MaxValue" />.
		/// </returns>
		/// <remarks>
		///   Loses timestamp component, that is less than 1 millisecond. Use <see cref="ToJavaMilliseconds(System.DateTime, out System.Int64)" /> to
		///   convert timestamp with no loss.
		/// </remarks>
		[Obsolete("Use EpochMilliseconds property instead")]
		public static Int64 ToEpochMilliseconds(HdDateTime timestamp) => timestamp.EpochMilliseconds;

		[Obsolete("Use EpochMilliseconds property instead")]
		public static Int64 ToJavaMilliseconds(HdDateTime timestamp) => timestamp.EpochMilliseconds;


		/// <summary>
		///   <para>
		///     Converts timestamp, represented by <paramref name="timestamp" /> from <see cref="DateTime" /> to
		///     <see cref="Int64" />.
		///   </para>
		/// </summary>
		/// <param name="timestamp">Timestamp to convert.</param>
		/// <param name="nanosRemainder">Will contain nanosecond part that was lost after truncation to milliseconds. Range: 0..999999
		///   <para>0, if <paramref name="timestamp" /> == <see cref="DateTime.MinValue" />.</para>
		///   <para>0, if <paramref name="timestamp" /> == <see cref="DateTime.MaxValue" />.</para>
		/// </param>
		/// <returns>
		///   <para>
		///     <paramref name="timestamp" /> truncated to millisecond precision towards negative infinity, as <see cref="Int64" />
		///     with 0 corresponding to January 1, 1970 (Start of Unix Epoch). Can have negative value.
		///   </para>
		///   <para><see cref="Int64.MinValue" />, if <paramref name="timestamp" /> == <see cref="DateTime.MinValue" />.</para>
		///   <para><see cref="Int64.MaxValue" />, if <paramref name="timestamp" /> == <see cref="DateTime.MaxValue" />.</para>
		/// </returns>
		public static Int64 ToEpochMilliseconds(DateTime timestamp, out Int64 nanosRemainder)
		{
			nanosRemainder = 0;
			return timestamp == DateTime.MinValue ? Int64.MinValue : timestamp == DateTime.MaxValue ? Int64.MaxValue
					: Convert.DateTime.TicksToMillisUnchecked(timestamp.Ticks, out nanosRemainder);
		}

		[Obsolete("Use ToEpochMilliseconds instead")]
		public static Int64 ToJavaMilliseconds(DateTime timestamp, out Int64 nanosRemainder) =>
			ToEpochMilliseconds(timestamp, out nanosRemainder);


		/// <summary>
		/// <para/>Converts time offset from .NET TimeSpan to <see cref="Int64" /> milliseconds.
		/// </summary>
		/// <param name="timeSpan">TimeSpan to be converted to millisecond time interval</param>
		/// <returns>
		/// <para/>A time offset expressed in <see cref="Int64" /> milliseconds.
		/// </returns>
		public static Int64 ToMilliseconds(TimeSpan timeSpan) => Convert.TimeSpan.TicksToMillis(timeSpan.Ticks);

		[Obsolete("Use ToMilliseconds instead")]
		public static Int64 ToJavaOffset(TimeSpan timeSpan) => ToMilliseconds(timeSpan);
	}
}