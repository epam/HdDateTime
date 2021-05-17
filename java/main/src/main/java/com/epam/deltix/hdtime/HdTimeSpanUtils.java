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

import java.io.IOException;

/**
 * Static methods to work with time span in nanoseconds resolution.
 *
 * Value is supposed to be number of nanoseconds.
 */
public class HdTimeSpanUtils {
    public static final String DEFAULT_FORMAT = "d HH:mm:ss.fffffffff";

    static final long MIN  = -Long.MAX_VALUE;
    static final long MAX  =  Long.MAX_VALUE;
    static final long NULL =  Long.MIN_VALUE;

    /**
     * Null value should not be passed to HdTimeSpanUtils methods by user's code
     * It is also never returned by HdTimeSpanUtils methods
     */
    public static final long NULL_VALUE = NULL;

    public static final long MIN_VALUE = MIN;
    public static final long MAX_VALUE = MAX;

    public static final long NANOSECONDS_IN_DAY = Convert.NS_IN_DAY;
    public static final long NANOSECONDS_IN_HOUR = Convert.NS_IN_HOUR;
    public static final long NANOSECONDS_IN_MINUTE = Convert.NS_IN_MINUTE;
    public static final long NANOSECONDS_IN_SECOND = Convert.NS_IN_SECOND;
    public static final long NANOSECONDS_IN_MICROSECOND = Convert.NS_IN_MICROSECOND;
    public static final long NANOSECONDS_IN_MILLISECOND = Convert.NS_IN_MILLISECOND;
    public static final long ZERO = 0;

    /////////////////////////////////////////////////////////////////////////////////////
    // region Constructors
    /////////////////////////////////////////////////////////////////////////////////////

    /**
     * Creates an instance of HdTimeSpan without performing range check
     * @param value HdTimeSpan as long
     * @return new instance of HdTimeSpan
     */
    public static long fromUnderlying(long value) {
        return value;
    }

    public static long toUnderlying(long value) {
        return value;
    }

    // TODO: Validate input parameters

    public static long newInstance(int days, int hours, int minutes, int seconds) {
        return newInstance(days, hours, minutes, seconds,  0);
    }

    public static long newInstance(int hours, int minutes, int seconds) {
        return Convert.TimeSpan.from(hours, minutes, seconds);
    }

    public static long newInstance(int days, int hours, int minutes, int seconds, int nanoseconds) {
        return Convert.TimeSpan.from(days, hours, minutes, seconds, nanoseconds);
    }

    public static long fromNanoseconds(long nanoseconds) {
        return Convert.TimeSpan.fromNanoseconds(nanoseconds);
    }

    public static long fromMicroseconds(long microseconds) {
        return Convert.TimeSpan.fromMicroseconds(microseconds);
    }

    public static long fromMilliseconds(long milliseconds) {
        return Convert.TimeSpan.fromMilliseconds(milliseconds);
    }

    public static long fromSeconds(long seconds) {
        return Convert.TimeSpan.fromSeconds(seconds);
    }

    public static long fromMinutes(long minutes) {
        return Convert.TimeSpan.fromMinutes(minutes);
    }

    public static long fromHours(long hours) {
        return Convert.TimeSpan.fromHours(hours);
    }

    public static long fromDays(long days) {
        return Convert.TimeSpan.fromDays(days);
    }

    public static long fromMicroseconds(double microseconds) {
        return Convert.TimeSpan.fromMicroseconds(microseconds);
    }

    public static long fromMilliseconds(double milliseconds) {
        return Convert.TimeSpan.fromMilliseconds(milliseconds);
    }

    public static long fromSeconds(double seconds) {
        return Convert.TimeSpan.fromSeconds(seconds);
    }

    public static long fromMinutes(double minutes) {
        return Convert.TimeSpan.fromMinutes(minutes);
    }

    public static long fromHours(double hours) {
        return Convert.TimeSpan.fromHours(hours);
    }

    public static long fromDays(double days) {
        return Convert.TimeSpan.fromDays(days);
    }

    // endregion Constructors

    /////////////////////////////////////////////////////////////////////////////////////
    // region Total
    /////////////////////////////////////////////////////////////////////////////////////

    public static long totalNanoseconds(long value) {
        return value;
    }

    public static double totalMicroseconds(long value) {
        return Convert.TimeSpan.toDoubleMicros(value);
    }

    public static double totalMilliseconds(long value) {
        return Convert.TimeSpan.toDoubleMillis(value);
    }

    public static double totalSeconds(long value) {
        return Convert.TimeSpan.toDoubleSeconds(value);
    }

    public static double totalMinutes(long value) {
        return Convert.TimeSpan.toDoubleMinutes(value);
    }

    public static double totalHours(long value) {
        return Convert.TimeSpan.toDoubleHours(value);
    }

    public static double totalDays(long value) {
        return Convert.TimeSpan.toDoubleDays(value);
    }

    public static double totalWeeks(long value) {
        return Convert.TimeSpan.toDoubleWeeks(value);
    }

    // endregion Total

    /////////////////////////////////////////////////////////////////////////////////////
    // region Components
    /////////////////////////////////////////////////////////////////////////////////////
    // region Component Getters
    /////////////////////////////////////////////////////////////////////////////////////

    public static int getNanoseconds(long value) {
        return Convert.TimeSpan.nanosecondsComponent(value);
    }

    public static int getMicroseconds(long value) {
        return Convert.TimeSpan.microsecondsComponent(value);
    }

    public static int getMilliseconds(long value) {
        return Convert.TimeSpan.millisecondsComponent(value);
    }

    public static int getSeconds(long value) {
        return Convert.TimeSpan.secondsComponent(value);
    }

    public static int getMinutes(long value) {
        return Convert.TimeSpan.minutesComponent(value);
    }

    public static int getHours(long value) {
        return Convert.TimeSpan.hoursComponent(value);
    }

    public static int getDays(long value) {
        return Convert.TimeSpan.days(value);
    }

    // endregion Component Getters

    /////////////////////////////////////////////////////////////////////////////////////
    // region Component Setters
    /////////////////////////////////////////////////////////////////////////////////////

    // Existing component setters removed, because they are incorrect.
    //
    // Reason:
    // What will happen if we call newInstance(1,23,45).setHours(-23) ? What will getHours() return afterwards?

//    public static long setDays(long value, int days) {
//        return value % NANOS_IN_DAY
//                + days * NANOS_IN_DAY;
//    }
//
//    public static long setHours(long value, int hours) {
//        return value - getHours(value) * NANOS_IN_HOUR
//                + hours * NANOS_IN_HOUR;
//    }
//
//    public static long setMinutes(long value, int minutes) {
//        return value - getMinutes(value) * NANOS_IN_MINUTE
//                + minutes * NANOS_IN_MINUTE;
//    }
//
//    public static long setSeconds(long value, int seconds) {
//        return value - getSeconds(value) * NANOS_IN_SECOND
//                + seconds * NANOS_IN_SECOND;
//    }
//
//    public static long setMilliseconds(long value, int milliseconds) {
//        return value - getMilliseconds(value) * NANOS_IN_MILLISECOND
//                + milliseconds * NANOS_IN_MILLISECOND;
//    }
//
//    public static long setMicroseconds(long value, int microseconds) {
//        return value - getMicroseconds(value) * NANOS_IN_MICROSECOND
//                + microseconds * NANOS_IN_MICROSECOND;
//    }
//
//    public static long setNanoseconds(long value, int nanoseconds) {
//        return value - getNanoseconds(value) + nanoseconds;
//    }
    // endregion
    // endregion

    /////////////////////////////////////////////////////////////////////////////////////
    // region Comparison
    /////////////////////////////////////////////////////////////////////////////////////

    public static int compareTo(long a, long b) {
        return Long.compare(a, b);
    }

    public static int compare(long a, long b) {
        return Long.compare(a, b);
    }

    public static boolean isLess(long a, long b) {
        return a < b;
    }

    public static boolean isLessOrEqual(long a, long b) {
        return a <= b;
    }

    public static boolean isGreater(long a, long b) {
        return a > b;
    }

    public static boolean isGreaterOrEqual(long a, long b) {
        return a >= b;
    }

    public static boolean isZero(long value) {
        return 0 == value;
    }

    public static boolean isPositive(long value) {
        return value > 0;
    }

    public static boolean isNegative(long value) {
        return value < 0;
    }

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

    public static long add(long a, long b) {
        return Util.addToTs(a, b);
    }

    public static long subtract(long a, long b) {
        return Util.subtractToTs(a, b);
    }

    public static long addUnchecked(long a, long b) {
        return a + b;
    }

    public static long subtractUnchecked(long a, long b) {
        return a - b;
    }

    public static long negate(long value) {
        return -value;
    }

    public static long duration(long value) {
        return Math.abs(value);
    }

    public static long addDays(long value, long days) {
        return Util.addToTs(value, HdTimeSpanUtils.fromDays(days));
    }

    public static long addHours(long value, long hours) {
        return Util.addToTs(value, HdTimeSpanUtils.fromHours(hours));
    }

    public static long addMinutes(long value, long minutes) {
        return Util.addToTs(value, HdTimeSpanUtils.fromMinutes(minutes));
    }

    public static long addSeconds(long value, long seconds) {
        return Util.addToTs(value, HdTimeSpanUtils.fromSeconds(seconds));
    }

    public static long addMilliseconds(long value, long milliseconds) {
        return Util.addToTs(value, HdTimeSpanUtils.fromMilliseconds(milliseconds));
    }

    public static long addMicroseconds(long value, long microseconds) {
        return Util.addToTs(value, HdTimeSpanUtils.fromMicroseconds(microseconds));
    }

    public static long addNanoseconds(long value, long nanoseconds) {
        return Util.addToTs(value, nanoseconds);
    }

    // endregion Arithmetic

    /////////////////////////////////////////////////////////////////////////////////////
    // region Object interface
    /////////////////////////////////////////////////////////////////////////////////////

    public static int hashCode(long value) {
        return (int) (value ^ (value >>> 32));
    }

    public static boolean equals(long a, Object b) {
        return b != null && (b instanceof Long && equals(a, (long)(Long) b)
                || b instanceof HdTimeSpan && equals(a, ((HdTimeSpan)b).value));
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

    public static String toString(long value, final String format) {
        return Formatters.TimeSpan.format(value, format);
    }

    public static Appendable appendTo(long value, Appendable appendable) throws IOException {
        //return appendable.append(toString(value));
        return Formatters.TimeSpan.format(value, DEFAULT_FORMAT, appendable);
    }

    public static Appendable appendTo(long value, final Appendable appendable, final String format) throws IOException {
        return Formatters.TimeSpan.format(value, format, appendable);
    }

    public static long parse(CharSequence text) throws ParseException {
        return Parsers.TimeSpan.parse(text, DEFAULT_FORMAT);
    }

    public static long parse(CharSequence text, String fmt) throws ParseException {
        return Parsers.TimeSpan.parse(text, fmt);
    }

    // endregion Parsing and formatting

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
    public static HdTimeSpan[] fromLongArray( long[] src, int srcOffset, HdTimeSpan[] dst, int dstOffset, int length) {

        int srcLength = src.length;
        int srcEndOffset = srcOffset + length;

        // NOTE: no bounds checks
        for (int i = 0; i < length; ++i) {
            dst[dstOffset + i] = HdTimeSpan.fromUnderlying(src[srcOffset + i]);
        }

        return dst;
    }

    /// ValueType support, should not be used directly
    public static long[] toLongArray(HdTimeSpan[] src, int srcOffset,  long[] dst, int dstOffset, int length) {

        int srcLength = src.length;
        int srcEndOffset = srcOffset + length;

        // NOTE: no bounds checks
        for (int i = 0; i < length; ++i) {
            dst[dstOffset + i] = HdTimeSpan.toUnderlying(src[srcOffset + i]);
        }

        return dst;
    }

    /// ValueType support, should not be used directly
    public static HdTimeSpan[] fromLongArray(long[] src) {
        return null == src ? null : fromLongArray(src, 0, new HdTimeSpan[src.length], 0, src.length);
    }

    /// ValueType support, should not be used directly
    public static  long[] toLongArray(HdTimeSpan[] src) {
        return null == src ? null : toLongArray(src, 0, new long[src.length], 0, src.length);
    }

    // endregion

    /////////////////////////////////////////////////////////////////////////////////////
    // region ValueType support: Null-checking wrappers for non-static methods
    /////////////////////////////////////////////////////////////////////////////////////

    /////////////////////////////////////////////////////////////////////////////////////
    // region Total
    /////////////////////////////////////////////////////////////////////////////////////

    public static double totalWeeksChecked(long value) {
        return totalWeeks(checkNull(value));
    }

    public static double totalDaysChecked(long value) {
        return totalDays(checkNull(value));
    }

    public static double totalHoursChecked(long value) {
        return totalHours(checkNull(value));
    }

    public static double totalMinutesChecked(long value) {
        return totalMinutes(checkNull(value));
    }

    public static double totalSecondsChecked(long value) {
        return totalSeconds(checkNull(value));
    }

    public static double totalMillisecondsChecked(long value) {
        return totalMilliseconds(checkNull(value));
    }

    public static double totalMicrosecondsChecked(long value) {
        return totalMicroseconds(checkNull(value));
    }

    public static long totalNanosecondsChecked(long value) {
        return totalNanoseconds(checkNull(value));
    }

    // endregion Total

    /////////////////////////////////////////////////////////////////////////////////////
    // region Components
    /////////////////////////////////////////////////////////////////////////////////////
    // region Component Getters
    /////////////////////////////////////////////////////////////////////////////////////

    public static int getDaysChecked(long value) {
        return getDays(checkNull(value));
    }

    public static int getHoursChecked(long value) {
        return getHours(checkNull(value));
    }

    public static int getMinutesChecked(long value) {
        return getMinutes(checkNull(value));
    }

    public static int getSecondsChecked(long value) {
        return getSeconds(checkNull(value));
    }

    public static int getMillisecondsChecked(long value) {
        return getMilliseconds(checkNull(value));
    }

    public static int getMicrosecondsChecked(long value) {
        return getMicroseconds(checkNull(value));
    }

    public static int getNanosecondsChecked(long value) {
        return getNanoseconds(checkNull(value));
    }

    // endregion Component Getters
    // endregion

    /////////////////////////////////////////////////////////////////////////////////////
    // region Comparison
    /////////////////////////////////////////////////////////////////////////////////////

    public static int compareToChecked(long a, long b) {
        return compareTo(checkNull(a), checkNull(b));
    }

    public static int compareToChecked(long a, Object b) {
        return compareTo(checkNull(a), ((HdTimeSpan)b).value);
    }

    public static boolean isLessChecked(long a, long b) {
        return checkNull(a) < checkNull(b);
    }

    public static boolean isLessOrEqualChecked(long a, long b) {
        return checkNull(a) <= checkNull(b);
    }

    public static boolean isGreaterChecked(long a, long b) {
        return checkNull(a) > checkNull(b);
    }

    public static boolean isGreaterOrEqualChecked(long a, long b) {
        return checkNull(a) >= checkNull(b);
    }

    public static boolean isZeroChecked(long value) {
        return 0 == checkNull(value);
    }

    public static boolean isPositiveChecked(long value) {
        return checkNull(value) > 0;
    }

    public static boolean isNegativeChecked(long value) {
        return checkNull(value) < 0;
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

    public static long addChecked(long a, long b) {
        return add(checkNull(a), checkNull(b));
    }

    public static long subtractChecked(long a, long b) {
        return subtract(checkNull(a), checkNull(b));
    }

    public static long negateChecked(long value) {
        return negate(checkNull(value));
    }

    public static long durationChecked(long value) {
        return duration(checkNull(value));
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
    // region Object interface
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

    public static String toStringChecked(long value, final String format) {
        return toString(checkNull(value), format);
    }

    public static Appendable appendToChecked(long value, Appendable appendable) throws IOException {
        return appendTo(checkNull(value), appendable);
    }

    public static Appendable appendToChecked(long value, final Appendable appendable, final String format) throws IOException {
        return appendTo(checkNull(value), appendable, format);
    }

    // endregion Parsing and formatting

    // endregion
}
