package com.epam.deltix.hdtime;

import java.io.IOException;

/**
 * Time interval with nanoseconds resolution.
 *
 * This class is immutable.
 * Can be instantiated only through static constructors.
 */
public class HdTimeSpan implements Comparable<HdTimeSpan> {
    /////////////////////////////////////////////////////////////////////////////////////
    // region Constants
    /////////////////////////////////////////////////////////////////////////////////////

    public static final String DEFAULT_FORMAT = HdTimeSpanUtils.DEFAULT_FORMAT;

    public static final long DAYS_IN_WEEK = 7;

    public static final long HOURS_IN_DAY = 24;
    public static final long HOURS_IN_WEEK = HOURS_IN_DAY * DAYS_IN_WEEK;

    public static final long MINUTES_IN_HOUR = 60;
    public static final long MINUTES_IN_DAY = MINUTES_IN_HOUR * HOURS_IN_DAY;
    public static final long MINUTES_IN_WEEK = MINUTES_IN_HOUR * HOURS_IN_WEEK;

    public static final long SECONDS_IN_MINUTE = 60;
    public static final long SECONDS_IN_HOUR = SECONDS_IN_MINUTE * MINUTES_IN_HOUR;
    public static final long SECONDS_IN_DAY = SECONDS_IN_MINUTE * MINUTES_IN_DAY;
    public static final long SECONDS_IN_WEEK = SECONDS_IN_MINUTE * MINUTES_IN_WEEK;

    public static final long MILLISECONDS_IN_SECOND = 1000L;
    public static final long MILLISECONDS_IN_MINUTE = MILLISECONDS_IN_SECOND * SECONDS_IN_MINUTE;
    public static final long MILLISECONDS_IN_HOUR = MILLISECONDS_IN_SECOND * SECONDS_IN_HOUR;
    public static final long MILLISECONDS_IN_DAY = MILLISECONDS_IN_SECOND * SECONDS_IN_DAY;
    public static final long MILLISECONDS_IN_WEEK = MILLISECONDS_IN_SECOND * SECONDS_IN_WEEK;

    public static final long MICROSECONDS_IN_MILLISECOND = 1000L;
    public static final long MICROSECONDS_IN_SECOND = MICROSECONDS_IN_MILLISECOND * MILLISECONDS_IN_SECOND;
    public static final long MICROSECONDS_IN_MINUTE = MICROSECONDS_IN_MILLISECOND * MILLISECONDS_IN_MINUTE;
    public static final long MICROSECONDS_IN_HOUR = MICROSECONDS_IN_MILLISECOND * MILLISECONDS_IN_HOUR;
    public static final long MICROSECONDS_IN_DAY = MICROSECONDS_IN_MILLISECOND * MILLISECONDS_IN_DAY;
    public static final long MICROSECONDS_IN_WEEK = MICROSECONDS_IN_MILLISECOND * MILLISECONDS_IN_WEEK;

    public static final long NANOS_IN_MICROSECOND = 1000L;
    public static final long NANOS_IN_MILLISECOND = NANOS_IN_MICROSECOND * MICROSECONDS_IN_MILLISECOND;
    public static final long NANOS_IN_SECOND = NANOS_IN_MICROSECOND * MICROSECONDS_IN_SECOND;
    public static final long NANOS_IN_MINUTE = NANOS_IN_MICROSECOND * MICROSECONDS_IN_MINUTE;
    public static final long NANOS_IN_HOUR = NANOS_IN_MICROSECOND * MICROSECONDS_IN_HOUR;
    public static final long NANOS_IN_DAY = NANOS_IN_MICROSECOND * MICROSECONDS_IN_DAY;
    public static final long NANOS_IN_WEEK = NANOS_IN_MICROSECOND * MICROSECONDS_IN_WEEK;

    public static final HdTimeSpan NULL = null;

    // NULL _underlying_ value. Can only be passed to fromUnderlying, yielding NULL
    public static final long NULL_VALUE = HdTimeSpanUtils.NULL_VALUE;

    public static final HdTimeSpan MIN_VALUE = new HdTimeSpan(HdTimeSpanUtils.MIN_VALUE);
    public static final HdTimeSpan MAX_VALUE = new HdTimeSpan(HdTimeSpanUtils.MAX_VALUE);
    public static final HdTimeSpan ZERO = new HdTimeSpan(HdTimeSpanUtils.ZERO);

    // endregion Constants

    // Number of nanoseconds.
    final long value;

    /////////////////////////////////////////////////////////////////////////////////////
    // region Constructors
    /////////////////////////////////////////////////////////////////////////////////////

    HdTimeSpan(long value) {
        this.value = value;
    }

    /**
     * Creates an instance of HdTimeSpan without performing range check
     * @param value HdTimeSpan as long
     * @return new instance of HdTimeSpan
     */
    public static HdTimeSpan fromUnderlying(long value) {
        return HdTimeSpanUtils.isNull(value) ? null : new HdTimeSpan(value);
    }

    public static long toUnderlying(HdTimeSpan timeSpan) {
        return null != timeSpan ? timeSpan.value : HdTimeSpanUtils.NULL_VALUE;
    }

    public static HdTimeSpan newInstance(int hours, int minutes, int seconds) {
        return new HdTimeSpan(HdTimeSpanUtils.newInstance(hours, minutes, seconds));
    }

    public static HdTimeSpan newInstance(int days, int hours, int minutes, int seconds) {
        return new HdTimeSpan(HdTimeSpanUtils.newInstance(days, hours, minutes, seconds));
    }

    public static HdTimeSpan newInstance(int days, int hours, int minutes, int seconds, int nanoseconds) {
        return new HdTimeSpan(HdTimeSpanUtils.newInstance(days, hours, minutes, seconds, nanoseconds));
    }

    /**
     * Creates an instance of HdTimeSpan from time interval in days
     * @param days signed time interval in days
     * @return new instance of HdTimeSpan
     */
    public static HdTimeSpan fromDays(long days) {
        return new HdTimeSpan(HdTimeSpanUtils.fromDays(days));
    }

    /**
     * Creates an instance of HdTimeSpan from time interval in hours
     * @param hours signed time interval in hours
     * @return new instance of HdTimeSpan
     */
    public static HdTimeSpan fromHours(long hours) {
        return new HdTimeSpan(HdTimeSpanUtils.fromHours(hours));
    }

    /**
     * Creates an instance of HdTimeSpan from time interval in minutes
     * @param minutes signed time interval in minutes
     * @return new instance of HdTimeSpan
     */
    public static HdTimeSpan fromMinutes(long minutes) {
        return new HdTimeSpan(HdTimeSpanUtils.fromMinutes(minutes));
    }

    /**
     * Creates an instance of HdTimeSpan from time interval in seconds
     * @param seconds signed time interval in seconds
     * @return new instance of HdTimeSpan
     */
    public static HdTimeSpan fromSeconds(long seconds) {
        return new HdTimeSpan(HdTimeSpanUtils.fromSeconds(seconds));
    }

    /**
     * Creates an instance of HdTimeSpan from milliseconds time interval
     * @param milliseconds signed time interval in milliseconds
     * @return new instance of HdTimeSpan
     */
    public static HdTimeSpan fromMilliseconds(long milliseconds) {
        return new HdTimeSpan(HdTimeSpanUtils.fromMilliseconds(milliseconds));
    }

    /**
     * Creates an instance of HdTimeSpan from microsecond time interval
     * @param microseconds signed time interval in microseconds
     * @return new instance of HdTimeSpan
     */
    public static HdTimeSpan fromMicroseconds(long microseconds) {
        return new HdTimeSpan(HdTimeSpanUtils.fromMicroseconds(microseconds));
    }

    /**
     * Creates an instance of HdTimeSpan with range check. Will ignore NULL value
     * @param nanoseconds
     * @return new instance of HdTimeSpan
     */
    public static HdTimeSpan fromNanoseconds(long nanoseconds) {
        return new HdTimeSpan(HdTimeSpanUtils.fromNanoseconds(nanoseconds));
    }

    /**
     * Creates an instance of HdTimeSpan from time interval in days
     * @param days signed time interval in days
     * @return new instance of HdTimeSpan
     */
    public static HdTimeSpan fromDays(double days) {
        return new HdTimeSpan(HdTimeSpanUtils.fromDays(days));
    }

    /**
     * Creates an instance of HdTimeSpan from time interval in hours
     * @param hours signed time interval in hours
     * @return new instance of HdTimeSpan
     */
    public static HdTimeSpan fromHours(double hours) {
        return new HdTimeSpan(HdTimeSpanUtils.fromHours(hours));
    }

    /**
     * Creates an instance of HdTimeSpan from time interval in minutes
     * @param minutes signed time interval in minutes
     * @return new instance of HdTimeSpan
     */
    public static HdTimeSpan fromMinutes(double minutes) {
        return new HdTimeSpan(HdTimeSpanUtils.fromMinutes(minutes));
    }

    /**
     * Creates an instance of HdTimeSpan from time interval in seconds
     * @param seconds signed time interval in seconds
     * @return new instance of HdTimeSpan
     */
    public static HdTimeSpan fromSeconds(double seconds) {
        return new HdTimeSpan(HdTimeSpanUtils.fromSeconds(seconds));
    }

    /**
     * Creates an instance of HdTimeSpan from milliseconds time interval
     * @param milliseconds signed time interval in milliseconds
     * @return new instance of HdTimeSpan
     */
    public static HdTimeSpan fromMilliseconds(double milliseconds) {
        return new HdTimeSpan(HdTimeSpanUtils.fromMilliseconds(milliseconds));
    }

    /**
     * Creates an instance of HdTimeSpan from microsecond time interval
     * @param microseconds signed time interval in microseconds
     * @return new instance of HdTimeSpan
     */
    public static HdTimeSpan fromMicroseconds(double microseconds) {
        return new HdTimeSpan(HdTimeSpanUtils.fromMicroseconds(microseconds));
    }

    // endregion Constructors

    /////////////////////////////////////////////////////////////////////////////////////
    // region Components
    /////////////////////////////////////////////////////////////////////////////////////
    // region Component Getters
    /////////////////////////////////////////////////////////////////////////////////////

    public int getDays() {
        return HdTimeSpanUtils.getDays(value);
    }

    /**
     * Hour of day [-23; 23]
     */
    public int getHours() {
        return HdTimeSpanUtils.getHours(value);
    }

    /**
     * Minute of hour [-59; 59]
     */
    public int getMinutes() {
        return HdTimeSpanUtils.getMinutes(value);
    }

    /**
     * Second of minute [-59; 59]
     */
    public int getSeconds() {
        return HdTimeSpanUtils.getSeconds(value);
    }

    /**
     * Millisecond of second [-999; 999]
     */
    public int getMilliseconds() {
        return HdTimeSpanUtils.getMilliseconds(value);
    }

    /**
     * Microsecond of second [-999999; 999999]
     */
    public int getMicroseconds() {
        return HdTimeSpanUtils.getMicroseconds(value);
    }

    /**
     * Nanosecond of second [-999999999; 999999999]
     */
    public int getNanoseconds() {
        return HdTimeSpanUtils.getNanoseconds(value);
    }

    // endregion

    // region Component Setters

    // Component setters are removed due to being confusing when applied to HdTimeSpan
    // Need to decide: Remove altogether, check sign of the argument or ignore the sign

//    public HdTimeSpan setDays(int days) {
//        return new HdTimeSpan(HdTimeSpanUtils.setDays(value, days));
//    }
//
//    public HdTimeSpan setHours(int hours) {
//        return new HdTimeSpan(HdTimeSpanUtils.setHours(value, hours));
//    }
//
//    public HdTimeSpan setMinutes(int minutes) {
//        return new HdTimeSpan(HdTimeSpanUtils.setMinutes(value, minutes));
//    }
//
//    public HdTimeSpan setSeconds(int seconds) {
//        return new HdTimeSpan(HdTimeSpanUtils.setSeconds(value, seconds));
//    }
//
//    public HdTimeSpan setMilliseconds(int milliseconds) {
//        return new HdTimeSpan(HdTimeSpanUtils.setMilliseconds(value, milliseconds));
//    }
//
//    public HdTimeSpan setMicroseconds(int microseconds) {
//        return new HdTimeSpan(HdTimeSpanUtils.setMicroseconds(value, microseconds));
//    }
//
//    public HdTimeSpan setNanoseconds(int nanoseconds) {
//        return new HdTimeSpan(HdTimeSpanUtils.setNanoseconds(value, nanoseconds));
//    }

    // endregion
    // endregion

    /////////////////////////////////////////////////////////////////////////////////////
    // region Total
    /////////////////////////////////////////////////////////////////////////////////////

    public double totalWeeks() {
        return HdTimeSpanUtils.totalWeeks(value);
    }

    public double totalDays() {
        return HdTimeSpanUtils.totalDays(value);
    }

    public double totalHours() {
        return HdTimeSpanUtils.totalHours(value);
    }

    public double totalMinutes() {
        return HdTimeSpanUtils.totalMinutes(value);
    }

    public double totalSeconds() {
        return HdTimeSpanUtils.totalSeconds(value);
    }

    public double totalMilliseconds() {
        return HdTimeSpanUtils.totalMilliseconds(value);
    }

    public double totalMicroseconds() {
        return HdTimeSpanUtils.totalMicroseconds(value);
    }

    public long totalNanoseconds() {
        return HdTimeSpanUtils.totalNanoseconds(value);
    }

    // endregion Total

    /////////////////////////////////////////////////////////////////////////////////////
    // region Comparison
    /////////////////////////////////////////////////////////////////////////////////////

    public int compareTo(HdTimeSpan b) {
        return HdTimeSpanUtils.compareTo(value, b.value);
    }

    public static int compare(HdTimeSpan a, HdTimeSpan b) {
        return HdTimeSpanUtils.compare(a.value, b.value);
    }

    public static boolean isLess(HdTimeSpan a, HdTimeSpan b) {
        return HdTimeSpanUtils.isLess(a.value, b.value);
    }

    public static boolean isLessOrEqual(HdTimeSpan a, HdTimeSpan b) {
        return HdTimeSpanUtils.isLessOrEqual(a.value, b.value);
    }

    public static boolean isGreater(HdTimeSpan a, HdTimeSpan b) {
        return HdTimeSpanUtils.isGreater(a.value, b.value);
    }

    public static boolean isGreaterOrEqual(HdTimeSpan a, HdTimeSpan b) {
        return HdTimeSpanUtils.isGreaterOrEqual(a.value, b.value);
    }

    public boolean isLess(HdTimeSpan b) {
        return HdTimeSpanUtils.isLess(this.value, b.value);
    }

    public boolean isLessOrEqual(HdTimeSpan b) {
        return HdTimeSpanUtils.isLessOrEqual(this.value, b.value);
    }

    public boolean isGreater(HdTimeSpan b) {
        return HdTimeSpanUtils.isGreater(this.value, b.value);
    }

    public boolean isGreaterOrEqual(HdTimeSpan b) {
        return HdTimeSpanUtils.isGreaterOrEqual(this.value, b.value);
    }


    public boolean isZero() {
        return HdTimeSpanUtils.isZero(value);
    }

    public boolean isPositive() {
        return HdTimeSpanUtils.isPositive(value);
    }

    public boolean isNegative() {
        return HdTimeSpanUtils.isNegative(value);
    }

    // endregion Comparison

    /////////////////////////////////////////////////////////////////////////////////////
    // region Arithmetic
    /////////////////////////////////////////////////////////////////////////////////////

    /**
     * Round down to the specified resolution by truncating towards 0
     * @param resolution time resolution as a *positive* TimeSpan
     * @return new instance of HdTimeSpan
     */
    public HdTimeSpan roundTo(HdTimeSpan resolution) {
        return new HdTimeSpan(HdTimeSpanUtils.roundTo(value, resolution.value));
    }

    /**
     * Round down to the specified resolution by truncating towards 0
     * @param resolution
     * @return new instance of HdTimeSpan
     */
    public HdTimeSpan roundTo(Resolution resolution) {
        /*
         * TODO: This looks misleading. Rounding for HdDateTime and rounding for HdTimeSpan should behave differently
         * and this is not obvious to user
         */
        return new HdTimeSpan(HdTimeSpanUtils.roundTo(value, resolution));
    }
    
    public HdTimeSpan add(HdTimeSpan other) {
        return new HdTimeSpan(HdTimeSpanUtils.add(value, other.value));
    }

    public HdTimeSpan addDays(long days) {
        return new HdTimeSpan(HdTimeSpanUtils.addDays(value, days));
    }

    public HdTimeSpan addHours(long hours) {
        return new HdTimeSpan(HdTimeSpanUtils.addHours(value, hours));
    }

    public HdTimeSpan addMinutes(long minutes) {
        return new HdTimeSpan(HdTimeSpanUtils.addMinutes(value, minutes));
    }

    public HdTimeSpan addSeconds(long seconds) {
        return new HdTimeSpan(HdTimeSpanUtils.addSeconds(value, seconds));
    }

    public HdTimeSpan addMilliseconds(long milliseconds) {
        return new HdTimeSpan(HdTimeSpanUtils.addMilliseconds(value, milliseconds));
    }

    public HdTimeSpan addMicroseconds(long microseconds) {
        return new HdTimeSpan(HdTimeSpanUtils.addMicroseconds(value, microseconds));
    }

    public HdTimeSpan addNanoseconds(long nanoseconds) {
        return new HdTimeSpan(HdTimeSpanUtils.addNanoseconds(value, nanoseconds));
    }

    // NOTE: Under ValueType agent this method won't even throw exception if this == null
    @ValueType(impl="addUnchecked")
    public HdTimeSpan addUnchecked(HdTimeSpan other) {
        return new HdTimeSpan(HdTimeSpanUtils.addUnchecked(value, other.value));
    }

    public HdTimeSpan subtract(HdTimeSpan other) {
        return new HdTimeSpan(HdTimeSpanUtils.subtract(value, other.value));
    }

    // NOTE: Under ValueType agent this method won't even throw exception if this == null
    @ValueType(impl="subtractUnchecked")
    public HdTimeSpan subtractUnchecked(HdTimeSpan other) {
        return new HdTimeSpan(HdTimeSpanUtils.subtractUnchecked(value, other.value));
    }

    public HdTimeSpan negate() {
        return new HdTimeSpan(HdTimeSpanUtils.negate(value));
    }

    public HdTimeSpan duration() {
        return new HdTimeSpan(HdTimeSpanUtils.duration(value));
    }

    // endregion Arithmetic

    /////////////////////////////////////////////////////////////////////////////////////
    // region Object interface
    /////////////////////////////////////////////////////////////////////////////////////

    @Override
    public int hashCode() {
        return HdTimeSpanUtils.hashCode(value);
    }

    @Override
    public boolean equals(Object other) {
        return this == other || other instanceof HdTimeSpan && HdTimeSpanUtils.equals(value, ((HdTimeSpan)other).value);
    }

    public boolean equals(HdTimeSpan other) {
        return this == other || other != null && HdTimeSpanUtils.equals(value, other.value);
    }

    public static boolean equals(HdTimeSpan a, HdTimeSpan b) {
        return a == b || a != null && b != null && HdTimeSpanUtils.equals(a.value, b.value);
    }

    // endregion Object interface

    /////////////////////////////////////////////////////////////////////////////////////
    // region Parsing and formatting
    /////////////////////////////////////////////////////////////////////////////////////

    @Override
    public String toString() {
        return HdTimeSpanUtils.toString(value);
    }

    public String toString(final String format) {
        return HdTimeSpanUtils.toString(value, format);
    }

    public Appendable appendTo(final Appendable appendable) throws IOException {
        return HdTimeSpanUtils.appendTo(value, appendable);
    }

    public Appendable appendTo(final Appendable appendable, final String format) throws IOException {
        return HdTimeSpanUtils.appendTo(value, appendable, format);
    }

    public static HdTimeSpan parse(final CharSequence text) throws ParseException {
        return new HdTimeSpan(HdTimeSpanUtils.parse(text));
    }

    public static HdTimeSpan parse(final CharSequence text, final String fmt) throws ParseException {
        return new HdTimeSpan(HdTimeSpanUtils.parse(text, fmt));
    }

    // endregion Parsing and formatting
}
