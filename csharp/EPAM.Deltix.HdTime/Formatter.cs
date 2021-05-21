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
using System.Diagnostics;
using System.Globalization;

/*
 * Old DateTime formatter, left for reference
 */
namespace EPAM.Deltix.HdTime.Old
{
	internal static class Formatter
	{
		internal const Int32 CharNotFound = -1;
		internal const Int32 DateTimeFormatSymbolsCount = 7;
		
		/// <summary>
		///   PreciseTime format, used to format timestamp in a string. Contains years, month, days, hours, minutes, seconds and
		///   milliseconds.
		/// </summary>
		internal static readonly String PreciseTimeFormat = "yyyy-MM-dd HH:mm:ss.fffffffff";
		internal static readonly String PreciseTimeFormatAdjusted = "yyyy-MM-dd HH:mm:ss.fffffff##";

		/// <summary>
		/// Strict compliance formatting char. as in <see cref="System.DateTime"/> formatting.
		/// </summary>
		internal static readonly Char LowerCaseExtendedDateTimeFormatChar = 'f';

		/// <summary>
		/// Lax compliance formatting char, as in <see cref="System.DateTime"/> formatting.
		/// </summary>
		internal static readonly Char UpperCaseExtendedDateTimeFormatChar = 'F';

		private static readonly Char SupersedingChar = '#';
		private static readonly String SupersedingString = "#";
		private static readonly String DoubledSupersedingStirng = "##";

		internal static String GetDefaultFormat()
		{
			return PreciseTimeFormat;
		}


		internal unsafe static Char* ToStringReverse(Char *to, UInt64 x)
		{
			do
			{	// This is to make the code generator generate 1 DIV instead of 2
				UInt64 old = x + '0';
				x /= 10U;
				*to-- = (char)(old - x * 10U); // = [old - new * 10]
			} while (x != 0);

			return to;
		}

		internal unsafe static Char* ToStringReverse(Char* to, UInt64 x, uint n)
		{
			do
			{   // This is to make the code generator generate 1 DIV instead of 2
				UInt64 old = x + '0';
				x /= 10U;
				*to-- = (char)(old - x * 10U); // = [old - new * 10]
			} while (n-- > 0);

			return to;
		}


		internal static String ToString(UInt64 x, uint n)
		{
			unsafe
			{
				char* buffer = stackalloc char[32];
				buffer[31] = '\0';
				return new String(ToStringReverse(buffer + 30, x, n));
			}
		}


		internal static unsafe void RollbackPreciseTimeSourceReplacement(String source, Int32 position, Int32 count, Byte nanoseconds)
		{
			fixed (Char* pStr = source)
			{
				Char* ch = pStr + position;

				Int32 digitPosition = 1;
				while (count > 0)
				{
					Int32 digit = SubstractDigit(nanoseconds, digitPosition);
					*ch = ConvertInt32ToChar(digit);
					ch++;
					count --;
					digitPosition++;
				}
			}
		}


		internal static void RollbackPreciseTimeFormatReplacement(String formant, Int32 position, Int32 count, Boolean isLowerCaseFormat)
		{
			// replaces without any assert
			ReplaceChars(formant, position, count, isLowerCaseFormat ? LowerCaseExtendedDateTimeFormatChar : UpperCaseExtendedDateTimeFormatChar);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="timeFormat"></param>
		/// <param name="preciseTimeFormatPosition"></param>
		/// <param name="preciseTimeFormatSymbolsCount"></param>
		/// <param name="isLowerCaseFormat"></param>
		/// <returns>True, if the format is found.</returns>
		internal static Boolean FindAndReplacePreciseTimeFormat(
			String timeFormat,
			out Int32 preciseTimeFormatPosition,
			out Int32 preciseTimeFormatSymbolsCount,
			out Boolean isLowerCaseFormat)
		{
			Boolean isFound = FindPreciseTimeFormat(timeFormat, out preciseTimeFormatPosition, out preciseTimeFormatSymbolsCount, out isLowerCaseFormat);
			if (!isFound)
			{
				return false;
			}

			ReplaceChars(timeFormat, preciseTimeFormatPosition, preciseTimeFormatSymbolsCount, SupersedingChar);
			return true;
		}


		internal static Boolean FindPreciseTimeFormat(String timeFormat,
			out Int32 preciseTimeFormatPosition,
			out Int32 preciseTimeFormatSymbolsCount,
			out Boolean isLowerCaseFormat)
		{
			Int32 startPosition, endPosition, caretPosition = 0;
			Boolean isFormatFound = TryFindPreciseTimeFormat(timeFormat, caretPosition, out startPosition, out endPosition);
			if (!isFormatFound)
			{
				preciseTimeFormatPosition = -1;
				preciseTimeFormatSymbolsCount = 0;
				isLowerCaseFormat = true;
				return false;
			}

			// there is no reason to store precise time format
			// we replace just precise time format occurence
			// and indicate, that this format is UpperCase or LowerCase
			Debug.Assert(endPosition >= startPosition);
			preciseTimeFormatPosition = startPosition;
			preciseTimeFormatSymbolsCount = endPosition - startPosition + 1;
			isLowerCaseFormat = timeFormat[preciseTimeFormatPosition] == LowerCaseExtendedDateTimeFormatChar;
			return true;
		}

#if HasMutableString
		/// <summary>
		///   Takes away all occurences of PreciseTime formatting. Lefts DateTime format.
		/// </summary>
		/// <param name="timeFormat"></param>
		/// <param name="dateTimeFormat"></param>
		/// <param name="preciseTimeFormatAndPosition"></param>
		/// <returns></returns>
		internal static Boolean TakeAwayPreciseTimeFormat(
			String timeFormat,
			out MutableString dateTimeFormat,
			out IDictionary<Int32, String> preciseTimeFormatAndPosition)
		{
			Int32 startPosition, endPosition, caretPosition = 0;
			Boolean isFormatFound = TryFindPreciseTimeFormat(timeFormat, caretPosition, out startPosition, out endPosition);
			if (!isFormatFound)
			{
				dateTimeFormat = null;
				preciseTimeFormatAndPosition = null;
				return false;
			}

			Dictionary<Int32, String> preciseTimeFormats = new Dictionary<Int32, String>();
			dateTimeFormat = new MutableString();

			Int32 length = endPosition - startPosition + 1;
			preciseTimeFormats.Add(startPosition, timeFormat.Substring(startPosition, length));

			dateTimeFormat.Append(timeFormat.Substring(caretPosition, startPosition - caretPosition));

			caretPosition = endPosition;
			isFormatFound = TryFindPreciseTimeFormat(timeFormat, caretPosition, out startPosition, out endPosition);
			while (isFormatFound)
			{
				length = endPosition - startPosition + 1;
				preciseTimeFormats.Add(startPosition, timeFormat.Substring(startPosition, length));

				dateTimeFormat.Append(timeFormat.Substring(caretPosition + 1, startPosition - caretPosition - 1));

				caretPosition = endPosition;
				isFormatFound = TryFindPreciseTimeFormat(timeFormat, caretPosition, out startPosition, out endPosition);
			}

			// at least length >= 1
			// append the last part of date time format, in case, if it exists.
			if (caretPosition + length < timeFormat.Length)
			{
				dateTimeFormat.Append(timeFormat.Substring(caretPosition + 1));
			}

			preciseTimeFormatAndPosition = preciseTimeFormats;
			return true;
		}
#endif


		internal static void CorrectPreciseTimeFormat(ref String format, Int32 position, Int32 count)
		{
			format = format.Remove(position, count);
		}
		

		internal static void ExtractAndReplacePreciseTimeSource(
			ref String timeSource,
			Int32 preciseTimeFormatPosition,
			Int32 preciseTimeFormatSymbolsCount,
			Boolean isLowerCaseFormat, 
			out Byte preciseTimeSource, 
			out Int32 preciseTimeSourceSymbolsCount)
		{
			Boolean isUpperCaseFormat = !isLowerCaseFormat;
			// find
			Int32 preciseTimeSourcePosition = 0;
			preciseTimeSourceSymbolsCount = isUpperCaseFormat
				? FindPreciseTimeSourceLength(timeSource, preciseTimeFormatPosition, preciseTimeFormatSymbolsCount, out preciseTimeSourcePosition)
				: preciseTimeFormatSymbolsCount;

			// extract
			preciseTimeSource = (Byte)ConstructNumberFromChars(
				timeSource,
				isUpperCaseFormat ? preciseTimeSourcePosition : preciseTimeFormatPosition, // if upper case, we use changed source position. it is <= preciseTimeFormatPosition.
				preciseTimeSourceSymbolsCount,
				preciseTimeFormatSymbolsCount == 1 ? 2 : preciseTimeFormatSymbolsCount //
			);

			// in case when there is lossy upper case format and the last byte is lost.
			// we do not need additional F symbol in format
			// because parsing exact in DateTime will fail. (format will not be as the source).
			if (isUpperCaseFormat && preciseTimeSource % 10 == 0)  // some part of source can be lost.
			{
				if (preciseTimeSource == 0)
				{   // preciseTime.TimestampModulo == 0   =>  we insert special symbols foreach expected nanos format symbol. 
					// There are can be 2 or 1 symbols. Depends on preciseTimeFormatSymbolsCount
					timeSource = timeSource.Insert(preciseTimeSourcePosition, preciseTimeFormatSymbolsCount == 2 ? DoubledSupersedingStirng : SupersedingString);	
				}
				else
				{
					if (preciseTimeFormatSymbolsCount == 2) // only ones is lost. so we insert special symbol onto the ones position.
					{
						timeSource = timeSource.Insert(preciseTimeSourcePosition + 1, SupersedingString);	
					}
				}
			}
			
			// replace existing nano source chars.
			ReplaceChars(timeSource, preciseTimeFormatPosition, preciseTimeSourceSymbolsCount, SupersedingChar);
			
			// we do not need to remember the length of the preciseTimeSourceLength
			// we can find out from preciseTimeSource, preciseTimeFormatPosition, preciseTimeFormatSymbolsCount and isLowerCaseFormat
		}
		

		internal static Boolean FindAndReplacePreciseTimeSource(
			String source,
			out Byte preciseTimeSource,
			out Int32 preciseTimeDigitsCount,
			out Int32 preciseTimeDigitsPosition)
		{
			Int32 digitSequenceCount = 0;
			Int32 endOfSequencePosition = -1;
			for (Int32 index = 0; index < source.Length; index++)
			{
				// find sequence of 8 or 9 digits in source
				Char symbol = source[index];
				if (Char.IsDigit(symbol))
				{
					digitSequenceCount++;
				}
				else
				{
					// stop, if sequence is already found
					if (digitSequenceCount > DateTimeFormatSymbolsCount)
					{
						endOfSequencePosition = index - 1;
						break;
					}

					digitSequenceCount = 0;
				}
			}

			// in case, if the sequence is in the end of the source
			if (endOfSequencePosition == -1)
			{
				if (digitSequenceCount > DateTimeFormatSymbolsCount)
				{
					endOfSequencePosition = source.Length - 1;
				}
				else
				{
					preciseTimeSource = 0;
					preciseTimeDigitsCount = preciseTimeDigitsPosition = 0;
					return false;
				}
			}

			// count digits positions
			preciseTimeDigitsCount = digitSequenceCount - DateTimeFormatSymbolsCount;
			preciseTimeDigitsPosition = endOfSequencePosition - preciseTimeDigitsCount + 1;

			// get digits of nanoseconds
			preciseTimeSource = (Byte) ConstructNumberFromChars(source, preciseTimeDigitsPosition, preciseTimeDigitsCount, 2);
			
			// replace digits of nanoseconds with '1'. it helps to avoid incorrect DateTime.Parse rounding of ticks.
			ReplaceChars(source, preciseTimeDigitsPosition, preciseTimeDigitsCount, '1');

			return true;
		}


		private static Boolean TryFindPreciseTimeFormat(String format, Int32 initialPosition, out Int32 startPosition, out Int32 endPosition)
		{
			Boolean isFound = false;
			startPosition = endPosition = 0;

			for (Int32 index = initialPosition; index < format.Length; index++)
			{
				Char c = format[index];
				if (IsNotPreciseTimeFormatSymbol(c))
				{
					continue;
				}

				endPosition = FindSequenceEnd(format, index, c);
				Int32 sequenceLength = endPosition - index + 1;

				if (IsNotPreciseTimeFormat(sequenceLength))
				{
					index = endPosition;
					continue;
				}

				if (IsNotSupportedFormat(sequenceLength))
				{
					throw new FormatException("Invalid format.");
				}

				startPosition = index + DateTimeFormatSymbolsCount;
				isFound = true;
				break;
			}

			if (!isFound)
			{
				endPosition = startPosition = -1;
			}

			return isFound;
		}


		private static Int64 ConstructNumberFromChars(String source, Int32 offset, Int32 length, Int32 expectedLength)
		{
			Int64 result = 0;

			Int32 coefficient = Pow(10, expectedLength - 1);
			for (Int32 index = offset; index < offset + length; index++)
			{
				Debug.Assert(source.Length > index, "Index is out of range.");
				Char digit = source[index];
				result += ConvertCharToInt32(digit) * coefficient;

				Debug.Assert(coefficient != 0);
				coefficient /= 10;
			}

			return result;
		}


		private static Int32 ConvertCharToInt32(Char value)
		{
			return ((Int32) value - 0x30);
		}


		private static Char ConvertInt32ToChar(Int32 value)
		{
			return (Char) (value + 0x30);
		}


		private static Int32 FindPreciseTimeSourceLength(String sourceChars, Int32 preciseTimeFormatPosition, Int32 preciseTimeFormatLength, out Int32 preciseTimeSourcePosition)
		{
			Int32 preciseTimeSourceLength;
			// find sourcePosition
			Int32 timeSourceDigits = CalculateDigitLength(
				sourceChars,
				preciseTimeFormatPosition - DateTimeFormatSymbolsCount,
				DateTimeFormatSymbolsCount + preciseTimeFormatLength);

			// if there is no digits
			// date time cuts also previous punctuation symbol before expected digit (for example it time hh.MM.ss.fff should be 12.22.45., but really it is 12.22.45 (without '.'))
			if (timeSourceDigits == 0)
			{
				// getting the previous symbol
				Int32 previousCharPosition = preciseTimeFormatPosition - DateTimeFormatSymbolsCount - 1;
				if (sourceChars.Length > previousCharPosition && previousCharPosition >= 0 && Char.IsPunctuation(sourceChars[previousCharPosition]))
				{	// if previous symbol exists && it is punctuation
					preciseTimeSourcePosition = preciseTimeFormatPosition - DateTimeFormatSymbolsCount;
				}
				else if (sourceChars.Length > previousCharPosition + 1 && previousCharPosition + 1 >= 0 && Char.IsPunctuation(sourceChars[previousCharPosition + 1]))
				{	// if previous symbol does not exist
					preciseTimeSourcePosition = preciseTimeFormatPosition - DateTimeFormatSymbolsCount;
				}
				else
				{	// 
					preciseTimeSourcePosition = preciseTimeFormatPosition - DateTimeFormatSymbolsCount - 1;
				}

				preciseTimeSourceLength = 0;
			}
			else
			{
				Int32 dif = timeSourceDigits - DateTimeFormatSymbolsCount;

				preciseTimeSourceLength = dif < 0 ? 0 : dif;
				preciseTimeSourcePosition = preciseTimeFormatPosition - DateTimeFormatSymbolsCount + timeSourceDigits - preciseTimeSourceLength;
			}

			return preciseTimeSourceLength;
		}

		
		private static Int32 CalculateDigitLength(String source, Int32 position, Int32 expectedLength)
		{
			Int32 index;
			for (index = position; index < position + expectedLength && index < source.Length; index ++)
			{
				Char sourceChar = source[index];
				if (Char.IsDigit(sourceChar))
				{
					continue;
				}

				break;
			}

			return index - position;
		}

		private static Int32 FindSequenceEnd(String format, Int32 startPosition, Int32 sequenceChar)
		{
			Int32 endPosition = startPosition;
			for (Int32 index = startPosition; index < format.Length; index++)
			{
				Char c = format[index];
				if (c != sequenceChar)
				{	
					endPosition = index == startPosition ? index : index - 1;
					break;
				}

				if (index + 1 == format.Length)
				{
					endPosition = index;
				}
				
				//endPosition++;
			}

			return endPosition;//- 1;
		}


		private static Boolean IsNotPreciseTimeFormat(Int32 formatSymbolsCount)
		{
			return formatSymbolsCount < 8;
		}


		private static Boolean IsNotPreciseTimeFormatSymbol(Char c)
		{
			return c != LowerCaseExtendedDateTimeFormatChar && c != UpperCaseExtendedDateTimeFormatChar;
		}


		private static Boolean IsNotSupportedFormat(Int32 formatSymbolsCount)
		{
			return formatSymbolsCount > 9;
		}


		/// <summary>
		///   Used to pow integers. Using Math.Pow with doubles can loose precision.
		/// </summary>
		/// <param name="base"></param>
		/// <param name="exponent"></param>
		/// <returns></returns>
		private static Int32 Pow(Int32 @base, Int32 exponent)
		{
			Int32 result = @base;
			if (exponent == 0)
			{
				return 1;
			}

			for (Int32 index = 1; index < exponent; index++)
			{
				result *= @base;
			}
			return result;
		}


		internal static unsafe void ReplaceChars(String source, Int32 position, Int32 count, Char replacementChar)
		{
			fixed (Char* pStr = source)
			{
				Char* ch = pStr + position;

				while (count > 0)
				{
					*ch = replacementChar;
					ch++;
					count --;
				}
			}
		}


		private static Int32 SubstractDigit(Byte value, Int32 digitPosition)
		{
			// value is less than 100
			Debug.Assert(digitPosition > 0, "Incorrect digit position.");
			Debug.Assert(value < 100, "Incorrect value");

			if (digitPosition == 1)
				return value / 10;

			if (digitPosition == 2)
				return value % 10;

			throw new NotSupportedException();
		}
	}


	internal static class PreciseTimeFormat
	{
		public static String Format(HdDateTime time, String format, IFormatProvider formatProvider)
		{
			String localFormat;
			bool isDefaultFormat = false;
			if (String.IsNullOrEmpty(format))
			{
				isDefaultFormat = true;
				localFormat = Formatter.GetDefaultFormat();
			}
			else
			{
				localFormat = String.Copy(format); // Does not actually copy an interned string
			}

			Byte timestampModulo = time.TimestampModulo;
			Boolean isZeroModulo = timestampModulo == 0;
			Boolean isZeroModuloTens = timestampModulo / 10 == 0;
			

			// ptf is PreciseTimeFormat
			Int32 ptfPosition, ptfSymbolsCount;
			Boolean isPtfLowerCase;
			Boolean isPtfFound = Formatter.FindPreciseTimeFormat(localFormat, out ptfPosition, out ptfSymbolsCount, out isPtfLowerCase);
			if (!isPtfFound)
			{
				return time.RawDateTime.ToString(format);
			}

			// if format is in the middle.
			if (ptfPosition + ptfSymbolsCount < localFormat.Length)
			{
				throw new FormatException(String.Format("Format {0} is unsupported.", format));
			}

			Boolean isPtfUpperCase = !isPtfLowerCase;
			// case, when we can substring format, because nanoseconds should not be represented in formatting result.
			if (isPtfUpperCase && (isZeroModulo || (ptfSymbolsCount == 1 && isZeroModuloTens)))
			{
				return time.RawDateTime.ToString(localFormat.Substring(0, localFormat.Length - ptfSymbolsCount));	
			}

			Boolean isZeroModuloCount = timestampModulo % 10 == 0 && ptfSymbolsCount == 2;
			if (isPtfUpperCase && isZeroModuloCount)
			{
				localFormat = localFormat.Substring(0, localFormat.Length - 1);
				ptfSymbolsCount--;
			}

			if (isDefaultFormat)
			{
				localFormat = Formatter.PreciseTimeFormatAdjusted;
			}
			else
			{
				if (isPtfUpperCase && ptfSymbolsCount > 0)
					CorrectDateTimeFormat(localFormat, ptfPosition);

				Formatter.ReplaceChars(localFormat, ptfPosition, ptfSymbolsCount, '#');
			}

			Formatter.ReplaceChars(localFormat, ptfPosition, ptfSymbolsCount, '#');

			try
			{
				String dateTimeFormatterResult = time.RawDateTime.ToString(localFormat);
				Formatter.RollbackPreciseTimeSourceReplacement(dateTimeFormatterResult,
					dateTimeFormatterResult.Length - ptfSymbolsCount, ptfSymbolsCount, timestampModulo);

				return dateTimeFormatterResult;
			}
			finally
			{
				if (0 != ptfSymbolsCount && !isDefaultFormat)
					Formatter.RollbackPreciseTimeFormatReplacement(
						localFormat,
						ptfPosition - Formatter.DateTimeFormatSymbolsCount,
						ptfSymbolsCount + Formatter.DateTimeFormatSymbolsCount,
						isPtfLowerCase);
			}
		}

#if HasMutableString
		// this method is unused, because it can partically format timestamp, including intermediate nanoseconds format.
		private static String FormatMiddleOccurences(HdDateTime time, String format, IFormatProvider formatProvider)
		{
			if (String.IsNullOrEmpty(format))
			{
				format = HdDateTimeFormatter.GetDefaultFormat();
			}

			// takes away all precise time formats.
			MutableString dateTimeFormat;
			IDictionary<Int32, String> preciseTimeFormats;
			Boolean isPreciseTimeFormat = HdDateTimeFormatter.TakeAwayPreciseTimeFormat(format, out dateTimeFormat, out preciseTimeFormats);
			if (!isPreciseTimeFormat)
			{
				return time.RawDateTime.ToString(format, formatProvider);
			}

			Byte timestampModulo = time.TimestampModulo;
			// in case, when preciseTime is GMT time with nanos component we skip its formatting
			if (time.Nanoseconds / 100 != 0)
			{ // replace all upper case FFFF symbols with ffff, in case, when whey are upper case and timestamp ticks != 0
				CorrectDateTimeFormat(dateTimeFormat, preciseTimeFormats, timestampModulo);
			}

			MutableString preciseTimeFormattingResult = FormatTimestamp(time.RawDateTime, dateTimeFormat.ToString(), formatProvider);

			ValidateDateTimeFormat(format, preciseTimeFormattingResult);
			FormatTimestampModuloOccurences(timestampModulo, preciseTimeFormats, preciseTimeFormattingResult);

			return preciseTimeFormattingResult.ToString();
		}


		private static void CorrectDateTimeFormat(MutableString dateTimeFormat, IDictionary<Int32, String> preciseTimeFormatPositions, Byte timestampModulo)
		{
			Boolean isZeroModulo = timestampModulo == 0;

			if (!isZeroModulo)
			{
				Int32 cutLength = 0;
				foreach (var preciseTimeFormat in preciseTimeFormatPositions)
				{
					Int32 formatPosition = preciseTimeFormat.Key;

					Int32 previousSymbolPosition = formatPosition - 1;
					Char previousSymbol = dateTimeFormat[previousSymbolPosition - cutLength];

					if (previousSymbol == HdDateTimeFormatter.UpperCaseExtendedDateTimeFormatChar)
					{
						Int32 dateTimeTicksFormatPosition = formatPosition - HdDateTimeFormatter.DateTimeFormatSymbolsCount;
						for (Int32 index = dateTimeTicksFormatPosition; index < formatPosition; index++)
						{
							dateTimeFormat[index - cutLength] = HdDateTimeFormatter.LowerCaseExtendedDateTimeFormatChar;
						}
					}

					cutLength += preciseTimeFormat.Value.Length;
				}
			}
		}
#endif

		private static void CorrectDateTimeFormat(String dateTimeFormat, Int32 preciseTimeFormatPosition)
		{
			Formatter.ReplaceChars(dateTimeFormat, preciseTimeFormatPosition - Formatter.DateTimeFormatSymbolsCount, Formatter.DateTimeFormatSymbolsCount, Formatter.LowerCaseExtendedDateTimeFormatChar);
		}

#if HasMutableString
		private static MutableString FormatTimestamp(DateTime timestamp, String dateTimeFormat, IFormatProvider formatProvider)
		{
			MutableString formattingResult = new MutableString(timestamp.ToString(dateTimeFormat, formatProvider));
			return formattingResult;
		}
#endif


		private static String FormatTimestampModulo(Byte timestampModulo, String preciseTimeFormat)
		{
			Boolean isLowerCaseFormat = preciseTimeFormat[0] == Formatter.LowerCaseExtendedDateTimeFormatChar;

			if (!isLowerCaseFormat && (timestampModulo == 0 || (timestampModulo / 10 == 0 && preciseTimeFormat.Length == 1 )))
			{
				return String.Empty;
			}

			String timestampModuloFormattingResult = timestampModulo.ToString("00");
			if (preciseTimeFormat.Length == 1)
			{
				timestampModuloFormattingResult = timestampModuloFormattingResult.Remove(1);
			}

			if (isLowerCaseFormat)
			{
				return timestampModuloFormattingResult;
			}

			if (timestampModulo % 10 == 0)
			{
				return timestampModuloFormattingResult[0].ToString();
			}

			return timestampModuloFormattingResult;
		}

#if HasMutableString
		/// <exception cref="FormatException">Throws, when format <paramref name="format"/> is unsupported.</exception>
		private static void ValidateDateTimeFormat(String format, MutableString source)
		{
			if (format.Length != source.Length)
			{
				throw new FormatException(String.Format("Unsupported format {0}", format));
			}
		}

		private static void FormatTimestampModuloOccurences(Byte timestampModulo, IDictionary<Int32, String> preciseTimeFormats, MutableString formattingResult)
		{
			foreach (var preciseTimeFormat in preciseTimeFormats)
			{
				String timestampModuloFormatting = FormatTimestampModulo(timestampModulo, preciseTimeFormat.Value);
				Insert(formattingResult, preciseTimeFormat.Key, timestampModuloFormatting);
			}
		}


			private static void Insert(MutableString source, Int32 position, String destination)
			{
				if (destination == String.Empty)
					return;

				source.Insert(position, destination);
			}
#endif
	}


	internal static class PreciseTimeParse
	{
		public static HdDateTime Parse(String source, IFormatProvider formatProvider)
		{
			HdDateTime result;
			Boolean isParsed = TryParse(source, formatProvider, out result);
			if (!isParsed)
			{
				throw new FormatException("Not a valid PreciseTime format.");
			}

			return result;
		}


		/// <exception cref="FormatException">Thrown, when <paramref name="format" /> is not valid PreciseTime format.</exception>
		/// <exception cref="ArgumentNullException">Thrown, when <paramref name="source" /> is <see langword="null" />.</exception>
		public static HdDateTime ParseExact(String source, String format, IFormatProvider formatProvider)
		{
			HdDateTime result;
			Boolean isParsed = TryParseExact(source, format, formatProvider, out result);
			if (!isParsed)
			{
				throw new FormatException("Not a valid PreciseTime format.");
			}

			return result;
		}


		public static Boolean TryParse(String source, IFormatProvider formatProvider, out HdDateTime hdDateTime)
		{
			if (String.IsNullOrEmpty(source))
			{
				throw new ArgumentNullException("source", "source is null.");
			}

			Byte preciseTimeSource;
			Int32 preciseTimeCount, preciseTimePosition;
			
			// find and replace preciseTime source
			Boolean isPreciseTimeFormat = Formatter.FindAndReplacePreciseTimeSource(source, out preciseTimeSource, out preciseTimeCount, out preciseTimePosition);

			// parse time
			DateTime dateTime;
			Boolean isSuccessfullyParsed = false;

			try
			{
				isSuccessfullyParsed = DateTime.TryParse(source, formatProvider, DateTimeStyles.None, out dateTime);
				hdDateTime = new HdDateTime(dateTime, preciseTimeSource);
				return isSuccessfullyParsed;
			}

			finally
			{
				// rollback replacement
				if (isPreciseTimeFormat)
					Formatter.RollbackPreciseTimeSourceReplacement(source, preciseTimePosition, preciseTimeCount, preciseTimeSource);
			}
		}


		/// <exception cref="NotSupportedException">Thrown, when parsing format contains more than 3 occurences of precise time formatting.</exception>
		/// <exception cref="FormatException">Thrown, when multiple occurences of precise time formatting exist, and precise time source is not identical in all of the cases.</exception>
		/// <exception cref="ArgumentNullException">Thrown, when <paramref name="source"/> or <paramref name="format"/> is <see langword="null" />.</exception>
		public static Boolean TryParseExact(String source, String format, IFormatProvider formatProvider, out HdDateTime result)
		{
			if (String.IsNullOrEmpty(source))
			{
				throw new ArgumentNullException("source", "source is null.");
			}

			if (String.IsNullOrEmpty(format))
			{
				throw new ArgumentNullException("format", "format is null.");
			}

			// we can parse from exactly 3 occurences of precise time format in format and source. 
			// if it will be more, we NotSupportedException will be thrown.

			// ptf is PreciseTimeFormat
			// pts is PreciseTimeSource
			Int32 ptfPosition1, ptfCount1, ptsCount1;
			Boolean isLowerCasePtf1;
			
			// first occurence of precise time format and source
			Boolean isPreciseTimeFormat = Formatter.FindAndReplacePreciseTimeFormat(format, out ptfPosition1, out ptfCount1, out isLowerCasePtf1);
			if (!isPreciseTimeFormat)
			{
				DateTime dateTime;
				Boolean isParsed = DateTime.TryParseExact(source, format, formatProvider, DateTimeStyles.None, out dateTime);
				result = dateTime;
				return isParsed;
			}

			// due to contract with DateTime ParseExact
			// source of ticks (nanoseconds) must be the same in all of the occurences
			// it means, that nanoseconds in the first occurence are the same as in other; otherwise FormatException will be thrown.
			Byte ptfSource1;
			Formatter.ExtractAndReplacePreciseTimeSource(ref source, ptfPosition1, ptfCount1, isLowerCasePtf1, out ptfSource1, out ptsCount1);
			
			Int32 ptfPosition2, ptfCount2, ptsCount2 = 0, ptfPosition3 = 0, ptfCount3 = 0, ptsCount3 = 0;
			Boolean isLowerCasePtf2, ptfExists2, isLowerCasePtf3 = false, ptfExists3 = false;
			Byte ptfSource2 = 0, ptfSource3 = 0;
			
			// second occurence of precise time format and source
			ptfExists2 = Formatter.FindAndReplacePreciseTimeFormat(format, out ptfPosition2, out ptfCount2, out isLowerCasePtf2);
			if (ptfExists2)
			{ // add reverted offset, because of format != source in case of UpperCase letters.
				Formatter.ExtractAndReplacePreciseTimeSource(ref source, ptfPosition2, ptfCount2, isLowerCasePtf2, out ptfSource2, out ptsCount2);
				if (ptfSource1 != ptfSource2)
				{
					throw new FormatException("In multiple occurences of precise time format, precise time source is not identical in all of the cases.");
				}
				
				// third occurence of precise time format and source
				ptfExists3 = Formatter.FindAndReplacePreciseTimeFormat(format, out ptfPosition3, out ptfCount3, out isLowerCasePtf3);
				if (ptfExists3)
				{
					Formatter.ExtractAndReplacePreciseTimeSource(ref source, ptfPosition3, ptfCount3, isLowerCasePtf3, out ptfSource3, out ptsCount3);
					if (ptfSource1 != ptfSource3)
					{
						throw new FormatException("In multiple occurences of precise time format, precise time source is not identical in all of the cases.");
					}
				}

				// forth occurence of precise time format and source
				Int32 ptfPosition4, ptfCount4;
				Boolean isLowerCasePtf4, ptfExists4;
				ptfExists4 = Formatter.FindAndReplacePreciseTimeFormat(format, out ptfPosition4, out ptfCount4, out isLowerCasePtf4);
				if (ptfExists4)
				{
					throw new NotSupportedException("Parsing format with more than 3 occurences is not supported.");
				}
			}

			// parse time
			DateTime timestamp;
			Boolean isTimestampParsed = false;
			try
			{
				isTimestampParsed = DateTime.TryParseExact(source, format, formatProvider, DateTimeStyles.None, out timestamp);
				result = new HdDateTime(timestamp, ptfSource1);
				return isTimestampParsed;
			}

			finally
			{
				// rollback all replacements
				// rollback first
				Formatter.RollbackPreciseTimeSourceReplacement(source, ptfPosition1, ptsCount1, ptfSource1);
				Formatter.RollbackPreciseTimeFormatReplacement(format, ptfPosition1, ptfCount1, isLowerCasePtf1);

				// rollback second
				if (ptfExists2)
				{
					Formatter.RollbackPreciseTimeSourceReplacement(source, ptfPosition2, ptsCount2, ptfSource2);
					Formatter.RollbackPreciseTimeFormatReplacement(format, ptfPosition2, ptfCount2, isLowerCasePtf2);

					// rollback third
					if (ptfExists3)
					{
						Formatter.RollbackPreciseTimeSourceReplacement(source, ptfPosition3, ptsCount3, ptfSource3);
						Formatter.RollbackPreciseTimeFormatReplacement(format, ptfPosition3, ptfCount3,
							isLowerCasePtf3);
					}
				}
			}
		}
	}
}