package com.epam.deltix.hdtime;

import java.io.IOException;
import java.time.DayOfWeek;
import java.time.Month;

/**
 * Date and time representation with nanoseconds resolution without time zones.
 *
 * This class is immutable.
 * Can be instantiated only through static constructors.
 */
public class HdDateTime implements Comparable<HdDateTime> {
    /////////////////////////////////////////////////////////////////////////////////////
    // region Constants
    /////////////////////////////////////////////////////////////////////////////////////

    public static final String DEFAULT_FORMAT = HdDateTimeUtils.DEFAULT_FORMAT;

    // NULL reference constant, use instead of normal Java null to initialize references to instances of HdDateTime
    public static final HdDateTime NULL = null;

    // NULL _underlying_ value. Can only be passed to fromUnderlying, yielding NULL
    public static final long NULL_VALUE = HdDateTimeUtils.NULL_VALUE;

    public static final HdDateTime MIN_VALUE = new HdDateTime(HdDateTimeUtils.MIN_VALUE);
    public static final HdDateTime MAX_VALUE = new HdDateTime(HdDateTimeUtils.MAX_VALUE);
    // endregion

    // Number of nanoseconds since 1970-01-01 00:00:00.000000000 UTC.
    final long value;

    /////////////////////////////////////////////////////////////////////////////////////
    // region Constructors
    /////////////////////////////////////////////////////////////////////////////////////

    // The main constructor is package-private
    HdDateTime(long value) {
        this.value = value;
    }

    /**
     * Construct HdDateTime from underlying long value. The value is assumed to be valid.
     * Does not perform full range validation. Use fromEpochNanoseconds if validation is needed.
     * @param value underlying representation of HdDateTime
     * @return new instance of HdDateTime or null, if NULL_VALUE is passed
     */
    public static HdDateTime fromUnderlying(long value) {
        return HdDateTimeUtils.isNull(value) ? null : new HdDateTime(value);
    }

    /**
     * Get underlying long value
     * @param dateTime instance of HdDateTime. May be null.
     * @return long underlying value. NULL_VALUE is returned if null is passed as argument.
     */
    public static long toUnderlying(HdDateTime dateTime) {
        return null == dateTime ? HdDateTimeUtils.NULL_VALUE : dateTime.value;
    }

    /**
     * Construct HdDateTime from year, month and day
     * @param year
     * @param month
     * @param day
     * @return new HdDateTime instance
     */
    public static HdDateTime newInstance(int year, Month month, int day) {
        return new HdDateTime(HdDateTimeUtils.newInstance(year, month, day));
    }

    public static HdDateTime newInstance(int year, Month month, int day, int hour, int minute, int second) {
        return new HdDateTime(HdDateTimeUtils.newInstance(year, month, day, hour, minute, second));
    }

    public static HdDateTime newInstance(int year, Month month, int day, int hour, int minute, int second, int nanosecond) {
        return new HdDateTime(HdDateTimeUtils.newInstance(year, month, day, hour, minute, second, nanosecond));
    }

    public static HdDateTime now() {
        return new HdDateTime(HdDateTimeUtils.now());
    }

    public static HdDateTime today() {
        return new HdDateTime(HdDateTimeUtils.today());
    }

    /**
     * Construct from the amount of milliseconds since start of the Unix epoch
     * @param value
     * @return
     */
    public static HdDateTime fromEpochMilliseconds(long value) {
        return NULL_VALUE == value ? null : new HdDateTime(HdDateTimeUtils.fromEpochMilliseconds(value));
    }

    /**
     * Construct from the amount of nanoseconds since start of the Unix epoch.
     * Almost the same as fromUnderlying but performs additional range validation.
     * @param value
     * @return
     */
    public static HdDateTime fromEpochNanoseconds(long value) {
        return NULL_VALUE == value ? null : new HdDateTime(HdDateTimeUtils.fromEpochNanoseconds(value));
    }

    // endregion Constructors

    /////////////////////////////////////////////////////////////////////////////////////
    // region Conversion
    /////////////////////////////////////////////////////////////////////////////////////

    /**
     * Get milliseconds since Unix Epoch
     * @return milliseconds singe 1970-1-1 00:00:00 as long
     */
    public long toEpochMilliseconds() {
        return HdDateTimeUtils.toEpochMilliseconds(value);
    }

    /**
     * Get nanoseconds since Unix Epoch
     * @return nanoseconds singe 1970-1-1 00:00:00 as long
     */
    public long toEpochNanoseconds() {
        return HdDateTimeUtils.toEpochNanoseconds(value);
    }

    // endregion Conversion

    /////////////////////////////////////////////////////////////////////////////////////
    // region Components
    /////////////////////////////////////////////////////////////////////////////////////
    // region Component Getters
    /////////////////////////////////////////////////////////////////////////////////////

    /**
     * Get date part of HdDateTime, with Time of Day part removed.
     * @return new instance of HdDateTime.
     */
    public HdDateTime getDate() {
        return new HdDateTime(HdDateTimeUtils.getDate(value));
    }

    /**
     * Get Time of Day part of HdDateTime, with Date part removed, as HdTimeSpan.
     * @return new HdTimeSpan representing time interval since beginning of the day.
     */
    public HdTimeSpan getTimeOfDay() {
        return new HdTimeSpan(HdDateTimeUtils.getTimeOfDay(value));
    }

    public int getYear() {
        return HdDateTimeUtils.getYear(value);
    }

    public Month getMonth() {
        return HdDateTimeUtils.getMonth(value);
    }

    public int getWeekOfYear() {
        return HdDateTimeUtils.getWeekOfYear(value);
    }

    public int getWeekOfMonth() {
        return HdDateTimeUtils.getWeekOfMonth(value);
    }

    public int getDayOfYear() {
        return HdDateTimeUtils.getDayOfYear(value);
    }

    public int getDayOfMonth() {
        return HdDateTimeUtils.getDayOfMonth(value);
    }

    public DayOfWeek getDayOfWeek() {
        return HdDateTimeUtils.getDayOfWeek(value);
    }

    public int getHour() {
        return HdDateTimeUtils.getHour(value);
    }

    public int getMinute() {
        return HdDateTimeUtils.getMinute(value);
    }

    public int getSecond() {
        return HdDateTimeUtils.getSecond(value);
    }

    /**
     * Get fractional part (below second) of HdDateTime as Milliseconds
     * @return milliseconds, [0..999] unsigned int
     */
    public int getMillisecond() {
        return HdDateTimeUtils.getMillisecond(value);
    }

    /**
     * Get fractional part (below second) of HdDateTime as Microseconds
     * @return microseconds, [0..999999] unsigned int
     */
    public int getMicrosecond() {
        return HdDateTimeUtils.getMicrosecond(value);
    }

    /**
     * Get fractional part (below second) of HdDateTime as Nanoseconds
     * @return Nanoseconds, [0..999999999] unsigned int
     */
    public int getNanosecond() {
        return HdDateTimeUtils.getNanosecond(value);
    }

    // endregion Component Getters

    // region Component Setters

    /**
     * Copy date part from another HdDateTime value, leaving Time of Day part unchanged
     * @param date HdDateTime value to copy Date from
     * @return new HdDateTime instance
     */
    public HdDateTime setDate(HdDateTime date) {
        return new HdDateTime(HdDateTimeUtils.setDate(value, date.value));
    }

    /**
     * Set Tinme of day component from the specified HdTimeSpan value, leaving Date part unchanged
     * @param time HdTimeSpan that will replace time of day.
     * @return new HdDateTime instance
     */
    public HdDateTime setTime(HdTimeSpan time) {
        return new HdDateTime(HdDateTimeUtils.setTime(value, time.value));
    }

    public HdDateTime setYear(int year) {
        return new HdDateTime(HdDateTimeUtils.setYear(value, year));
    }

    public HdDateTime setWeekOfYear(int week) {
        return new HdDateTime(HdDateTimeUtils.setWeekOfYear(value, week));
    }

    public HdDateTime setWeekOfMonth(int week) {
        return new HdDateTime(HdDateTimeUtils.setWeekOfMonth(value, week));
    }

    public HdDateTime setMonth(Month month) {
        return new HdDateTime(HdDateTimeUtils.setMonth(value, month));
    }

    public HdDateTime setDayOfYear(int day) {
        return new HdDateTime(HdDateTimeUtils.setDayOfYear(value, day));
    }

    public HdDateTime setDayOfMonth(int day) {
        return new HdDateTime(HdDateTimeUtils.setDayOfMonth(value, day));
    }

    public HdDateTime setDayOfWeek(DayOfWeek dayOfWeek) {
        return new HdDateTime(HdDateTimeUtils.setDayOfWeek(value, dayOfWeek));
    }

    /**
     * Set hours (below day) component of the HdDateTime value
     * @param hours amount of hours [0..23]
     * This is slower than addHours, use addHours if the previous value of hour component was 0
     * @return new HdDateTime instance
     */
    public HdDateTime setHour(int hours) {
        return new HdDateTime(HdDateTimeUtils.setHour(value, hours));
    }

    /**
     * Set minutes (below hour) component of the HdDateTime value
     * This is slower than addMinutes, use addMinutes if the previous value of minute component was 0
     * @param minutes amount of minutes [0..59]
     * @return new HdDateTime instance
     */
    public HdDateTime setMinute(int minutes) {
        return new HdDateTime(HdDateTimeUtils.setMinute(value, minutes));
    }

    /**
     * Set seconds (below minute) component of the HdDateTime value
     * This is slower than addSeconds, use addSeconds if the previous value of second component was 0
     * @param seconds amount of seconds [0..59]
     * @return new HdDateTime instance
     */
    public HdDateTime setSecond(int seconds) {
        return new HdDateTime(HdDateTimeUtils.setSecond(value, seconds));
    }

//    /**
//     * Set fractional part (below second) of the HdDateTime value
//     * This is slower than addMilliseconds, use addMilliseconds if the previous value of millisecond component was 0
//     * @param milliseconds amount of milliseconds [0..999]
//     * @return new HdDateTime instance
//     */
//    public HdDateTime setMilliseconds(int milliseconds) {
//        return new HdDateTime(HdDateTimeUtils.setMilliseconds(value, milliseconds));
//    }
//
//    /**
//     * Set fractional part (below second) of the HdDateTime value
//     * This is slower than addMicroseconds, use addMicroseconds if the previous value of microsecond component was 0
//     * @param microseconds amount of microseconds [0..999999]
//     * @return new HdDateTime instance
//     */
//    public HdDateTime setMicroseconds(int microseconds) {
//        return new HdDateTime(HdDateTimeUtils.setMicroseconds(value, microseconds));
//    }

    /**
     * Set fractional part (below second) of the HdDateTime value
     * This is slower than addNanoseconds, use addNanoseconds if the previous value of nanosecond component was 0
     * @param nanoseconds amount of nanoseconds [0..999999999]
     * @return new HdDateTime instance
     */
    public HdDateTime setNanosecond(int nanoseconds) {
        return new HdDateTime(HdDateTimeUtils.setNanosecond(value, nanoseconds));
    }

    // endregion Component Setters
    // endregion Components

    /////////////////////////////////////////////////////////////////////////////////////
    // region Comparison
    /////////////////////////////////////////////////////////////////////////////////////

    public int compareTo(HdDateTime b) {
        return HdDateTimeUtils.compareTo(value, b.value);
    }

    public static int compare(HdDateTime a, HdDateTime b) {
        return HdDateTimeUtils.compare(a.value, b.value);
    }

    // endregion Comparison

    /////////////////////////////////////////////////////////////////////////////////////
    // region Arithmetic and rounding
    /////////////////////////////////////////////////////////////////////////////////////

    /**
     * Round down to the specified resolution by truncating towards MIN_VALUE
     * @param resolution time resolution as a *positive* TimeSpan
     * @return new instance of HdDateTime
     */
    public HdDateTime roundTo(HdTimeSpan resolution) {
        return new HdDateTime(HdDateTimeUtils.roundTo(value, resolution.value));
    }

    /**
     * Round down to the specified resolution by truncating towards MIN_VALUE
     * @param resolution
     * @return
     */
    public HdDateTime roundTo(Resolution resolution) {
        return new HdDateTime(HdDateTimeUtils.roundTo(value, resolution));
    }

    public HdDateTime add(HdTimeSpan timeSpan) {
        return new HdDateTime(HdDateTimeUtils.add(value, timeSpan.value));
    }

    // NOTE: Under ValueType agent this method won't even throw exception if this == null
    @ValueType(impl="addUnchecked")
    public HdDateTime addUnchecked(HdTimeSpan timeSpan) {
        return new HdDateTime(HdDateTimeUtils.addUnchecked(value, timeSpan.value));
    }

    public HdDateTime addYears(int years) {
        return new HdDateTime(HdDateTimeUtils.addYears(value, years));
    }

    public HdDateTime addMonths(int months) {
        return new HdDateTime(HdDateTimeUtils.addMonths(value, months));
    }

    public HdDateTime addDays(long days) {
        return new HdDateTime(HdDateTimeUtils.addDays(value, days));
    }

    public HdDateTime addHours(long hours) {
        return new HdDateTime(HdDateTimeUtils.addHours(value, hours));
    }

    public HdDateTime addMinutes(long minutes) {
        return new HdDateTime(HdDateTimeUtils.addMinutes(value, minutes));
    }

    public HdDateTime addSeconds(long seconds) {
        return new HdDateTime(HdDateTimeUtils.addSeconds(value, seconds));
    }

    public HdDateTime addMilliseconds(long milliseconds) {
        return new HdDateTime(HdDateTimeUtils.addMilliseconds(value, milliseconds));
    }

    public HdDateTime addMicroseconds(long microseconds) {
        return new HdDateTime(HdDateTimeUtils.addMicroseconds(value, microseconds));
    }

    public HdDateTime addNanoseconds(long nanoseconds) {
        return new HdDateTime(HdDateTimeUtils.addNanoseconds(value, nanoseconds));
    }

    @ValueType(impl="subtractDateTimeChecked")
    public HdTimeSpan subtract(HdDateTime other) {
        return new HdTimeSpan(HdDateTimeUtils.subtractDateTime(value, other.value));
    }

    @ValueType(impl="subtractTimeSpanChecked")
    public HdDateTime subtract(HdTimeSpan other) {
        return new HdDateTime(HdDateTimeUtils.subtractTimeSpan(value, other.value));
    }

    // NOTE: Under ValueType agent this method won't even throw exception if this == null
    @ValueType(impl="subtractUnchecked")
    public HdTimeSpan subtractUnchecked(HdDateTime dateTime) {
        return new HdTimeSpan(HdDateTimeUtils.subtractUnchecked(value, dateTime.value));
    }

    // NOTE: Under ValueType agent this method won't even throw exception if this == null
    @ValueType(impl="subtractUnchecked")
    public HdDateTime subtractUnchecked(HdTimeSpan timeSpan) {
        return new HdDateTime(HdDateTimeUtils.subtractUnchecked(value, timeSpan.value));
    }

    // endregion Arithmetic

    /////////////////////////////////////////////////////////////////////////////////////
    // region Helpers
    /////////////////////////////////////////////////////////////////////////////////////

    public int getDaysInMonth() {
        return HdDateTimeUtils.getDaysInMonth(value);
    }

    public int getDaysInYear() {
        return HdDateTimeUtils.getDaysInYear(value);
    }

    public boolean isLeapYear() {
        return HdDateTimeUtils.isLeapYear(value);
    }

//    public static boolean isLeapYear(int year) {
//        return HdDateTimeUtils.isLeapYear(year);
//    }

    // endregion Helpers

    /////////////////////////////////////////////////////////////////////////////////////
    // region Object interface
    /////////////////////////////////////////////////////////////////////////////////////

    @Override
    public int hashCode() {
        return HdDateTimeUtils.hashCode(value);
    }

    @Override
    public boolean equals(Object other) {
        return this == other || other instanceof HdDateTime && HdDateTimeUtils.equals(value, ((HdDateTime)other).value);
    }

    public boolean equals(HdDateTime other) {
        return this == other || other != null && HdDateTimeUtils.equals(value, other.value);
    }

    public static boolean equals(HdDateTime a, HdDateTime b) {
        return a == b || a != null && b != null && HdDateTimeUtils.equals(a.value, b.value);
    }

    // endregion Object interface

    /////////////////////////////////////////////////////////////////////////////////////
    // region Parsing and formatting
    /////////////////////////////////////////////////////////////////////////////////////

    @Override
    public String toString() {
        return HdDateTimeUtils.toString(value);
    }

    public String toString(String format) {
        return HdDateTimeUtils.toString(value, format);
    }

    public Appendable appendTo(final Appendable appendable) throws IOException {
        return HdDateTimeUtils.appendTo(value, appendable);
    }

    public Appendable appendTo(final Appendable appendable, String format) throws IOException {
        return HdDateTimeUtils.appendTo(value, appendable, format);
    }

    public static HdDateTime parse(final CharSequence text) throws ParseException {
        return new HdDateTime(HdDateTimeUtils.parse(text));
    }

    public static HdDateTime parse(final CharSequence text, final String fmt) throws ParseException {
        return new HdDateTime(HdDateTimeUtils.parse(text, fmt));
    }

    // endregion Parsing and formatting
}


