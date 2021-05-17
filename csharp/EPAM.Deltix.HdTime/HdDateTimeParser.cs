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
using EPAM.Deltix.HdTime.Old;
using System;
using System.Collections.Generic;


namespace EPAM.Deltix.HdTime
{
	/// <summary>
	///   Used to parse string representation of the <see cref="HdDateTime" />.
	///   Not thread safe.
	/// </summary>
	public class HdDateTimeParser
	{
		private readonly Dictionary<String, String> _allocatedFormats = new Dictionary<String, String>();

		/// <summary>
		///   Converts the specified string representation of a date and time to its <see cref="HdDateTime" /> equivalent.
		/// </summary>
		/// <param name="source">A string containing a date and time to convert.</param>
		/// <returns>
		///   An object that is equivalent to the date and time contained in <paramref name="source" />.
		/// </returns>
		public HdDateTime Parse(String source)
		{
			String ownSource = String.Copy(source);
			return PreciseTimeParse.Parse(ownSource, null);
		}


		/// <summary>
		///   Converts the specified string representation of a date and time to its <see cref="HdDateTime" /> equivalent.
		/// </summary>
		/// <param name="provider">An object that supplies culture-specific format information about <paramref name="source" />.</param>
		/// <param name="source">A string containing a date and time to convert.</param>
		/// <returns>
		///   An object that is equivalent to the date and time contained in <paramref name="source" />.
		/// </returns>
		public HdDateTime Parse(String source, IFormatProvider provider)
		{
			String ownSource = String.Copy(source);
			return PreciseTimeParse.Parse(ownSource, provider);
		}


		/// <summary>
		///   Converts the specified string representation of a date and time to its <see cref="HdDateTime" /> equivalent. The
		///   format of the string representation must match the specified format exactly.
		/// </summary>
		/// <param name="source">A string containing a date and time to convert.</param>
		/// <param name="format">
		///   A format specifier, that defines the required format of <paramref name="source" />. The format of
		///   the string representation must match the specified format exactly.
		/// </param>
		/// <returns>
		///   An object that is equivalent to the date and time contained in <paramref name="source" />, as specified by
		///   <paramref name="format" />.
		/// </returns>
		/// <remarks>
		///   A date and time format, which can be the same as <see cref="System.DateTime" /> format. If you want to specify
		///   nanoseconds formatting, you can use "FFFFFFFFF" (9 Fs) or "fffffffff" (9 fs).
		/// </remarks>
		public HdDateTime ParseExact(String source, String format)
		{
			String ownSource = String.Copy(source);
			String ownFormat = GetAllocatedFormat(format);
			return PreciseTimeParse.ParseExact(ownSource, ownFormat, null);
		}


		/// <summary>
		///   Converts the specified string representation of a date and time to its <see cref="HdDateTime" /> equivalent. The
		///   format of the string representation must match the specified format exactly.
		/// </summary>
		/// <param name="provider">An object that supplies culture-specific format information about <paramref name="source" />.</param>
		/// <param name="source">A string containing a date and time to convert.</param>
		/// <param name="format">
		///   A format specifier, that defines the required format of <paramref name="source" />. The format of
		///   the string representation must match the specified format exactly.
		/// </param>
		/// <returns>
		///   An object that is equivalent to the date and time contained in <paramref name="source" />, as specified by
		///   <paramref name="format" />.
		/// </returns>
		/// <remarks>
		///   A date and time format, which can be the same as <see cref="System.DateTime" /> format. If you want to specify
		///   nanoseconds formatting, you can use "FFFFFFFFF" (9 Fs) or "fffffffff" (9 fs).
		/// </remarks>
		public HdDateTime ParseExact(String source, String format, IFormatProvider provider)
		{
			String ownSource = String.Copy(source);
			String ownFormat = GetAllocatedFormat(format);
			return PreciseTimeParse.ParseExact(ownSource, ownFormat, provider);
		}


		/// <summary>
		///   Converts the specified string representation of a date and time to its <see cref="HdDateTime" /> equivalent and
		///   returns a value that indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="source">A string containing a date and time to convert.</param>
		/// <param name="result">
		///   An object that is equivalent to the date and time contained in <paramref name="source" />.
		/// </param>
		/// <returns>
		///   A value that indicates whether the conversion succeeded.
		/// </returns>
		public Boolean TryParse(String source, out HdDateTime result)
		{
			String ownSource = String.Copy(source);
			return PreciseTimeParse.TryParse(ownSource, null, out result);
		}


		/// <summary>
		///   Converts the specified string representation of a date and time to its <see cref="HdDateTime" /> equivalent and
		///   returns a value that indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="source">A string containing a date and time to convert.</param>
		/// <param name="provider">An object that supplies culture-specific formatting information.</param>
		/// <param name="result">
		///   An object that is equivalent to the date and time contained in <paramref name="source" />.
		/// </param>
		/// <returns>
		///   A value that indicates whether the conversion succeeded.
		/// </returns>
		public Boolean TryParse(String source, IFormatProvider provider, out HdDateTime result)
		{
			String ownSource = String.Copy(source);
			return PreciseTimeParse.TryParse(ownSource, provider, out result);
		}


		/// <summary>
		///   Converts the specified string representation of a date and time to its <see cref="HdDateTime" /> equivalent and
		///   returns a value that indicates whether the conversion succeeded. The format of the string representation must match
		///   the specified format exactly.
		/// </summary>
		/// <param name="source">A string containing a date and time to convert.</param>
		/// <param name="format">
		///   A format specifier, that defines the required format of <paramref name="source" />. The format of
		///   the string representation must match the specified format exactly.
		/// </param>
		/// <param name="result">
		///   An object that is equivalent to the date and time contained in <paramref name="source" />, as specified by
		///   <paramref name="format" />.
		/// </param>
		/// <returns>
		///   A value that indicates whether the conversion succeeded.
		/// </returns>
		/// <remarks>
		///   A date and time format, which can be the same as <see cref="System.DateTime" /> format. If you want to specify
		///   nanoseconds formatting, you can use "FFFFFFFFF" (9 Fs) or "fffffffff" (9 fs).
		/// </remarks>
		public Boolean TryParseExact(String source, String format, out HdDateTime result)
		{
			String ownSource = String.Copy(source);
			String ownFormat = GetAllocatedFormat(format);
			return PreciseTimeParse.TryParseExact(ownSource, ownFormat, null, out result);
		}


		/// <summary>
		///   Converts the specified string representation of a date and time to its <see cref="HdDateTime" /> equivalent and
		///   returns a value that indicates whether the conversion succeeded. The format of the string representation must match
		///   the specified format exactly.
		/// </summary>
		/// <param name="source">A string containing a date and time to convert.</param>
		/// <param name="format">
		///   A format specifier, that defines the required format of <paramref name="source" />. The format of
		///   the string representation must match the specified format exactly.
		/// </param>
		/// <param name="provider">An object that supplies culture-specific formatting information.</param>
		/// <param name="result">
		///   An object that is equivalent to the date and time contained in <paramref name="source" />, as specified by
		///   <paramref name="format" />.
		/// </param>
		/// <returns>
		///   A value that indicates whether the conversion succeeded.
		/// </returns>
		/// <remarks>
		///   A date and time format, which can be the same as <see cref="System.DateTime" /> format. If you want to specify
		///   nanoseconds formatting, you can use "FFFFFFFFF" (9 Fs) or "fffffffff" (9 fs).
		/// </remarks>
		public Boolean TryParseExact(String source, String format, IFormatProvider provider, out HdDateTime result)
		{
			String ownSource = String.Copy(source);
			String ownFormat = GetAllocatedFormat(format);
			return PreciseTimeParse.TryParseExact(ownSource, ownFormat, provider, out result);
		}


		private String GetAllocatedFormat(String format)
		{
			String ownFormat;
			if (_allocatedFormats.ContainsKey(format))
			{
				ownFormat = _allocatedFormats[format];
			}
			else
			{
				ownFormat = String.Copy(format);
				_allocatedFormats.Add(format, ownFormat);
			}
			return ownFormat;
		}
	}
}