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
using System.Collections.Generic;
using System.Diagnostics;

namespace EPAM.Deltix.HdTime
{
	internal static class Formatters
	{
		private const int MAX_SERIALIZERS = 0x100;
		private const int BUFFER_LENGTH = 0x100 - 16;

		protected internal interface IFormattable
		{
			int Format(char[] to, int ofs, ref FormatComponents components);
		}

		private class Context
		{
			internal static readonly Dictionary<String, Template> GlobalDtTemplateCache =
				new Dictionary<String, Template>();

			internal static readonly Dictionary<String, Template> GlobalTsTemplateCache =
				new Dictionary<String, Template>();

			internal readonly char[] Buffer = new char[BUFFER_LENGTH];

			internal DateTime DateTimeFormatter = new Formatters.DateTime(GlobalDtTemplateCache);
			internal readonly Dictionary<String, Template> DtTemplateCache = new Dictionary<String, Template>();
			internal String LastDtFmtStr;
			internal Template LastDtTemplate;

			internal TimeSpan TimeSpanFormatter = new Formatters.TimeSpan(GlobalTsTemplateCache);
			internal readonly Dictionary<String, Template> TsTemplateCache = new Dictionary<String, Template>();
			internal String LastTsFmtStr;
			internal Template LastTsTemplate;
		};

		//static ThreadLocal<Context> tls = new ThreadLocal<Context>(() => { return new Context(); });
		[ThreadStatic]
		private static Context tls;

		// Print various basic values. Unrolled methods for various lengths.
		#region Printing
		private class Print
		{
			// For all decimal printing methods it is guaranteed that the value is not bigger than the specified number of total digits

			internal static int Dec1(char[] to, int ofs, int x)
			{
				to[ofs] = (char)('0' + x);
				return ofs - 1;
			}

			internal static int Dec2(char[] to, int ofs, int x)
			{
				int x10 = x / 10;
				int newOfs = x >= 10 ? ofs - 2 : ofs - 1;
				to[ofs - 1] = (char)('0' + x10);
				to[ofs] = (char)(x + '0' - x10 * 10);
				return newOfs;
			}

			internal static int Dec3(char[] to, int ofs, int x)
			{
				int x10 = x / 10;
				to[ofs--] = (char)(x + '0' - x10 * 10);
				int x100 = x10 / 10;
				to[ofs - 1] = (char)('0' + x100);
				int newOfs = x >= 100 ? ofs - 2 : x >= 10 ? ofs - 1 : ofs;
				to[ofs] = (char)(x10 + '0' - x100 * 10);
				return newOfs;
			}

			internal static int Dec2w2(char[] to, int ofs, int c)
			{
				int hi = c / 10;
				to[ofs - 1] = (char)('0' + hi);
				to[ofs] = (char)('0' + c - hi * 10);
				return ofs - 2;
			}

			internal static int Dec3w2(char[] to, int ofs, int c)
			{
				int hi = c / 100;
				to[ofs - 2] = (char)('0' + hi);
				Dec2w2(to, ofs, c - hi * 100);
				return c >= 100 ? ofs - 3 : ofs - 2;
			}

			internal static int Dec3w3(char[] to, int ofs, int c)
			{
				int hi = c / 100;
				to[ofs - 2] = (char)('0' + hi);
				Dec2w2(to, ofs, c - hi * 100);
				return ofs - 3;
			}

			internal static int Dec4w4(char[] to, int ofs, int value)
			{
				int hi = value / 100;
				Dec2w2(to, ofs - 2, hi);
				Dec2w2(to, ofs, value - hi * 100);
				return ofs - 4;
			}

			internal static int Dec(char[] to, int ofs, int x)
			{
				do
				{
					int y = x / 10;
					to[ofs--] = (char)(x + '0' - y * 10);
					x = y;
				} while (x != 0);

				return ofs;
			}

			internal static int Dec(char[] to, int ofs, int x, int n)
			{
				ofs -= n;
				do
				{
					int y = x / 10;
					to[ofs + n] = (char)(x + '0' - y * 10);
					x = y;
				} while (--n != 0);

				return ofs;
			}

			internal static int sign(char[] to, int ofs, int sign)
			{
				Debug.Assert(sign == 0 || sign == -1);
				to[ofs] = '-';
				return ofs + sign;
			}

			internal static int Str(char[] to, int ofs, String str)
			{
				return Str(to, ofs, str, str.Length);
			}

			internal static int Str(char[] to, int ofs, String str, int n)
			{

				ofs -= n;
				for (int i = n - 1; i >= 0; --i)
					to[ofs + 1 + i] = str[i];

				return ofs;
			}

			internal static int Str(char[] to, int ofs, char[] str)
			{
				return Str(to, ofs, str, str.Length);
			}

			internal static int Str(char[] to, int ofs, char[] str, int n)
			{
				ofs -= n;
				for (int i = n - 1; i >= 0; --i)
					to[ofs + 1 + i] = str[i];

				return ofs;
			}
		}   //	class Print
		#endregion Printing

		// Callbacks that print various format fields
		#region Field handlers

		private abstract class Field : FormatField, IFormattable
		{
			public abstract int Format(char[] to, int ofs, ref FormatComponents components);
		}

		private abstract class StaticField : StaticFormatField, IFormattable
		{
			public abstract int Format(char[] to, int ofs, ref FormatComponents components);
		}

		private class Fail : StaticField
		{
			readonly String Str;
			internal Fail(String str) => Str = str;

			public override int Format(char[] to, int ofs, ref FormatComponents components)
			{
				throw new InvalidOperationException();
			}
		}

		private sealed class StringField : StaticField
		{
			readonly char[] chars;
			readonly int hashCode;

			internal StringField(String str)
			{
				this.chars = str.ToCharArray();
				hashCode = base.GetHashCode() ^ str.GetHashCode();
			}


			public override int GetHashCode() => hashCode;

			public override bool Equals(Object other)
			{
				return this == other || other is StringField && hashCode == ((StringField) other).hashCode;
			}

			public override int Format(char[] to, int ofs, ref FormatComponents components) => Print.Str(to, ofs, chars);

			public override String ToString() => new String(chars);
		}

		private sealed class CharField : StaticField
		{
			char ch;

			internal CharField(char ch) => this.ch = ch;

			public override int GetHashCode() => base.GetHashCode() * 31 ^ ch;

			public override bool Equals(Object other)
			{
				return this == other || other is CharField && ch == ((CharField) other).ch;
			}

			public override int Format(char[] to, int ofs, ref FormatComponents components)
			{
				to[ofs] = ch;
				return ofs - 1;
			}
		}


		private sealed class SignField : Field
		{
			public override int Format(char[] to, int ofs, ref FormatComponents components)
			{
				return Print.sign(to, ofs, components.sign);
			}
		}


		private sealed class YearsField2w02 : Field
		{
			public override int Format(char[] to, int ofs, ref FormatComponents components)
			{
				return Print.Dec2w2(to, ofs, components.year % 100);
			}
		}


		private sealed class YearsField4w04 : Field
		{
			public override int Format(char[] to, int ofs, ref FormatComponents components)
			{
				return Print.Dec4w4(to, ofs, components.year);
			}
		}


		private sealed class MonthNumField2 : Field
		{
			public override int Format(char[] to, int ofs, ref FormatComponents components)
			{
				return Print.Dec2(to, ofs, components.month);
			}
		}

		private sealed class MonthNumField2w02 : Field
		{
			public override int Format(char[] to, int ofs, ref FormatComponents components)
			{
				return Print.Dec2w2(to, ofs, components.month);
			}
		}

		private sealed class MonthTextField3w3 : Field
		{
			public override int Format(char[] to, int ofs, ref FormatComponents components)
			{
				return Print.Str(to, ofs, Months.Months3[components.month], 3);
			}
		}

		private sealed class MonthTextField : Field
		{
			private readonly int n;

			internal MonthTextField(int n) => this.n = n;

			public override int Format(char[] to, int ofs, ref FormatComponents components)
			{
				String s = Months.MonthsFull[components.month];
				int len = s.Length;
				Print.Str(to, ofs, Months.MonthsFull[components.month], len);
				ofs -= len;
				if (len < n)
				{
					for (int i = n - len; i != 0; --i)
					{
						to[ofs--] = ' ';
					}
				}

				return ofs;
			}
		}

		private sealed class DaysCountField : Field
		{
			public override int Format(char[] to, int ofs, ref FormatComponents components) => Print.Dec(to, ofs, components.day);
		}

		private sealed class DayInMonthField2 : Field
		{
			public override int Format(char[] to, int ofs, ref FormatComponents components) => Print.Dec2(to, ofs, components.day);
		}

		private sealed class DayInMonthField2w02 : Field
		{
			public override int Format(char[] to, int ofs, ref FormatComponents components) => Print.Dec2w2(to, ofs, components.day);
		}

		private sealed class Hours24Field2 : Field
		{
			public override int Format(char[] to, int ofs, ref FormatComponents components) => Print.Dec2(to, ofs, components.hour);
		}

		private sealed class Hours24Field2w02 : Field
		{
			public override int Format(char[] to, int ofs, ref FormatComponents components) => Print.Dec2w2(to, ofs, components.hour);
		}

		private sealed class MinutesField2 : Field
		{
			public override int Format(char[] to, int ofs, ref FormatComponents components) => Print.Dec2(to, ofs, components.minute);
		}

		private sealed class MinutesField2w02 : Field
		{
			public override int Format(char[] to, int ofs, ref FormatComponents components) => Print.Dec2w2(to, ofs, components.minute);
		}

		private sealed class SecondsField2 : Field
		{
			public override int Format(char[] to, int ofs, ref FormatComponents components) => Print.Dec2(to, ofs, components.second);
		}

		private sealed class SecondsField2w02 : Field
		{
			public override int Format(char[] to, int ofs, ref FormatComponents components) => Print.Dec2w2(to, ofs, components.second);
		}

		private sealed class MillisecondsField3 : Field
		{
			public override int Format(char[] to, int ofs, ref FormatComponents components) => Print.Dec3(to, ofs, components.nanosecond / 1000000);
		}

		private sealed class MillisecondsField3w02 : Field
		{
			public override int Format(char[] to, int ofs, ref FormatComponents components) => Print.Dec3w2(to, ofs, components.nanosecond / 1000000);
		}

		private sealed class FractionsField9 : Field
		{
			public override int Format(char[] to, int ofs, ref FormatComponents components) => Print.Dec(to, ofs, components.nanosecond, 9);
		}

		private sealed class FractionsField8 : Field
		{
			public override int Format(char[] to, int ofs, ref FormatComponents components) => Print.Dec(to, ofs, components.nanosecond / 10, 8);
		}

		private sealed class FractionsField7 : Field
		{
			public override int Format(char[] to, int ofs, ref FormatComponents components) => Print.Dec(to, ofs, components.nanosecond / 100, 7);
		}

		private sealed class FractionsField6 : Field
		{
			public override int Format(char[] to, int ofs, ref FormatComponents components) => Print.Dec(to, ofs, components.nanosecond / 1000, 6);
		}

		private sealed class FractionsField5 : Field
		{
			public override int Format(char[] to, int ofs, ref FormatComponents components) => Print.Dec(to, ofs, components.nanosecond / 10000, 5);
		}

		private sealed class FractionsField4 : Field
		{
			public override int Format(char[] to, int ofs, ref FormatComponents components) => Print.Dec4w4(to, ofs, components.nanosecond / 100000);
		}

		// Also used for Millis field
		private sealed class FractionsField3 : Field
		{
			public override int Format(char[] to, int ofs, ref FormatComponents components) => Print.Dec3w3(to, ofs, components.nanosecond / 1000000);
		}

		private sealed class FractionsField2 : Field
		{
			public override int Format(char[] to, int ofs, ref FormatComponents components) => Print.Dec2w2(to, ofs, components.nanosecond / 10000000);
		}

		private sealed class FractionsField1 : Field
		{
			public override int Format(char[] to, int ofs, ref FormatComponents components) => Print.Dec1(to, ofs, components.nanosecond / 100000000);
		}

		#endregion Field handlers

		// Formatters template
		// Prints a sequence of format fields.
		// Can be replaced with more specialized hardcoded implementations for various pre-defined format strings
		internal class Template : IFormattable
		{
			private readonly IFormattable[] fields;
			protected Template(IFormattable[] fields) => this.fields = fields;

			#region IFormattable
			public int Format(char[] to, int ofs, ref FormatComponents components)
			{
				for (int i = fields.Length - 1; i >= 0; --i)
					ofs = fields[i].Format(to, ofs, ref components);

				return ofs;
			}

			#endregion IFormattable

			public String Format(FormatComponents components)
			{
				// TODO: Temporary implementation, remove soon
				char[] tmp = new char[0x100];
				int length = Format(tmp, 0, ref components);
				return new String(tmp, 0, length);
			}

			internal class Builder : ArrayBuilder<IFormattable>
			{
				public Template Get() => new Template((IFormattable[])GetFields());
			}
		}

		/**
		 * Formatter describes how to build FormatTemplate from a Format String
		 * In this implementation it is already specialized with some default date/time fields
		 * Almost all logic and data of the class is static
		 * Instance only does the following extra things:
		 * 1) Hosts an instance of FormatTemplate.Builder
		 * 2) Implements FormatString delegate interface, for creating a new FormatTemplate via the Builder
		 * 3) getTemplate() instance method invokes FormatString.parse with a new format string
		 *    and additionally implements caching logic.
		 *    Can be called multiple times to parse multiple format strings.
		 *
		 * NOTE: The current implementation via static members for field handlers is ugly and redundant,
		 * will probably be redone after the benchmarks are in place to avoid performance degradation
		 *
		 */
		internal abstract class Formatter : FormatString.ITarget
		{
			// Instance fields
			private readonly Dictionary<String, Template> globalTemplateCache; // Set by a child, references single global instance of template Cache
			protected readonly Template.Builder builder = new Template.Builder();

			protected Formatter(Dictionary<String, Template> globalTemplateCache)
			{
				this.globalTemplateCache = globalTemplateCache;
			}

			#region Formatter: FormatString.ITarget delegate

			public void AddString(String str, int ofs, int n)
			{
				AddString(str.Substring(ofs, n));
			}

			public void AddString(String str)
			{
				int len = str.Length;
				if (len > 0)
					builder.Add(len == 1 ? (StaticField)new CharField(str[0]) : new StringField(str));
			}

			public abstract int FieldLength(char fieldChar);
			public abstract char PaddingForField(char fieldChar);
			public abstract void AddField(char fieldChar, int length);

			#endregion Formatter: FormatString.ITarget delegate

			protected void AddField(IFormattable handler) => builder.Add(handler);

			protected virtual void ParseFormat(String fmt) => FormatString.Parse(fmt, this);

			protected Template GetCachedOrNewTemplate(String fmt, Dictionary<String, Template> local)
			{
				Template f = null;
				Dictionary<String, Template> global = globalTemplateCache;
				// Try to find in the global formatter cache
				lock (global)
				{
					if (global.ContainsKey(fmt))
						f = global[fmt];
				}

				if (null == f)
				{
					builder.Clear();
					ParseFormat(fmt);
					f = builder.Get();
					lock(global)
					{
						if (!global.ContainsKey(fmt))
							global.Add(fmt, f);
						else
							f = global[fmt];
					}

					Debug.Assert(null != f);
				}

				// Copy the reference to local cache
				local.Add(fmt, f);
				return f;
			}

			protected Template GetTemplate(String fmt, Dictionary<String, Template> local)
			{
				Template f;
				return local.TryGetValue(fmt, out f) ? f : GetCachedOrNewTemplate(fmt, local);
			}

			protected static int Format(char[] buffer, ref FormatComponents components, Template f)
			{
				return f.Format(buffer, BUFFER_LENGTH - 1, ref components);
			}
		}

		/**
		 * DefaultTimeFormatter specializes Formatter with some default date/time fields
		 */
		internal abstract class DefaultTimeFormatter : Formatters.Formatter
		{
			// Static fields
			protected static readonly IFormattable[][] fields;
			protected static readonly int[] fieldLengths;

			protected DefaultTimeFormatter(Dictionary<String, Template> globalTemplateCache) : base(globalTemplateCache)
			{
			}

			#region Formatter: Static methods

			// TODO: NOTE: The static storage for field format descriptors is maybe redundant and the necessity of these methods is questionable
			// If benchmarks later prove there is no harm in using singleton instances instead, the code will be significantly simplified
			static private void RegisterField(char letter, int maxlength, params IFormattable[] handlers)
			{
				fieldLengths[letter] = maxlength;
				fields[letter] = handlers;
			}

			static private void RegisterSynonymousField(char letterSrc, char letterDst)
			{
				fieldLengths[letterDst] = fieldLengths[letterSrc];
				fields[letterDst] = fields[letterSrc];
			}

			#endregion Formatter: Static methods

			static DefaultTimeFormatter()
			{
				const int N = SByte.MaxValue + 1;
				fieldLengths = new int[N];
				fields = new IFormattable[N][];
				IFormattable f;
				// Hour in day [0..23] (Java & .NET)
				RegisterField('H', 2, f = new Hours24Field2(), f, new Hours24Field2w02());
				// Minute in hour (Java & .NET)
				RegisterField('m', 2, f = new MinutesField2(), f, new MinutesField2w02());
				// Second in minute (Java & .NET)
				RegisterField('s', 2, f = new SecondsField2(), f, new SecondsField2w02());
				// Millisecond (Java)
				//registerField('S', 3, f = new MillisecondsField3(), f, new MillisecondsField3w02(), new FractionsField3());
				// Fractions (.NET, also replaces Java8Time 'S' char)
				RegisterField('f', Int32.MaxValue,
	
					new Fail("Fractions field can't be longer than 9 digits"),
					new FractionsField1(), new FractionsField2(), new FractionsField3(),
					new FractionsField4(), new FractionsField5(), new FractionsField6(),
					new FractionsField7(), new FractionsField8(), new FractionsField9());

				RegisterSynonymousField('f', 'S');
				RegisterSynonymousField('f', 'F');
			}

			#region Formatter: FormatString.ITarget delegate

			public override char PaddingForField(char fieldChar)
			{
				// All fields that need padding are 0-padded
				return '0';
			}

			protected void AddField(IFormattable[] handlers, int length)
			{
				int handlersCount = handlers.Length;
				Debug.Assert(handlersCount > 0); // Should never be called with invalid fieldChar
				AddField(handlers[length < handlersCount ? length : 0]);
			}

			#endregion Formatter: FormatString.ITarget delegate
		}

		internal class DateTime : DefaultTimeFormatter
		{
			protected static readonly IFormattable[][] fields;
			protected static readonly int[] fieldLengths;

			internal DateTime(Dictionary<String, Template> globalTemplateCache) : base(globalTemplateCache)
			{
			}

			static protected void RegisterField(char letter, int maxlength, params IFormattable[] handlers)
			{
				fieldLengths[letter] = maxlength;
				fields[letter] = handlers;
			}

			static protected void RegisterSynonymousField(char letterSrc, char letterDst)
			{
				fieldLengths[letterDst] = fieldLengths[letterSrc];
				fields[letterDst] = fields[letterSrc];
			}

			static DateTime()
			{
				fields = (IFormattable[][])DefaultTimeFormatter.fields.Clone();
				fieldLengths = (int[])DefaultTimeFormatter.fieldLengths.Clone();
				Field f;
				// Epoch name (Java, no support for locale)
				RegisterField('G', Int32.MaxValue, new StringField("AD"));
				// Epoch name (.NET, no support for locale)
				RegisterSynonymousField('G', 'g');

				// Year number (Java & .NET) Java substitution logic is used (y/yyy->yyyy)
				RegisterField('y', 4, f = new YearsField4w04(), f, new YearsField2w02(), f, f);

				// Year number (Java)
				RegisterSynonymousField('y', 'u');

				// Month number or name (Java & .NET)
				RegisterField('M', Int32.MaxValue, (IFormattable)null,

								new MonthNumField2(), new MonthNumField2w02(),
								new MonthTextField3w3(), new MonthTextField(0));

				// Day of month [0..31] (Java & .NET)
				RegisterField('d', 2, f = new DayInMonthField2(), f, new DayInMonthField2w02());
			}

			#region Formatter: FormatString.ITarget delegate

			public override int FieldLength(char fieldChar) => fieldLengths[fieldChar & 0x7F];

			public override void AddField(char fieldChar, int length)
			{
				if ('M' == fieldChar && length > 4)
					AddField(new MonthTextField(length));
				else
					AddField(fields[fieldChar], length);
			}

			#endregion Formatter: FormatString.ITarget delegate

			static int FormatChars(Int64 dt, String fmt, Context ctx, char[] buffer)
			{
				FormatComponents components = Convert.DateTime.ToComponents(dt);
				// TODO: This code can now be simplified by moving methods/fields to Formatter instance
				return 1 + Format(
					       buffer,
					       ref components,
					       fmt == ctx.LastDtFmtStr ? ctx.LastDtTemplate : ctx.DateTimeFormatter.GetTemplate(fmt, ctx.DtTemplateCache));
			}

			internal static String Format(Int64 dt, String fmt)
			{
				Context ctx = tls;
				if (null == ctx)
					ctx = tls = new Context();

				Debug.Assert(fmt != null);
				char[] buffer = ctx.Buffer;
				int start = FormatChars(dt, fmt, ctx, buffer);
				return new String(ctx.Buffer, start, BUFFER_LENGTH - start);
			}
		}

		internal class TimeSpan : DefaultTimeFormatter
		{
			protected static readonly IFormattable[][] fields;
			protected static readonly int[] fieldLengths;

			internal TimeSpan(Dictionary<String, Template> globalTemplateCache) : base(globalTemplateCache)
			{
			}

			// Easier to just redefine.
			// This copy-pasted ugliness will be removed later
			private static void RegisterField(char letter, int maxlength, params IFormattable[] handlers)
			{
				fieldLengths[letter] = maxlength;
				fields[letter] = handlers;
			}

			static TimeSpan()
			{
				fields = (IFormattable[][])DefaultTimeFormatter.fields.Clone();
				fieldLengths = (int[])DefaultTimeFormatter.fieldLengths.Clone();

				IFormattable f;
				// Days count
				RegisterField('d', int.MaxValue, f = new DaysCountField(), f, f = new DaysCountField());
			}

			#region Formatter: FormatString.ITarget delegate

			public override int FieldLength(char fieldChar) => fieldLengths[fieldChar & 0x7F];

			public override void AddField(char fieldChar, int length) => AddField(fields[fieldChar], length);

			#endregion Formatter: FormatString.ITarget delegate

			protected override void ParseFormat(String fmt)
			{
				base.ParseFormat(fmt);

				// "Smart" sign insertion
				List<IFormattable> items = builder.Items;
				int n = items.Count;
				for (int i = 0; i < n; i++)
				{

					if (items[i] is StaticField)
						continue;

					IFormattable f;
					String s;
					// More magic
					if (i > 0 && (f = items[i - 1]) is StringField && (s = f.ToString()).EndsWith("0"))
					{
						int j, m = s.Length;
						for (j = m - 1; j >= 0 && s[j] == '0'; --j) { }
						if (0 == ++j)
						{
							--i;
						}
						else
						{
							items.Insert(i, new StringField(s.Substring(j)));
							items[i - 1] = new StringField(s.Substring(0, j));
						}
					}

					items.Insert(i, new SignField());
					return;
				}
			}

			static int FormatChars(long ts, String fmt, Context ctx, char[] buffer)
			{
				FormatComponents components = Convert.TimeSpan.ToComponents(ts);
				// TODO: This code can now be simplified by moving methods/fields to Formatter instance
				return 1 + Format(
						buffer,
						ref components,
						fmt == ctx.LastTsFmtStr ? ctx.LastTsTemplate : ctx.TimeSpanFormatter.GetTemplate(fmt, ctx.TsTemplateCache));
			}

			internal static String Format(long ts, String fmt)
			{
				Context ctx = tls;
				if (null == ctx)
					ctx = tls = new Context();

				Debug.Assert(fmt != null);
				char[] buffer = ctx.Buffer;
				int start = FormatChars(ts, fmt, ctx, buffer);
				return new String(ctx.Buffer, start, BUFFER_LENGTH - start);
			}
		}
	}
}
