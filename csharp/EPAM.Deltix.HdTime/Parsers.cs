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
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Text;


namespace EPAM.Deltix.HdTime
{
	internal static class Parsers
	{
		protected internal interface IParseable
		{
			int Parse(String from, int ofs, ref ParsedValue dst);
		}

		private class Context
		{
			internal static readonly Dictionary<String, Template> GlobalDtTemplateCache =
				new Dictionary<String, Template>();

			internal static readonly Dictionary<String, Template> GlobalTsTemplateCache =
				new Dictionary<String, Template>();

			internal Parser DateTimeParser = new Parsers.DateTime(GlobalDtTemplateCache);
			internal readonly Dictionary<String, Template> DtTemplateCache = new Dictionary<String, Template>();
			internal String LastDtFmtStr;
			internal Template LastDtTemplate;

			internal Parser TimeSpanParser = new Parsers.TimeSpan(GlobalTsTemplateCache);
			internal readonly Dictionary<String, Template> TsTemplateCache = new Dictionary<String, Template>();
			internal String LastTsFmtStr;
			internal Template LastTsTemplate;
		};

		[ThreadStatic] private static Context tls;

		private class Read
		{
			private static int Digit(String from, int i)
			{
				uint x = (uint)from[i] - '0';
				if (x > 9)
					throw new ParseException(from, i);

				return (int)x;
			}

			internal static int Component(String from, int ofs, ref ParsedValue dst, long scale)
			{

				int n = from.Length;
				if (ofs >= n)
					throw new ParseException(from, ofs);

				int x = Digit(from, ofs++);
				for (; ofs < n; ++ofs)
				{
					int c = from[ofs] - '0';
					if (c < 0 | c > 9)
						break;

					x = x * 10 + c;
				}

				dst.x += scale * x;
				return ofs;
			}

			internal static long Decimal(String from, int ofs)
			{
				int end = from.Length;
				int x = Digit(from, ofs++);
				for (; ofs < end; ++ofs)
				{
					uint c = (uint)from[ofs] - '0';
					if (c > 9)
						break;

					x = x * 10 + (int)c;
				}

				return ((long) ofs << 32) | (UInt32) x;
			}

			internal static int DecimalFixed(String from, int ofs, int end)
			{
				if (end > from.Length)
					throw new ParseException(from, ofs);

				int x = Digit(from, ofs++);
				for (; ofs < end; ++ofs)
				{
					x = x * 10 + Digit(from, ofs);
				}

				return x;
			}

			internal static int Component(String from,
				int ofs, int length,
				ref ParsedValue dst, long scale)
			{
				dst.x += scale * DecimalFixed(from, ofs, ofs + length);
				return ofs + length;
			}

			internal static int Sign(String from, int ofs, ref ParsedValue dst)
			{
				if (ofs >= from.Length)
					throw new ParseException(from, ofs);

				int sign = from[ofs] == '-' ? -1 : 0;
				dst.sign = (sbyte) sign;
				return ofs - sign;
			}
		}

		private abstract class Field : FormatField, IParseable
		{
			public abstract int Parse(String from, int ofs, ref ParsedValue dst);
		}

		private abstract class FixedLengthField : FormatField, IParseable
		{
			protected readonly int length;

			public FixedLengthField(int length)
			{
				this.length = length;
			}

			public abstract int Parse(String from, int ofs, ref ParsedValue dst);
		}

		private abstract class StaticField : StaticFormatField, IParseable
		{
			public abstract int Parse(String from, int ofs, ref ParsedValue dst);
		}

		private class Fail : StaticField
		{
			readonly String str;

			public Fail(String str)
			{
				this.str = str;
			}

			public override int Parse(String from, int ofs, ref ParsedValue dst)
			{
				throw new InvalidOperationException(str);
			}

			public override String ToString() => str;
		}

		private sealed class StringField : StaticField
		{
			readonly String str;

			internal StringField(String str)
			{
				this.str = str;
			}

			override public int GetHashCode()
			{
				return base.GetHashCode() ^ str.GetHashCode();
			}

			public override int Parse(String from, int ofs, ref ParsedValue dst)
			{
				int n = str.Length;
				for (int i = 0; i < n; ++i)
					if (from[ofs + i] != str[i])
						throw new ParseException(from, ofs + i);

				return n + ofs;
			}

			public override bool Equals(Object other)
			{
				return this == other || other is StringField && str.Equals(((StringField) other).str);
			}

			public override String ToString() => str;
		}

		private sealed class CharField : StaticField
		{
			char ch;

			public CharField(char ch) => this.ch = ch;

			public override int GetHashCode() => base.GetHashCode() * 31 ^ ch;

			public override bool Equals(Object other)
			{
				return this == other || other is CharField && ch == ((CharField) other).ch;
			}

			public override int Parse(String from, int ofs, ref ParsedValue dst)
			{
				if (ch != from[ofs])
					throw new ParseException(from, ofs);

				return ofs + 1;
			}

			public override String ToString() => ch.ToString();
		}

		private sealed class SignField : Field
		{
			public override int Parse(String from, int ofs, ref ParsedValue dst)
			{
				return Read.Sign(from, ofs, ref dst);
			}

			public override String ToString() => "-";
		}

		private sealed class YearsField4w4 : Field
		{
			public override int Parse(String from, int ofs, ref ParsedValue dst)
			{
				dst.year = Read.DecimalFixed(from, ofs, ofs + 4);
				return ofs + 4;
			}

			public override String ToString() => "YYYY";
		}

		private sealed class YearsField4 : Field
		{
			public override int Parse(String from, int ofs, ref ParsedValue dst)
			{
				long t = Read.Decimal(from, ofs);
				dst.year = (int) t;
				return (int) (t >> 32);
			}

			public override String ToString() => "Y";
		}

		private sealed class MonthNumField2 : Field
		{
			public override int Parse(String from, int ofs, ref ParsedValue dst)
			{
				long t = Read.Decimal(from, ofs);
				dst.month = (int) t;
				return (int) (t >> 32);
			}

			public override String ToString() => "M";
		}

		private sealed class MonthNumField2w2 : Field
		{
			public override int Parse(String from, int ofs, ref ParsedValue dst)
			{
				dst.month = Read.DecimalFixed(from, ofs, ofs + 2);
				return ofs + 2;
			}

			public override String ToString() => "MM";
		}

		private sealed class DaysCountField : Field
		{
			public override int Parse(String from, int ofs, ref ParsedValue dst)
			{
				// TODO: Range check
				return Read.Component(from, ofs, ref dst, Convert.NanosInDay);
			}

			public override String ToString() => "d";
		}

		private sealed class DaysCountField1w1 : Field
		{
			public override int Parse(String from, int ofs, ref ParsedValue dst)
			{
				dst.x += Convert.NanosInDay * Read.DecimalFixed(from, ofs, ofs + 1);
				return ofs + 1;
			}

			public override String ToString() => "d";
		}

		private sealed class DaysCountField2w2 : Field
		{
			public override int Parse(String from, int ofs, ref ParsedValue dst)
			{
				dst.x += Convert.NanosInDay * Read.DecimalFixed(from, ofs, ofs + 2);
				return ofs + 2;
			}

			public override String ToString() => "dd";
		}

		private sealed class DaysCountFieldN : FixedLengthField
		{
			internal DaysCountFieldN(int length) : base(length)
			{
			}

			public override int Parse(String from, int ofs, ref ParsedValue dst)
			{
				// TODO: Rangecheck for length > 5
				return Read.Component(from, ofs, length, ref dst, Convert.NanosInDay);
			}

			public override String ToString() => new String('d', length);
		}

		private sealed class DayOfMonthField2w2 : Field
		{
			public override int Parse(String from, int ofs, ref ParsedValue dst)
			{
				dst.day = Read.DecimalFixed(from, ofs, ofs + 2);
				return ofs + 2;
			}

			public override String ToString() => "dd";
		}

		private sealed class DayOfMonthField2 : Field
		{
			public override int Parse(String from, int ofs, ref ParsedValue dst)
			{
				long t = Read.Decimal(from, ofs);
				dst.day = (int) t;
				return (int) (t >> 32);
			}

			public override String ToString() => "d";
		}

		private sealed class Hours24Field2 : Field
		{
			public override int Parse(String from, int ofs, ref ParsedValue dst)
			{
				// TODO: Range check
				return Read.Component(from, ofs, ref dst, Convert.NanosInHour);
			}

			public override String ToString() => "H";
		}

		private sealed class Hours24Field2w2 : Field
		{
			public override int Parse(String from, int ofs, ref ParsedValue dst)
			{
				// TODO: Range check
				return Read.Component(from, ofs, 2, ref dst, Convert.NanosInHour);
			}

			public override String ToString() => "HH";
		}

		private sealed class MinutesField2 : Field
		{
			public override int Parse(String from, int ofs, ref ParsedValue dst)
			{
				// TODO: Range check
				return Read.Component(from, ofs, ref dst, Convert.NanosInMinute);
			}

			public override String ToString() => "m";
		}

		private sealed class MinutesField2w2 : Field
		{
			public override int Parse(String from, int ofs, ref ParsedValue dst)
			{
				// TODO: Range check
				return Read.Component(from, ofs, 2, ref dst, Convert.NanosInMinute);
			}

			public override String ToString() => "mm";
		}

		private sealed class SecondsField2 : Field
		{
			public override int Parse(String from, int ofs, ref ParsedValue dst)
			{
				// TODO: Range check
				return Read.Component(from, ofs, ref dst, Convert.NanosInSecond);
			}

			public override String ToString() => "s";
		}

		private sealed class SecondsField2w2 : Field
		{
			public override int Parse(String from, int ofs, ref ParsedValue dst)
			{
				// TODO: Range check
				return Read.Component(from, ofs, 2, ref dst, Convert.NanosInSecond);
			}

			public override String ToString() => "ss";
		}

		private sealed class FractionsField : FixedLengthField
		{
			readonly int scale;

			internal FractionsField(int length) : base(length)
			{
				int scale = 1000000000;
				while (0 != length--)
					scale /= 10;

				this.scale = scale;
			}

			public override int Parse(String from, int ofs, ref ParsedValue dst)
			{
				return Read.Component(from, ofs, length, ref dst, scale);
			}

			public override String ToString() => new String('f', length);
		}

		// Parsers template
		// Scans a sequence of format fields.
		// Can be replaced with more specialized hardcoded implementations for various pre-defined format strings
		internal class Template : IParseable
		{
			readonly IParseable[] fields;

			public Template(IParseable[] fields)
			{
				this.fields = fields;
			}

			public int Parse(String from, int ofs, ref ParsedValue dst)
			{
				for (int i = 0, n = fields.Length; i < n; ++i)
					ofs = fields[i].Parse(from, ofs, ref dst);

				return ofs;
			}

			internal class Builder : ArrayBuilder<IParseable>
			{
				int mask;

				internal int Mask => mask;

				public Template Get()
				{
					return new Template(GetFields());
				}

				public Builder Add(IParseable handler, int mask)
				{
					Add(handler);
					int m = this.mask;
					this.mask = m | mask;
					return 0 == (m & mask) ? this : null;
				}

				public override void Clear()
				{
					base.Clear();
					mask = 0;
				}

				public override String ToString() => Get().ToString();
			}

			public override String ToString()
			{
				StringBuilder b = new StringBuilder();
				foreach (var f in fields)
					b.Append(f);

				return b.ToString();
			}
		}

		internal abstract class Parser : FormatString.ITarget
		{
			// Instance fields
			protected readonly Dictionary<String, Template>
				globalTemplateCache; // Set by a child, references single global instance of template Cache

			protected internal readonly Template.Builder builder = new Template.Builder();

			internal Parser(Dictionary<String, Template> globalTemplateCache)
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
					builder.Add(len == 1 ? (StaticField) new CharField(str[0]) : new StringField(str));
			}

			public abstract int FieldLength(char fieldChar);
			public abstract char PaddingForField(char fieldChar);
			public abstract void AddField(char fieldChar, int length);

			#endregion Formatter: FormatString.ITarget delegate

			protected Template.Builder AddField(IParseable handler, int mask) => builder.Add(handler, mask);

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
					lock (global)
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

			internal Template GetTemplate(String fmt, Dictionary<String, Template> local)
			{
				Template f;
				return local.TryGetValue(fmt, out f) ? f : GetCachedOrNewTemplate(fmt, local);
			}

			protected static int Parse(String from, ref ParsedValue value, Template t)
			{
				return t.Parse(from, 0, ref value);
			}
		}

		internal abstract class DefaultTimeParser : Parsers.Parser
		{
			// Static fields
			protected static readonly IParseable[][] fields;
			protected static readonly int[] fieldLengths;
			protected static readonly int[] conditions;

			public DefaultTimeParser(Dictionary<String, Template> globalTemplateCache) : base(globalTemplateCache)
			{
			}

			#region Formatter: Static methods

			// TODO: NOTE: The static storage for field format descriptors is maybe redundant and the necessity of these methods is questionable
			// If benchmarks later prove there is no harm in using singleton instances instead, the code will be significantly simplified
			static private void RegisterField(char letter, int mask, int maxlength, params IParseable[] handlers)
			{
				fieldLengths[letter] = maxlength;
				conditions[letter] = mask;
				fields[letter] = handlers;
			}

			static private void RegisterSynonymousField(char letterSrc, char letterDst)
			{
				fieldLengths[letterDst] = fieldLengths[letterSrc];
				fields[letterDst] = fields[letterSrc];
				fields[letterDst] = fields[letterSrc];
			}

			#endregion Formatter: Static methods

			static DefaultTimeParser()
			{
				const int N = SByte.MaxValue + 1;
				fieldLengths = new int[N];
				conditions = new int[N];
				fields = new IParseable[N][];
				IParseable f;

				// Hour in day [0..23] (Java & .NET)
				RegisterField('H', 0xC0, 2, f = new Hours24Field2(), f, new Hours24Field2w2());

				// Minute in hour (Java & .NET)
				RegisterField('m', 0x100, 2, f = new MinutesField2(), f, new MinutesField2w2());

				// Second in minute (Java & .NET)
				RegisterField('s', 0x200, 2, f = new SecondsField2(), f, new SecondsField2w2());

				RegisterField('f', 0x400, Int32.MaxValue, new Fail("Fractions field can't be longer than 9 digits"),
					// Fractions (.NET)
					new FractionsField(1), new FractionsField(2), new FractionsField(3),
					new FractionsField(4), new FractionsField(5), new FractionsField(6),
					new FractionsField(7), new FractionsField(8), new FractionsField(9));

				RegisterSynonymousField('f', 'S');
			}

			#region Formatter: FormatString.ITarget delegate

			public override char PaddingForField(char fieldChar)
			{
				// All fields that need padding are 0-padded
				return '0';
			}

			#endregion Formatter: FormatString.ITarget delegate

			protected Template.Builder AddField(IParseable[] handlers, int mask, int length)
			{
				int handlersCount = handlers.Length;
				Debug.Assert(handlersCount > 0); // Should never be called with invalid fieldChar
				return AddField(handlers[length < handlersCount ? length : 0], mask);
			}
		}


		internal class DateTime : DefaultTimeParser
		{
			protected static readonly IParseable[][] fields;
			protected static readonly int[] fieldLengths;
			protected static readonly int[] conditions;

			internal DateTime(Dictionary<String, Template> globalTemplateCache) : base(globalTemplateCache)
			{
			}

			// region Parser: Static methods
			static private void RegisterField(char letter, int mask, int maxlength, params IParseable[] handlers)
			{
				fieldLengths[letter] = maxlength;
				conditions[letter] = mask;
				fields[letter] = handlers;
			}

			static private void RegisterSynonymousField(char letterSrc, char letterDst)
			{
				fieldLengths[letterDst] = fieldLengths[letterSrc];
				conditions[letterDst] = conditions[letterSrc];
				fields[letterDst] = fields[letterSrc];
			}

			static DateTime()
			{
				fields = (IParseable[][]) DefaultTimeParser.fields.Clone();
				fieldLengths = (int[]) DefaultTimeParser.fieldLengths.Clone();
				conditions = (int[]) DefaultTimeParser.conditions.Clone();

				IParseable f;
				// Epoch name (Java, no support for locale)
				RegisterField('G', 0, Int32.MaxValue, new StringField("AD"));
				// Epoch name (.NET, no support for locale)
				RegisterSynonymousField('G', 'g');

				// Year number (Java & .NET) Java substitution logic is used (y/yyy->yyyy)
				RegisterField('y', 1, 4, new Fail("Not implemented/y"), f = new YearsField4(),
					new Fail("2-digit year not implemented"), f, new YearsField4w4());

				// Year number (Java)
				RegisterSynonymousField('y', 'u');

				// Month number or name (Java & .NET)
				// TODO: Month names are currently unsupported
				RegisterField('M', 2, 2, new Fail("Not implemented/M"), new MonthNumField2(), new MonthNumField2w2());

				// Day of month [0..31] (Java & .NET)
				RegisterField('d', 0x3C, 2, new Fail("Not implemented/d"), new DayOfMonthField2(),
					new DayOfMonthField2w2());
			}

			#region Formatter: FormatString.ITarget delegate

			public override int FieldLength(char fieldChar) => fieldLengths[fieldChar & 0x7F];

			public override void AddField(char fieldChar, int length)
			{
				if (null == AddField(fields[fieldChar], conditions[fieldChar], length))
					throw new FormatError("Duplicate format field: " + fieldChar);
			}

			#endregion Formatter: FormatString.ITarget delegate

			protected override void ParseFormat(String fmt)
			{
				base.ParseFormat(fmt);
				int mask = builder.Mask;
				if (0 != (mask & mask + 1) || mask == 0)
					throw new FormatError("Incomplete DateTime format string: " + fmt);
			}

			internal static long Parse(String src, String fmt)
			{
				Context ctx = tls;
				if (null == ctx)
					ctx = tls = new Context();

				Debug.Assert(fmt != null);
				ParsedValue value = new ParsedValue();
				value.ResetDt();
				int end = Parse(src, ref value,
					fmt == ctx.LastDtFmtStr
						? ctx.LastDtTemplate
						: ctx.DateTimeParser.GetTemplate(fmt, ctx.DtTemplateCache));

				return value.GetDt();
			}
		}

		internal class TimeSpan : DefaultTimeParser
		{

			protected static readonly IParseable[][] fields;

			protected static readonly int[] fieldLengths;
			protected static readonly int[] conditions;

			internal TimeSpan(Dictionary<String, Template> globalTemplateCache) : base(globalTemplateCache)
			{
			}

			// region Parser: Static methods
			static private void RegisterField(char letter, int mask, int maxlength, params IParseable[] handlers)
			{
				fieldLengths[letter] = maxlength;
				conditions[letter] = mask;
				fields[letter] = handlers;
			}

			static TimeSpan()
			{
				fields = (IParseable[][]) DefaultTimeParser.fields.Clone();
				fieldLengths = (int[]) DefaultTimeParser.fieldLengths.Clone();
				conditions = (int[]) DefaultTimeParser.conditions.Clone();

				IParseable f;

				// Days count
				RegisterField('d', 0x3C, 6,
					f = new DaysCountField(), f, new DaysCountField2w2(),
					new DaysCountFieldN(3), new DaysCountFieldN(4),
					new DaysCountFieldN(5), new DaysCountFieldN(6));
			}

			#region Formatter: FormatString.ITarget delegate

			public override int FieldLength(char fieldChar) => fieldLengths[fieldChar & 0x7F];

			public override void AddField(char fieldChar, int length)
			{
				if (null == AddField(fields[fieldChar], conditions[fieldChar], length))
					throw new FormatError("Duplicate format field: " + fieldChar);
			}

			#endregion Formatter: FormatString.ITarget delegate


			protected override void ParseFormat(String fmt)
			{
				base.ParseFormat(fmt);

				// "Smart" sign insertion
				List<IParseable> items = builder.Items;
				int n = items.Count;
				for (int i = 0; i < n; i++)
				{

					if (items[i] is StaticField)
						continue;

					IParseable f;
					String s;
					// More magic
					if (i > 0 && (f = items[i - 1]) is StringField && (s = f.ToString()).EndsWith("0"))
					{
						int j, m = s.Length;
						for (j = m - 1; j >= 0 && s[j] == '0'; --j)
						{
						}

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

			internal static long Parse(String src, String fmt)
			{
				Context ctx = tls;
				if (null == ctx)
					ctx = tls = new Context();

				Debug.Assert(fmt != null);
				ParsedValue value = new ParsedValue();
				value.ResetTs(); // TODO: Remove
				int end = Parse(src, ref value,
					fmt == ctx.LastTsFmtStr
						? ctx.LastTsTemplate
						: ctx.TimeSpanParser.GetTemplate(fmt, ctx.TsTemplateCache));

				return value.GetTs();
			}
		}
	}
}