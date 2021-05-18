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
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using NUnit.Framework;
using EPAM.Deltix.HdTime;


namespace EPAM.Deltix.HdTime.Tests
{
	[TestFixture]
	public class HdDateTimeTest
	{
		private static readonly DateTime[] TestDates =
			{DateTime.MinValue, DateTime.MaxValue, DateTime.Now, DateTime.UtcNow, DateTime.Today,};

		private static readonly HdTimeSpan day = new HdTimeSpan(new TimeSpan(1, 0, 0, 0));
		private static readonly HdTimeSpan thousandDays = new HdTimeSpan(new TimeSpan(1000, 0, 0, 0));

		private static readonly Object[] TestDatesWithBytes =
		{
			new Object[] {DateTime.MinValue, (Byte) 0}, // HdDateTime.MinValue.TimestampModulo == 0
			new Object[] {DateTime.MaxValue, (Byte) 99},  // HdDateTime.MinValue.TimestampModulo == 99
			new Object[] {DateTime.MinValue, (Byte) 92}, // HdDateTime.MinValue.TimestampModulo == 92
			new Object[] {DateTime.MaxValue, (Byte) 7},  // HdDateTime.MinValue.TimestampModulo == 7
			new Object[] {DateTime.Now, (Byte) 0},
			new Object[] {DateTime.Now, (Byte) 15},
			new Object[] {DateTime.Now, (Byte) 99},
		};

		private static readonly String[] TestPreciseTimeToStringFormats =
		{
			HdDateTime.DefaultFormat,
//			 commented formats are not supported any more.
//			"fff.fff",
//			"FFF.FFF",
//
//			"fff.fffff",
//			"FFF.FFFFF",
//
//			"fff.fffff.fffffff",
//			"FFF.FFFFF.FFFFFFF",
//
//			"fff.fffff.fffffff.ffffffff",
//			"FFF.FFFFF.FFFFFFF.FFFFFFFF",
//
//			"fff.fffff.fffffff.fffffffff",
//			"FFF.FFFFF.FFFFFFF.FFFFFFFFF",
//
//			"fff.fffff.fffffff.fffffffff.fff",
//			"FFF.FFFFF.FFFFFFF.FFFFFFFFF.FFF",
//
//			"fff.fffff.fffffff.fffffffff.fff.fffff.fffffff.fffffffff",
//			"FFF.FFFFF.FFFFFFF.FFFFFFFFF.FFF.FFFFF.FFFFFFF.FFFFFFFFF",
//
//			"fffffffff.ffffffff.fffffff.fffff.fff.fffff.fffffff.ffffffff.fffffffff",
//			"FFFFFFFFF.FFFFFFFF.FFFFFFF.FFFFF.FFF.FFFFF.FFFFFFF.FFFFFFFF.FFFFFFFFF",
//			"yyyy-MM-dd HH:mm:ss.fff.fffff",
//			"yyyy-MM-dd HH:mm:ss.FFF.FFFFF",
//			
//			"yyyy-MM-dd HH:mm:ss.fff.ffffffff",
//			"yyyy-MM-dd HH:mm:ss.FFF.FFFFFFFF",
//
//			"yyyy-MM-dd HH:mm:fff.ffffffff.ss",
//			"yyyy-MM-dd HH:mm:FFF.FFFFFFFF.ss",

			"yyyy-MM-dd HH:mm:ss.ffffffff",
			"yyyy-MM-dd HH:mm:ss.FFFFFFFF",

			"yyyy-MM-dd HH:mm:ss.fffffffff",
			"yyyy-MM-dd HH:mm:ss.FFFFFFFFF",
		};

		private static readonly Int64[] TestNanoseconds =
			{Int64.MinValue, Int64.MaxValue, 0, 123487861281L, -123456789L};


		/// <summary>
		///   Test formats, which <see cref="System.DateTime" /> can Parse/TryParse. Should be structurally the same as
		///   <see cref="TestPreciseTimeParseFormats" />.
		/// </summary>
		private static readonly String[] TestDateTimeParseFormats =
		{
			"yyyy-MM-dd HH:mm:ss.fffffff",
			"yyyy-MM-dd HH:mm:ss.FFFFFFF",
		};


		/// <summary>
		///   Test formats, which <see cref="HdDateTime" /> can Parse/TryParse. This formats must be tested with
		///   <see cref="System.DateTime" /> (if <see cref="System.DateTime" /> can parse the similar formats). See
		///   <see cref="TestDateTimeParseFormats" />.
		/// </summary>
		private static readonly String[] TestPreciseTimeParseFormats =
		{
			HdDateTime.DefaultFormat,

			"yyyy-MM-dd HH:mm:ss.ffffffff",
			//"yyyy-MM-dd HH:mm:ss.FFFFFFFF",

			"yyyy-MM-dd HH:mm:ss.fffffffff",
			//"yyyy-MM-dd HH:mm:ss.FFFFFFFFF",

			"yyyy-MM-dd HH:mm:ss.ffffffff",
			//"yyyy-MM-dd HH:mm:ss.FFFFFFFF",
		};


		private static readonly String[] TestDateTimeParseExactFormats =
		{
			"yyyy-MM-dd HH:mm:ss.fffffff",
			"yyyy-MM-dd HH:mm:fffffff.ss",
			"fffffff.yyyy-MM-dd HH:mm.ss",

			// 'F' Variants are disabled
			//"yyyy-MM-dd HH:mm:ss.FFFFFFF",
			//			"yyyy-MM-dd HH:mm:FFFFFFF.ss",
			//"FFFFFFF.yyyy-MM-dd HH:mm.ss",
		};

		private static readonly String[] TestPreciseTimeParseExactFormats =
		{
			HdDateTime.DefaultFormat,

			"yyyy-MM-dd HH:mm:ss.fffffffff",
			"yyyy-MM-dd HH:mm:ss.ffffffff",
			"yyyy-MM-dd HH:mm:ffffffff.ss",
			"yyyy-MM-dd HH:mm:fffffffff.ss",
			"ffffffff.yyyy-MM-dd HH:mm.ss",
			"fffffffff.yyyy-MM-dd HH:mm.ss"

			// 'F' Variants are disabled
			//"yyyy-MM-dd HH:mm:ss.FFFFFFFFF",
			//"yyyy-MM-dd HH:mm:ss.FFFFFFFF",
			//"yyyy-MM-dd HH:mm:FFFFFFFF.ss",
			//"yyyy-MM-dd HH:mm:FFFFFFFFF.ss",
			//"FFFFFFFF.yyyy-MM-dd HH:mm.ss",
			//"FFFFFFFFF.yyyy-MM-dd HH:mm.ss",
		};


		/// <summary>
		///   Test formats, which <see cref="System.DateTime" /> cannot parse, using parser without format.
		/// </summary>
		private static readonly String[] InvalidDateTimeParseFormats =
		{
			"yyyy-MM-dd HH:mm:fffffff.ss",
			"yyyy-MM-dd HH:mm:FFFFFFF.ss",
			"fffffff.yyyy-MM-dd HH:mm.ss",
			"FFFFFFF.yyyy-MM-dd HH:mm.ss",
		};

		private static readonly HdDateTime[] TestHdDateDates;


		static HdDateTimeTest()
		{
			List<HdDateTime> testDates = new List<HdDateTime>();
			DateTime timestamp = DateTime.Now;

			Int64 gmtTicks = DateConversionConstants.Gmt1970Ticks;
			Random rand = new Random();
			for (Int32 cases = 1000; cases > 0; cases--)
			{
				/*DateTime testTimestamp = timestamp.AddTicks(ticks);
				for (Byte bytes = 99; bytes > 0; bytes --)
				{
					testDates.Add(new PreciseTime(testTimestamp, bytes));
				}
				testDates.Add(new PreciseTime(testTimestamp, 99));*/

				Int64 generatedTicks = rand.Next(0, 100000000);
				testDates.Add(new HdDateTime(new DateTime(gmtTicks + generatedTicks), (Byte) (generatedTicks % 100)));
			}


			testDates.Add(new HdDateTime(new DateTime(gmtTicks + 999999999), 99));
			testDates.Add(new HdDateTime(new DateTime(gmtTicks + 99999999), 99));
			testDates.Add(new HdDateTime(new DateTime(gmtTicks + 9999999), 99));
			testDates.Add(new HdDateTime(new DateTime(gmtTicks + 999999), 99));
			testDates.Add(new HdDateTime(new DateTime(gmtTicks + 99999), 99));
			testDates.Add(new HdDateTime(new DateTime(gmtTicks + 9999), 99));
			testDates.Add(new HdDateTime(new DateTime(gmtTicks + 999), 99));
			testDates.Add(new HdDateTime(new DateTime(gmtTicks + 99), 99));
			testDates.Add(new HdDateTime(new DateTime(gmtTicks + 9), 99));

			testDates.AddRange(TestDatesWithBytes.OfType<HdDateTime>());

			testDates.Add(new HdDateTime(new DateTime(2016, 04, 01)));

			DateTime testDate = new DateTime(2016, 04, 01, 11, 26, 33);
			testDates.Add(new HdDateTime(testDate));
			testDates.Add(new HdDateTime(testDate.AddTicks(1)));
			testDates.Add(new HdDateTime(testDate.AddTicks(11)));
			testDates.Add(new HdDateTime(testDate.AddTicks(111)));
			testDates.Add(new HdDateTime(testDate.AddTicks(1111)));
			testDates.Add(new HdDateTime(testDate.AddTicks(11111)));
			testDates.Add(new HdDateTime(testDate.AddTicks(111111)));
			testDates.Add(new HdDateTime(testDate.AddTicks(1111111)));

			testDates.Add(new HdDateTime(1464088799463051899));
			testDates.Add(new HdDateTime(1464088799463051890));
			testDates.Add(new HdDateTime(1464090961567377109));
			testDates.Add(new HdDateTime(1464088799463051009));
			testDates.Add(new HdDateTime(1459468800000000000));


			TestHdDateDates = testDates.ToArray();
		}


		#region Testing DateTime Parse/ParseExact capabilities with PreciseTime capabilities: if DateTime can parse the same tests formats as PreciseTime.

		[Test]
		[TestCaseSource("InvalidDateTimeParseFormats")]
		public void TestInvalidDateTimeParseFormats(String testFormat)
		{
			Boolean isParsed = true;
			foreach (HdDateTime testValue in TestHdDateDates)
			{
				DateTime testTimestamp = testValue.DateTime;
				String source = testTimestamp.ToString(testFormat);

				DateTime parsedValue;
				isParsed &= DateTime.TryParse(source, out parsedValue);
			}

			Assert.IsFalse(isParsed,
				String.Format("All the DateTime test values are successfully parsed with the specified format: {0}.",
					testFormat));
		}


		[Test]
		[TestCaseSource("TestDateTimeParseFormats")]
		public void TestValidDateTimeParseFormats(String testFormat)
		{
			Boolean isParsed = true;
			foreach (HdDateTime testValue in TestHdDateDates)
			{
				DateTime testTimestamp = testValue.DateTime;
				String source = testTimestamp.ToString(testFormat);

				DateTime parsedValue;
				isParsed &= DateTime.TryParse(source, out parsedValue);
			}

			Assert.IsTrue(isParsed,
				String.Format(
					"One of the DateTime test values is NOT successfully parsed with the specified format: {0}.",
					testFormat));
		}


		[Test]
		[TestCaseSource("TestDateTimeParseExactFormats")]
		public void TestValidDateTimeParseExactFormats(String testFormat)
		{
			Boolean isParsed = true;
			foreach (HdDateTime testValue in TestHdDateDates)
			{
				DateTime testTimestamp = testValue.DateTime;
				String source = testTimestamp.ToString(testFormat);

				DateTime parsedValue;
				isParsed &= DateTime.TryParseExact(source, testFormat, null, DateTimeStyles.None, out parsedValue);
			}

			Assert.IsTrue(isParsed,
				String.Format(
					"One of the DateTime test values is NOT successfully parsed with the specified format: {0}.",
					testFormat));
		}

		#endregion


		#region Testing PreciseTime parse and format methods

		[Test]
		[TestCaseSource("TestPreciseTimeParseFormats")]
		public void TestParse(String testFormat)
		{
			//HdDateTimeParser parser = new HdDateTimeParser();
			String immutableFormat = String.Copy(testFormat);

			foreach (HdDateTime testValue in TestHdDateDates)
			{
				String source = testValue.ToString(testFormat);
				String immutableSource = String.Copy(source);

				HdDateTime parsedValue = HdDateTime.Parse(source, testFormat);
				if (testFormat.ToLower().Contains("ffffffff") && !testFormat.ToLower().Contains("fffffffff"))
				{
					Assert.IsTrue(testValue.EpochNanoseconds / 10 == parsedValue.EpochNanoseconds / 10,
						String.Format("{0} != {1}.", testValue.EpochNanoseconds, parsedValue.EpochNanoseconds));
				}
				else if (testValue.TimestampModulo != 0)
				{
					Assert.IsTrue(testValue == parsedValue,
						String.Format("{0} != {1}.", testValue.EpochNanoseconds, parsedValue.EpochNanoseconds));
				}
				else
				{
					Assert.IsTrue(testValue.DateTime == parsedValue.DateTime,
						String.Format("{0} != {1}.", testValue.EpochNanoseconds, parsedValue.EpochNanoseconds));
				}

				Assert.IsTrue(immutableFormat == testFormat, "Format has been changed.");
				Assert.IsTrue(immutableSource == source, "Source has been changed.");
			}
		}


		[Test]
		[TestCaseSource("TestPreciseTimeParseExactFormats")]
		public void TestParseExact(String testFormat)
		{
			HdDateTimeParser parser = new HdDateTimeParser();
			String immutableFormat = String.Copy(testFormat);

			foreach (HdDateTime testValue in TestHdDateDates)
			{
				String source = testValue.ToString(testFormat);
				String immutableSource = String.Copy(source);

				HdDateTime parsedValue = parser.ParseExact(source, testFormat);
				if (testFormat.ToLower().Contains("ffffffff") && !testFormat.ToLower().Contains("fffffffff"))
				{
					Assert.IsTrue(testValue.EpochNanoseconds / 10 == parsedValue.EpochNanoseconds / 10,
						String.Format("{0} != {1}.", testValue.EpochNanoseconds, parsedValue.EpochNanoseconds));
				}
				else if (testValue.TimestampModulo != 0)
				{
					Assert.IsTrue(testValue == parsedValue,
						String.Format("{0} != {1}.", testValue.EpochNanoseconds, parsedValue.EpochNanoseconds));
				}
				else
				{
					Assert.IsTrue(testValue.DateTime == parsedValue.DateTime,
						String.Format("{0} != {1}.", testValue.EpochNanoseconds, parsedValue.EpochNanoseconds));
				}

				Assert.IsTrue(immutableFormat == testFormat, "Format has been changed.");
				Assert.IsTrue(immutableSource == source, "Source has been changed.");
			}
		}


		[Test]
		[TestCaseSource("TestPreciseTimeToStringFormats")]
		public void TestToString(String testFormat)
		{
			foreach (HdDateTime testValue in TestHdDateDates)
			{
				String formatting = testValue.ToString(testFormat);
			}
		}


		[Test]
		[TestCaseSource("TestPreciseTimeParseFormats")]
		public void TestTryParse(String testFormat)
		{
			HdDateTimeParser parser = new HdDateTimeParser();
			String immutableFormat = testFormat;

			foreach (HdDateTime testValue in TestHdDateDates)
			{
				String source = testValue.ToString(testFormat);
				String immutableSource = source;

				HdDateTime parsedValue;
				Boolean isParsed = parser.TryParse(source, out parsedValue);
				if (testFormat.ToLower().Contains("ffffffff") && !testFormat.ToLower().Contains("fffffffff"))
				{
					Assert.IsTrue(testValue.EpochNanoseconds / 10 == parsedValue.EpochNanoseconds / 10,
						String.Format("{0} != {1}.", testValue.EpochNanoseconds, parsedValue.EpochNanoseconds));
				}
				else if (testValue.TimestampModulo != 0)
				{
					Assert.IsTrue(testValue == parsedValue,
						String.Format("{0} != {1}.", testValue.EpochNanoseconds, parsedValue.EpochNanoseconds));
				}
				else
				{
					Assert.IsTrue(testValue.DateTime == parsedValue.DateTime,
						String.Format("{0} != {1}.", testValue.EpochNanoseconds, parsedValue.EpochNanoseconds));
				}

				Assert.IsTrue(immutableFormat == testFormat, "Format has been changed.");
				Assert.IsTrue(immutableSource == source, "Source has been changed.");
			}
		}


		[Test]
		[TestCaseSource("TestPreciseTimeParseExactFormats")]
		public void TestTryParseExact(String testFormat)
		{
			HdDateTimeParser parser = new HdDateTimeParser();
			String immutableFormat = testFormat;

			foreach (HdDateTime testValue in TestHdDateDates)
			{
				String source = testValue.ToString(testFormat);
				String immutableSource = source;

				HdDateTime parsedValue;
				Boolean isParsed = parser.TryParseExact(source, testFormat, out parsedValue);
				if (testFormat.ToLower().Contains("ffffffff") && !testFormat.ToLower().Contains("fffffffff"))
				{
					Assert.IsTrue(testValue.EpochNanoseconds / 10 == parsedValue.EpochNanoseconds / 10,
						String.Format("{0} != {1}.", testValue.EpochNanoseconds, parsedValue.EpochNanoseconds));
				}
				else if (testValue.TimestampModulo != 0)
				{
					Assert.IsTrue(testValue == parsedValue,
						String.Format("{0} != {1}.", testValue.EpochNanoseconds, parsedValue.EpochNanoseconds));
				}
				else
				{
					Assert.IsTrue(testValue.DateTime == parsedValue.DateTime,
						String.Format("{0} != {1}.", testValue.EpochNanoseconds, parsedValue.EpochNanoseconds));
				}

				Assert.IsTrue(immutableFormat == testFormat, "Format has been changed.");
				Assert.IsTrue(immutableSource == source, "Source has been changed.");
			}
		}

		public void CheckPrint(String str, String fmt, HdDateTime dt)
		{
			Assert.AreEqual(str, dt.ToString(fmt));

			// TODO: Enable later
			//Assert.AreEqual(dt.DateTime.ToString(), ((HdDateTime)(dt.DateTime)).ToString(fmt));

			//StringBuilder sb = new StringBuilder();
			//Assert.AreEqual(str, num.appendTo(sb, fmt).toString());
		}

		private void CheckParse(String from, String fmt, HdDateTime expected)
		{
			HdDateTime parsed = HdDateTime.Parse(from, fmt);
			if (!expected.Equals(parsed))
			{
				// Comparison is here to avoid problems with Formatter affecting tests for Parser
				Assert.AreEqual(expected, parsed);
				Assert.AreEqual(expected.ToString(), parsed.ToString());
			}

			Assert.AreEqual(expected.EpochNanoseconds, parsed.EpochNanoseconds);

		}

		// TODO: Convert to Test Cases (low priority)
		[Test]
		public void TestFormat2()
		{
			// Plain numbers
			CheckPrint("34627623,.45634", "34627623,.45634", HdDateTime.FromEpochMilliseconds(12));

			// Check quoted text
			CheckPrint("Abcmsy", "'Abcmsy'", HdDateTime.FromEpochMilliseconds(0));
			CheckPrint("00Abcmsy000", "00'Abcmsy'000", HdDateTime.FromEpochMilliseconds(0));
			CheckPrint("'Abc'msy", "'''Abc''msy'", HdDateTime.FromEpochMilliseconds(0));
			CheckPrint("0'0Abc''msy00'0", "0''0'Abc''''msy'00''0", HdDateTime.FromEpochMilliseconds(0));

			// Seconds
			CheckPrint("12", "s", HdDateTime.FromEpochMilliseconds(12000));
			CheckPrint("0", "s", HdDateTime.FromEpochMilliseconds(0));
			CheckPrint("00", "ss", HdDateTime.FromEpochMilliseconds(0));
			CheckPrint("005", "0ss", HdDateTime.FromEpochMilliseconds(65000));
			CheckPrint("000005", "ssssss", HdDateTime.FromEpochMilliseconds(65000));

			// Seconds & Fractions of Second. 'S' and 'f' are now synonyms
			CheckPrint("05.0001", "ss.ffff", HdDateTime.FromEpochNanoseconds(65_000_123_000L));
			CheckPrint("05.00012", "ss.fffff", HdDateTime.FromEpochNanoseconds(65_000_123_000L));
			CheckPrint("05.000123", "ss.ffffff", HdDateTime.FromEpochNanoseconds(65_000_123_000L));
			CheckPrint("05.123000", "ss.ffffff", HdDateTime.FromEpochNanoseconds(65_123_000_123L));
			CheckPrint("05.123000", "ss.ffffff", HdDateTime.FromEpochNanoseconds(65_123_000_999L));
			CheckPrint("05.123000", "ss.ffffff", HdDateTime.FromEpochNanoseconds(65_123_000_999L));
			CheckPrint("05.1230009", "ss.fffffff", HdDateTime.FromEpochNanoseconds(65_123_000_999L));
			CheckPrint("05.12300012", "ss.ffffffff", HdDateTime.FromEpochNanoseconds(65_123_000_123L));
			CheckPrint("05.123000123", "ss.fffffffff", HdDateTime.FromEpochNanoseconds(65_123_000_123L));
			CheckPrint("05.000000123", "ss.fffffffff", HdDateTime.FromEpochNanoseconds(65_000_000_123L));
			CheckPrint("5.000123000", "s.fffffffff", HdDateTime.FromEpochNanoseconds(65_000_123_000L));

			CheckPrint("12.3", "s.S", HdDateTime.FromEpochMilliseconds(12_300));
			CheckPrint("0.345", "s.SSS", HdDateTime.FromEpochNanoseconds(345_000_000));
			CheckPrint("00.023", "ss.SSS", HdDateTime.FromEpochMilliseconds(600_023));
			CheckPrint("05.123", "ss.SSS", HdDateTime.FromEpochMilliseconds(65_123));
			CheckPrint("05.123000", "ss.SSSSSS", HdDateTime.FromEpochMilliseconds(65_123));

			// Minutes
			CheckPrint("5", "m", HdDateTime.Now.Date.AddMinutes(425));
			CheckPrint("7", "m", HdDateTime.FromEpochMilliseconds(425_000));
			CheckPrint("05", "mm", HdDateTime.Now.Date.AddMinutes(425));
			CheckPrint("00005", "0mmmm", HdDateTime.Now.Date.AddMinutes(425));

			// Hours
			CheckPrint("5", "H", HdDateTime.Now.Date.AddHours(48 + 5));
			CheckPrint("4", "H", HdDateTime.Now.Date.AddMinutes(245));
			CheckPrint("07", "HH", HdDateTime.Now.Date.AddMinutes(425));
			CheckPrint("0007005", "0HHHmmm", HdDateTime.Now.Date.AddMinutes(425));
			CheckPrint("07:5.789", "HH:m.SSS", HdDateTime.Now.Date.AddMinutes(425).AddMilliseconds(789));

			CheckPrint("0007005", "0HHHmmm", HdDateTime.Now.Date.AddMinutes(425));
			CheckPrint("1999-4-1 0:7:5.656789", "yyyy-M-d H:m:s.SSSSSS",
					new HdDateTime(1999, 4, 1).AddSeconds(425).AddNanoseconds(656789000));

			CheckPrint("1999-4-1 0:7:5.656789", "yyyy-M-d H:m:s.SSSSSS",
					new HdDateTime(1999, 4, 1, 0, 7, 5, 656789000));
			CheckPrint("990401000705656789", "yyMMddHHmmssSSSSSS",
					new HdDateTime(1999, 4, 1).AddSeconds(425).AddNanoseconds(656789000));
			CheckPrint("1999Apr01000705656789999", "yyyMMMddHHmmssSSSSSSSSS",
					new HdDateTime(1999, 4, 1).AddSeconds(425).AddNanoseconds(656789999));

			CheckPrint("2002-January-01", "y-MMMM-dd", new HdDateTime(2002, 1, 1));
			CheckPrint("31 May 2002", "d MMMM yyy", new HdDateTime(2002, 5, 31));
			CheckPrint("31       May 2002", "dMMMMMMMMMM yyy", new HdDateTime(2002, 5, 31));
			CheckPrint("31  December 2002", "dMMMMMMMMMM yyy", new HdDateTime(2002, 12, 31));

			CheckPrint("1910-4-1 0:7:5.656789", "yyyy-M-d H:m:s.ffffff",
					new HdDateTime(1910, 4, 1).AddSeconds(425).AddNanoseconds(656789000));

			CheckPrint("1910-4-1 0:7:5.656789", "yyyy-M-d H:m:s.ffffff",
					new HdDateTime(1910, 4, 1).AddSeconds(425).AddNanoseconds(656789000));

			CheckPrint("1866-1-22 20:40:40.123456789", "yyyy-M-d H:m:s.fffffffff",
					new HdDateTime(1866, 1, 22, 20, 40, 40, 123456789));
		}

		[Test]
		public void TestParse()
		{
        CheckParse("2109","yyyy", new HdDateTime(2109, 1, 1));
        CheckParse("1864","yyyy", new HdDateTime(1864, 1, 1));

        CheckParse("197005","yyyyMM", new HdDateTime(1970, 5, 1));
        CheckParse("19700531 ","yyyyMMdd ", new HdDateTime(1970, 5, 31));

        CheckParse("19700531 13","yyyyMMdd HH",
                new HdDateTime(1970, 5, 31, 13, 0, 0));

        CheckParse("19700531 1342","yyyyMMdd HHmm",
                new HdDateTime(1970, 5, 31, 13, 42, 0));

        CheckParse("19700531 134259","yyyyMMdd HHmmss",
                new HdDateTime(1970, 5, 31, 13, 42, 59));

        CheckParse("19700101000000","yyyyMMddHHmmss", HdDateTime.FromEpochMilliseconds(0));
        CheckParse("197001010000000","yyyyMMddHHmmssf", HdDateTime.FromEpochMilliseconds(0));
        CheckParse("1970010100000000","yyyyMMddHHmmssff", HdDateTime.FromEpochMilliseconds(0));
        CheckParse("19700101000000000","yyyyMMddHHmmssfff", HdDateTime.FromEpochMilliseconds(0));
        CheckParse("197001010000000000","yyyyMMddHHmmssffff", HdDateTime.FromEpochMilliseconds(0));
        CheckParse("1970010100000000000","yyyyMMddHHmmssfffff", HdDateTime.FromEpochMilliseconds(0));
        CheckParse("19700101000000000000","yyyyMMddHHmmssffffff", HdDateTime.FromEpochMilliseconds(0));
        CheckParse("197001010000000000000","yyyyMMddHHmmssfffffff", HdDateTime.FromEpochMilliseconds(0));
        CheckParse("1970010100000000000000","yyyyMMddHHmmssffffffff", HdDateTime.FromEpochMilliseconds(0));
        CheckParse("19700101000000000000000","yyyyMMddHHmmssfffffffff", HdDateTime.FromEpochMilliseconds(0));
        CheckParse("197012345678901010000000","yyyyfffffffffMMddHHmmss",
                HdDateTime.FromEpochNanoseconds(123456789));
        CheckParse("197012345678901010000000123456789","yyyyfffffffffMMddHHmmss",
                HdDateTime.FromEpochNanoseconds(123456789));

        CheckParse("197001010000 5", "yyyyMMddHHmm s", HdDateTime.FromEpochMilliseconds(5000));
        CheckParse("197001010000 0", "yyyyMMddHHmm s", HdDateTime.FromEpochMilliseconds(0));
        CheckParse("197001010000 005", "yyyyMMddHHmm s", HdDateTime.FromEpochMilliseconds(5000));

        CheckParse("19700101 12:34:56", "yyyyMMdd H:m:s",
                new HdDateTime(1970, 1, 1, 12, 34, 56));

        CheckParse("19700101 9:8:7", "yyyyMMdd H:m:s",
                new HdDateTime(1970, 1, 1, 9, 8, 7));

        CheckParse("19700101 12:34:56", "yyyyMMdd HH:mm:ss",
                new HdDateTime(1970, 1, 1, 12, 34, 56));

        CheckParse("197001 24T12:34:56", "yyyyMM ddTH:m:s",
                new HdDateTime(1970, 1, 24, 12, 34, 56));
        CheckParse("197001 24T12:34:56", "yyyyMM dd'T'H:m:s",
                new HdDateTime(1970, 1, 24, 12, 34, 56));

        CheckParse("197001 0024123456", "yyyyMM ddddHHmmss",
                new HdDateTime(1970, 1, 24,12, 34, 56));
        CheckParse("197001 0000000024123456", "yyyyMM ddddddddddHHmmss",
                new HdDateTime(1970, 1, 24,12, 34, 56));

        CheckParse("1999-11-22 19:18:17", "y-M-d H:m:s",
                new HdDateTime(1999, 11, 22, 19, 18, 17));

        // Fractions (Only fixed length patterns supported)
        CheckParse("197001010000 4.2", "yyyyMMddHHmm s.f", HdDateTime.FromEpochMilliseconds(4200));
        CheckParse("197001010000 4.200000", "yyyyMMddHHmm s.ffffff", HdDateTime.FromEpochMilliseconds(4200));
        CheckParse("197001010000 4.020", "yyyyMMddHHmm s.ff", HdDateTime.FromEpochMilliseconds(4020));
        CheckParse("197001010000 4.200000000", "yyyyMMddHHmm s.fffffffff", HdDateTime.FromEpochMilliseconds(4200));
        CheckParse("197001010000 4.000000002", "yyyyMMddHHmm s.fffffffff", HdDateTime.FromEpochNanoseconds(4000000002L));

        CheckParse("197001010000 4.2", "yyyyMMddHHmm s.S", HdDateTime.FromEpochMilliseconds(4200));
        CheckParse("197001010000 4.123", "yyyyMMddHHmm s.SSS", HdDateTime.FromEpochMilliseconds(4123));
    }

	#endregion


	#region Testing PreciseTime accuracy and calculation methods.

	[Test]
		[TestCaseSource("TestNanoseconds")]
		public void Validate_Nanoseconds(Int64 nanoseconds)
		{
			HdDateTime time = new HdDateTime(nanoseconds);
			Assert.IsTrue(time.EpochNanoseconds == nanoseconds,
				String.Format("Nanoseconds : {0} != {1}", time.EpochNanoseconds, nanoseconds));
		}


		[Test]
		[TestCaseSource("TestDatesWithBytes")]
		public void Validate_PreciseTimestamp(DateTime testTimestamp, Byte testTimestampModulo)
		{
			HdDateTime testTime = new HdDateTime(testTimestamp, testTimestampModulo);
			HdDateTime testTimeFromNanoseconds = new HdDateTime(testTime.EpochNanoseconds);
			HdDateTime testTimeFromMilliseconds =
				HdDateTime.FromEpochMilliseconds(testTime.EpochMilliseconds, testTime.EpochMillisecondsModulo);
			HdDateTime testTimeFromTimestamp =
				new HdDateTime(testTime.DateTime, testTime.TimestampModulo);

			Assert.AreEqual(testTime.EpochNanoseconds, testTimeFromNanoseconds.EpochNanoseconds);
			Assert.AreEqual(testTime.EpochNanoseconds, testTimeFromMilliseconds.EpochNanoseconds);
			Assert.AreEqual(testTime.EpochNanoseconds, testTimeFromTimestamp.EpochNanoseconds);
			Assert.AreEqual(testTime, testTimeFromNanoseconds);
			Assert.AreEqual(testTime, testTimeFromMilliseconds);
			Assert.AreEqual(testTime, testTimeFromTimestamp);
		}


		[Test]
		[TestCaseSource("TestDates")]
		public void Validate_Timestamp(DateTime testTimestamp)
		{
			HdDateTime time = new HdDateTime(testTimestamp);
			Assert.IsTrue(time.DateTime == testTimestamp,
				String.Format("DateTime {0} != {1}", time.DateTime.Ticks, testTimestamp.Ticks));
		}


		[Test]
		[TestCaseSource("TestDatesWithBytes")]
		public void Validate_TimestampAndTimestampModulo(DateTime testTimestamp, Byte testTimestampModulo)
		{
			HdDateTime time = new HdDateTime(testTimestamp, testTimestampModulo);
			Assert.IsTrue(time.DateTime == testTimestamp,
				String.Format("Timestamp is incorrect: {0}. Must be {1}", time.DateTime.Ticks, testTimestamp.Ticks));
			if (testTimestamp != DateTime.MinValue && testTimestamp != DateTime.MaxValue)
			Assert.IsTrue(
				time.TimestampModulo == testTimestampModulo,
				String.Format("Timestamp modulo is incorrect: {0}. Must be {1}.", time.TimestampModulo,
					testTimestampModulo));
		}

		#endregion


		#region Testing .Net Framework implementation contract.

		/// <summary>
		///   Used to validate implementation of DateTime.Parse.
		///   In case, if it receives source with nanoseconds and these nanoseconds >= 50 it rounds them.
		///   And creates date time with additionals rounded tick.
		/// </summary>
		[Test]
		[TestCaseSource("TestDateTimeParseFormats")]
		public void ValidateFrameworkDateTimeImplementationContract(String testFormat)
		{
			List<TestCaseError> incorrectResults = new List<TestCaseError>();
			foreach (HdDateTime testValue in TestHdDateDates)
			{
				String source = testValue.ToString(testFormat);
				DateTime parsedValue = DateTime.Parse(source);

				if (testValue.DateTime != parsedValue)
				{
					Int64 nanoseconds = testValue.EpochNanoseconds;
					Int64 testTicks = testValue.DateTime.Ticks;
					Int64 parsedTicks = parsedValue.Ticks;

					// remove loose precision
					if (testTicks - parsedTicks == -1 && (nanoseconds % 100 >= 50))
					{
					}
					else
					{
						TestCaseError error = new TestCaseError()
						{
							Nanoseconds = nanoseconds,
							TestTicks = testTicks,
							ParsedTicks = parsedTicks,
							Format = testFormat,
							Source = source
						};
						incorrectResults.Add(error);
					}
				}
			}

			Assert.IsTrue(incorrectResults.Count == 0);
		}


		#region Nested type: TestCaseError

		private class TestCaseError
		{
			public String Format { get; set; }
			public Int64 Nanoseconds { get; set; }
			public Int64 ParsedTicks { get; set; }
			public String Source { get; set; }
			public Int64 TestTicks { get; set; }


			public override String ToString()
			{
				return String.Format("{0}, {1}, {2}, {3}00, {4}00", Format, Source, Nanoseconds, TestTicks,
					ParsedTicks);
			}
		}

		#endregion

		#endregion


		[Test]
		public void PrintMinMaxConstants()
		{
			HdDateTime maxValue = HdDateTime.MaxValue;
			HdDateTime minValue = HdDateTime.MinValue;
			var tickNanosMin = 100 - ((DateTime)minValue.AddTicks(1) - minValue).TotalNanoseconds;
			var tickNanosMax = (maxValue.AddTicks(-1) - (DateTime)(maxValue.AddTicks(-1))).TotalNanoseconds;

			var secondNanosMin = 1000000000 + minValue.AddSeconds(1).EpochNanoseconds % 1000000000;
			var secondNanosMax = maxValue.EpochNanoseconds % 1000000000;

			Console.WriteLine($"HDT MinValue       = {minValue.AddSeconds(1)} - 1s, {tickNanosMin} / {HdDateTime.MinValue.EpochNanoseconds}");
			Console.WriteLine($"HDT MinValue       = {minValue.RawDateTime} {secondNanosMin}");
			Console.WriteLine();
			Console.WriteLine($"HDT MaxValue       = {maxValue.AddSeconds(-1)} + 1s, {tickNanosMax} / {HdDateTime.MaxValue.EpochNanoseconds}");
			Console.WriteLine($"HDT MaxValue       = {maxValue.RawDateTime} {secondNanosMax}");
		}

		[Test]
		public void TestMinMaxConstants()
		{
			HdDateTime maxValue = HdDateTime.MaxValue;
			HdDateTime minValue = HdDateTime.MinValue;

			Assert.True(maxValue > minValue);
			Assert.True(maxValue > new HdDateTime(DateTime.Now));
			Assert.True(minValue < new HdDateTime(DateTime.Now));

			Assert.True(new HdDateTime(maxValue.EpochNanoseconds + 1) > maxValue);
			Assert.True(new HdDateTime(minValue.EpochNanoseconds - 1) < minValue);

			// False actually
			//Assert.True(maxValue - minValue > new TimeSpan(0));

			Random rnd = new Random();
			for (int i = 1000000; i != 0; --i)
			{
				var t = new HdDateTime(rnd.Next() & 0xFFFFFFFFL | rnd.Next() * 0x80000000L);
				Assert.True(maxValue >= t);
				Assert.True(minValue <= t);
			}
		}

		[Test]
		public void TestMinMaxConstantsExtra()
		{
			HdDateTime maxValue = HdDateTime.MaxValue;
			HdDateTime minValue = HdDateTime.MinValue;

			var nanosMin = 100 - ((DateTime)minValue.AddTicks(1) - minValue).TotalNanoseconds;
			var nanosMax = (maxValue.AddTicks(-1) - (DateTime)(maxValue.AddTicks(-1))).TotalNanoseconds;

			var secondNanosMin = 1000000000 + minValue.AddSeconds(1).EpochNanoseconds % 1000000000;
			var secondNanosMax = maxValue.EpochNanoseconds % 1000000000;

			Assert.True(nanosMax == maxValue.AddTicks(-1).TimestampModulo);
			Assert.True(nanosMin == minValue.AddTicks(1).TimestampModulo);

			Assert.True(nanosMax == secondNanosMax % 100);
			Assert.True(nanosMin == secondNanosMin % 100);

			Assert.AreEqual(minValue.EpochNanoseconds, new HdDateTime((DateTime)minValue.AddTicks(1), (Byte)nanosMin).AddTicks(-1).EpochNanoseconds);
			Assert.True(minValue == new HdDateTime((DateTime)minValue.AddTicks(1), (Byte)nanosMin).AddTicks(-1));
			Assert.AreEqual(maxValue.EpochNanoseconds, new HdDateTime((DateTime)maxValue.AddTicks(-1), (Byte)nanosMax).AddTicks(1).EpochNanoseconds);
			Assert.True(maxValue == new HdDateTime((DateTime)maxValue.AddTicks(-1), (Byte)nanosMax).AddTicks(1));

			Assert.AreEqual(maxValue.EpochNanoseconds, HdDateTime.FromEpochMilliseconds(maxValue.EpochNanoseconds / 1000000000 * 1000, secondNanosMax).EpochNanoseconds);
		}

		static void ToVoid(Object hdt)
		{
			// Do nothing!
		}

		// Tests validity of comparisons ans typecasts vs min/max constants
		[Test]
		public void TestMinMaxVsDateTime()
		{
			HdDateTime maxValue = HdDateTime.MaxValue;
			HdDateTime minValue = HdDateTime.MinValue;

			// Min/Max casts are a special case, treated like +Inf, -Inf, should be equal
			Assert.True(HdDateTime.MaxValue == DateTime.MaxValue);
			Assert.True(HdDateTime.MinValue == DateTime.MinValue);

			for (int i = 1; i < 0x400; ++i)
			// But in reality HdDateTime representable time interval is much smaller
			// and we are going to test this and also the validity of our typecasts
			{
				// Exceptions are slow, so don't test each iteration
				if (i < 120 || (i & 3) == 0)
				{
					Assert.Throws<ArgumentOutOfRangeException>(
						() => ToVoid((HdDateTime) DateTime.MaxValue.AddTicks(-i)));
					Assert.Throws<ArgumentOutOfRangeException>(() =>
						ToVoid((HdDateTime) DateTime.MinValue.AddTicks(i)));

					// Maybe there is a point in implementing comparison with out-of-range DateTime
					// but currently this throws
					Assert.Throws<ArgumentOutOfRangeException>(() =>
						ToVoid(maxValue < (HdDateTime) DateTime.MaxValue.AddTicks(-i)));
					Assert.Throws<ArgumentOutOfRangeException>(() =>
						ToVoid(minValue > (HdDateTime) DateTime.MinValue.AddTicks(i)));
				}

				Assert.True(maxValue > ((HdDateTime)DateTime.MaxValue).AddNanoseconds(-i));
				Assert.True(minValue < ((HdDateTime)DateTime.MinValue).AddNanoseconds(i));
				Assert.True(maxValue > ((HdDateTime)DateTime.MaxValue).AddTicks(-i));
				Assert.True(minValue < ((HdDateTime)DateTime.MinValue).AddTicks(i));

				Assert.True(maxValue.AddNanoseconds(-i) < DateTime.MaxValue);
				Assert.True(minValue.AddNanoseconds(i) > DateTime.MinValue);
				Assert.True(maxValue.AddTicks(-i) < DateTime.MaxValue);
				Assert.True(minValue.AddTicks(i) > DateTime.MinValue);
				Assert.True(maxValue.AddMilliseconds(-i) < DateTime.MaxValue);
				Assert.True(minValue.AddMilliseconds(i) > DateTime.MinValue);

				Assert.True((DateTime)maxValue.AddNanoseconds(-i) < DateTime.MaxValue);
				Assert.True((DateTime)minValue.AddNanoseconds(i) > DateTime.MinValue);
				Assert.True((DateTime)maxValue.AddTicks(-i) < DateTime.MaxValue);
				Assert.True((DateTime)minValue.AddTicks(i) > DateTime.MinValue);
				Assert.True((DateTime)maxValue.AddMilliseconds(-i) < DateTime.MaxValue);
				Assert.True((DateTime)minValue.AddMilliseconds(i) > DateTime.MinValue);

				// Must add at least 1 nanosecond to avoid Max<=>Max Min<=>Min conversion
				Assert.True(((DateTime)maxValue.AddNanoseconds(-1)).AddTicks(-i) < DateTime.MaxValue);
				Assert.True(((DateTime)maxValue.AddNanoseconds(-1)).AddTicks(i) < DateTime.MaxValue);
				Assert.True(((DateTime)minValue.AddNanoseconds(1)).AddTicks(-i) > DateTime.MinValue);
				Assert.True(((DateTime)minValue.AddNanoseconds(1)).AddTicks(i) > DateTime.MinValue);
			}
		}


		[Test]
		public void TestModuloPositive()
		{
			HdDateTime dt = new HdDateTime(123);
			Assert.AreEqual(23, dt.TimestampModulo);
			Assert.AreEqual(23, (dt + new HdTimeSpan(1000000000000L)).TimestampModulo);
			dt = new HdDateTime(new DateTime(1971, 1, 1), 42);
			Assert.AreEqual(42, dt.TimestampModulo);
			Assert.AreEqual(42, (dt + thousandDays + thousandDays + thousandDays).TimestampModulo);
			Assert.AreEqual(42, (dt - thousandDays + thousandDays - day).TimestampModulo);
		}

		[Test]
		public void TestModuloNegative()
		{
			HdDateTime dt = new HdDateTime(123);
			Assert.AreEqual(23, (dt - new HdTimeSpan(1000000000000L)).TimestampModulo);
			dt = new HdDateTime(new DateTime(1971, 1, 1), 42);
			Assert.AreEqual(42, dt.TimestampModulo);
			Assert.AreEqual(42, (dt - thousandDays).TimestampModulo);
			Assert.AreEqual(42, (dt + thousandDays - thousandDays + day).TimestampModulo);
		}

		[Test]
		public void TestMillisPositive()
		{
			HdDateTime dt = new HdDateTime(567 * 1000000 + 789123);
			Assert.AreEqual(567, dt.EpochMilliseconds);
			Assert.AreEqual(567 + 1000000, (dt + new HdTimeSpan(1000000 * 1000000L)).EpochMilliseconds);
		}

		[Test]
		public void TestMillisNegative()
		{

			HdDateTime dt = new HdDateTime(567 * 1000000 + 789123);
			Assert.AreEqual(567, dt.EpochMilliseconds);
			Assert.AreEqual(567 - 1000000, (dt - new HdTimeSpan(1000000 * 1000000L)).EpochMilliseconds);
		}

		[Test]
		public void TestFromTicks()
		{
			var dt = DateTime.UtcNow;
			Assert.AreEqual(new HdDateTime(dt), HdDateTime.FromTicks(dt.Ticks));
			Assert.AreEqual(new HdDateTime(dt).EpochNanoseconds, HdDateTime.FromTicks(dt.Ticks).EpochNanoseconds);
		}

		[Test]
		public void TestMinValueComponments()
		{
			var dt = HdDateTime.MinValue;
			Assert.AreEqual(0, dt.Nanosecond);
			Assert.AreEqual(0, dt.Microsecond);
			Assert.AreEqual(0, dt.Millisecond);
			Assert.AreEqual(0, dt.Second);
			Assert.AreEqual(0, dt.Minute);
			Assert.AreEqual(0, dt.Hour);
			Assert.AreEqual(1, dt.Day);
			Assert.AreEqual(1, dt.Month);
			Assert.AreEqual(((DateTime)dt).Year, dt.Year);
		}

		[Test]
		public void TestMaxValueComponments()
		{
			var dt = HdDateTime.MaxValue;
			Assert.AreEqual(999999999, dt.Nanosecond);
			Assert.AreEqual(999999, dt.Microsecond);
			Assert.AreEqual(999, dt.Millisecond);
			Assert.AreEqual(59, dt.Second);
			Assert.AreEqual(59, dt.Minute);
			Assert.AreEqual(23, dt.Hour);
			Assert.AreEqual(31, dt.Day);
			Assert.AreEqual(12, dt.Month);
			Assert.AreEqual(((DateTime)dt).Year, dt.Year);
		}

#pragma warning disable 1718
		[Test]
		public void TestEquals()
		{
			HdDateTime a1 = new HdDateTime(123);
			HdDateTime a2 = new HdDateTime(123);
			HdDateTime b = new HdDateTime(42);

			Assert.True(a1 == a1);
			Assert.False(a1 != a1);

			Assert.True(a1 == a2);
			Assert.False(a1 != a2);
			Assert.True(a1.Equals(a2));
			Assert.True(a2.Equals(a1));

			Assert.True(a1.Equals((Object) a1));
			Assert.True(a1.Equals((Object) a2));
			Assert.True(a2.Equals((Object) a1));
			Assert.True(((Object) a1).Equals((Object) a1));
			Assert.True(((Object) a1).Equals((Object) a2));
			Assert.True(((Object) a2).Equals((Object) a1));
			Assert.True(((Object) a1).Equals((Object) a2));
			Assert.True(((Object) a2).Equals((Object) a1));

			Assert.False(a1 == b);
			Assert.False(a2 == b);
			Assert.True(a1 != b);

			Assert.False(a1.Equals(b));
			Assert.False(a2.Equals(b));

			Assert.False(a1.Equals((Object) b));
			Assert.False(((Object) a1).Equals(b));
			Assert.False(((Object) a1).Equals((Object) b));
		}
#pragma warning restore 1718

		[Test]
		public void TestAddXX()
		{
			AddXX((HdDateTime x, int y) => x.AddTicks(y), (DateTime x, int y) => x.AddTicks(y));
			AddXX((HdDateTime x, int y) => x.AddMilliseconds(y), (DateTime x, int y) => x.AddMilliseconds(y));
			AddXX((HdDateTime x, int y) => x.AddSeconds(y), (DateTime x, int y) => x.AddSeconds(y));
			AddXX((HdDateTime x, int y) => x.AddMinutes(y), (DateTime x, int y) => x.AddMinutes(y));
			AddXX((HdDateTime x, int y) => x.AddHours(y), (DateTime x, int y) => x.AddHours(y));
			AddXX((HdDateTime x, int y) => x.AddDays(y), (DateTime x, int y) => x.AddDays(y));
			AddXX((HdDateTime x, int y) => x.AddMonths(y % 1000), (DateTime x, int y) => x.AddMonths(y % 1000), 5000);
			AddXX((HdDateTime x, int y) => x.AddYears(y % 100), (DateTime x, int y) => x.AddYears(y % 100), 2000);

			AddXX((HdDateTime x, Int64 y) => x.AddTicks(y), (DateTime x, Int64 y) => x.AddTicks(y));
			AddXX((HdDateTime x, Int64 y) => x.AddMilliseconds(y), (DateTime x, Int64 y) => x.AddMilliseconds(y));
			AddXX((HdDateTime x, Int64 y) => x.AddSeconds(y), (DateTime x, Int64 y) => x.AddSeconds(y));
			AddXX((HdDateTime x, Int64 y) => x.AddMinutes(y), (DateTime x, Int64 y) => x.AddMinutes(y));
			AddXX((HdDateTime x, Int64 y) => x.AddHours(y), (DateTime x, Int64 y) => x.AddHours(y));
			AddXX((HdDateTime x, Int64 y) => x.AddDays(y), (DateTime x, Int64 y) => x.AddDays(y));
		}

		private void AddXX(Func<HdDateTime, long, HdDateTime> a, Func<DateTime, long, DateTime> b)
		{
			var r = new Random();

			DateTime dt0 = DateTime.UtcNow.AddDays((r.NextDouble() * 20 - 10) * 365);
			HdDateTime hdt0 = dt0;
			Assert.AreEqual(a(hdt0, 0L), (HdDateTime) b(dt0, 0L));

			for (int i = 0; i < 10000; ++i)
			{
				DateTime dt = DateTime.UtcNow.AddDays((r.NextDouble() * 10 - 5) * 365);
				HdDateTime hdt = dt;
				int value = (int) (r.NextDouble() * (Int16.MaxValue * 2.0) + Int16.MinValue);
				HdDateTime r0 = HdDateTime.MinValue, r1 = HdDateTime.MinValue;
				bool r0ok = false, r1ok = false;
				Exception e0 = null, e1 = null;
				try
				{
					r0 = a(hdt, value);
					r0ok = true;
				}
				catch (Exception e)
				{
					e0 = e;
				}

				try
				{
					r1 = (HdDateTime) b(dt, value);
					r1ok = true;
				}
				catch (Exception e)
				{
					e1 = e;
				}

				Assert.True(r0ok == r1ok && (r0ok == false || r0 == r1),
					$"a({hdt}, {value})/{r0ok} != b({(HdDateTime) dt}, {value})/{r1ok} {(r0ok == false ? e0 : null)} {(r1ok == false ? e1 : null)}");

			}
		}

		private void AddXX(Func<HdDateTime, int, HdDateTime> a, Func<DateTime, int, DateTime> b, int n = 10000)
		{
			AddXX((x, y) => a(x, (int)y), (x, y) => b(x, (int)y)); 
		}
	}
}
