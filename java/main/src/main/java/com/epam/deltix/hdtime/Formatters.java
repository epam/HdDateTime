package com.epam.deltix.hdtime;

import java.io.IOException;
import java.util.ArrayList;
import java.util.HashMap;

class Formatters {
    int MAX_SERIALIZERS = 0x100;
    private static final int BUFFER_LENGTH = 0x100 - 16;
    private final static ThreadLocal<Context> tls = new ThreadLocal<Context>() {
        @Override
        protected Context initialValue() {
            return new Context();
        }
    };

//    private static void add(Field f) {
//        int hash = f.hashCode();
//        if (!fields.contains(f)) {
//            synchronized(fields) {
//                fields.add(f);
//            }
//        }
//    }

    static {
        //fields = new HashSet<>();
        // TODO: Fields cache is not used yet
//        synchronized(fields) {
//            add(new SecondsField2());
//            add(new SecondsField2w02());
//        }
        // This is for test, remove soon
//        synchronized(formatters) {
//            formatters.put("s", new FormatTemplate.Builder().add(new SecondsField2()).get());
//            formatters.put("ss", new FormatTemplate.Builder().add(new SecondsField2w02()).get());
//            formatters.put("s.S", new FormatTemplate.Builder().add(new SecondsField2w02()).get());
//        }
    }


    static class Context {
        static final HashMap<String, FormatTemplate> globalTsTemplateCache = new HashMap<>();
        static final HashMap<String, FormatTemplate> globalDtTemplateCache = new HashMap<>();

        public final Components components = new Components();
        public final char[] buffer = new char[BUFFER_LENGTH];
        public CharSequence bufferWrapper = java.nio.CharBuffer.wrap(buffer); // Also try https://gist.github.com/ncruces/ca9f91d89630d27ff05e35410a89022b

        public Formatter dateTimeFormatter = new DateTime(globalDtTemplateCache);
        final HashMap<String, FormatTemplate> dtTemplateCache = new HashMap<>();
        public String lastDtFmtStr;
        public FormatTemplate lastDtTemplate;

        public Formatter timeSpanFormatter = new TimeSpan(globalTsTemplateCache);
        final HashMap<String, FormatTemplate> tsTemplateCache = new HashMap<>();
        public String lastTsFmtStr;
        public FormatTemplate lastTsTemplate;
    }

    static class Print {
        // For all decimal printing methods it is guaranteed that the value is not bigger than the specified number of total digits

        static public int dec1(char[] to, int ofs, int x) {
            to[ofs] = (char)('0' + x);
            return ofs - 1;
        }

        static public int dec2(char[] to, int ofs, int x) {
            int x10 = x / 10;
            int newOfs = x >= 10 ? ofs - 2 : ofs - 1;
            to[ofs - 1] = (char)('0' + x10);
            to[ofs] = (char)(x + '0' - x10 * 10);
            return newOfs;
        }

        static public int dec3(char[] to, int ofs, int x) {
            int x10 = x / 10;
            to[ofs--] = (char)(x + '0' - x10 * 10);
            int x100 = x10 / 10;
            to[ofs - 1] = (char)('0' + x100);
            int newOfs = x >= 100 ? ofs - 2 : x >= 10 ?  ofs - 1 : ofs;
            to[ofs] = (char)(x10 + '0' - x100 * 10);
            return newOfs;
        }

        static public int dec2w2(char[] to, int ofs, int c) {
            int hi = c / 10;
            to[ofs - 1] = (char)('0' + hi);
            to[ofs] = (char)('0' + c - hi * 10);
            return ofs - 2;
        }

        static public int dec3w2(char[] to, int ofs, int c) {
            int hi = c / 100;
            to[ofs - 2] = (char)('0' + hi);
            dec2w2(to, ofs, c - hi * 100);
            return c >= 100 ? ofs - 3 : ofs - 2;
        }

        static public int  dec3w3(char[] to, int ofs, int c) {
            int hi = c / 100;
            to[ofs - 2] = (char)('0' + hi);
            dec2w2(to, ofs, c - hi * 100);
            return ofs - 3;
        }

        public static int dec4w4(char[] to, int ofs, int value) {
            int hi = value / 100;
            dec2w2(to, ofs - 2 , hi);
            dec2w2(to, ofs, value - hi * 100);
            return ofs - 4;
        }

        public static int dec(char[] to, int ofs, int x) {

            do {
                int y = x / 10;
                to[ofs--] = (char)(x + '0' - y * 10);
                x = y;
            } while (x != 0);

            return ofs;
        }

        public static int dec(char[] to, int ofs, int x, int n) {

            ofs -= n;
            do {
                int y = x / 10;
                to[ofs + n] = (char)(x + '0' - y * 10);
                x = y;
            } while (--n != 0);

            return ofs;
        }

        public static int sign(char[] to, int ofs, int sign) {
            assert sign == 0 || sign == -1;
            to[ofs] = '-';
            return ofs + sign;
        }

        public static int str(char[] to, int ofs, String str) {
            return str(to, ofs, str, str.length());
        }

        public static int str(char[] to, int ofs, String str, int n) {

            ofs -= n;
            for (int i = n - 1; i >= 0; --i)
                to[ofs + 1 + i] = str.charAt(i);

            return ofs;
        }

        public static int str(char[] to, int ofs, char[] str) {
            return str(to, ofs, str, str.length);
        }

        public static int str(char[] to, int ofs, char[] str, int n) {
            ofs -= n;
            for (int i = n - 1; i >= 0; --i)
                to[ofs + 1 + i] = str[i];

            return ofs;
        }
    }

    abstract static class Field extends FormatField implements Formattable {
    }

    static abstract class StaticField extends StaticFormatField implements Formattable {
    }

    static class Fail extends StaticField {
        final String str;

        public Fail(String str) {
            this.str = str;
        }

        @Override
        public int format(char[] to, int ofs, Components components) {
            throw new UnsupportedOperationException(str);
        }
    }

    static class StringField extends StaticField {
        final char[] chars;
        final int hashCode;

        StringField(String str) {
            this.chars = str.toCharArray();
            hashCode = super.hashCode() ^ str.hashCode();
        }

        @Override
        public int hashCode() {
            return hashCode;
        }

        @Override
        public boolean equals(Object other) {
            return this == other || other instanceof StringField && hashCode == other.hashCode();
        }

        @Override
        public int format(char[] to, int ofs, Components components) {
            return Print.str(to, ofs, chars);
        }

        @Override
        public String toString() { return new String(chars); }
//
//        @Override
//        public int length() {
//            return chars.length;
//        }
//
//        @Override
//        public char charAt(int index) {
//            return chars[index];
//        }
//
//        @Override
//        public CharSequence subSequence(int start, int end) {
//            return this(String(Arrays.copyOfRange(chars, start, end)));
//        }
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
        public boolean equals(Object other) {
            return this == other || other instanceof CharField && ch == ((CharField)other).ch;
        }

        @Override
        public int format(char[] to, int ofs, Components components) {

            to[ofs] = ch;
            return ofs - 1;
        }
    }


    static class SignField extends Field {
        @Override
        public int format(char[] to, int ofs, Components components) {
            return Print.sign(to, ofs, components.sign);
        }
    }

    static class YearsField2w02 extends Field {
        @Override
        public int format(char[] to, int ofs, Components components) {
            return Print.dec2w2(to, ofs, components.year % 100);
        }
    }

    static class YearsField4w04 extends Field {
        @Override
        public int format(char[] to, int ofs, Components components) {
            return Print.dec4w4(to, ofs, components.year);
        }
    }

    static class MonthNumField2 extends Field {
        @Override
        public int format(char[] to, int ofs, Components components) {
            return Print.dec2(to, ofs, components.month);
        }
    }

    static class MonthNumField2w02 extends Field {
        @Override
        public int format(char[] to, int ofs, Components components) {
            return Print.dec2w2(to, ofs, components.month);
        }
    }

    static class MonthTextField3w3 extends Field {
        @Override
        public int format(char[] to, int ofs, Components components) {
            return Print.str(to, ofs, Months.MONTHS3[components.month], 3);
        }
    }

    static class MonthTextField extends Field {
        private final int n;

        MonthTextField(int n) {
            this.n = n;
        }

        @Override
        public int format(char[] to, int ofs, Components components) {
            String s = Months.MONTHS[components.month];
            int len = s.length();
            Print.str(to, ofs, Months.MONTHS[components.month], len);
            ofs -= len;
            if (len < n) {
                for (int i = n - len; i != 0; --i) {
                    to[ofs--] = ' ';
                }
            }

            return ofs;
        }
    }

    static class DaysCountField extends Field {
        @Override
        public int format(char[] to, int ofs, Components components) {
            return Print.dec(to, ofs, components.day);
        }
    }

    static class DayInMonthField2 extends Field {
        @Override
        public int format(char[] to, int ofs, Components components) {
            return Print.dec2(to, ofs, components.day);
        }
    }

    static class DayInMonthField2w02 extends Field {
        @Override
        public int format(char[] to, int ofs, Components components) {
            return Print.dec2w2(to, ofs, components.day);
        }
    }

    static class Hours24Field2 extends Field {
        @Override
        public int format(char[] to, int ofs, Components components) {
            return Print.dec2(to, ofs, components.hour);
        }
    }

    static class Hours24Field2w02 extends Field {
        @Override
        public int format(char[] to, int ofs, Components components) {
            return Print.dec2w2(to, ofs, components.hour);
        }
    }

    static class MinutesField2 extends Field {
        @Override
        public int format(char[] to, int ofs, Components components) {
            return Print.dec2(to, ofs, components.minute);
        }
    }

    static class MinutesField2w02 extends Field {
        @Override
        public int format(char[] to, int ofs, Components components) {
            return Print.dec2w2(to, ofs, components.minute);
        }
    }

    static class SecondsField2 extends Field {
//        @Override
//        public int hash() {
//            return hash("SecondsField2");
//        }

        @Override
        public int format(char[] to, int ofs, Components components) {
            return Print.dec2(to, ofs, components.second);
        }
    }

    static class SecondsField2w02 extends Field {
        @Override
        public int format(char[] to, int ofs, Components components) {
            return Print.dec2w2(to, ofs, components.second);
        }
    }

    static class MillisecondsField3 extends Field {
        @Override
        public int format(char[] to, int ofs, Components components) {
            return Print.dec3(to, ofs, components.nanosecond / 1000000);
        }
    }

    static class MillisecondsField3w02 extends Field {
        @Override
        public int format(char[] to, int ofs, Components components) {
            return Print.dec3w2(to, ofs, components.nanosecond / 1000000);
        }
    }

    static class FractionsField9 extends Field {
        @Override
        public int format(char[] to, int ofs, Components components) {
            return Print.dec(to, ofs, components.nanosecond, 9);
        }
    }

    static class FractionsField8 extends Field {
        @Override
        public int format(char[] to, int ofs, Components components) {
            return Print.dec(to, ofs, components.nanosecond / 10, 8);
        }
    }

    static class FractionsField7 extends Field {
        @Override
        public int format(char[] to, int ofs, Components components) {
            return Print.dec(to, ofs, components.nanosecond / 100, 7);
        }
    }

    static class FractionsField6 extends Field {
        @Override
        public int format(char[] to, int ofs, Components components) {
            return Print.dec(to, ofs, components.nanosecond / 1000, 6);
        }
    }

    static class FractionsField5 extends Field {
        @Override
        public int format(char[] to, int ofs, Components components) {
            return Print.dec(to, ofs, components.nanosecond / 10000, 5);
        }
    }

    static class FractionsField4 extends Field {
        @Override
        public int format(char[] to, int ofs, Components components) {
            return Print.dec4w4(to, ofs, components.nanosecond / 100000);
        }
    }

    static class FractionsField3 extends Field {
        // Also used for Millis field
        @Override
        public int format(char[] to, int ofs, Components components) {
            return Print.dec3w3(to, ofs, components.nanosecond / 1000000);
        }
    }

    static class FractionsField2 extends Field {
        @Override
        public int format(char[] to, int ofs, Components components) {
            return Print.dec2w2(to, ofs, components.nanosecond / 10000000);
        }
    }

    static class FractionsField1 extends Field {
        @Override
        public int format(char[] to, int ofs, Components components) {
            return Print.dec1(to, ofs, components.nanosecond / 100000000);
        }
    }

    static class FormatTemplate implements Formattable {
        final Formattable[] fields;

        FormatTemplate(final Formattable[] fields) {
            this.fields = fields;
        }

        @Override
        public int format(char[] to, int ofs, Components components) {
            for (int i = fields.length - 1; i >= 0 ; --i)
                ofs = fields[i].format(to, ofs, components);

            return ofs;
        }

        String format(Components components) {
            // TODO: Temporary implementation, remove soon
            char[] tmp = new char[0x100];
            int length = format(tmp, 0, components);
            return new String(tmp, 0, length);
        }

        static class Builder extends ArrayBuilder<Formattable> {
            FormatTemplate get() {
                return new FormatTemplate(getFields(Formattable.class));
            }
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
     * specializations of the
     */
    protected static abstract class Formatter implements FormatString.Target {
        // Static Fields
        protected final HashMap<String,FormatTemplate> globalTemplateCache; // Set by a child
        // Instance fields
        protected final FormatTemplate.Builder fb = new FormatTemplate.Builder();

        Formatter(HashMap<String,FormatTemplate> globalTemplateCache) {
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
                fb.add(len == 1 ? new CharField(str.charAt(0)) : new StringField(str.toString()));
        }

        // endregion Formatter: FormatString delegate

        protected void parseFormat(String fmt) {
            FormatString.parse(fmt, this);
        }

        protected FormatTemplate getCachedOrNewTemplate(final String fmt, final HashMap<String, FormatTemplate> local) {
            FormatTemplate f;
            final HashMap<String, FormatTemplate> global = globalTemplateCache;
            // Try to find in the global formatter cache
            synchronized (global) {
                f = global.get(fmt);
            }

            if (null == f) {
                fb.clear();
                parseFormat(fmt);
                f = fb.get();
                synchronized (global) {
                    FormatTemplate tmp = global.get(fmt);
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

        protected FormatTemplate getTemplate(final String fmt, final HashMap<String, FormatTemplate> local) {
            FormatTemplate f;
            return null != (f = local.get(fmt)) ? f : getCachedOrNewTemplate(fmt, local);
        }

        protected static int format(char[] buffer, Components components, FormatTemplate f) {
            return f.format(buffer, BUFFER_LENGTH - 1, components);
        }
    }

    /**
     * DefaultTimeFormatter specializes Formatter with some default date/time fields
     */
    protected static class DefaultTimeFormatter extends Formatter {
        // Static fields
        protected static final Formattable[][] fields;
        protected static final int[] fieldLengths;

        DefaultTimeFormatter(HashMap<String, FormatTemplate> globalTemplateCache) {
            super(globalTemplateCache);
        }

        // region Formatter: Static methods
        static protected void registerField(char letter, int maxlength, Formattable... handlers) {
            fieldLengths[letter] = maxlength;
            fields[letter] = handlers;
        }

        static protected void registerSynonymousField(char letterSrc, char letterDst) {
            fieldLengths[letterDst] = fieldLengths[letterSrc];
            fields[letterDst] = fields[letterSrc];
        }

        static {
            fieldLengths = new int[0x80];
            fields = new Formattable[0x80][];
            Formattable f;
            // Hour in day [0..23] (Java & .NET)
            registerField('H', 2, f = new Hours24Field2(), f, new Hours24Field2w02());
            // Minute in hour (Java & .NET)
            registerField('m', 2, f = new MinutesField2(), f, new MinutesField2w02());
            // Second in minute (Java & .NET)
            registerField('s', 2, f = new SecondsField2(), f, new SecondsField2w02());
            // Millisecond (Java)
            //registerField('S', 3, f = new MillisecondsField3(), f, new MillisecondsField3w02(), new FractionsField3());
            // Fractions (.NET, also replaces Java8Time 'S' char)
            registerField('f', Integer.MAX_VALUE,
                    new Fail("Fractions field can't be longer than 9 digits"),
                    new FractionsField1(), new FractionsField2(), new FractionsField3(),
                    new FractionsField4(), new FractionsField5(), new FractionsField6(),
                    new FractionsField7(), new FractionsField8(), new FractionsField9());

            registerSynonymousField('f', 'S');
        }

//        static abstract class FormatStringTarget implements FormatString.Target {
//            protected final FormatTemplate.Builder builder = new FormatTemplate.Builder();
//            @Override
//            public int fieldLength(char fieldChar) {
//                return fieldLengths[fieldChar & 0x7F];
//            }
//
//            @Override
//            public void addString(CharSequence str, int ofs, int n) {
//                addString(str.subSequence(ofs, n));
//            }
//
//            @Override
//            public void addString(CharSequence str) {
//                builder.add(new StringField(str.toString()));
//            }

//            protected void addCommonField(char fieldChar, int length) {
//                Field f;
//                switch (fieldChar) {
//                    // Hour in day [0..23] (Java & .NET)
//                    case 'H': f = length == 2 ? new Hours24Field2w02() : new Hours24Field2(); break;
//                    // Minute in hour (Java & .NET)
//                    case 'm': f = length == 2 ? new MinutesField2w02() : new MinutesField2(); break;
//                    // Second in minute (Java & .NET)
//                    case 's': f = length == 2 ? new SecondsField2w02() : new SecondsField2(); break;
//                    // Millisecond (Java)
//                    case 'S': f = new FractionsField3(); break;
//                    default:
//                        throw new IllegalArgumentException();
//                }
//
//                builder.add(f);
//            }
//        }
        // endregion Formatter: Static methods
        // region Formatter: FormatString delegate

        @Override
        public char paddingForField(char fieldChar) {
            // All fields that need padding are 0-padded
            return '0';
        }

        @Override
        public int fieldLength(char fieldChar) {
            return fieldLengths[fieldChar & 0x7F];
        }

        protected void addField(Formattable handler) {
            fb.add(handler);
        }

        protected void addField(Formattable[] handlers, int length) {
            int handlersCount = handlers.length;
            assert (handlersCount > 0); // Should never be called with invalid fieldChar
            addField(handlers[length < handlersCount ? length : 0]);
        }

        @Override
        public void addField(char fieldChar, int length) {
            addField(fields[fieldChar], length);
        }

        // endregion Formatter: FormatString delegate
    }

    static class DateTime extends DefaultTimeFormatter {
        protected static final Formattable[][] fields;
        protected static final int[] fieldLengths;

        DateTime(HashMap<String, FormatTemplate> globalTemplateCache) {
            super(globalTemplateCache);
        }

        static protected void registerField(char letter, int maxlength, Formattable... handlers) {
            fieldLengths[letter] = maxlength;
            fields[letter] = handlers;
        }

        static protected void registerSynonymousField(char letterSrc, char letterDst) {
            fieldLengths[letterDst] = fieldLengths[letterSrc];
            fields[letterDst] = fields[letterSrc];
        }

        static {
            fields = DefaultTimeFormatter.fields.clone();
            fieldLengths = DefaultTimeFormatter.fieldLengths.clone();
            Field f;
            // Epoch name (Java, no support for locale)
            registerField('G', Integer.MAX_VALUE, new StringField("AD"));
            // Epoch name (.NET, no support for locale)
            registerSynonymousField('G', 'g');

            // Year number (Java & .NET) Java substitution logic is used (y/yyy->yyyy)
            registerField('y', 4, f = new YearsField4w04(), f, new YearsField2w02(), f, f);

            // Year number (Java)
            registerSynonymousField('y', 'u');

            // Month number or name (Java & .NET)
            registerField('M', Integer.MAX_VALUE, (Formattable)null,
                    new MonthNumField2(), new MonthNumField2w02(),
                    new MonthTextField3w3(), new MonthTextField(0));

            // Day of month [0..31] (Java & .NET)
            registerField('d', 2, f = new DayInMonthField2(), f, new DayInMonthField2w02());
        }

        // region Formatter: FormatString delegate
        @Override
        public int fieldLength(char fieldChar) {
            return fieldLengths[fieldChar & 0x7F];
        }

        @Override
        public void addField(char fieldChar, int length) {
            if ('M' == fieldChar && length > 4)
                addField(new MonthTextField(length));
            else
                addField(fields[fieldChar], length);
        }

        // endregion Formatter: FormatString delegate

        static int formatChars(long ts, final String fmt, Context ctx, char[] buffer) {
            Components components = ctx.components;
            Convert.DateTime.toComponents(ts, components);
            // TODO: This code can now be simplified by moving methods/fields to Formatter instance
            return 1 + format(
                    buffer,
                    components,
                    fmt == ctx.lastDtFmtStr ? ctx.lastDtTemplate : ctx.dateTimeFormatter.getTemplate(fmt, ctx.dtTemplateCache));
        }

        static String format(long ts, final String fmt) {
            Context ctx = tls.get();
            char[] buffer = ctx.buffer;
            int start = formatChars(ts, fmt, ctx, buffer);
            return new String(ctx.buffer, start, BUFFER_LENGTH - start);
        }

        public static Appendable format(long ts, String fmt, Appendable appendable) throws IOException {
            Context ctx = tls.get();
            char[] buffer = ctx.buffer;
            int start = formatChars(ts, fmt, ctx, buffer);
            return appendable.append(ctx.bufferWrapper, start, BUFFER_LENGTH);
        }
    }

    static class TimeSpan extends DefaultTimeFormatter {
        protected static final Formattable[][] fields;
        protected static final int[] fieldLengths;

        TimeSpan(HashMap<String, FormatTemplate> globalTemplateCache) {
            super(globalTemplateCache);
        }

        // Easier to just redefine
        static protected void registerField(char letter, int maxlength, Formattable... handlers) {
            fieldLengths[letter] = maxlength;
            fields[letter] = handlers;
        }

        static {
            fields = DefaultTimeFormatter.fields.clone();
            fieldLengths = DefaultTimeFormatter.fieldLengths.clone();

            Formattable f;
            // Days count
            registerField('d', Integer.MAX_VALUE, f = new DaysCountField(), f, f = new DaysCountField());
        }

        // region Formatter: FormatString delegate
        @Override
        public int fieldLength(char fieldChar) {
            return fieldLengths[fieldChar & 0x7F];
        }

        @Override
        public void addField(char fieldChar, int length) {
            addField(fields[fieldChar], length);
        }

        // endregion Formatter: FormatString delegate

        @Override
        protected void parseFormat(String fmt) {
            super.parseFormat(fmt);

            // "Smart" sign insertion
            ArrayList<Formattable> items = fb.items;
            int n = items.size();
            for (int i = 0; i < n; i++) {

                if (items.get(i) instanceof StaticField)
                    continue;

                Formattable f;
                String s;
                // More magic
                if (i > 0 && (f = items.get(i - 1)) instanceof StringField && (s = f.toString()).endsWith("0")) {
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

        static int formatChars(long ts, final String fmt, Context ctx, char[] buffer) {
            Components components = ctx.components;
            Convert.TimeSpan.toComponents(ts, components);
            // TODO: This code can now be simplified by moving methods/fields to Formatter instance
            return 1 + format(
                    buffer,
                    components,
                    fmt == ctx.lastTsFmtStr ? ctx.lastTsTemplate : ctx.timeSpanFormatter.getTemplate(fmt, ctx.tsTemplateCache));
        }

        static String format(long ts, final String fmt) {
            Context ctx = tls.get();
            char[] buffer = ctx.buffer;
            int start = formatChars(ts, fmt, ctx, buffer);
            return new String(ctx.buffer, start, BUFFER_LENGTH - start);
        }

        public static Appendable format(long ts, String fmt, Appendable appendable) throws IOException {
            Context ctx = tls.get();
            char[] buffer = ctx.buffer;
            int start = formatChars(ts, fmt, ctx, buffer);
            // TODO: Later: compare performance, also vs different charbuffer impl
            //return appendable.append(new String(ctx.buffer, start, BUFFER_LENGTH));
            return appendable.append(ctx.bufferWrapper,   start, BUFFER_LENGTH);
        }
    }
}
