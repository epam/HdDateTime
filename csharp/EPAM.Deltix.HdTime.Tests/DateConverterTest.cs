using System;
using NUnit.Framework;

namespace EPAM.Deltix.HdTime.Tests
{
	[TestFixture]
	public class DateConverterTest
	{
		private static readonly Int64[] TestMilliseconds =
		{
			Int64.MinValue,
			Int64.MaxValue,
			3600000000,
			3612344321,
			DateTime.Now.Ticks / DateConversionConstants.TicksPerMillisecond,
			DateTime.Now.AddYears(10).Ticks / DateConversionConstants.TicksPerMillisecond,
			DateTime.Now.AddYears(20).Ticks / DateConversionConstants.TicksPerMillisecond,
			DateTime.Now.AddYears(30).Ticks / DateConversionConstants.TicksPerMillisecond,
		};

		private static readonly DateTime[] TestDates =
		{
			DateTime.MinValue,
			DateTime.MaxValue,
			DateTime.Now,
			DateTime.Now.AddYears(10),
			DateTime.Now.AddYears(20),
			DateTime.Now.AddYears(30)
		};

		private static readonly Int64[] TestOffset =
		{
			3600000000,
			177887871234,
			(Int64) TimeSpan.FromMilliseconds((Double) 1000 * 60 * 60 * 24 * 365).TotalMilliseconds,
			(Int64) TimeSpan.FromMilliseconds((Double) 1000 * 60 * 60 * 10 * 24 * 365).TotalMilliseconds,
			(Int64) TimeSpan.FromMilliseconds((Double) 1000 * 60 * 60 * 20 * 24 * 365).TotalMilliseconds,
			(Int64) TimeSpan.FromMilliseconds((Double) 1000 * 60 * 60 * 30 * 24 * 365).TotalMilliseconds,
			(Int64) TimeSpan.FromHours(24 * 365).TotalMilliseconds,
			(Int64) TimeSpan.FromHours(10 * 24 * 365).TotalMilliseconds,
			(Int64) TimeSpan.FromHours(20 * 24 * 365).TotalMilliseconds,
			(Int64) TimeSpan.FromHours(30 * 24 * 365).TotalMilliseconds,
			(Int64) TimeSpan.FromDays(365).TotalMilliseconds,
			(Int64) TimeSpan.FromDays(10 * 365).TotalMilliseconds,
			(Int64) TimeSpan.FromDays(20 * 365).TotalMilliseconds,
			(Int64) TimeSpan.FromDays(30 * 365).TotalMilliseconds,
		};

		[Test]
		[TestCaseSource("TestMilliseconds")]
		public void Validate_FromJavaMilliseconds(Int64 testValue)
		{
			DateTime timestamp = DateConverter.FromEpochMilliseconds(testValue);
			Int64 javaMilliseconds = DateConverter.ToEpochMilliseconds(timestamp);

			Assert.IsTrue(javaMilliseconds == testValue);
		}

		[Test]
		[TestCaseSource("TestDates")]
		public void Validate_ToJavaMilliseconds(DateTime testValue)
		{
			Int64 javaMillisecondsModulo;
			HdDateTime d1 = new HdDateTime(testValue);
			Int64 javaMilliseconds = DateConverter.ToEpochMilliseconds(testValue, out javaMillisecondsModulo);
			DateTime timestamp = HdDateTime.FromEpochMilliseconds(javaMilliseconds, javaMillisecondsModulo).DateTime;

			Assert.AreEqual(testValue, d1.DateTime);
			//Assert.AreEqual(timestamp.Ticks, testValue.Ticks, String.Format("{0} != {1}", timestamp.Ticks, testValue.Ticks));
			Assert.AreEqual(testValue, timestamp, String.Format("{0} != {1}", timestamp.Ticks, testValue.Ticks));
		}


		[Test]
		[TestCaseSource("TestOffset")]
		public void ValidateToJavaOffset(Int64 testValue)
		{
			TimeSpan offset = DateConverter.FromMilliseconds(testValue);
			Int64 javaOffset = DateConverter.ToMilliseconds(offset);

			Assert.IsTrue(javaOffset == testValue);
		}

		// TODO: WIP On new range checks
		//[Test]
		public void TestMinvalueConversion()
		{
			HdDateTime time1 = HdDateTime.MinValue;
			HdDateTime time2 = time1.Add(new HdTimeSpan(10));

			var dt = time2.Date;
		}
		

		[Test]
		public void TestConversionConstants()
		{
			// We must verify that our min/max constants match for all the types
			// If these tests are broken, all data conversions should be considered invalid until the code is fixed
			Assert.AreEqual(Int64.MaxValue, Convert.TimeSpan.Max);
			Assert.AreEqual(-Int64.MaxValue, Convert.TimeSpan.Min);
			Assert.Greater(Int64.MaxValue, Convert.DateTime.Max);
			Assert.Less(-Int64.MaxValue, Convert.DateTime.Min);

			Assert.AreEqual(Int64.MaxValue, TimeSpan.MaxValue.Ticks);
			Assert.AreEqual(Int64.MinValue, TimeSpan.MinValue.Ticks);
			Assert.AreEqual(Int64.MaxValue, HdTimeSpan.MaxValue.TotalNanoseconds);
			Assert.AreEqual(-Int64.MaxValue, HdTimeSpan.MinValue.TotalNanoseconds);

			Assert.AreNotEqual(Int64.MaxValue, DateTime.MaxValue.Ticks);
			Assert.AreNotEqual(Int64.MinValue, DateTime.MinValue.Ticks);
			Assert.Greater(Int64.MaxValue, HdDateTime.MaxValue.EpochNanoseconds);
			Assert.Less(-Int64.MaxValue, HdDateTime.MinValue.EpochNanoseconds);

			//Console.Error.WriteLine((new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Ticks);
			Assert.AreEqual((new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Ticks, Convert.DateTime.Gmt1970Ticks);

			Assert.AreEqual(-Convert.TimeSpan.Max, Convert.TimeSpan.Min);
			Assert.GreaterOrEqual(-Convert.TimeSpan.MaxTickMillis, Convert.TimeSpan.MinTickMillis);
			Assert.GreaterOrEqual(-Convert.TimeSpan.MaxTickSeconds, Convert.TimeSpan.MinTickSeconds);
			Assert.GreaterOrEqual(-Convert.TimeSpan.MaxMillis, Convert.TimeSpan.MinMillis);
			Assert.GreaterOrEqual(-Convert.TimeSpan.MaxSeconds, Convert.TimeSpan.MinSeconds);
			Assert.GreaterOrEqual(-Convert.TimeSpan.MaxMinutes, Convert.TimeSpan.MinMinutes);
			Assert.GreaterOrEqual(-Convert.TimeSpan.MaxHours, Convert.TimeSpan.MinHours);
			Assert.GreaterOrEqual(-Convert.TimeSpan.MaxDays, Convert.TimeSpan.MinDays);

			Assert.Greater(0, Convert.TimeSpan.Min);
			Assert.Greater(0, Convert.TimeSpan.MinTickMillis);
			Assert.Greater(0, Convert.TimeSpan.MinTickSeconds);
			Assert.Greater(0, Convert.TimeSpan.MinMillis);
			Assert.Greater(0, Convert.TimeSpan.MinSeconds);
			Assert.Greater(0, Convert.TimeSpan.MinMinutes);
			Assert.Greater(0, Convert.TimeSpan.MinHours);
			Assert.Greater(0, Convert.TimeSpan.MinDays);
			Assert.Greater(0, Convert.DateTime.Min);

			Assert.Greater(0, Convert.DateTime.MinMillis);
			Assert.Greater(0, Convert.DateTime.MinSeconds);
			Assert.Greater(0, Convert.DateTime.MinMinutes);
			Assert.Greater(0, Convert.DateTime.MinHours);
			Assert.Greater(0, Convert.DateTime.MinDays);

			Assert.Less(Convert.NanosInMillisecond, (Int64)Int32.MaxValue);
			Assert.Less(Convert.NanosInSecond, (Int64)Int32.MaxValue);
			Assert.Greater(Convert.NanosInMinute, (Int64)Int32.MaxValue);
			Assert.Greater(Convert.NanosInHour, (Int64)Int32.MaxValue);
			Assert.Greater(Convert.NanosInDay, (Int64)Int32.MaxValue);

			Assert.Greater(Convert.TimeSpan.MaxMillis, (Int64)Int32.MaxValue);
			Assert.Greater(Convert.TimeSpan.MaxSeconds, (Int64)Int32.MaxValue);
			Assert.Less((Int64)Convert.TimeSpan.MaxMinutes, (Int64)Int32.MaxValue);
			Assert.Less((Int64)Convert.TimeSpan.MaxHours, (Int64)Int32.MaxValue);
			Assert.Less((Int64)Convert.TimeSpan.MaxDays, (Int64)Int32.MaxValue);

			Assert.Greater(Convert.DateTime.MaxMillis, (Int64)Int32.MaxValue);
			Assert.Greater(Convert.DateTime.MaxSeconds, (Int64)Int32.MaxValue);
			Assert.Less((Int64)Convert.DateTime.MaxMinutes, (Int64)Int32.MaxValue);
			Assert.Less((Int64)Convert.DateTime.MaxHours, (Int64)Int32.MaxValue);
			Assert.Less((Int64)Convert.DateTime.MaxDays, (Int64)Int32.MaxValue);

			Assert.Greater(Convert.TimeSpan.MaxTickMillis, (Int64)Int32.MaxValue);
			Assert.Greater((Int64)Convert.TimeSpan.MaxTickSeconds, (Int64)Int32.MaxValue);

			Assert.Less((Int64)Convert.TicksInMillisecond, (Int64)Int32.MaxValue);
			Assert.Less((Int64)Convert.TicksInSecond, (Int64)Int32.MaxValue);

			Assert.AreEqual(Convert.DateTime.DateTimeMinValueTicks, DateTime.MinValue.Ticks);
			Assert.AreEqual(Convert.DateTime.DateTimeMaxValueTicks, DateTime.MaxValue.Ticks);
		}
	}
}
