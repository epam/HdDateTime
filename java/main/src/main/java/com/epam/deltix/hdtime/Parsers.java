/*
 * Copyright 2021 EPAM Systems, Inc
 *
 * See the NOTICE file distributed with this work for additional information
 * regarding copyright ownership. Licensed under the Apache License,
 * Version 2.0 (the "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the
 * License for the specific language governing permissions and limitations under
 * the License.
 */
package com.epam.deltix.hdtime;

import java.util.ArrayList;
import java.util.HashMap;

public class Parsers {
    private final static ThreadLocal<Context> tls = new ThreadLocal<Context>() {
        @Override
        protected Context initialValue() {
            return new Context();
        }
    };

    static class Context {
        static final HashMap<String, ParseTemplate> globalTsTemplateCache = new HashMap<>();
        static final HashMap<String, ParseTemplate> globalDtTemplateCache = new HashMap<>();

        public Parser dateTimeParser = new DateTime(globalDtTemplateCache);
        final HashMap<String, ParseTemplate> dtTemplateCache = new HashMap<>();
        public String lastDtFmtStr;
        public ParseTemplate lastDtTemplate;
        public ParsedDateTimeValue dtValue = new ParsedDateTimeValue();

        public Parser timeSpanParser = new TimeSpan(globalTsTemplateCache);
        final HashMap<String, ParseTemplate> tsTemplateCache = new HashMap<>();
        public String lastTsFmtStr;
        public ParseTemplate lastTsTemplate;
        public ParsedValue tsValue = new ParsedValue();
    }

    static class Parse {
        private static int digit(CharSequence from, int i) throws ParseException {
            int x = from.charAt(i) - '0';
            if (x < 0 | x > 9)
                throw new ParseException(from.toString(), i);

            return x;
        }

        static int component(final CharSequence from, int ofs, final ParsedValue dst, long scale) throws ParseException {
            int n = from.length();
            if (ofs >= n)
                throw new ParseException(from.toString(), ofs);

            int x = digit(from, ofs++);
            for (; ofs < n; ++ofs) {
                int c = from.charAt(ofs) - '0';
                if (c < 0 | c > 9)
                    break;

                x = x * 10 + c;
            }

            dst.x += scale * x;
            return ofs;
        }

        static long decimal(final CharSequence from, int ofs) throws ParseException {
            int end = from.length();
            int x = digit(from, ofs++);
            for (; ofs < end; ++ofs) {
                int c = from.charAt(ofs) - '0';
                if (c < 0 | c > 9)
                    break;

                x = x * 10 + c;
            }

            return ((long)ofs << 32) | x;
        }

        static int decimalFixed(final CharSequence from, int ofs, int end) throws ParseException {
            if (end > from.length())
                throw new ParseException(from.toString(), ofs);

            int x = digit(from, ofs++);
            for (; ofs < end; ++ofs) {
                x = x * 10 + digit(from, ofs);
            }

            return x;
        }

        static int component(final CharSequence from,
                             int ofs, int length,
                             final ParsedValue dst, long scale) throws ParseException {
            dst.x += scale * decimalFixed(from, ofs, ofs + length);
            return ofs + length;
        }

        public static int sign(CharSequence from, int ofs, ParsedValue dst) throws ParseException {
            if (ofs >= from.length())
                throw new ParseException(from.toString(), ofs);

            int sign = from.charAt(ofs) == '-' ? -1 : 0;
            dst.sign = (byte) sign;
            return ofs - sign;
        }
    }

    abstract static class Field extends FormatField implements Parseable {
    }

    abstract static class FixedLengthField extends FormatField implements Parseable {
        final int length;
        FixedLengthField(int length) {
            this.length = length;
        }
    }

    static abstract class StaticField extends StaticFormatField implements Parseable {
    }

    static class Fail extends StaticField {
        final String str;

        public Fail(String str) {
            this.str = str;
        }

        @Override
        public int parse(CharSequence from, int ofs, ParsedValue dst) {
            throw new UnsupportedOperationException(str);
        }
    }

    static class StringField extends StaticField {
        final String str;

        StringField(String str) {
            this.str = str;
        }

        @Override
        public int hashCode() {
            return super.hashCode() ^ str.hashCode();
        }

        @Override
        public int parse(CharSequence from, int ofs, ParsedValue dst) throws ParseException {
            int n = str.length();
            for (int i = 0; i < n; ++i)
                if (from.charAt(ofs + i) != str.charAt(i))
                    throw new ParseException(from.toString(), ofs + i);

            return n + ofs;
        }

        @Override
        public boolean equals(Object other) {
            return this == other || other instanceof StringField && str.equals(((StringField)other).str);
        }
    }

    static class CharField extends StaticField {
        final char ch;

        CharField(char ch) {
            this.ch = ch;
        }

        @Override
        public int hashCode() {
            return super.hashCode() * 31 ^ ch;
        }

        @Override
        public int parse(CharSequence from, int ofs, ParsedValue dst) throws ParseException {
            if (ch != from.charAt(ofs))
                throw new ParseException(from.toString(), ofs);

            return ofs + 1;
        }

        @Override
        public boolean equals(Object other) {
            return this == other || other instanceof CharField && ch == ((CharField)other).ch;
        }
    }

    static class SignField extends Field {
        @Override
        public int parse(CharSequence from, int ofs, ParsedValue dst) throws ParseException {
            return Parse.sign(from, ofs, dst);
        }
    }

    static class YearsField4w4 extends Field {
        @Override
        public int parse(CharSequence from, int ofs, ParsedValue dst) throws ParseException {
            ((ParsedDateTimeValue)dst).year = Parse.decimalFixed(from, ofs, ofs + 4);
            return ofs + 4;
        }
    }

    static class YearsField4 extends Field {
        @Override
        public int parse(CharSequence from, int ofs, ParsedValue dst) throws ParseException {
            long t = Parse.decimal(from, ofs);
            ((ParsedDateTimeValue)dst).year = (int)t;
            return (int)(t >> 32);
        }
    }

    static class MonthNumField2 extends Field {
        @Override
        public int parse(CharSequence from, int ofs, ParsedValue dst) throws ParseException {
            long t = Parse.decimal(from, ofs);
            ((ParsedDateTimeValue)dst).month = (int)t;
            return (int)(t >> 32);
        }
    }

    static class MonthNumField2w2 extends Field {
        @Override
        public int parse(CharSequence from, int ofs, ParsedValue dst) throws ParseException {
            ((ParsedDateTimeValue)dst).month = Parse.decimalFixed(from, ofs, ofs + 2);
            return ofs + 2;
        }
    }

    static class DaysCountField extends Field {
        @Override
        public int parse(CharSequence from, int ofs, ParsedValue dst) throws ParseException {
            // TODO: Range check
            return Parse.component(from, ofs, dst, Convert.NS_IN_DAY);
        }
    }

    static class DaysCountField1w1 extends Field {
        @Override
        public int parse(CharSequence from, int ofs, ParsedValue dst) throws ParseException {
            dst.x += Convert.NS_IN_DAY * Parse.decimalFixed(from, ofs, ofs + 1);
            return ofs + 1;
        }
    }

    static class DaysCountField2w2 extends Field {
        @Override
        public int parse(CharSequence from, int ofs, ParsedValue dst) throws ParseException {
            dst.x += Convert.NS_IN_DAY * Parse.decimalFixed(from, ofs, ofs + 2);
            return ofs + 2;
        }
    }

    static class DayOfMonthField2w2 extends Field {
        @Override
        public int parse(CharSequence from, int ofs, ParsedValue dst) throws ParseException {
            ((ParsedDateTimeValue)dst).day = Parse.decimalFixed(from, ofs, ofs + 2);
            return ofs + 2;
        }
    }

    static class DayOfMonthField2 extends Field {
        @Override
        public int parse(CharSequence from, int ofs, ParsedValue dst) throws ParseException {
            long t = Parse.decimal(from, ofs);
            ((ParsedDateTimeValue)dst).day = (int)t;
            return (int)(t >> 32);
        }
    }

    static class DaysCountFieldN extends FixedLengthField {
        DaysCountFieldN(int length) {
            super(length);
        }

        @Override
        public int parse(CharSequence from, int ofs, ParsedValue dst) throws ParseException {
            // TODO: Rangecheck for length > 5
            return Parse.component(from, ofs, length, dst, Convert.NS_IN_DAY);
        }
    }

    static class Hours24Field2 extends Field {
        @Override
        public int parse(CharSequence from, int ofs, ParsedValue dst) throws ParseException {
            // TODO: Range check
            return Parse.component(from, ofs, dst, Convert.NS_IN_HOUR);
        }
    }

    static class Hours24Field2w2 extends Field {
        @Override
        public int parse(CharSequence from, int ofs, ParsedValue dst) throws ParseException {
            // TODO: Range check
            return Parse.component(from, ofs, 2, dst, Convert.NS_IN_HOUR);
        }
    }

    static class MinutesField2 extends Field {
        @Override
        public int parse(CharSequence from, int ofs, ParsedValue dst) throws ParseException {
            // TODO: Range check
            return Parse.component(from, ofs, dst, Convert.NS_IN_MINUTE);
        }
    }

    static class MinutesField2w2 extends Field {
        @Override
        public int parse(CharSequence from, int ofs, ParsedValue dst) throws ParseException {
            // TODO: Range check
            return Parse.component(from, ofs, 2, dst, Convert.NS_IN_MINUTE);
        }
    }

    static class SecondsField2 extends Field {
        @Override
        public int parse(CharSequence from, int ofs, ParsedValue dst) throws ParseException {
            // TODO: Range check
            return Parse.component(from, ofs, dst, Convert.NS_IN_SECOND);
        }
    }

    static class SecondsField2w2 extends Field {
        @Override
        public int parse(CharSequence from, int ofs, ParsedValue dst) throws ParseException {
            // TODO: Range check
            return Parse.component(from, ofs, 2, dst, Convert.NS_IN_SECOND);
        }
    }

    static class FractionsField extends FixedLengthField {
        final int scale;
        FractionsField(int length) {
            super(length);
            int scale = 1000000000;
            while (0 != length--)
                scale /= 10;

            this.scale = scale;
        }
        @Override
        public int parse(CharSequence from, int ofs, ParsedValue dst) throws ParseException {
            return Parse.component(from, ofs, length, dst, scale);
        }
    }

    static class ParseTemplate implements Parseable {
        final Parseable[] fields;

        ParseTemplate(final Parseable[] fields) {
            this.fields = fields;
        }

        @Override
        public int parse(CharSequence from, int ofs, ParsedValue dst) throws ParseException {
            for (int i = 0, n = fields.length; i < n ; ++i)
                ofs = fields[i].parse(from, ofs, dst);

            return ofs;
        }

        static class Builder extends ArrayBuilder<Parseable> {
            int mask;
            ParseTemplate get() {
                return new ParseTemplate(getFields(Parseable.class));
            }

            public Builder add(Parseable handler, int mask) {
                add(handler);
                int m = this.mask;
                this.mask = m | mask;
                return 0 == (m & mask) ? this : null;
            }

            @Override
            void clear() {
                super.clear();
                mask = 0;
            }
        }
    }

    protected static abstract class Parser implements FormatString.Target {
        // Static Fields
        protected final HashMap<String, ParseTemplate> globalTemplateCache; // Set by a child
        // Instance fields
        protected final ParseTemplate.Builder builder = new ParseTemplate.Builder();

        Parser(HashMap<String, ParseTemplate> globalTemplateCache) {
            this.globalTemplateCache = globalTemplateCache;
        }

        // region Formatter: FormatString delegate

        @Override
        public void addString(CharSequence str, int ofs, int n) {
            addString(str.subSequence(ofs, n));
        }

        @Override
        public void addString(CharSequence str) {
            int len = str.length();
            if (len > 0)
                builder.add(len == 1 ? new CharField(str.charAt(0)) : new StringField(str.toString()));
        }

        // endregion Formatter: FormatString delegate

        protected void parseFormat(String fmt) {
            FormatString.parse(fmt, this);
        }

        protected ParseTemplate getCachedOrNewTemplate(final String fmt, final HashMap<String, ParseTemplate> local) {
            ParseTemplate f;
            final HashMap<String, ParseTemplate> global = globalTemplateCache;
            // Try to find in the global formatter cache
            synchronized (global) {
                f = global.get(fmt);
            }

            if (null == f) {
                builder.clear();
                parseFormat(fmt);
                f = builder.get();
                synchronized (global) {
                    ParseTemplate tmp = global.get(fmt);
                    if (null == tmp)
                        global.put(fmt, f);
                    else
                        f = tmp;
                }

                assert (null != f);
            }

            // Copy the reference to local cache
            local.put(fmt, f);
            return f;
        }

        protected ParseTemplate getTemplate(final String fmt, final HashMap<String, ParseTemplate> local) {
            ParseTemplate f;
            return null != (f = local.get(fmt)) ? f : getCachedOrNewTemplate(fmt, local);
        }

        protected static int parse(CharSequence from, ParsedValue value, ParseTemplate f) throws ParseException {
            return f.parse(from, 0, value);
        }
    }

    static abstract class DefaultTimeParser extends Parser {
        // Static fields
        protected static final Parseable[][] fields;
        protected static final int[] fieldLengths;
        protected static final int[] conditions;

        public DefaultTimeParser(HashMap<String, ParseTemplate> globalTemplateCache) {
            super(globalTemplateCache);
        }

        // region Parser: Static methods
        static private void registerField(char letter, int mask, int maxlength, Parseable... handlers) {
            fieldLengths[letter] = maxlength;
            conditions[letter] = mask;
            fields[letter] = handlers;
        }

        static private void registerSynonymousField(char letterSrc, char letterDst) {
            fieldLengths[letterDst] = fieldLengths[letterSrc];
            conditions[letterDst] = conditions[letterSrc];
            fields[letterDst] = fields[letterSrc];
        }

        static {
            fieldLengths = new int[0x80];
            conditions = new int[0x80];
            fields = new Parseable[0x80][];
            Parseable f;

            // Hour in day [0..23] (Java & .NET)
            registerField('H', 0xC0, 2, f = new Hours24Field2(), f, new Hours24Field2w2());

            // Minute in hour (Java & .NET)
            registerField('m', 0x100,2, f = new MinutesField2(), f, new MinutesField2w2());

            // Second in minute (Java & .NET)
            registerField('s', 0x200, 2, f = new SecondsField2(), f, new SecondsField2w2());

            // Fractions (.NET)
            registerField('f', 0x400, Integer.MAX_VALUE, new Fail("Fractions field can't be longer than 9 digits"),
                    new FractionsField(1), new FractionsField(2), new FractionsField(3),
                    new FractionsField(4), new FractionsField(5), new FractionsField(6),
                    new FractionsField(7), new FractionsField(8), new FractionsField(9));

            registerSynonymousField('f', 'S');
        }

        @Override
        public int fieldLength(char fieldChar) {
            return fieldLengths[fieldChar & 0x7F];
        }

        protected ParseTemplate.Builder addField(Parseable[] handlers, int mask, int length) {
            int handlersCount = handlers.length;
            assert (handlersCount > 0); // Should never be called with invalid fieldChar
            return builder.add(handlers[length < handlersCount ? length : 0], mask);
        }

        @Override
        public char paddingForField(char fieldChar) {
            // All fields that need padding are 0-padded
            return '0';
        }
    }

    static class DateTime extends DefaultTimeParser {
        protected static final Parseable[][] fields;
        protected static final int[] fieldLengths;
        protected static final int[] conditions;

        DateTime(HashMap<String, ParseTemplate> globalTemplateCache) {
            super(globalTemplateCache);
        }

        // region Parser: Static methods
        static private void registerField(char letter, int mask, int maxlength, Parseable... handlers) {
            fieldLengths[letter] = maxlength;
            conditions[letter] = mask;
            fields[letter] = handlers;
        }

        static private void registerSynonymousField(char letterSrc, char letterDst) {
            fieldLengths[letterDst] = fieldLengths[letterSrc];
            conditions[letterDst] = conditions[letterSrc];
            fields[letterDst] = fields[letterSrc];
        }

        static {
            fields = DefaultTimeParser.fields.clone();
            fieldLengths = DefaultTimeParser.fieldLengths.clone();
            conditions = DefaultTimeParser.conditions.clone();

            Parseable f;
            // Epoch name (Java, no support for locale)
            registerField('G', 0,  Integer.MAX_VALUE, new StringField("AD"));
            // Epoch name (.NET, no support for locale)
            registerSynonymousField('G', 'g');

            // Year number (Java & .NET) Java substitution logic is used (y/yyy->yyyy)
            registerField('y', 1, 4, new Fail("Not implemented/y"), f = new YearsField4(), new Fail("2-digit year not implemented"), f, new YearsField4w4());

            // Year number (Java)
            registerSynonymousField('y', 'u');

            // Month number or name (Java & .NET)
            // TODO: Month names are currently unsupported
            registerField('M', 2, 2,  new Fail("Not implemented/M"), new MonthNumField2(), new MonthNumField2w2());

            // Day of month [0..31] (Java & .NET)
            registerField('d', 0x3C, 2, new Fail("Not implemented/d"), new DayOfMonthField2(), new DayOfMonthField2w2());
        }

        // region Formatter: FormatString delegate
        @Override
        public int fieldLength(char fieldChar) {
            return fieldLengths[fieldChar & 0x7F];
        }

        @Override
        public void addField(char fieldChar, int length) throws FormatError {
            if (null == addField(fields[fieldChar], conditions[fieldChar], length)) {
                throw new FormatError("Duplicate format field: " + fieldChar);
            }
        }

        @Override
        protected void parseFormat(String fmt) {
            super.parseFormat(fmt);
            int mask = builder.mask;
            if (0 != (mask & mask + 1) || mask == 0)
                throw new FormatError("Incomplete DateTime format string: " + fmt);
        }

        // endregion Formatter: FormatString delegate

        static long parse(CharSequence src, String fmt) throws ParseException {
            Context ctx = tls.get();
            ParsedDateTimeValue value = ctx.dtValue;
            value.reset();
            int end = parse(src, value,
                    fmt == ctx.lastDtFmtStr ? ctx.lastDtTemplate : ctx.dateTimeParser.getTemplate(fmt, ctx.dtTemplateCache));

            return value.get();
        }
    }

    static class TimeSpan extends DefaultTimeParser {
        protected static final Parseable[][] fields;
        protected static final int[] fieldLengths;
        protected static final int[] conditions;

        TimeSpan(HashMap<String, ParseTemplate> globalTemplateCache) {
            super(globalTemplateCache);
        }

        // region Parser: Static methods
        static private void registerField(char letter, int mask, int maxlength, Parseable... handlers) {
            fieldLengths[letter] = maxlength;
            conditions[letter] = mask;
            fields[letter] = handlers;
        }

        static {
            fields = DefaultTimeParser.fields.clone();
            fieldLengths = DefaultTimeParser.fieldLengths.clone();
            conditions = DefaultTimeParser.conditions.clone();

            Parseable f;
            // Days count
            registerField('d', 0x3C,6, f = new DaysCountField(),
                    f, new DaysCountField2w2(),
                    new DaysCountFieldN(3), new DaysCountFieldN(4),
                    new DaysCountFieldN(5), new DaysCountFieldN(6));
        }

        // region Formatter: FormatString delegate
        @Override
        public int fieldLength(char fieldChar) {
            return fieldLengths[fieldChar & 0x7F];
        }

        @Override
        public void addField(char fieldChar, int length) throws FormatError {
            if (null == addField(fields[fieldChar], conditions[fieldChar], length)) {
                throw new FormatError("Duplicate format field: " + fieldChar);
            }
        }

        // endregion Formatter: FormatString delegate

        @Override
        protected void parseFormat(String fmt) {
            super.parseFormat(fmt);

            // "Smart" sign insertion
            ArrayList<Parseable> items = builder.items;
            int n = items.size();
            for (int i = 0; i < n; i++) {

                if (items.get(i) instanceof StaticField)
                    continue;

                Parseable f;
                String s;
                // More magic
                if (i > 0 && (f = items.get(i - 1)) instanceof StringField && (s = ((StringField)f).str).endsWith("0")) {
                    int j, m = s.length();
                    for (j = m - 1; j >= 0 && s.charAt(j) == '0'; --j) {}
                    if (0 == ++j) {
                        --i;
                    } else {
                        items.add(i, new StringField(s.substring(j)));
                        items.set(i - 1, new StringField(s.substring(0, j)));
                    }
                }

                items.add(i, new SignField());
                return;
            }
        }

        static long parse(CharSequence src, String fmt) throws ParseException {
            Context ctx = tls.get();
            ParsedValue value = ctx.tsValue;
            value.reset();
            int end = parse(src, value,
                    fmt == ctx.lastTsFmtStr ? ctx.lastTsTemplate : ctx.timeSpanParser.getTemplate(fmt, ctx.tsTemplateCache));

            return value.get();
        }
    }
}
