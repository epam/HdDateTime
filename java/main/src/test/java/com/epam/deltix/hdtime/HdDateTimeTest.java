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

import org.junit.Assert;
import org.junit.Test;

import java.io.IOException;
import java.text.SimpleDateFormat;
import java.time.DayOfWeek;
import java.time.Month;
import java.time.format.DateTimeFormatter;
import java.util.Calendar;
import java.util.Date;
import java.util.GregorianCalendar;
import java.util.TimeZone;

public class HdDateTimeTest {
    @Test
    @ValueTypeSuppressWarnings({"refArgs"})
    public void testNull() {
        Assert.assertNull(HdDateTime.fromUnderlying(HdDateTimeUtils.NULL_VALUE));
        Assert.assertNull(HdDateTime.fromUnderlying(HdDateTime.NULL_VALUE));
        Assert.assertNull(HdDateTime.NULL);

        HdDateTime nul = HdDateTime.NULL;
        Assert.assertNull(nul);
        Assert.assertEquals(nul, HdDateTime.fromUnderlying(HdDateTime.NULL_VALUE));
    }

    private long dateTime(int year, Month month, int day, int hour, int minute, int second, int nanosecond) {
        return HdDateTimeUtils.newInstance(year, month, day, hour, minute, second, nanosecond);
    }

    void checkPrint(String str, String fmt, HdDateTime num) throws IOException {
        Assert.assertEquals(str, num.toString(fmt));
        StringBuilder sb = new StringBuilder();
        Assert.assertEquals(str, num.appendTo(sb, fmt).toString());
    }

    @ValueTypeSuppressWarnings({"refArgs"})
    private void checkParse(String from, String fmt, HdDateTime expected) throws ParseException {
        HdDateTime parsed = HdDateTime.parse(from, fmt);
        if (!expected.equals(parsed)) {
            // Comparison is here to avoid problems with Formatter affecting tests for Parser
            Assert.assertEquals(expected, parsed);
            Assert.assertEquals(expected.toString(), parsed.toString());
        }

        Assert.assertEquals(HdDateTime.toUnderlying(expected), HdDateTime.toUnderlying(parsed));
    }

    private void checkFormatFail(String from, String fmt, String msg) throws ParseException {
        try {
            HdDateTime.parse(from, fmt);
        }
        catch (FormatError e) {
            if (e.getMessage().contains(msg)) return;
        }

        Assert.fail("Was expected to throw");
    }

    @Test
    public void testPrint() throws IOException {

        // Plain numbers
        checkPrint("34627623,.45634", "34627623,.45634", HdDateTime.fromEpochMilliseconds(12));

        // Check quoted text
        checkPrint("Abcmsy", "'Abcmsy'", HdDateTime.fromEpochMilliseconds(0));
        checkPrint("00Abcmsy000", "00'Abcmsy'000", HdDateTime.fromEpochMilliseconds(0));
        checkPrint("'Abc'msy", "'''Abc''msy'", HdDateTime.fromEpochMilliseconds(0));
        checkPrint("0'0Abc''msy00'0", "0''0'Abc''''msy'00''0", HdDateTime.fromEpochMilliseconds(0));

        // Seconds
        checkPrint("12", "s", HdDateTime.fromEpochMilliseconds(12000));
        checkPrint("0", "s", HdDateTime.fromEpochMilliseconds(0));
        checkPrint("00", "ss", HdDateTime.fromEpochMilliseconds(0));
        checkPrint("005", "0ss", HdDateTime.fromEpochMilliseconds(65000));
        checkPrint("000005", "ssssss", HdDateTime.fromEpochMilliseconds(65000));

        // Seconds & Fractions of Second. 'S' and 'f' are now synonyms
        checkPrint("05.0001", "ss.ffff", HdDateTime.fromEpochNanoseconds(65_000_123_000L));
        checkPrint("05.00012", "ss.fffff", HdDateTime.fromEpochNanoseconds(65_000_123_000L));
        checkPrint("05.000123", "ss.ffffff", HdDateTime.fromEpochNanoseconds(65_000_123_000L));
        checkPrint("05.123000", "ss.ffffff", HdDateTime.fromEpochNanoseconds(65_123_000_123L));
        checkPrint("05.123000", "ss.ffffff", HdDateTime.fromEpochNanoseconds(65_123_000_999L));
        checkPrint("05.123000", "ss.ffffff", HdDateTime.fromEpochNanoseconds(65_123_000_999L));
        checkPrint("05.1230009", "ss.fffffff", HdDateTime.fromEpochNanoseconds(65_123_000_999L));
        checkPrint("05.12300012", "ss.ffffffff", HdDateTime.fromEpochNanoseconds(65_123_000_123L));
        checkPrint("05.123000123", "ss.fffffffff", HdDateTime.fromEpochNanoseconds(65_123_000_123L));
        checkPrint("05.000000123", "ss.fffffffff", HdDateTime.fromEpochNanoseconds(65_000_000_123L));
        checkPrint("5.000123000", "s.fffffffff", HdDateTime.fromEpochNanoseconds(65_000_123_000L));

        checkPrint("12.3", "s.S", HdDateTime.fromEpochMilliseconds(12_300));
        checkPrint("0.345", "s.SSS", HdDateTime.fromEpochNanoseconds(345_000_000));
        checkPrint("00.023", "ss.SSS", HdDateTime.fromEpochMilliseconds(600_023));
        checkPrint("05.123", "ss.SSS", HdDateTime.fromEpochMilliseconds(65_123));
        checkPrint("05.123000", "ss.SSSSSS", HdDateTime.fromEpochMilliseconds(65_123));

        // Minutes
        checkPrint("5", "m", HdDateTime.now().getDate().addMinutes(425));
        checkPrint("7", "m", HdDateTime.fromEpochMilliseconds(425_000));
        checkPrint("05", "mm", HdDateTime.now().getDate().addMinutes(425));
        checkPrint("00005", "0mmmm", HdDateTime.now().getDate().addMinutes(425));

        // Hours
        checkPrint("5", "H", HdDateTime.now().getDate().addHours(48 + 5));
        checkPrint("4", "H", HdDateTime.now().getDate().addMinutes(245));
        checkPrint("07", "HH", HdDateTime.now().getDate().addMinutes(425));
        checkPrint("0007005", "0HHHmmm", HdDateTime.now().getDate().addMinutes(425));
        checkPrint("07:5.789", "HH:m.SSS", HdDateTime.now().getDate().addMinutes(425).addMilliseconds(789));

        checkPrint("0007005", "0HHHmmm", HdDateTime.now().getDate().addMinutes(425));
        checkPrint("1999-4-1 0:7:5.656789", "yyyy-M-d H:m:s.SSSSSS",
                HdDateTime.newInstance(1999, Month.APRIL, 1).addSeconds(425).addNanoseconds(656789000));

        checkPrint("1999-4-1 0:7:5.656789", "yyyy-M-d H:m:s.SSSSSS",
                HdDateTime.newInstance(1999, Month.APRIL, 1, 0, 7, 5, 656789000));
        checkPrint("990401000705656789", "yyMMddHHmmssSSSSSS",
                HdDateTime.newInstance(1999, Month.APRIL, 1).addSeconds(425).addNanoseconds(656789000));
        checkPrint("1999Apr01000705656789999", "yyyMMMddHHmmssSSSSSSSSS",
                HdDateTime.newInstance(1999, Month.APRIL, 1).addSeconds(425).addNanoseconds(656789999));

        checkPrint("2002-January-01", "y-MMMM-dd", HdDateTime.newInstance(2002, Month.JANUARY, 1));
        checkPrint("31 May 2002", "d MMMM yyy", HdDateTime.newInstance(2002, Month.MAY, 31));
        checkPrint("31       May 2002", "dMMMMMMMMMM yyy", HdDateTime.newInstance(2002, Month.MAY, 31));
        checkPrint("31  December 2002", "dMMMMMMMMMM yyy", HdDateTime.newInstance(2002, Month.DECEMBER, 31));

        checkPrint("1910-4-1 0:7:5.656789", "yyyy-M-d H:m:s.ffffff",
                HdDateTime.newInstance(1910, Month.APRIL, 1).addSeconds(425).addNanoseconds(656789000));

        checkPrint("1910-4-1 0:7:5.656789", "yyyy-M-d H:m:s.ffffff",
                HdDateTime.newInstance(1910, Month.APRIL, 1).addSeconds(425).addNanoseconds(656789000));

        checkPrint("1866-1-22 20:40:40.123456789", "yyyy-M-d H:m:s.fffffffff",
                HdDateTime.newInstance(1866, Month.JANUARY, 22, 20, 40, 40, 123456789));
    }

    @Test
    public void testPrintExtra() throws IOException {
        // Overflow (actualy this tests arithmetics)
        checkPrint("48", "s", HdDateTime.now().getDate().addSeconds(-12));
        checkPrint("0", "s", HdDateTime.now().getDate().addMilliseconds(1));
        checkPrint("59", "s", HdDateTime.now().getDate().addMilliseconds(-1));
        checkPrint("55", "ss", HdDateTime.now().getDate().addSeconds(-425));

        // Some redundant tests for padding and absense of HdTimeSpan sign insertion
        checkPrint("000055", "ssssss", HdDateTime.now().getDate().addSeconds(-425));
        checkPrint("00000055", "00ssssss", HdDateTime.now().getDate().addSeconds(-425));
        checkPrint("Abc00000055", "Abc00ssssss", HdDateTime.now().getDate().addSeconds(-425));
        checkPrint("Abcmsy00000055", "'Abcmsy'00ssssss", HdDateTime.now().getDate().addSeconds(-425));
        checkPrint(";.:000055", ";.:ssssss", HdDateTime.now().getDate().addSeconds(-425));
        checkPrint(";.:00000055", ";.:00ssssss", HdDateTime.now().getDate().addSeconds(-425));
    }

    @Test
    public void testParse() throws IOException, ParseException {

        checkParse("2109","yyyy", HdDateTime.newInstance(2109, Month.JANUARY, 1));
        checkParse("1864","yyyy", HdDateTime.newInstance(1864, Month.JANUARY, 1));

        checkParse("197005","yyyyMM", HdDateTime.newInstance(1970, Month.MAY, 1));
        checkParse("19700531 ","yyyyMMdd ", HdDateTime.newInstance(1970, Month.MAY, 31));

        checkParse("19700531 13","yyyyMMdd HH",
                HdDateTime.newInstance(1970, Month.MAY, 31, 13, 0, 0));

        checkParse("19700531 1342","yyyyMMdd HHmm",
                HdDateTime.newInstance(1970, Month.MAY, 31, 13, 42, 0));

        checkParse("19700531 134259","yyyyMMdd HHmmss",
                HdDateTime.newInstance(1970, Month.MAY, 31, 13, 42, 59));

        checkParse("19700101000000","yyyyMMddHHmmss", HdDateTime.fromEpochMilliseconds(0));
        checkParse("197001010000000","yyyyMMddHHmmssf", HdDateTime.fromEpochMilliseconds(0));
        checkParse("1970010100000000","yyyyMMddHHmmssff", HdDateTime.fromEpochMilliseconds(0));
        checkParse("19700101000000000","yyyyMMddHHmmssfff", HdDateTime.fromEpochMilliseconds(0));
        checkParse("197001010000000000","yyyyMMddHHmmssffff", HdDateTime.fromEpochMilliseconds(0));
        checkParse("1970010100000000000","yyyyMMddHHmmssfffff", HdDateTime.fromEpochMilliseconds(0));
        checkParse("19700101000000000000","yyyyMMddHHmmssffffff", HdDateTime.fromEpochMilliseconds(0));
        checkParse("197001010000000000000","yyyyMMddHHmmssfffffff", HdDateTime.fromEpochMilliseconds(0));
        checkParse("1970010100000000000000","yyyyMMddHHmmssffffffff", HdDateTime.fromEpochMilliseconds(0));
        checkParse("19700101000000000000000","yyyyMMddHHmmssfffffffff", HdDateTime.fromEpochMilliseconds(0));
        checkParse("197012345678901010000000","yyyyfffffffffMMddHHmmss",
                HdDateTime.fromEpochNanoseconds(123456789));
        checkParse("197012345678901010000000123456789","yyyyfffffffffMMddHHmmss",
                HdDateTime.fromEpochNanoseconds(123456789));

        checkParse("197001010000 5", "yyyyMMddHHmm s", HdDateTime.fromEpochMilliseconds(5000));
        checkParse("197001010000 0", "yyyyMMddHHmm s", HdDateTime.fromEpochMilliseconds(0));
        checkParse("197001010000 005", "yyyyMMddHHmm s", HdDateTime.fromEpochMilliseconds(5000));

        checkParse("19700101 12:34:56", "yyyyMMdd H:m:s",
                HdDateTime.newInstance(1970, Month.JANUARY, 1, 12, 34, 56));

        checkParse("19700101 9:8:7", "yyyyMMdd H:m:s",
                HdDateTime.newInstance(1970, Month.JANUARY, 1, 9, 8, 7));

        checkParse("19700101 12:34:56", "yyyyMMdd HH:mm:ss",
                HdDateTime.newInstance(1970, Month.JANUARY, 1, 12, 34, 56));

        checkParse("197001 24T12:34:56", "yyyyMM ddTH:m:s",
                HdDateTime.newInstance(1970, Month.JANUARY, 24, 12, 34, 56));
        checkParse("197001 24T12:34:56", "yyyyMM dd'T'H:m:s",
                HdDateTime.newInstance(1970, Month.JANUARY, 24, 12, 34, 56));

        checkParse("197001 0024123456", "yyyyMM ddddHHmmss",
                HdDateTime.newInstance(1970, Month.JANUARY, 24,12, 34, 56));
        checkParse("197001 0000000024123456", "yyyyMM ddddddddddHHmmss",
                HdDateTime.newInstance(1970, Month.JANUARY, 24,12, 34, 56));

        checkParse("1999-11-22 19:18:17", "y-M-d H:m:s",
                HdDateTime.newInstance(1999, Month.NOVEMBER, 22, 19, 18, 17));

        // Fractions (Only fixed length patterns supported)
        checkParse("197001010000 4.2", "yyyyMMddHHmm s.f", HdDateTime.fromEpochMilliseconds(4200));
        checkParse("197001010000 4.200000", "yyyyMMddHHmm s.ffffff", HdDateTime.fromEpochMilliseconds(4200));
        checkParse("197001010000 4.020", "yyyyMMddHHmm s.ff", HdDateTime.fromEpochMilliseconds(4020));
        checkParse("197001010000 4.200000000", "yyyyMMddHHmm s.fffffffff", HdDateTime.fromEpochMilliseconds(4200));
        checkParse("197001010000 4.000000002", "yyyyMMddHHmm s.fffffffff", HdDateTime.fromEpochNanoseconds(4000000002L));

        checkParse("197001010000 4.2", "yyyyMMddHHmm s.S", HdDateTime.fromEpochMilliseconds(4200));
        checkParse("197001010000 4.123", "yyyyMMddHHmm s.SSS", HdDateTime.fromEpochMilliseconds(4123));
    }

    @Test
    public void testParseInvalidFormat() throws IOException, ParseException {
        checkFormatFail("2002 2002", "yyyy yyyy", "Dup");
        checkFormatFail("2002 2002", "yyyy uuuu", "Dup");
        checkFormatFail("2002 2002", "y y", "Dup");
        checkFormatFail("2002 2002", "y uuuu", "Dup");
        checkFormatFail("5 5", "s s", "Dup");
        checkFormatFail("197012345678901010000000123456789","yyyyfffffffffMMddHHmmssfffffffff","Dup");
    }

    @Test
    public void testParseIncomplete() throws IOException, ParseException {

        checkFormatFail("05","MM", "Incompl");
        checkFormatFail("197031 ","yyyydd ", "Incompl");

        checkFormatFail("0531 13","MMdd HH", "Incompl");

        checkFormatFail("05 1342","MM HHmm", "Incompl");

        checkFormatFail("05 4259","MM mmss", "Incompl");

        checkFormatFail("0101000000","MMddHHmmss", "Incompl");
        checkFormatFail("197001000000","yyyyddHHmmss", "Incompl");
        checkFormatFail("197001000000","yyyyMMHHmmss", "Incompl");
        checkFormatFail("197001000000","yyyyddmmss", "Incompl");
        checkFormatFail("197001000000","yyyyddHHss", "Incompl");
        checkFormatFail("1970010000.0","yyyyddHHmm.f", "Incompl");

        checkFormatFail("5", "s", "Incompl");
        checkFormatFail("0", "s", "Incompl");
        checkFormatFail("005", "s", "Incompl");

        checkFormatFail("12:34:56", "H:m:s", "Incompl");
        checkFormatFail("12:34:56", "HH:mm:ss", "Incompl");

        checkFormatFail("24T12:34:56", "ddTH:m:s", "Incompl");
        checkFormatFail("24T12:34:56", "dd'T'H:m:s", "Incompl");

        // TODO: ..
//        checkParse("24T12:34:56", "dTH:m:s",
//                HdDateTime.newInstance(1970, Month.JANUARY, 24, 12, 34, 56));
//        checkParse("24T12:34:56", "d'T'H:m:s",
//                HdDateTime.newInstance(1970, Month.JANUARY, 24, 12, 34, 56));

        // Unseparated fixed width fields
        checkFormatFail("123", "ss", "Incompl");
        checkFormatFail("123456", "HHmmss", "Incompl");
        checkFormatFail("0024123456", "ddddHHmmss", "Incompl");
        checkFormatFail("0000000024123456", "ddddddddddHHmmss", "Incompl");

        // Fractions (Only fixed length patterns supported)
        checkFormatFail("4.2", "s.f", "Incompl");
        checkFormatFail("4.200000", "s.ffffff", "Incompl");
        checkFormatFail("4.020", "s.ff", "Incompl");
        checkFormatFail("4.200000000", "s.fffffffff", "Incompl");
        checkFormatFail("4.000000002", "s.fffffffff", "Incompl");

        checkFormatFail("4.2", "s.S", "Incompl");
        checkFormatFail("4.123", "s.SSS", "Incompl");
    }

    @Test
    public void toStringVsCalendar() {
        GregorianCalendar gc = new GregorianCalendar(2000, 0, 1, 1, 1, 1);
        gc.set(Calendar.MILLISECOND, 12);
        gc.setTimeZone(TimeZone.getTimeZone("UTC"));
        SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSSZ");

        sdf.setTimeZone(TimeZone.getTimeZone("UTC"));
        HdDateTime dt0 = HdDateTime.newInstance(2000, Month.JANUARY, 1, 1, 1, 1, 12 * 1000_000);
        System.out.printf("Java GregorianCalendar: %s\n", sdf.format(new Date(gc.getTimeInMillis())));
        System.out.printf("HdDateTime: %s\n", dt0.toString("yyyy-MM-dd'T'HH:mm:ss.SSS"));

        SimpleDateFormat sdf2 = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SS");
        System.out.printf("Java GregorianCalendar: %s\n", sdf2.format(new Date(gc.getTimeInMillis())));


        HdDateTime dt = HdDateTime.newInstance(2000, Month.JANUARY, 1, 1, 1, 1, 12 * 1000_000);
        System.out.println(dt.toString("yyyy-MM-dd HH:mm:ss.SSS"));
        System.out.println(dt.toString("yyyy-MM-dd HH:mm:ss.SS"));
    }

    @Test
    public void allMethods() throws ParseException, IOException {

        Assert.assertEquals(HdDateTime.NULL_VALUE, HdDateTime.toUnderlying(HdDateTime.NULL));
        HdDateTime.fromUnderlying(123456789);
        HdDateTime.fromUnderlying(123456789123456789L);
        HdDateTime.newInstance(1970,Month.JANUARY,1);
        HdDateTime.newInstance(1970,Month.JANUARY,1, 0,0,0);
        HdDateTime.newInstance(1970,Month.JANUARY,1, 0,0,0, 0);
        HdDateTime now = HdDateTime.now();
        HdDateTime.fromEpochMilliseconds(0L);
        HdDateTime.fromEpochNanoseconds(0L);

        now.toEpochMilliseconds();
        now.toEpochNanoseconds();

        now.getDate();
        now.getTimeOfDay();
        now.getYear();
        now.getMonth();
        now.getWeekOfYear();
        now.getWeekOfMonth();
        now.getDayOfYear();
        now.getDayOfMonth();
        now.getDayOfWeek();
        now.getHour();
        now.getMinute();
        now.getSecond();
        now.getMillisecond();
        now.getMicrosecond();
        now.getNanosecond();

        HdDateTime a = now, b = HdDateTime.now();
        now.setDate(now);
        now.setTime(HdTimeSpan.fromHours(3));
        now.setYear(1984);
        now.setMonth(Month.APRIL);
        now.setWeekOfYear(42);
        now.setWeekOfMonth(1);
        now.setDayOfYear(44);
        now.setDayOfMonth(11);
        now.setDayOfWeek(DayOfWeek.FRIDAY);
        now.setHour(11);
        now.setMinute(42);
        now.setSecond(17);
        now.setNanosecond(1337);

        a.compareTo(b);
        HdDateTime.compare(a, b);
        now.add(HdTimeSpan.fromMinutes(123));
        now.addUnchecked(HdTimeSpan.fromDays(321));
        now.addYears(123);
        now.addMonths(123);
        now.addDays(123);
        now.addHours(123);
        now.addMinutes(123);
        now.addSeconds(123);
        now.addMilliseconds(123);
        now.addMicroseconds(123);
        now.addNanoseconds(123);
        a.subtract(b);
        now.subtract(HdTimeSpan.fromMinutes(123));
        now.subtractUnchecked(b);
        now.subtractUnchecked(HdTimeSpan.fromDays(321));

        now.getDaysInMonth();
        now.getDaysInYear();
        now.isLeapYear();

        now.hashCode();
        now.equals("nothing");
        a.equals(b);
        HdDateTime.equals(a, b);

        now.toString();
        now.toString("H:m:s,fffffffff");
        StringBuilder sb = new StringBuilder();
        now.appendTo(sb);
        now.appendTo(sb, "H:m:s");

        HdDateTime.parse("1984-01-22 11:11:11.111111111");
        HdDateTime.parse("1984-1-22", "y-M-d");
    }
}
