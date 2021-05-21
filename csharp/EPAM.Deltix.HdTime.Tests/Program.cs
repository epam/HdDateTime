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

using EPAM.Deltix.HdTime;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Convert = EPAM.Deltix.HdTime.Convert;

namespace EPAM.Deltix.HdTime.Tests
{
	using T =
		HdTimeSpan;
		//HdTimeSpan;
		//TimeSpan;
		//Int64;

	class Program
	{

		public void TestPrint()
		{
			TimeSpan ts = new TimeSpan(123, 45, 67, 9, 1234);
			HdTimeSpan hts = new HdTimeSpan(ts);
			Console.WriteLine(ts);
			Console.WriteLine(hts);
			Assert.Equals(ts.ToString(), hts.ToString());
		}

		public class SimpleBenchmark
		{
			//[MethodImpl(MethodImplOptions.NoInlining)]
			//static private T F(T x) { return x.Negate(); }
			//static private T F(T x) { return x.Add(new T(1)); }
			//static private T F(T x) { return new T((long)x.TotalSeconds); }
			//static private T F(T x) { return x.AddUnchecked(new T(1)); }
			//static private T F(T x) { return x.Add(new TimeSpan(1)); }
			//static private T F(T x)
			//{
			//	return T.FromTicks((x.Nanoseconds & 0x7fff) + Convert.DateTime.Gmt1970Ticks);
			//}
			static private T F(T x) { return new T(x.ToString().Length + Convert.DateTime.Gmt1970Ticks); }
			//static private T F(T x) { return new T(((DateTime)x).ToString("yyyy-MM-dd HH:mm:ss.fffffff").Length + Convert.DateTime.Gmt1970Ticks); }
			//static private T F(T x) { return HdDateTime.Parse(x.ToString()); }

			private static string FName =
					"ToString"
					//"FromTicks"
			//"new"
			//"Add"
			//"Duration"
			//"AddUnchecked"
			//"Negate"
			;


			private static T F0(T x)
			{
				return F(x);
			}

			public static void Run()
			{
				const int N = 20000000;
				double seconds = 2;
				TimeSpan timeLimit = TimeSpan.FromSeconds(seconds);
				Stopwatch t0 = Stopwatch.StartNew();
				Stopwatch t = new Stopwatch();
				double Best = 1E99, Total = 0;
				T y = new T(12345); // dummy value

				int j = 0;
				for (;;)
				{
					T x = F0(y);
					t.Restart();
					for (uint i = N / 8; i != 0; --i)
					{
						x = F(F(F(F(F(F(F(F(x))))))));
						//x = F(x); x = F(x); x = F(x); x = F(x);
						//x = F(x); x = F(x); x = F(x); x = F(x);
					}

					t.Stop();
					if (t.Elapsed.TotalSeconds < Best)
					{
						Best = t.Elapsed.TotalSeconds;
						y = x;
					}

					Total += t.Elapsed.TotalSeconds;
					if (t0.Elapsed > timeLimit)
						break;
					++j;
				}

				Console.WriteLine($"Testing {typeof(T)}.{FName}() in {IntPtr.Size*8}-bit mode\n {1.0E9 / N * Best} ns/op\n {N/Best/1E6} M/s rate\nTime Elapsed: {Total} s\ntest iterations: {j}");

				Console.WriteLine(F0(y).ToString());
				Console.WriteLine(((TimeSpan)(y)).ToString());
			}
		}


		static public void Scratchpad()
		{
			var dt = DateTime.Now;
			HdDateTime hdt = DateTime.UtcNow;
			var ts = TimeSpan.FromSeconds(123);
			var hts = HdTimeSpan.FromDays(123L);
			//ts.Days
			//dt.Day
			//dt.AddDays()
			//HdTimeSpan.
			Console.WriteLine(HdDateTime.MinValue);
			Console.WriteLine(HdDateTime.MaxValue);
			Console.WriteLine(new HdDateTime(1678, 1, 1).EpochNanoseconds);
			Console.WriteLine(new HdDateTime(2262, 1, 1).EpochNanoseconds);
			Console.WriteLine(new HdDateTime(2262, 1, 1).AddNanoseconds(-1));
			Console.WriteLine(HdDateTime.MaxValue);

			Console.WriteLine(hdt);
			Console.WriteLine(hdt.AddTicks(1));
			Console.WriteLine(hdt.AddMilliseconds(1));
			Console.WriteLine(hdt.AddSeconds(1));
			Console.WriteLine(hdt.AddHours(12));
			Console.WriteLine(hdt.AddDays(123));

			int a = hdt.Millisecond, b = hdt.Second, c = hdt.Minute, d = hdt.Hour, e = hdt.Day, f = hdt.Month, g = hdt.Year, h = hdt.DayOfYear;
			hdt = hdt.Date;
			HdTimeSpan tsss = hdt.TimeOfDay;

			DayOfWeek dw = hdt.DayOfWeek;

			//dt.AddTicks();
			//hdt.AddTicks()
		}

		static void Main(string[] args)
		{
			HdDateTime dt = new HdDateTime(2000, 1, 1, 1, 1, 1, 12);
			Console.WriteLine(dt.ToString("yyyy-MM-dd HH:mm:ss.fff"));
			Console.WriteLine(dt.ToString("yyyy-MM-dd HH:mm:ss.ff"));

			SimpleBenchmark.Run();
			//Scratchpad();
		}
	}
}
