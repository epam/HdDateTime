package com.epam.deltix.hdtime;

import java.io.IOException;
import java.time.DayOfWeek;
import java.time.Month;
import java.util.Calendar;
import java.util.GregorianCalendar;

/**
 * Static methods to work with date and time in nanoseconds resolution.
 *
 * Value is supposed to be number of nanoseconds since 1970-01-01 00:00:00.000000000 UTC.
 */
public class HdDateTimeUtils {
    public static final String DEFAULT_FORMAT = "yyyy-MM-dd HH:mm:ss.fffffffff";

    static final long MIN = -9214560000_000_000_000L;       // 1678-01-01 00:00:00.000000000
    static final long MAX =  9214646400_000_000_000L - 1;   // 2261-12-31 23:59:59.999999999
    static final long NULL = Long.MIN_VALUE;

    /**
     * Null value should not be passed to HdDateTimeUtils methods by user's code
     * It is also never returned by HdDateTimeUtils methods
     */
    public static final long NULL_VALUE = NULL;

    public static final long MIN_VALUE = MIN;               // 1678-01-01 00:00:00.000000000
    public static final long MAX_VALUE = MAX;               // 2261-12-31 23:59:59.999999999

    /////////////////////////////////////////////////////////////////////////////////////
    // region Constructors
    /////////////////////////////////////////////////////////////////////////////////////

    public static long fromUnderlying(long value) {
        return value;
    }

    public static long toUnderlying(long value) {
        return value;
    }

    public static long newInstance(int year, Month month, int day) {
        return Convert.DateTime.from(year, month, day);
    }

    public static long newInstance(int year, Month month, int day, int hour, int minute, int second) {
        return Convert.DateTime.from(year, month, day, hour, minute, second);
    }

    public static long newInstance(int year, Month month, int day, int hour, int minute, int second, int nanosecond) {
        return Convert.DateTime.from(year, month, day, hour, minute, second, nanosecond);
    }

    public static long fromEpochMilliseconds(long value) {
        // TODO: Min/Max checks?
        return Convert.DateTime.fromMilliseconds(value);
    }

    public static long fromEpochNanoseconds(long value) {
        return Convert.DateTime.fromNanoseconds(value);
    }

    public static long now() {
        return fromEpochMilliseconds(System.currentTimeMillis());
    }

    public static long today() {
        return Convert.DateTime.roundToDays(now());
    }

    // endregion

    /////////////////////////////////////////////////////////////////////////////////////
    // region Conversion
    /////////////////////////////////////////////////////////////////////////////////////

    /**
     * Get milliseconds since Unix Epoch
     * @return milliseconds singe 1970-1-1 00:00:00 as long
     */
    public static long toEpochMilliseconds(long value) {
        return Convert.DateTime.toMillis(value);
    }

    /**
     * Get nanoseconds since Unix Epoch
     * @return nanoseconds singe 1970-1-1 00:00:00 as long
     */
    public static long toEpochNanoseconds(long value) {
        return value;
    }

    // endregion Conversion

    /////////////////////////////////////////////////////////////////////////////////////
    // region  Components
    /////////////////////////////////////////////////////////////////////////////////////
    // region Component Getters
    /////////////////////////////////////////////////////////////////////////////////////

    public static long getDate(long value) {
        return Convert.DateTime.roundToDays(value);
    }

    public static long getTimeOfDay(long value) {
        return Convert.DateTime.extractTimeOfDay(value);
    }

    public static int getYear(long value) {
        return Convert.DateTime.extractYear(value);
    }

    public static Month getMonth(long value) {
        return Convert.DateTime.extractMonthOfYear(value);
    }

    public static int getWeekOfYear(long value) {
        return Convert.DateTime.extractWeekOfYear(value);
    }

    public static int getWeekOfMonth(long value) {
        return Convert.DateTime.extractWeekOfMonth(value);
    }

    public static int getDayOfYear(long value) {
        return Convert.DateTime.extractDayOfYear(value);
    }

    public static int getDayOfMonth(long value) {
        return Convert.DateTime.extractDayOfMonth(value);
    }

    public static DayOfWeek getDayOfWeek(long value) {
        return Convert.DateTime.extractDayOfWeek(value);
    }

    public static int getHour(long value) {
        return Convert.DateTime.extractHourOfDay(value);
    }

    public static int getMinute(long value) {
        return Convert.DateTime.extractMinuteOfHour(value);
    }

    public static int getSecond(long value) {
        return Convert.DateTime.extractSecondOfMinute(value);
    }

    public static int getMillisecond(long value) {
        return Convert.DateTime.extractMillisecondOfSecond(value);
    }

    public static int getMicrosecond(long value) {
        return Convert.DateTime.extractMicrosecondOfSecond(value);
    }

    public static int getNanosecond(long value) {
        return Convert.DateTime.extractNanosecondOfSecond(value);
    }

    // endregion Component Getters

    /////////////////////////////////////////////////////////////////////////////////////
    // region Component Setters
    /////////////////////////////////////////////////////////////////////////////////////

    public static long setDate(long a, long b) {
        return Convert.DateTime.extractTimeOfDay(a) + Convert.DateTime.roundToDays(b);
    }

    public static long setTime(long dateTime, long timeSpan) {
        return Convert.DateTime.roundToDays(dateTime) + Convert.TimeSpan.checkTimeOfDay(timeSpan);
    }

    public static long setYear(long value, int year) {
        return Convert.DateTime.setYear(value, year);
    }

    public static long setMonth(long value, Month month) {
        return Convert.DateTime.setMonth(value, month);
    }

    public static long setWeekOfYear(long value, int week) {
        return Convert.DateTime.setWeekOfYear(value, week);
    }

    public static long setWeekOfMonth(long value, int week) {
        return Convert.DateTime.setWeekOfMonth(value, week);
    }

    public static long setDayOfYear(long value, int day) {
        return Convert.DateTime.setDayOfYear(value, day);
    }

    public static long setDayOfMonth(long value, int day) {
        return Convert.DateTime.setDayOfMonth(value, day);
    }

    public static long setDayOfWeek(long value, DayOfWeek dayOfWeek) {
        return Convert.DateTime.setDayOfWeek(value, dayOfWeek);
    }

    public static long setHour(long value, int hour) {
        return Convert.DateTime.setHourOfDay(value, hour);
    }

    public static long setMinute(long value, int minute) {
        return Convert.DateTime.setMinuteOfHour(value, minute);
    }

    public static long setSecond(long value, int second) {
        return Convert.DateTime.setSecondOfMinute(value, second);
    }

    /**
     * set fractional component (below 1 second)
     * @param value
     * @param nanosecond
     * @return
     */
    public static long setNanosecond(long value, int nanosecond) {
        return Convert.DateTime.setNanosecondOfSecond(value, nanosecond);
    }

    // endregion Component Setters
    // endregion Components

    /////////////////////////////////////////////////////////////////////////////////////
    // region Comparison
    /////////////////////////////////////////////////////////////////////////////////////

    public static int compareTo(long a, long b) {
        return Long.compare(a, b);
    }

    public static int compareTo(long a, Object b) {
        return Long.compare(a, ((HdDateTime)b).value);
    }

    public static int compare(long a, long b) {
        return Long.compare(a, b);
    }

// TODO: Less/Greater? Before/After ? Earlier/Later ?

//    public static boolean isEqual(long a, long b) {
//        return a == b;
//    }
//
//    public static boolean isLess(long a, long b) {
//        return a < b;
//    }
//
//    public static boolean isLessOrEqual(long a, long b) {
//        return a <= b;
//    }
//
//    public static boolean isGreater(long a, long b) {
//        return a > b;
//    }


    // endregion Comparison

    /////////////////////////////////////////////////////////////////////////////////////
    // region Arithmetic and rounding
    /////////////////////////////////////////////////////////////////////////////////////

    static long roundToU(long value, long unitSizeNs) {
        return Convert.DateTime.roundTo(value, unitSizeNs);
    }

    public static long roundTo(long value, long timeSpan) {
        if (timeSpan <= 0)
            throw new IllegalArgumentException("resolution must be positive");

        return roundToU(value, timeSpan);
    }

    public static long roundTo(long value, Resolution resolution) {
        /*
         * TODO: This looks misleading. Rounding for HdDateTime and rounding for HdTimeSpan
         * should behave differently and this is not obvious to user
         */

        switch (resolution) {
            case DAY:
                return roundToU(value, Convert.NS_IN_DAY);

            case HOUR:
                return roundToU(value, Convert.NS_IN_HOUR);

            case MINUTE:
                return roundToU(value, Convert.NS_IN_MINUTE);

            case SECOND:
                return roundToU(value, Convert.NS_IN_SECOND);

            case MILLISECOND:
                return roundToU(value, Convert.NS_IN_MILLISECOND);

            case MICROSECOND:
                return roundToU(value, Convert.NS_IN_MICROSECOND);

            case NANOSECOND:
                return value;
        }

        throw new IllegalArgumentException("Unsupported resolution: " + resolution);
    }

    public static long add(long dateTime, long timeSpan) {
        return Util.addToDt(dateTime, timeSpan);
    }

    public static long subtractDateTime(long a, long b) {
        return Util.subtractToTs(a, b);
    }

    public static long subtractTimeSpan(long a, long b) {
        return Util.subtractToDt(a, b);
    }

    public static long addUnchecked(long dt, long ts) {
        return dt + ts;
    }

    public static long subtractUnchecked(long dt, long ts) {
        return dt - ts;
    }

    public static long addYears(long value, int years) {
        return Convert.DateTime.addCalendarComponent(value, Calendar.YEAR, years);
    }

    public static long addMonths(long value, int months) {
        return Convert.DateTime.addCalendarComponent(value, Calendar.MONTH, months);
    }

    public static long addDays(long value, long days) {
        return Util.addToDt(value, HdTimeSpanUtils.fromDays(days));
    }

    public static long addHours(long value, long hours) {
        return Util.addToDt(value, HdTimeSpanUtils.fromHours(hours));
    }

    public static long addMinutes(long value, long minutes) {
        return Util.addToDt(value, HdTimeSpanUtils.fromMinutes(minutes));
    }

    public static long addSeconds(long value, long seconds) {
        return Util.addToDt(value, HdTimeSpanUtils.fromSeconds(seconds));
    }

    public static long addMilliseconds(long value, long milliseconds) {
        return Util.addToDt(value, HdTimeSpanUtils.fromMilliseconds(milliseconds));
    }

    public static long addMicroseconds(long value, long microseconds) {
        return Util.addToDt(value, HdTimeSpanUtils.fromMicroseconds(microseconds));
    }

    public static long addNanoseconds(long value, long nanoseconds) {
        return Util.addToDt(value, nanoseconds);
    }

    // endregion Arithmetic

    /////////////////////////////////////////////////////////////////////////////////////
    // region Helpers
    /////////////////////////////////////////////////////////////////////////////////////

    public static int getDaysInMonth(long value) {
        return Convert.DateTime.calendarFromNanos(Convert.DateTime.checkNanos(value)).getActualMaximum(Calendar.MONTH);
    }

    public static int getDaysInYear(long value) {
        return Convert.DateTime.calendarFromNanos(Convert.DateTime.checkNanos(value)).getActualMaximum(Calendar.YEAR);
    }

    public static boolean isLeapYear(long value) {
        GregorianCalendar calendar = Convert.DateTime.calendarFromNanos(Convert.DateTime.checkNanos(value));
        return isLeapYear(calendar.get(Calendar.YEAR));
    }

    public static boolean isLeapYear(int year) {
        return (year & 3) == 0 && (year % 100 != 0 || year % 400 == 0);
    }

    // endregion Helpers

    /////////////////////////////////////////////////////////////////////////////////////
    // region  Object interface
    /////////////////////////////////////////////////////////////////////////////////////

    public static int hashCode(long value) {
        return (int) (value ^ (value >>> 32));
    }

    public static boolean equals(long a, Object b) {
        return b != null && (b instanceof Long && equals(a, (long)(Long) b)
                || b instanceof HdDateTime && equals(a, ((HdDateTime)b).value));
    }

    public static boolean equals(long a, long b) {
        return a == b;
    }

    // endregion Object interface

    /////////////////////////////////////////////////////////////////////////////////////
    // region Parsing and formatting
    /////////////////////////////////////////////////////////////////////////////////////

    public static String toString(long value) {
        return toString(value, DEFAULT_FORMAT);
    }

    public static String toString(long value, String format) {
        return Formatters.DateTime.format(value, format);
    }

    public static Appendable appendTo(long value, Appendable appendable) throws IOException {
        return Formatters.DateTime.format(value, DEFAULT_FORMAT, appendable);
    }

    public static Appendable appendTo(long value, Appendable appendable, String format) throws IOException {
        return Formatters.DateTime.format(value, format, appendable);
    }

    public static long parse(CharSequence text) throws ParseException {
        return Parsers.DateTime.parse(text, DEFAULT_FORMAT);
    }

    public static long parse(CharSequence text, String fmt) throws ParseException {
        return Parsers.DateTime.parse(text, fmt);
    }

    // endregion Parsing and formatting

    /////////////////////////////////////////////////////////////////////////////////////
    // region Implementation Utility methods
    /////////////////////////////////////////////////////////////////////////////////////

    // endregion

    /////////////////////////////////////////////////////////////////////////////////////
    // region ValueType support: Helpers
    /////////////////////////////////////////////////////////////////////////////////////

    /// ValueType support, should not be used directly
    public static boolean isNull(long value) {
        return value == NULL;
    }

    static protected final long checkNull(final long value) {
        if (NULL == value)
            throw new NullPointerException();

        return value;
    }

    /// ValueType support, should not be used directly
    // ValueType requires "bitwise equality" method to be defined in addition to equals
    public static boolean isIdentical(long a, long b) {
        return a == b;
    }

    /// ValueType support, should not be used directly
    public static HdDateTime[] fromLongArray( long[] src, int srcOffset, HdDateTime[] dst, int dstOffset, int length) {
        int srcLength = src.length;
        int srcEndOffset = srcOffset + length;

        // NOTE: no bounds checks
        for (int i = 0; i < length; ++i) {
            dst[dstOffset + i] = HdDateTime.fromUnderlying(src[srcOffset + i]);
        }

        return dst;
    }

    /// ValueType support, should not be used directly
    public static long[] toLongArray(HdDateTime[] src, int srcOffset,  long[] dst, int dstOffset, int length) {
        int srcLength = src.length;
        int srcEndOffset = srcOffset + length;

        // NOTE: no bounds checks
        for (int i = 0; i < length; ++i) {
            dst[dstOffset + i] = HdDateTime.toUnderlying(src[srcOffset + i]);
        }

        return dst;
    }

    /// ValueType support, should not be used directly
    public static HdDateTime[] fromLongArray(long[] src) {
        return null == src ? null : fromLongArray(src, 0, new HdDateTime[src.length], 0, src.length);
    }

    /// ValueType support, should not be used directly
    public static long[] toLongArray(HdDateTime[] src) {
        return null == src ? null : toLongArray(src, 0, new long[src.length], 0, src.length);
    }

    // endregion

    /////////////////////////////////////////////////////////////////////////////////////
    // region ValueType support: Null-checking wrappers for non-static methods
    /////////////////////////////////////////////////////////////////////////////////////

    /////////////////////////////////////////////////////////////////////////////////////
    // region Conversion
    /////////////////////////////////////////////////////////////////////////////////////

    public static long toEpochMillisecondsChecked(long value) {
        return toEpochMilliseconds(checkNull(value));
    }

    public static long toEpochNanosecondsChecked(long value) {
        return checkNull(value);
    }

    // endregion Conversion

    /////////////////////////////////////////////////////////////////////////////////////
    // region  Components
    /////////////////////////////////////////////////////////////////////////////////////
    // region Component Getters
    /////////////////////////////////////////////////////////////////////////////////////

    public static long getDateChecked(long value) {
        return getDate(checkNull(value));
    }

    public static long getTimeOfDayChecked(long value) {
        return getTimeOfDay(checkNull(value));
    }
    
    public static int getYearChecked(long value) {
        return getYear(checkNull(value));
    }

    public static Month getMonthChecked(long value) {
        return getMonth(checkNull(value));
    }

    public static int getWeekOfYearChecked(long value) {
        return getWeekOfYear(checkNull(value));
    }

    public static int getWeekOfMonthChecked(long value) {
        return getWeekOfMonth(checkNull(value));
    }

    public static int getDayOfYearChecked(long value) {
        return getDayOfYear(checkNull(value));
    }

    public static int getDayOfMonthChecked(long value) {
        return getDayOfMonth(checkNull(value));
    }

    public static DayOfWeek getDayOfWeekChecked(long value) {
        return getDayOfWeek(checkNull(value));
    }

    public static int getHourChecked(long value) {
        return getHour(checkNull(value));
    }

    public static int getMinuteChecked(long value) {
        return getMinute(checkNull(value));
    }

    public static int getSecondChecked(long value) {
        return getSecond(checkNull(value));
    }

    public static int getMillisecondChecked(long value) {
        return getMillisecond(checkNull(value));
    }

    public static int getMicrosecondChecked(long value) {
        return getMicrosecond(checkNull(value));
    }

    public static int getNanosecondChecked(long value) {
        return getNanosecond(checkNull(value));
    }

    // endregion Component Getters

    /////////////////////////////////////////////////////////////////////////////////////
    // region Component Setters
    /////////////////////////////////////////////////////////////////////////////////////

    public static long setDateChecked(long a, long b) {
        return setDate(checkNull(a), checkNull(b));
    }

    public static long setTimeChecked(long dateTime, long timeSpan) {
        return setTime(checkNull(dateTime), checkNull(timeSpan));
    }

    public static long setYearChecked(long value, int year) {
        return setYear(value, year);
    }

    public static long setMonthChecked(long value, Month month) {
        return setMonth(value, month);
    }

    public static long setWeekOfYearChecked(long value, int week) {
        return setWeekOfYear(value, week);
    }

    public static long setWeekOfMonthChecked(long value, int week) {
        return setWeekOfMonth(value, week);
    }

    public static long setDayOfYearChecked(long value, int day) {
        return setDayOfYear(value, day);
    }

    public static long setDayOfMonthChecked(long value, int day) {
        return setDayOfMonth(value, day);
    }

    public static long setDayOfWeekChecked(long value, DayOfWeek dayOfWeek) {
        return setDayOfWeek(value, dayOfWeek);
    }

    public static long setHourChecked(long value, int hour) {
        return setHour(value, hour);
    }

    public static long setMinuteChecked(long value, int minute) {
        return setMinute(value, minute);
    }

    public static long setSecondChecked(long value, int second) {
        return setSecond(value, second);
    }

    public static long setNanosecondChecked(long value, int nanosecond) {
        return setNanosecond(value, nanosecond);
    }

    // endregion Component Setters
    // endregion Components

    /////////////////////////////////////////////////////////////////////////////////////
    // region Comparison
    /////////////////////////////////////////////////////////////////////////////////////

    public static int compareToChecked(long a, long b) {
        return compareTo(checkNull(a), checkNull(b));
    }

    public static int compareToChecked(long a, Object b) {
        return compareTo(checkNull(a), ((HdDateTime)b).value);
    }

    // endregion Comparison

    /////////////////////////////////////////////////////////////////////////////////////
    // region Arithmetic
    /////////////////////////////////////////////////////////////////////////////////////

    public static long roundToChecked(long value, long timeSpan) {
        return roundTo(checkNull(value), timeSpan /*Will be checked later anyway*/);
    }

    public static long roundToChecked(long value, Resolution resolution) {
        return roundTo(checkNull(value), resolution);
    }

    public static long addChecked(long dateTime, long timeSpan) {
        return add(checkNull(dateTime), checkNull(timeSpan));
    }

    public static long subtractDateTimeChecked(long a, long b) {
        return subtractDateTime(checkNull(a), checkNull(b));
    }

    public static long subtractTimeSpanChecked(long a, long b) {
        return subtractTimeSpan(checkNull(a), checkNull(b));
    }

    public static long addYearsChecked(long value, int years) {
        return addYears(checkNull(value), years);
    }

    public static long addMonthsChecked(long value, int months) {
        return addMonths(checkNull(value), months);
    }

    public static long addDaysChecked(long value, long days) {
        return addDays(checkNull(value), days);
    }

    public static long addHoursChecked(long value, long hours) {
        return addHours(checkNull(value), hours);
    }

    public static long addMinutesChecked(long value, long minutes) {
        return addMinutes(checkNull(value), minutes);
    }

    public static long addSecondsChecked(long value, long seconds) {
        return addSeconds(checkNull(value), seconds);
    }

    public static long addMillisecondsChecked(long value, long milliseconds) {
        return addMilliseconds(checkNull(value), milliseconds);
    }

    public static long addMicrosecondsChecked(long value, long microseconds) {
        return addMicroseconds(checkNull(value), microseconds);
    }

    public static long addNanosecondsChecked(long value, long nanoseconds) {
        return addNanoseconds(checkNull(value), nanoseconds);
    }

    // endregion Arithmetic

    /////////////////////////////////////////////////////////////////////////////////////
    // region Helpers
    /////////////////////////////////////////////////////////////////////////////////////

    public static int getDaysInMonthChecked(long value) {
        return getDaysInMonth(checkNull(value));
    }

    public static int getDaysInYearChecked(long value) {
        return getDaysInYear(checkNull(value));
    }

    public static boolean isLeapYearChecked(long value) {
        return isLeapYear(checkNull(value));
    }

    // endregion Helpers

    /////////////////////////////////////////////////////////////////////////////////////
    // region  Object interface
    /////////////////////////////////////////////////////////////////////////////////////

    public static int hashCodeChecked(long value) {
        return hashCode(checkNull(value));
    }

    public static boolean equalsChecked(long a, Object b) {
        return equals(checkNull(a), b);
    }

    public static boolean equalsChecked(long a, long b) {
        return equals(checkNull(a), b);
    }

    // endregion Object interface

    /////////////////////////////////////////////////////////////////////////////////////
    // region Parsing and formatting
    /////////////////////////////////////////////////////////////////////////////////////

    public static String toStringChecked(long value) {
        return toString(checkNull(value));
    }

    public static String toStringChecked(long value, String format) {
        return toString(checkNull(value), format);
    }

    public static Appendable appendToChecked(long value, Appendable appendable) throws IOException {
        return appendTo(checkNull(value), appendable);
    }

    public static Appendable appendToChecked(long value, Appendable appendable, String format) throws IOException {
        return appendTo(checkNull(value), appendable, format);
    }



    // endregion Parsing and formatting

    // endregion
}
