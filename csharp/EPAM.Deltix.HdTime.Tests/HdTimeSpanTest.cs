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
using System.Text;
using NUnit.Framework;


namespace EPAM.Deltix.HdTime.Tests
{
	[TestFixture]
	public class HdTimeSpanTest
	{
		private static Object[] TestDates = {new Object[] {DateTime.Now, DateTime.Now.AddDays(-1)}};
		private static Object[] TestIntervals = {new Object[] {TimeSpan.FromDays(123), TimeSpan.FromDays(12345)}};

		// These intervals were broken and don't fit into HdTimeSpan (>1900 years each)
		//private static Object[] TestIntervals = { new Object[] { TimeSpan.FromDays(DateTime.Now.Ticks), TimeSpan.FromTicks(DateTime.Now.AddDays(-1).Ticks) } };

		[Test]
		public void TestConstants()
		{
			Assert.AreEqual(HdTimeSpan.NanosInDay, 100 * TimeSpan.TicksPerDay);
			Assert.AreEqual(HdTimeSpan.NanosInHour, 100 * TimeSpan.TicksPerHour);
			Assert.AreEqual(HdTimeSpan.NanosInMillisecond, 100 * TimeSpan.TicksPerMillisecond);
			Assert.AreEqual(HdTimeSpan.NanosInMinute, 100 * TimeSpan.TicksPerMinute);
			Assert.AreEqual(HdTimeSpan.NanosInSecond, 100 * TimeSpan.TicksPerSecond);
			Assert.AreEqual(0, HdTimeSpan.Zero.TotalNanoseconds);
			Assert.AreEqual(-Int64.MaxValue, HdTimeSpan.MinValue.TotalNanoseconds);
			Assert.AreEqual(Int64.MaxValue, HdTimeSpan.MaxValue.TotalNanoseconds);
		}

		[Test]
		[TestCaseSource("TestIntervals")]
		public void TestAdd(TimeSpan a, TimeSpan b)
		{
			TimeSpan sum = a + b;

			HdTimeSpan hdA = a;
			HdTimeSpan hdB = b;
			HdTimeSpan hdSum = hdA + hdB;
			Assert.IsTrue(hdA == a);
			Assert.IsTrue(hdB == b);
			Assert.IsTrue(hdSum == sum);
		}


		[Test]
		[TestCaseSource("TestIntervals")]
		public void TestSubtract(TimeSpan a, TimeSpan b)
		{
			TimeSpan timeSpan = a - b;

			HdTimeSpan hdA = a;
			HdTimeSpan hdB = b;
			HdTimeSpan hdTimeSpan = hdA - hdB;
			Assert.IsTrue(hdTimeSpan == timeSpan);
		}


		[Test]
		[TestCaseSource("TestDates")]
		public void TestSubtractDates(DateTime a, DateTime b)
		{
			TimeSpan timeSpan = a - b;

			HdDateTime hdA = a;
			HdDateTime hdB = b;
			HdTimeSpan hdTimeSpan = hdA - hdB;
			Assert.IsTrue(hdTimeSpan == timeSpan);
		}

		[Test]
		public void TestAdditionOverflow()
		{
			Assert.Throws<OverflowException>(delegate { var t = TimeSpan.MaxValue + TimeSpan.MaxValue; });
			Assert.Throws<OverflowException>(delegate { var t = TimeSpan.MinValue + TimeSpan.MinValue; });
			Assert.Throws<OverflowException>(delegate { var t = HdTimeSpan.MaxValue + HdTimeSpan.MaxValue; });
			Assert.Throws<OverflowException>(delegate { var t = HdTimeSpan.MinValue + HdTimeSpan.MinValue; });
			Assert.Throws<OverflowException>(delegate { var t = HdTimeSpan.MaxValue + new HdTimeSpan(1); });
			Assert.Throws<OverflowException>(delegate { var t = HdTimeSpan.MinValue + new HdTimeSpan(-1); });
		}

		[Test]
		public void TestSubtractionOverflow()
		{
			Assert.Throws<OverflowException>(delegate { var t = TimeSpan.MaxValue - TimeSpan.MinValue; });
			Assert.Throws<OverflowException>(delegate { var t = TimeSpan.MinValue - TimeSpan.MaxValue; });
			Assert.Throws<OverflowException>(delegate { var t = HdTimeSpan.MaxValue - HdTimeSpan.MinValue; });
			Assert.Throws<OverflowException>(delegate { var t = HdTimeSpan.MinValue - HdTimeSpan.MaxValue; });
			Assert.Throws<OverflowException>(delegate { var t = HdTimeSpan.MinValue - new HdTimeSpan(1); });
			Assert.Throws<OverflowException>(delegate { var t = HdTimeSpan.MaxValue - new HdTimeSpan(-1); });
		}


#pragma warning disable 1718
		[Test]
		public void TestEquals()
		{
			HdTimeSpan a1 = new HdTimeSpan(123);
			HdTimeSpan a2 = new HdTimeSpan(123);
			HdTimeSpan b = new HdTimeSpan(42);

			Assert.True(a1 == a1);
			Assert.False(a1 != a1);

			Assert.True(a1 == a2);
			Assert.False(a1 != a2);
			Assert.True(a1.Equals(a2));
			Assert.True(a2.Equals(a1));

			Assert.True(a1.Equals((Object)a1));
			Assert.True(a1.Equals((Object)a2));
			Assert.True(a2.Equals((Object)a1));
			Assert.True(((Object)a1).Equals((Object)a1));
			Assert.True(((Object)a1).Equals((Object)a2));
			Assert.True(((Object)a2).Equals((Object)a1));
			Assert.True(((Object)a1).Equals((Object)a2));
			Assert.True(((Object)a2).Equals((Object)a1));

			Assert.False(a1 == b);
			Assert.False(a2 == b);
			Assert.True(a1 != b);

			Assert.False(a1.Equals(b));
			Assert.False(a2.Equals(b));

			Assert.False(a1.Equals((Object)b));
			Assert.False(((Object)a1).Equals(b));
			Assert.False(((Object)a1).Equals((Object)b));
		}
#pragma warning restore 1718

		[Test]
		public void TestPrint()
		{
			TimeSpan ts = new TimeSpan(123, 45, 67, 9, 1234);
			HdTimeSpan hts = new HdTimeSpan(ts);
			Console.Out.WriteLine(ts);
			Console.Out.WriteLine(hts);
			Assert.AreEqual(ts.ToString(), hts.ToString("d.HH:mm:ss.fffffff"));
		}

		public void CheckPrint(String str, String fmt, HdTimeSpan ts, bool compareWithTimeSpan = false)
		{
			Assert.AreEqual(str, ts.ToString(fmt));
			if (compareWithTimeSpan)
				Assert.AreEqual(ts.TimeSpan.ToString(fmt), ((HdTimeSpan)(ts.TimeSpan)).ToString(fmt));
			//StringBuilder sb = new StringBuilder();
			//Assert.AreEqual(str, num.appendTo(sb, fmt).toString());
		}

		private void CheckParse(String from, String fmt, HdTimeSpan expected)
		{
			HdTimeSpan parsed = HdTimeSpan.Parse(from, fmt);
			if (!expected.Equals(parsed))
			{
				// Comparison is here to avoid problems with Formatter affecting tests for Parser
				Assert.AreEqual(expected, parsed);
				Assert.AreEqual(expected.ToString(), parsed.ToString());
			}

			Assert.AreEqual(expected.TotalNanoseconds, parsed.TotalNanoseconds);

		}

		private void CheckFormatFail(String from, String fmt, String msg)
		{
			try
			{
				HdTimeSpan.Parse(from, fmt);
			}
			catch (FormatError e)
			{
				if (e.Message.Contains(msg))
					return;
			}

			Assert.Fail("Was expected to throw");
		}

		// TODO: Convert to Test Cases (low priority)
		[Test]
		public void TestFormat()
		{
			// Plain numbers
			CheckPrint("34627623,.45634", "34627623,.45634", HdTimeSpan.FromSeconds(12), false);

			// Check quoted text
			CheckPrint("Abcmsy", "'Abcmsy'", HdTimeSpan.Zero);
			CheckPrint("00Abcmsy000", "00'Abcmsy'000", HdTimeSpan.Zero, false);
			CheckPrint("'Abc'msy", "'''Abc''msy'", HdTimeSpan.Zero, false);
			CheckPrint("0'0Abc''msy00'0", "0''0'Abc''''msy'00''0", HdTimeSpan.Zero, false);

			// Seconds
			CheckPrint("12", "s", HdTimeSpan.FromSeconds(12));
			CheckPrint("0", "s", HdTimeSpan.FromSeconds(0));
			CheckPrint("00", "ss", HdTimeSpan.FromSeconds(0));
			CheckPrint("005", "0ss", HdTimeSpan.FromSeconds(65));
			CheckPrint("000005", "ssssss", HdTimeSpan.FromSeconds(65));

			// Seconds & Fractions of Second. 'S' and 'f' are now synonyms
			CheckPrint("12.3", "s.S", HdTimeSpan.FromMilliseconds(12_300));
			CheckPrint("0.345", "s.SSS", HdTimeSpan.FromMicroseconds(345_000));
			CheckPrint("00.023", "ss.SSS", HdTimeSpan.FromMilliseconds(600_023));
			CheckPrint("05.123", "ss.SSS", HdTimeSpan.FromMilliseconds(65_123));
			CheckPrint("05.123000", "ss.SSSSSS", HdTimeSpan.FromMilliseconds(65_123));

			CheckPrint("05.0001", "ss.ffff", HdTimeSpan.FromMicroseconds(65_000_123));
			CheckPrint("05.00012", "ss.fffff", HdTimeSpan.FromMicroseconds(65_000_123));
			CheckPrint("05.000123", "ss.ffffff", HdTimeSpan.FromMicroseconds(65_000_123));
			CheckPrint("05.123000", "ss.ffffff", HdTimeSpan.FromNanoseconds(65_123_000_123L));
			CheckPrint("05.123000", "ss.ffffff", HdTimeSpan.FromNanoseconds(65_123_000_999L));
			CheckPrint("05.123000", "ss.ffffff", HdTimeSpan.FromNanoseconds(65_123_000_999L));
			CheckPrint("05.1230009", "ss.fffffff", HdTimeSpan.FromNanoseconds(65_123_000_999L));
			CheckPrint("05.12300012", "ss.ffffffff", HdTimeSpan.FromNanoseconds(65_123_000_123L));
			CheckPrint("05.123000123", "ss.fffffffff", HdTimeSpan.FromNanoseconds(65_123_000_123L));
			CheckPrint("05.000000123", "ss.fffffffff", HdTimeSpan.FromNanoseconds(65_000_000_123L));
			CheckPrint("5.000123000", "s.fffffffff", HdTimeSpan.FromNanoseconds(65_000_123_000L));

			// Minutes
			CheckPrint("5", "m", HdTimeSpan.FromMinutes(425));
			CheckPrint("7", "m", HdTimeSpan.FromSeconds(425));
			CheckPrint("05", "mm", HdTimeSpan.FromMinutes(425));
			CheckPrint("00005", "0mmmm", HdTimeSpan.FromMinutes(425));

			// Hours
			CheckPrint("5", "H", HdTimeSpan.FromHours(48 + 5));
			CheckPrint("4", "H", HdTimeSpan.FromMinutes(245));
			CheckPrint("07", "HH", HdTimeSpan.FromMinutes(425));
			CheckPrint("0007005", "0HHHmmm", HdTimeSpan.FromMinutes(425));
			CheckPrint("07:5.789", "HH:m.SSS", HdTimeSpan.FromMinutes(425).Add(HdTimeSpan.FromMilliseconds(789)));

			// Sign insertion
			CheckPrint("-12", "s", HdTimeSpan.FromSeconds(-12));
			CheckPrint("-0", "s", HdTimeSpan.FromMilliseconds(-1));
			CheckPrint("-05", "ss", HdTimeSpan.FromSeconds(-425));

			// "Advanced" sign insertion
			CheckPrint("-000005", "ssssss", HdTimeSpan.FromSeconds(-425));
			CheckPrint("-00000005", "00ssssss", HdTimeSpan.FromSeconds(-425));
			CheckPrint("Abc-00000005", "Abc00ssssss", HdTimeSpan.FromSeconds(-425));
			CheckPrint("Abcmsy-00000005", "'Abcmsy'00ssssss", HdTimeSpan.FromSeconds(-425));
			CheckPrint(";.:-000005", ";.:ssssss", HdTimeSpan.FromSeconds(-425));
			CheckPrint(";.:-00000005", ";.:00ssssss", HdTimeSpan.FromSeconds(-425));
		}

		[Test]
		public void TestParse()
		{
			CheckParse("-0000091024123456", "ddddddddddHHmmss", new HdTimeSpan(-91024, -12, -34, -56));

			CheckParse("5", "s", HdTimeSpan.FromSeconds(5));
			CheckParse("0", "s", HdTimeSpan.FromSeconds(0));
			CheckParse("005", "s", HdTimeSpan.FromSeconds(5));

			CheckParse("12:34:56", "H:m:s", new HdTimeSpan(12, 34, 56));
			CheckParse("12:34:56", "HH:mm:ss", new HdTimeSpan(12, 34, 56));

			CheckParse("1024T12:34:56", "dTH:m:s", new HdTimeSpan(1024, 12, 34, 56));
			CheckParse("1024T12:34:56", "d'T'H:m:s", new HdTimeSpan(1024, 12, 34, 56));

			// Unseparated fixed width fields
			CheckParse("123", "ss", HdTimeSpan.FromSeconds(12));
			CheckParse("123456", "HHmmss", new HdTimeSpan(12, 34, 56));
			CheckParse("1024123456", "ddddHHmmss", new HdTimeSpan(1024, 12, 34, 56));
			CheckParse("91024123456", "dddddHHmmss", new HdTimeSpan(91024, 12, 34, 56));
			CheckParse("0000091024123456", "ddddddddddHHmmss", new HdTimeSpan(91024, 12, 34, 56));
			CheckParse("101024123456", "ddddddHHmmss", new HdTimeSpan(101024, 12, 34, 56));

			// Sign
			CheckParse("-5", "s", HdTimeSpan.FromSeconds(-5));
			CheckParse("-1024T12:34:56", "dTH:m:s", new HdTimeSpan(-1024, -12, -34, -56));
			CheckParse("-123456", "HHmmss", new HdTimeSpan(-12, -34, -56));
			CheckParse("-0000091024123456", "ddddddddddHHmmss", new HdTimeSpan(-91024, -12, -34, -56));

			// Fractions (Only fixed length patterns supported)
			CheckParse("4.2", "s.f", HdTimeSpan.FromMilliseconds(4200));
			CheckParse("4.200000", "s.ffffff", HdTimeSpan.FromMilliseconds(4200));
			CheckParse("4.020", "s.ff", HdTimeSpan.FromMilliseconds(4020));
			CheckParse("4.200000000", "s.fffffffff", HdTimeSpan.FromMilliseconds(4200));
			CheckParse("4.000000002", "s.fffffffff", HdTimeSpan.FromNanoseconds(4000000002L));

			CheckParse("4.2", "s.S", HdTimeSpan.FromMilliseconds(4200));
			CheckParse("4.123", "s.SSS", HdTimeSpan.FromMilliseconds(4123));
		}

		[Test]
		public void testParseInvalidFormat()
		{
			CheckFormatFail("5 5", "s s", "Dup");
			CheckFormatFail("2002 2002", "dddd dddd", "Dup");
			CheckFormatFail("1 1", "d d", "Dup");
		}
	}
}