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

import java.time.DayOfWeek;
import java.time.Month;
import java.util.Calendar;
import java.util.GregorianCalendar;
import java.util.Locale;
import java.util.TimeZone;

import static com.epam.deltix.hdtime.Util.inRange;

final class Convert {
    // Null value for both DateTime and TimeSpan.
    // This value is considered invalid and the methods here are not expected to behave
    // correctly if it is passed as an argument to a method taking int64 representation of
    // DateTime or TimeSpan.
    // It only exists to support ValueTypes and, possibly, encoding of "undefined"/"null"
    // in the user's code, in which case the user is responsible for the checks.
    static final long NULL = Long.MIN_VALUE;

    /////////////////////////////////////////////////////////////////////////////////////
    // region Constants
    /////////////////////////////////////////////////////////////////////////////////////
    static final long DAYS_IN_WEEK = 7;

    static final long HOURS_IN_DAY = 24;
    static final long HOURS_IN_WEEK = HOURS_IN_DAY * DAYS_IN_WEEK;

    static final long MINUTES_IN_HOUR = 60;
    static final long MINUTES_IN_DAY = MINUTES_IN_HOUR * HOURS_IN_DAY;
    static final long MINUTES_IN_WEEK = MINUTES_IN_HOUR * HOURS_IN_WEEK;

    static final long SECONDS_IN_MINUTE = 60;
    static final long SECONDS_IN_HOUR = SECONDS_IN_MINUTE * MINUTES_IN_HOUR;
    static final long SECONDS_IN_DAY = SECONDS_IN_MINUTE * MINUTES_IN_DAY;
    static final long SECONDS_IN_WEEK = SECONDS_IN_MINUTE * MINUTES_IN_WEEK;

    static final long MILLISECONDS_IN_SECOND = 1000L;
    static final long MILLISECONDS_IN_MINUTE = MILLISECONDS_IN_SECOND * SECONDS_IN_MINUTE;
    static final long MILLISECONDS_IN_HOUR = MILLISECONDS_IN_SECOND * SECONDS_IN_HOUR;
    static final long MILLISECONDS_IN_DAY = MILLISECONDS_IN_SECOND * SECONDS_IN_DAY;
    static final long MILLISECONDS_IN_WEEK = MILLISECONDS_IN_SECOND * SECONDS_IN_WEEK;

    static final long MICROSECONDS_IN_MILLISECOND = 1000L;
    static final long MICROSECONDS_IN_SECOND = MICROSECONDS_IN_MILLISECOND * MILLISECONDS_IN_SECOND;
    static final long MICROSECONDS_IN_MINUTE = MICROSECONDS_IN_MILLISECOND * MILLISECONDS_IN_MINUTE;
    static final long MICROSECONDS_IN_HOUR = MICROSECONDS_IN_MILLISECOND * MILLISECONDS_IN_HOUR;
    static final long MICROSECONDS_IN_DAY = MICROSECONDS_IN_MILLISECOND * MILLISECONDS_IN_DAY;
    static final long MICROSECONDS_IN_WEEK = MICROSECONDS_IN_MILLISECOND * MILLISECONDS_IN_WEEK;

    static final long NS_IN_MICROSECOND = 1000L;
    static final long NS_IN_MILLISECOND = NS_IN_MICROSECOND * MICROSECONDS_IN_MILLISECOND;
    static final long NS_IN_SECOND = NS_IN_MICROSECOND * MICROSECONDS_IN_SECOND;
    static final long NS_IN_MINUTE = NS_IN_MICROSECOND * MICROSECONDS_IN_MINUTE;
    static final long NS_IN_HOUR = NS_IN_MICROSECOND * MICROSECONDS_IN_HOUR;
    static final long NS_IN_DAY = NS_IN_MICROSECOND * MICROSECONDS_IN_DAY;
    static final long NS_IN_WEEK = NS_IN_MICROSECOND * MICROSECONDS_IN_WEEK;

    private static final TimeZone timeZoneUtc = TimeZone.getTimeZone("UTC");

    // endregion

    static class DateTime {
        static final long MIN = HdDateTimeUtils.MIN;        // 1678-01-01 00:00:00.000000000
        static final long MAX = HdDateTimeUtils.MAX;        // 2261-12-31 23:59:59.999999999

        private static final long MIN_TICKS = MIN / 100;
        private static final long MAX_TICKS = MAX / 100;
        private static final long MIN_MICROS = MIN / NS_IN_MICROSECOND;
        private static final long MAX_MICROS = MAX / NS_IN_MICROSECOND;
        private static final long MIN_MILLIS = MIN / NS_IN_MILLISECOND;
        private static final long MAX_MILLIS = MAX / NS_IN_MILLISECOND;
        private static final long MIN_SECONDS = MIN / NS_IN_SECOND;
        private static final long MAX_SECONDS = MAX / NS_IN_SECOND;
        private static final long MIN_MINUTES = MIN / NS_IN_MINUTE;
        private static final long MAX_MINUTES = MAX / NS_IN_MINUTE;
        private static final long MIN_HOURS = MIN / NS_IN_HOUR;
        private static final long MAX_HOURS = MAX / NS_IN_HOUR;
        private static final long MIN_DAYS = MIN / NS_IN_DAY;
        private static final long MAX_DAYS = MAX / NS_IN_DAY;
        private static final long MIN_YEAR = 1678;
        private static final long MAX_YEAR = 2261;

        // Converts Calendar enum to java.time.DayOfWeek simultaneously with range check
        static final DayOfWeek[] toDayOfWeek = {
                DayOfWeek.SUNDAY, DayOfWeek.MONDAY, DayOfWeek.TUESDAY, DayOfWeek.WEDNESDAY,
                DayOfWeek.THURSDAY, DayOfWeek.FRIDAY, DayOfWeek.SATURDAY
        };

        // Converts java.time.DayOfWeek enum value to Calendar enum
        static final int[] fromDayOfWeek = {
                Calendar.MONDAY, Calendar.TUESDAY, Calendar.WEDNESDAY,
                Calendar.THURSDAY, Calendar.FRIDAY, Calendar.SATURDAY, Calendar.SUNDAY
        };

        /////////////////////////////////////////////////////////////////////////////////////
        // region Utility/helper methods
        /////////////////////////////////////////////////////////////////////////////////////

        static long div(long x, long to) {
            long sign = x >> 63;
            return (x - sign) / to + sign;
        }

        static long mod(long x, long to) {
            long sign = x >> 63;
            return x - (x - sign) / to * to + (sign & to);
        }

        static long modDiv(long x, long divider1, long divider2) {
            return mod(x, divider1) / divider2;
        }

        private static GregorianCalendar zeroCalendar() {
            GregorianCalendar calendar = new GregorianCalendar(TimeZone.getTimeZone("UTC"), Locale.getDefault(Locale.Category.FORMAT));
            calendar.setTimeInMillis(0);
            return calendar;
        }

        private final static ThreadLocal<GregorianCalendar> tlsDateCalendar = new ThreadLocal<GregorianCalendar>() {
            @Override
            protected GregorianCalendar initialValue() {
                return zeroCalendar();
            }
        };

        private final static ThreadLocal<GregorianCalendar> tlsYearCalendar = new ThreadLocal<GregorianCalendar>() {
            @Override
            protected GregorianCalendar initialValue() {
                return zeroCalendar();
            }
        };

        private final static ThreadLocal<GregorianCalendar> tlsTempCalendar = new ThreadLocal<GregorianCalendar>() {
            @Override
            protected GregorianCalendar initialValue() {
                return zeroCalendar();
            }
        };

        static GregorianCalendar getTempDateCalendar() {
            return tlsDateCalendar.get();
        }

        static GregorianCalendar getTempCalendar() {
            return tlsTempCalendar.get();
        }

        public static GregorianCalendar getTempYearCalendar() {
            return tlsYearCalendar.get();
        }

        // endregion Utility/helper methods

        /////////////////////////////////////////////////////////////////////////////////////
        // region Range checks
        /////////////////////////////////////////////////////////////////////////////////////

        static boolean isValidNanos(long x) {
            return inRange(x, MIN, MAX);
        }

        // Returns true if given finite Ticks value fits into HdDateTime/HdTimeSpan
        static boolean isValidTicks(long x) {
            return inRange(x, MIN_TICKS, MAX_TICKS);
        }

        static boolean isValidMicros(long x) {
            return inRange(x, MIN_MICROS, MAX_MICROS);
        }

        static boolean isValidMillis(long x) {
            return inRange(x, MIN_MILLIS, MAX_MILLIS);
        }

        static boolean isValidSeconds(long x) {
            return inRange(x, MIN_SECONDS, MAX_SECONDS);
        }

        static boolean isValidMinutes(long x) {
            return inRange(x, MIN_MINUTES, MAX_MINUTES);
        }

        static boolean isValidHours(long x) {
            return inRange(x, MIN_HOURS, MAX_HOURS);
        }

        static boolean isValidDays(int x) {
            return inRange(x, MIN_DAYS, MAX_DAYS);
        }

        static boolean isValidTimeOfDay(long x) {
            return inRange(x, 0, NS_IN_DAY - 1);
        }

        static boolean isValidNanosComponent(int x) {
            return inRange(x, 0, 999_999_999);
        }

        static boolean isValidMicrosComponent(int x) {
            return inRange(x, 0, 999_999);
        }

        static boolean isValidMillisComponent(int x) {
            return inRange(x, 0, 999);
        }

        static boolean isValidSecondComponent(int x) {
            return inRange(x, 0 , 59);
        }

        static boolean isValidMinuteComponent(int x) {
            return inRange(x, 0, 59);
        }

        static boolean isValidHourComponent(int x) {
            return inRange(x, 0, 23);
        }

        //    static boolean isValidDayComponent(int x) {
//        throw new UnsupportedOperationException();
//        //return inRange(x, MIN_DAYS, MAX_DAYS);
//    }
//
//    static boolean isValidMonthsComponent(int x) {
//        throw new UnsupportedOperationException();
//        //return inRange(x, MIN_DAYS, MAX_DAYS);
//    }
        
        static boolean isValidYear(int x) {
            return inRange(x, MIN_YEAR, MAX_YEAR);
        }

        static long checkNanos(long nanoseconds) {
            if (!isValidNanos(nanoseconds))
                throwNanosOutOfRange();

            return nanoseconds;
        }

        static long checkTicks(long ticks) {
            if (!isValidTicks(ticks))
                throwTicksOutOfRange();

            return ticks;
        }

        static long checkMillis(long milliseconds) {
            if (!isValidMillis(milliseconds))
                throwMillisOutOfRange();

            return milliseconds;
        }

        static long checkMicros(long microseconds) {
            if (!isValidMicros(microseconds))
                throwMicrosOutOfRange();

            return microseconds;
        }

        static long checkSeconds(long seconds) {
            if (!isValidSeconds(seconds))
                throwSecondsOutOfRange();

            return seconds;
        }

        static long checkMinutes(long minutes) {
            if (!isValidMinutes(minutes))
                throwMinutesOutOfRange();

            return minutes;
        }

        static long checkHours(long hours) {
            if (!isValidHours(hours))
                throwHoursOutOfRange();

            return hours;
        }

        static long checkDays(int days) {
            if (!isValidDays(days))
                throwDaysOutOfRange();

            return days;
        }

        static long checkTimeOfDay(long nanos) {
            if (!isValidTimeOfDay(nanos))
                throwTimeOfDayOutOfRange();

            return nanos;
        }

        static int checkNanosComponent(int nanos) {
            if (!isValidNanosComponent(nanos))
                throwNanosOutOfRange();

            return nanos;
        }

        static long checkMicrosComponent(int micros) {
            if (!isValidMicrosComponent(micros))
                throwMicrosOutOfRange();

            return micros;
        }

        static long checkMillisComponent(int millis) {
            if (!isValidMillisComponent(millis))
                throwMillisOutOfRange();

            return millis;
        }

        static long checkSecondComponent(int seconds) {
            if (!isValidSecondComponent(seconds))
                throwSecondsOutOfRange();

            return seconds;
        }

        static int checkMinuteComponent(int minutes) {
            if (!isValidMinuteComponent(minutes))
                throwMinutesOutOfRange();

            return minutes;
        }

        static int checkHourComponent(int hours) {
            if (!isValidHourComponent(hours))
                throwHoursOutOfRange();

            return hours;
        }

        static int checkYearComponent(int year) {
            if (!isValidYear(year))
                throwYearsOutOfRange();

            return year;
        }
        
        // endregion

        /////////////////////////////////////////////////////////////////////////////////////
        // region Construct HdDateTime
        /////////////////////////////////////////////////////////////////////////////////////

        private static GregorianCalendar calendarFromDate(int year, int month, int day) {
            GregorianCalendar calendar = getTempDateCalendar();
            calendar.clear(); // TODO: Slow
            calendar.set(year, month - 1, day);
            return calendar;
        }

        static long from(int year, int month, int day) {
            return checkMillis(calendarFromDate(year, month, day).getTimeInMillis()) * NS_IN_MILLISECOND;
        }

        static long from(int year, int month, int day, int hour, int minute, int second) {
            return from(year, month, day) + checkHourComponent(hour) * NS_IN_HOUR
                    + checkMinuteComponent(minute) * NS_IN_MINUTE + checkSecondComponent(second) * NS_IN_SECOND;
        }

        static long from(int year, int month, int day, int hour, int minute, int second, int nanosecond) {
            return from(year, month, day, hour, minute, second) + checkNanosComponent(nanosecond);
        }

        static long from(int year, Month month, int day) {
            return from(year, month.getValue(), day);
        }

        static long from(int year, Month month, int day, int hour, int minute, int second) {
            return from(year, month.getValue(), day, hour, minute, second);
        }

        static long from(int year, Month month, int day, int hour, int minute, int second, int nanosecond) {
            return from(year, month, day, hour, minute, second) + checkNanosComponent(nanosecond);
        }

        static long fromNanoseconds(long value) {
            return checkNanos(value);
        }

        static long fromMilliseconds(long value) {
            return checkMillis(value) * NS_IN_MILLISECOND;
        }

        // endregion Construct HdDateTime

        /////////////////////////////////////////////////////////////////////////////////////
        // region Components
        /////////////////////////////////////////////////////////////////////////////////////
        // region Extract Components
        /////////////////////////////////////////////////////////////////////////////////////

        private static GregorianCalendar calendarFromMillis(long millis) {
            GregorianCalendar calendar = getTempCalendar();
            calendar.setTimeInMillis(millis);
            return calendar;
        }

        static GregorianCalendar calendarFromNanos(long dt) {
            return calendarFromMillis(div(dt, NS_IN_MILLISECOND));
        }


        private static int getCalendarComponent(long dt, int field) {
            return calendarFromNanos(dt).get(field);
        }

        static long extractTimeOfDay(long dt) {
            return mod(dt, NS_IN_DAY);
        }

        static int extractYear(long value) {
            return getCalendarComponent(value, Calendar.YEAR);
        }

        static Month extractMonthOfYear(long value) {
            return Month.of(getCalendarComponent(value, Calendar.MONTH) + 1);
        }

        static int extractWeekOfYear(long value) {
            return getCalendarComponent(value, Calendar.WEEK_OF_YEAR);
        }

        static int extractWeekOfMonth(long value) {
            return getCalendarComponent(value, Calendar.WEEK_OF_MONTH);
        }

        static int extractDayOfYear(long value) {
            return getCalendarComponent(value, Calendar.DAY_OF_YEAR);
        }

        static int extractDayOfMonth(long value) {
            return getCalendarComponent(value, Calendar.DAY_OF_MONTH);
        }

        static DayOfWeek extractDayOfWeek(long value) {
            return toDayOfWeek[getCalendarComponent(value, Calendar.DAY_OF_WEEK) - 1];
        }

        static int extractHourOfDay(long value) {
            return (int)modDiv(value, NS_IN_DAY, NS_IN_HOUR);
        }

        static int extractMinuteOfHour(long value) {
            return (int)modDiv(value, NS_IN_HOUR, NS_IN_MINUTE);
        }

        static int extractSecondOfMinute(long value) {
            return (int)modDiv(value, NS_IN_MINUTE, NS_IN_SECOND);
        }

        static int extractMillisecondOfSecond(long value) {
            return (int)modDiv(value, NS_IN_SECOND, NS_IN_MILLISECOND);
        }

        static int extractMicrosecondOfSecond(long value) {
            return (int)modDiv(value, NS_IN_SECOND, NS_IN_MICROSECOND);
        }

        static int extractNanosecondOfSecond(long value) {
            return (int)mod(value, NS_IN_SECOND);
        }

        // endregion Extract Components

        /////////////////////////////////////////////////////////////////////////////////////
        // region Set/Replace Components
        /////////////////////////////////////////////////////////////////////////////////////

        private static long setComponentViaCalendar(long millis, int field, int fieldValue) {
            GregorianCalendar calendar = calendarFromMillis(millis);
            calendar.set(field, fieldValue);
            return calendar.getTimeInMillis();
        }

        private static long setComponentViaCalendar(long millis, int field, int fieldValue, long nanosRemainder) {
            GregorianCalendar calendar = calendarFromMillis(millis);
            calendar.set(field, fieldValue);
            return calendar.getTimeInMillis()  * NS_IN_MILLISECOND + nanosRemainder;
        }

        private static long addCalendarMillisComponent(long millis, int field, int fieldValue, long nanosRemainder) {
            GregorianCalendar calendar = calendarFromMillis(millis);
            calendar.add(field, fieldValue);
            return calendar.getTimeInMillis()  * NS_IN_MILLISECOND + nanosRemainder;
        }

        private static long setCalendarComponent(long dt, int field, int fieldValue) {
            long millis = div(dt, NS_IN_MILLISECOND);
            dt -= millis * NS_IN_MILLISECOND;
            assert(inRange(dt, 0, NS_IN_MILLISECOND - 1));
            return setComponentViaCalendar(millis, field, fieldValue, dt);
        }

        static long addCalendarComponent(long dt, int field, int fieldValue) {
            long millis = div(dt, NS_IN_MILLISECOND);
            dt -= millis * NS_IN_MILLISECOND;
            assert(inRange(dt, 0, NS_IN_MILLISECOND - 1));
            return addCalendarMillisComponent(millis, field, fieldValue, dt);
        }

        private static long setComponent(long dt, long component, long upperDivisor, long lowerDivisor) {
            long upper = div(dt, upperDivisor) * upperDivisor;
            long remainder = (dt - upper) % lowerDivisor;
            assert(inRange(remainder, 0, lowerDivisor - 1));
            return upper + component * lowerDivisor + remainder;
        }

        private static long setNanosComponent(long dt, long component) {
            return div(dt, NS_IN_SECOND) * NS_IN_SECOND + component;
        }

        public static long setYear(long value, int year) {
            return setCalendarComponent(value, Calendar.YEAR, checkYearComponent(year));
        }

        public static long setMonth(long value, Month month) {
            return setCalendarComponent(value, Calendar.MONTH, month.getValue() - 1);
        }

        public static long setWeekOfYear(long value, int week) {
            return setCalendarComponent(value, Calendar.WEEK_OF_YEAR, week);
        }

        public static long setWeekOfMonth(long value, int week) {
            return setCalendarComponent(value, Calendar.WEEK_OF_MONTH, week);
        }

        public static long setDayOfYear(long value, int day) {
            return setCalendarComponent(value, Calendar.DAY_OF_YEAR, day);
        }

        public static long setDayOfMonth(long value, int day) {
            return setCalendarComponent(value, Calendar.DAY_OF_MONTH, day);
        }

        public static long setDayOfWeek(long value, DayOfWeek dayOfWeek) {
            return setCalendarComponent(value, Calendar.DAY_OF_WEEK, fromDayOfWeek[dayOfWeek.getValue() - 1]);
        }

        public static long setHourOfDay(long value, int hours) {
            return setComponent(value, checkHourComponent(hours), NS_IN_DAY, NS_IN_HOUR);
        }

        public static long setMinuteOfHour(long value, int minutes) {
            return setComponent(value, checkMinuteComponent(minutes), NS_IN_HOUR, NS_IN_MINUTE);
        }

        public static long setSecondOfMinute(long value, int seconds) {
            return setComponent(value, checkSecondComponent(seconds), NS_IN_MINUTE, NS_IN_SECOND);
        }

//    public static long setMillisecondOfSecond(long value, int milliseconds) {
//        return setComponent(value, checkMillisComponent(milliseconds), NS_IN_SECOND, NS_IN_MILLISECO);
//    }
//
//    public static long setMicrosecondOfSecond(long value, int microseconds) {
//        return setComponent(value, checkMicrosComponent(microseconds), NS_IN_HOUR, NS_IN_MINUTE);
//    }

        public static long setNanosecondOfSecond(long value, int nanoseconds) {
            return setNanosComponent(value, checkNanosComponent(nanoseconds));
        }

        // endregion Set Component
        // endregion Components

        /////////////////////////////////////////////////////////////////////////////////////
        // region Arithmetic
        /////////////////////////////////////////////////////////////////////////////////////

        static long roundTo(final long dt, final long unitSizeNs) {
            return div(dt, unitSizeNs) * unitSizeNs;
        }

        public static long roundToDays(long dt) {
            return roundTo(dt, NS_IN_DAY);
        }

        public static long toMillis(long dt) {
            return div(dt, NS_IN_MILLISECOND);
        }

        public static void toComponents(long dt, Components components) {
            components.sign = 0;
            long sign = dt >> 63;
            long day = (dt - sign) / NS_IN_DAY + sign;
            dt -= day * NS_IN_DAY;
            long old = dt;
            dt /= NS_IN_MINUTE;
            long sec = old - dt * NS_IN_MINUTE;
            old = dt;
            dt /= MINUTES_IN_HOUR;
            long secOld = sec;
            sec /= NS_IN_SECOND;
            components.minute = (int)(old - dt * MINUTES_IN_HOUR);
            components.nanosecond = (int)(secOld - sec * NS_IN_SECOND);
            components.second = (int)sec;
            components.hour = (int)(dt);

            GregorianCalendar calendar = calendarFromMillis(day * MILLISECONDS_IN_DAY);
            components.day = calendar.get(Calendar.DAY_OF_MONTH);
            components.month = calendar.get(Calendar.MONTH) + 1;
            components.year = calendar.get(Calendar.YEAR);
        }

        // endregion
        
    }

    static class TimeSpan {
        /////////////////////////////////////////////////////////////////////////////////////
        // region Constants
        /////////////////////////////////////////////////////////////////////////////////////
        static final long MIN = HdTimeSpanUtils.MIN;
        static final long MAX = HdTimeSpanUtils.MAX;

        private static final long MIN_TICKS = MIN / 100;
        private static final long MAX_TICKS = MAX / 100;
        private static final long MIN_MICROS = MIN / NS_IN_MICROSECOND;
        private static final long MAX_MICROS = MAX / NS_IN_MICROSECOND;
        private static final long MIN_MILLIS = MIN / NS_IN_MILLISECOND;
        private static final long MAX_MILLIS = MAX / NS_IN_MILLISECOND;
        private static final long MIN_SECONDS = MIN / NS_IN_SECOND;
        private static final long MAX_SECONDS = MAX / NS_IN_SECOND;
        private static final long MIN_MINUTES = MIN / NS_IN_MINUTE;
        private static final long MAX_MINUTES = MAX / NS_IN_MINUTE;
        private static final long MIN_HOURS = MIN / NS_IN_HOUR;
        private static final long MAX_HOURS = MAX / NS_IN_HOUR;
        private static final long MIN_DAYS = MIN / NS_IN_DAY;
        private static final long MAX_DAYS = MAX / NS_IN_DAY;
        private static final double F_MIN = (double)MIN;
        private static final double F_MAX = (double)MAX; // TODO: Verify vs integer values
        // endregion

        /////////////////////////////////////////////////////////////////////////////////////
        // region Range checks
        /////////////////////////////////////////////////////////////////////////////////////

        static boolean isValidNanos(long x) {
            return inRange(x, MIN, MAX);
        }

        // Returns true if given finite Ticks value fits into HdDateTime/HdTimeSpan
        static boolean isValidTicks(long x) {
            return inRange(x, MIN_TICKS, MAX_TICKS);
        }

        static boolean isValidMicros(long x) {
            return inRange(x, MIN_MICROS, MAX_MICROS);
        }

        static boolean isValidMillis(long x) {
            return inRange(x, MIN_MILLIS, MAX_MILLIS);
        }

        static boolean isValidSeconds(long x) {
            return inRange(x, MIN_SECONDS, MAX_SECONDS);
        }

        static boolean isValidMinutes(long x) {
            return inRange(x, MIN_MINUTES, MAX_MINUTES);
        }

        static boolean isValidHours(long x) {
            return inRange(x, MIN_HOURS, MAX_HOURS);
        }

        static boolean isValidDays(long x) {
            return inRange(x, MIN_DAYS, MAX_DAYS);
        }

        static boolean isValidTimeOfDay(long x) {
            return inRange(x, 0, NS_IN_DAY - 1);
        }

        static boolean isValidNanosComponent(int x) {
            return inRange(x, 0, 999_999_999);
        }

//        static boolean isValidMicrosComponent(int x) {
//            return inRange(x, 0, 999_999);
//        }
//
//        static boolean isValidMillisComponent(int x) {
//            return inRange(x, 0, 999);
//        }

        static boolean isValidSecondComponent(int x) {
            return inRange(x, 0 , 59);
        }

        static boolean isValidMinuteComponent(int x) {
            return inRange(x, 0, 59);
        }

        static boolean isValidHourComponent(int x) {
            return inRange(x, 0, 23);
        }

        private static boolean isValid(double ts) {
            return ts >= F_MIN && ts < F_MAX;
        }

        static long checkTimeOfDay(long ts) {
            if (!isValidTimeOfDay(ts))
                throwTimeOfDayOutOfRange();

            return ts;
        }

        static long checkNanos(long nanoseconds) {
            if (!isValidNanos(nanoseconds))
                throwNanosOutOfRange();

            return nanoseconds;
        }

        static long checkTicks(long ticks) {
            if (!isValidTicks(ticks))
                throwTicksOutOfRange();

            return ticks;
        }

        static long checkMillis(long milliseconds) {
            if (!isValidMillis(milliseconds))
                throwMillisOutOfRange();

            return milliseconds;
        }

        static long checkMicros(long microseconds) {
            if (!isValidMicros(microseconds))
                throwMicrosOutOfRange();

            return microseconds;
        }

        static long checkSeconds(long seconds) {
            if (!isValidSeconds(seconds))
                throwSecondsOutOfRange();

            return seconds;
        }

        static long checkMinutes(long minutes) {
            if (!isValidMinutes(minutes))
                throwMinutesOutOfRange();

            return minutes;
        }

        static long checkHours(long hours) {
            if (!isValidHours(hours))
                throwHoursOutOfRange();

            return hours;
        }

        static long checkDays(long days) {
            if (!isValidDays(days))
                throwDaysOutOfRange();

            return days;
        }

        static long check(double ts) {
            if (!isValid(ts))
                throwTimeSpanRange();

            return (long)ts;
        }

        // endregion Range checks

        /////////////////////////////////////////////////////////////////////////////////////
        // region Construct HdTimeSpan
        /////////////////////////////////////////////////////////////////////////////////////

        static long from(int days, int hours, int minutes, int seconds, int nanoseconds) {
//            int or = days | hours | minutes | seconds | nanoseconds;
//            int and = days & hours & minutes & seconds & nanoseconds;
//            if ((or ^ and) < 0)
//                throw rangeException("HdTimeSpan constructor: Argument sign must match");

            long s = (long)days * SECONDS_IN_DAY + (long)hours * SECONDS_IN_HOUR + (long)minutes * SECONDS_IN_MINUTE
                    + (long)seconds;

            // We perform range check against seconds
            if (!inRange(s, (MIN + Integer.MAX_VALUE)/NS_IN_SECOND, (MAX + Integer.MIN_VALUE)/NS_IN_SECOND))
                throw rangeException("HdTimeSpan");

            return s * NS_IN_SECOND + nanoseconds;
        }

        static long from(int hours, int minutes, int seconds) {

            long s = (long)hours * SECONDS_IN_HOUR + (long)minutes * SECONDS_IN_MINUTE
                    + (long)seconds;

            // We perform range check against seconds
            if (!inRange(s, (MIN + Integer.MAX_VALUE)/NS_IN_SECOND, (MAX + Integer.MIN_VALUE)/NS_IN_SECOND))
                throwTimeSpanRange();

            return s * NS_IN_SECOND;
        }

        public static long fromNanoseconds(long value) {
            // This is just a "range check", so it remains
            if (value == NULL)
                throwTimeSpanRange();

            return value;
        }

        public static long fromMicroseconds(long value) {
            return checkMicros(value) * NS_IN_MICROSECOND;
        }

        public static long fromMilliseconds(long value) {
            return checkMillis(value) * NS_IN_MILLISECOND;
        }

        public static long fromSeconds(long value) {
            return checkSeconds(value) * NS_IN_SECOND;
        }

        public static long fromMinutes(long value) {
            return checkMinutes(value) * NS_IN_MINUTE;
        }

        public static long fromHours(long value) {
            return checkHours(value) * NS_IN_HOUR;
        }

        public static long fromDays(long value) {
            return checkDays(value) * NS_IN_DAY;
        }

        public static long fromMicroseconds(double value) {
            return check(value * NS_IN_MICROSECOND);
        }

        public static long fromMilliseconds(double value) {
            return check(value * NS_IN_MILLISECOND);
        }

        public static long fromSeconds(double value) {
            return check(value * NS_IN_SECOND);
        }

        public static long fromMinutes(double value) {
            return check(value * NS_IN_MINUTE);
        }

        public static long fromHours(double value) {
            return check(value * NS_IN_HOUR);
        }

        public static long fromDays(double value) {
            return check(value * NS_IN_DAY);
        }

        // endregion Construct HdTimeSpan

        /////////////////////////////////////////////////////////////////////////////////////
        // region Components
        /////////////////////////////////////////////////////////////////////////////////////

        static long divMod(final long ts, final long div1, final long div2) {
            return ts / div1 % div2;
        }
        
        static long roundTo(final long ts, final long unitSizeNs) {
            //return NULL == ts ? NULL : (ts / unitSizeNs) * unitSizeNs; // Not doing any NULL special cases in the main impl.
            return (ts / unitSizeNs) * unitSizeNs;
        }

        public static int nanosecondsComponent(long ts) {
            return (int)(ts % NS_IN_SECOND);
        }

        public static int microsecondsComponent(long ts) {
            return (int)divMod(ts, NS_IN_MICROSECOND, MICROSECONDS_IN_SECOND);
        }

        public static int millisecondsComponent(long ts) {
            return (int)divMod(ts, NS_IN_MILLISECOND, MILLISECONDS_IN_SECOND);
        }

        public static int secondsComponent(long ts) {
            return (int)divMod(ts, NS_IN_SECOND, SECONDS_IN_MINUTE);
        }

        public static int minutesComponent(long ts) {
            return (int)divMod(ts, NS_IN_MINUTE, MINUTES_IN_HOUR);
        }

        public static int hoursComponent(long ts) {
            return (int)divMod(ts, NS_IN_HOUR, HOURS_IN_DAY);
        }

        public static int days(long ts) {
            return (int)(ts / NS_IN_DAY);
        }

        public static void toComponents(long ts, Components components) {
            long sign = ts >> 63;
            components.sign = (int)sign;
            ts = (ts + sign) ^ sign;
//                long old = ts;
//                ts /= NS_IN_SECOND;
//                components.nanoseconds = (int)(old - ts * NS_IN_SECOND);
//                old = ts;
//                ts /= SECONDS_IN_MINUTE;
//                components.seconds = (int)(old - ts * SECONDS_IN_MINUTE);
//                old = ts;
//                ts /= MINUTES_IN_HOUR;
//                components.minutes = (int)(old - ts * MINUTES_IN_HOUR);
//                old = ts;
//                ts /= HOURS_IN_DAY;
//                components.hours = (int)(old - ts * HOURS_IN_DAY);
//                components.days = (int)ts;

            long old = ts;
            ts /= NS_IN_MINUTE;
            long sec = old - ts * NS_IN_MINUTE;
            old = ts;
            ts /= MINUTES_IN_HOUR;
            long secOld = sec;
            sec /= NS_IN_SECOND;
            components.minute = (int)(old - ts * MINUTES_IN_HOUR);
            components.nanosecond = (int)(secOld - sec * NS_IN_SECOND);
            components.second = (int)sec;
            old = ts;
            ts /= HOURS_IN_DAY;
            components.hour = (int)(old - ts * HOURS_IN_DAY);
            components.day = (int)ts;
        }

        // endregion

        /////////////////////////////////////////////////////////////////////////////////////
        // region Scale to floating point value
        /////////////////////////////////////////////////////////////////////////////////////

        static double toDoubleNanos(final long dt) {
            return toDouble(dt, 1.0);
        }

        static double toDoubleMicros(final long dt) {
            return toDouble(dt, NS_IN_MICROSECOND);
        }

        static double toDoubleMillis(final long dt) {
            return toDouble(dt, NS_IN_MILLISECOND);
        }

        static double toDoubleSeconds(final long dt) {
            return toDouble(dt, NS_IN_SECOND);
        }

        static double toDoubleMinutes(final long dt) {
            return toDouble(dt, NS_IN_MINUTE);
        }

        static double toDoubleHours(final long dt) {
            return toDouble(dt, NS_IN_HOUR);
        }

        static double toDoubleDays(final long dt) {
            return toDouble(dt, NS_IN_DAY);
        }

        static double toDoubleWeeks(final long dt) {
            return toDouble(dt, NS_IN_WEEK);
        }

        // endregion
    }

    /////////////////////////////////////////////////////////////////////////////////////
    // region Utility/helper methods
    /////////////////////////////////////////////////////////////////////////////////////
    static double toDouble(final long value, final double unitSizeNs) {
        //return NULL == value ? Double.NaN : // We don't do any null-related logic anymore
        return value / unitSizeNs;
    }

    static double toDouble(final long value, final long unitSizeNs) {
        return toDouble(value, (double)unitSizeNs);
    }

    // endregion

//
//    public static long fromDate(long year, long month, long dayOfMonth) {
//        //return new GregorianCalendar(year, month, dayOfMonth).;
//    }
//
//    public static long fromDays(long hours) {
//        return Calendar.getInstance(TimeZone.getTimeZone("UTC")).;
//    }

//    private static long from(long value, long scale, long min, long max) {
//        if (value + (Long.MIN_VALUE - min) >= max + Long.MIN_VALUE)
//            throw new IllegalArgumentException();
//
//        return value * scale;
//    }

    static long fromMillisUnc(long milliseconds) {
        return milliseconds * NS_IN_MILLISECOND;
    }

    static long fromSecondsUnc(long seconds) {
        return seconds * NS_IN_SECOND;
    }

    static long fromMinutesUnc(long minutes) {
        return minutes * NS_IN_MINUTE;
    }

    static long fromHoursUnc(long hours) {
        return hours * NS_IN_HOUR;
    }

    static long fromDaysUnc(long days) {
        return days * NS_IN_DAY;
    }

    /////////////////////////////////////////////////////////////////////////////////////
    // region Range Exceptions
    /////////////////////////////////////////////////////////////////////////////////////

    static IllegalArgumentException rangeException(String msg)
    {
        return new IllegalArgumentException(msg + " is not within allowed range");
    }

    //static void throwOutOfRange(String msg) { throw new ArgumentOutOfRangeException(msg); }

    static void throwOutOfRange() { throw new IllegalArgumentException("Argument is not within allowed range"); }

    static void throwTimeSpanRange() { throw rangeException("HdTimeSpan"); }

    static void throwTimeOfDayOutOfRange() { throw rangeException("time of day"); }

    static void throwNanosOutOfRange() { throw rangeException("nanoseconds"); }

    static void throwTicksOutOfRange() { throw rangeException("ticks"); }

    static void throwMicrosOutOfRange() { throw rangeException("microseconds"); }

    static void throwMillisOutOfRange() { throw rangeException("milliseconds"); }

    static void throwSecondsOutOfRange() { throw rangeException("seconds"); }

    static void throwMinutesOutOfRange() { throw rangeException("minutes"); }

    static void throwHoursOutOfRange() { throw rangeException("hours"); }

    static void throwDaysOutOfRange() { throw rangeException("days"); }

    static void throwYearsOutOfRange() { throw rangeException("years"); }

    // endregion Range Exceptions
    
    public static long millisToNanos(long millis) {
        throw new UnsupportedOperationException();
//        if (isValidMillis(millis))
//            return fromMillis(millis);
    }

}
