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
import java.util.function.BiFunction;
import java.util.function.BinaryOperator;

public class HdTimeSpanTest {
    @Test
    @ValueTypeSuppressWarnings({"refArgs"})
    public void testNull() {
        Assert.assertNull(HdTimeSpan.fromUnderlying(HdDateTimeUtils.NULL_VALUE));
        Assert.assertNull(HdTimeSpan.fromUnderlying(HdDateTime.NULL_VALUE));
        Assert.assertNull(HdTimeSpan.NULL);

        HdTimeSpan nul = HdTimeSpan.NULL;
        Assert.assertNull(nul);
        Assert.assertEquals(nul, HdDateTime.fromUnderlying(HdTimeSpan.NULL_VALUE));
    }

    void checkPrint(String str, String fmt, HdTimeSpan num) throws IOException {
        Assert.assertEquals(str, num.toString(fmt));
        StringBuilder sb = new StringBuilder();
        Assert.assertEquals(str, num.appendTo(sb, fmt).toString());
    }

    private void checkFormatFail(String from, String fmt, String msg) throws ParseException {
        try {
            HdTimeSpan.parse(from, fmt);
        }
        catch (FormatError e) {
            if (e.getMessage().contains(msg))
                return;
        }

        Assert.fail("Was expected to throw");
    }

    @ValueTypeSuppressWarnings({"refArgs"})
    private void checkParse(String from, String fmt, HdTimeSpan expected) throws ParseException {
        HdTimeSpan parsed = HdTimeSpan.parse(from, fmt);
        if (!expected.equals(parsed)) {
            // Comparison is here to avoid problems with Formatter affecting tests for Parser
            Assert.assertEquals(expected, parsed);
            Assert.assertEquals(expected.toString(), parsed.toString());
        }

        Assert.assertEquals(HdTimeSpan.toUnderlying(expected), HdTimeSpan.toUnderlying(parsed));

    }

    @Test
    public void testPrint() throws IOException {

        // Plain numbers
        checkPrint("34627623,.45634", "34627623,.45634", HdTimeSpan.fromSeconds(12));

        // Check quoted text
        checkPrint("Abcmsy", "'Abcmsy'", HdTimeSpan.ZERO);
        checkPrint("00Abcmsy000", "00'Abcmsy'000", HdTimeSpan.ZERO);
        checkPrint("'Abc'msy", "'''Abc''msy'", HdTimeSpan.ZERO);
        checkPrint("0'0Abc''msy00'0", "0''0'Abc''''msy'00''0", HdTimeSpan.ZERO);

        // Seconds
        checkPrint("12", "s", HdTimeSpan.fromSeconds(12));
        checkPrint("0", "s", HdTimeSpan.fromSeconds(0));
        checkPrint("00", "ss", HdTimeSpan.fromSeconds(0));
        checkPrint("005", "0ss", HdTimeSpan.fromSeconds(65));
        checkPrint("000005", "ssssss", HdTimeSpan.fromSeconds(65));

        // Seconds & Fractions of Second. 'S' and 'f' are now synonyms
        checkPrint("12.3", "s.S", HdTimeSpan.fromMilliseconds(12_300));
        checkPrint("0.345", "s.SSS", HdTimeSpan.fromMicroseconds(345_000));
        checkPrint("00.023", "ss.SSS", HdTimeSpan.fromMilliseconds(600_023));
        checkPrint("05.123", "ss.SSS", HdTimeSpan.fromMilliseconds(65_123));
        checkPrint("05.123000", "ss.SSSSSS", HdTimeSpan.fromMilliseconds(65_123));

        checkPrint("05.0001", "ss.ffff", HdTimeSpan.fromMicroseconds(65_000_123));
        checkPrint("05.00012", "ss.fffff", HdTimeSpan.fromMicroseconds(65_000_123));
        checkPrint("05.000123", "ss.ffffff", HdTimeSpan.fromMicroseconds(65_000_123));
        checkPrint("05.123000", "ss.ffffff", HdTimeSpan.fromNanoseconds(65_123_000_123L));
        checkPrint("05.123000", "ss.ffffff", HdTimeSpan.fromNanoseconds(65_123_000_999L));
        checkPrint("05.123000", "ss.ffffff", HdTimeSpan.fromNanoseconds(65_123_000_999L));
        checkPrint("05.1230009", "ss.fffffff", HdTimeSpan.fromNanoseconds(65_123_000_999L));
        checkPrint("05.12300012", "ss.ffffffff", HdTimeSpan.fromNanoseconds(65_123_000_123L));
        checkPrint("05.123000123", "ss.fffffffff", HdTimeSpan.fromNanoseconds(65_123_000_123L));
        checkPrint("05.000000123", "ss.fffffffff", HdTimeSpan.fromNanoseconds(65_000_000_123L));
        checkPrint("5.000123000", "s.fffffffff", HdTimeSpan.fromNanoseconds(65_000_123_000L));

        // Minutes
        checkPrint("5", "m", HdTimeSpan.fromMinutes(425));
        checkPrint("7", "m", HdTimeSpan.fromSeconds(425));
        checkPrint("05", "mm", HdTimeSpan.fromMinutes(425));
        checkPrint("00005", "0mmmm", HdTimeSpan.fromMinutes(425));

        // Hours
        checkPrint("5", "H", HdTimeSpan.fromHours(48 + 5));
        checkPrint("4", "H", HdTimeSpan.fromMinutes(245));
        checkPrint("07", "HH", HdTimeSpan.fromMinutes(425));
        checkPrint("0007005", "0HHHmmm", HdTimeSpan.fromMinutes(425));
        checkPrint("07:5.789", "HH:m.SSS", HdTimeSpan.fromMinutes(425).add(HdTimeSpan.fromMilliseconds(789)));

        // Sign insertion
        checkPrint("-12", "s", HdTimeSpan.fromSeconds(-12));
        checkPrint("-0", "s", HdTimeSpan.fromMilliseconds(-1));
        checkPrint("-05", "ss", HdTimeSpan.fromSeconds(-425));

        // "Advanced" sign insertion
        checkPrint("-000005", "ssssss", HdTimeSpan.fromSeconds(-425));
        checkPrint("-00000005", "00ssssss", HdTimeSpan.fromSeconds(-425));
        checkPrint("Abc-00000005", "Abc00ssssss", HdTimeSpan.fromSeconds(-425));
        checkPrint("Abcmsy-00000005", "'Abcmsy'00ssssss", HdTimeSpan.fromSeconds(-425));
        checkPrint(";.:-000005", ";.:ssssss", HdTimeSpan.fromSeconds(-425));
        checkPrint(";.:-00000005", ";.:00ssssss", HdTimeSpan.fromSeconds(-425));
    }


    @Test
    public void testParse() throws IOException, ParseException {

        checkParse("5", "s", HdTimeSpan.fromSeconds(5));
        checkParse("0", "s", HdTimeSpan.fromSeconds(0));
        checkParse("005", "s", HdTimeSpan.fromSeconds(5));

        checkParse("12:34:56", "H:m:s", HdTimeSpan.newInstance(12, 34, 56));
        checkParse("12:34:56", "HH:mm:ss", HdTimeSpan.newInstance(12, 34, 56));

        checkParse("1024T12:34:56", "dTH:m:s", HdTimeSpan.newInstance(1024, 12, 34, 56));
        checkParse("1024T12:34:56", "d'T'H:m:s", HdTimeSpan.newInstance(1024, 12, 34, 56));

        // Unseparated fixed width fields
        checkParse("123", "ss", HdTimeSpan.fromSeconds(12));
        checkParse("123456", "HHmmss", HdTimeSpan.newInstance(12, 34, 56));
        checkParse("1024123456", "ddddHHmmss", HdTimeSpan.newInstance(1024,12, 34, 56));
        checkParse("91024123456", "dddddHHmmss", HdTimeSpan.newInstance(91024,12, 34, 56));
        checkParse("0000091024123456", "ddddddddddHHmmss", HdTimeSpan.newInstance(91024,12, 34, 56));
        checkParse("101024123456", "ddddddHHmmss", HdTimeSpan.newInstance(101024,12, 34, 56));

        // Sign
        checkParse("-5", "s", HdTimeSpan.fromSeconds(-5));
        checkParse("-1024T12:34:56", "dTH:m:s", HdTimeSpan.newInstance(-1024, -12, -34, -56));
        checkParse("-123456", "HHmmss", HdTimeSpan.newInstance(-12, -34, -56));
        checkParse("-0000091024123456", "ddddddddddHHmmss", HdTimeSpan.newInstance(-91024, -12, -34, -56));

        // Fractions (Only fixed length patterns supported)
        checkParse("4.2", "s.f", HdTimeSpan.fromMilliseconds(4200));
        checkParse("4.200000", "s.ffffff", HdTimeSpan.fromMilliseconds(4200));
        checkParse("4.020", "s.ff", HdTimeSpan.fromMilliseconds(4020));
        checkParse("4.200000000", "s.fffffffff", HdTimeSpan.fromMilliseconds(4200));
        checkParse("4.000000002", "s.fffffffff", HdTimeSpan.fromNanoseconds(4000000002L));

        checkParse("4.2", "s.S", HdTimeSpan.fromMilliseconds(4200));
        checkParse("4.123", "s.SSS", HdTimeSpan.fromMilliseconds(4123));
    }

    @Test
    public void testParseInvalidFormat() throws IOException, ParseException {
        checkFormatFail("5 5", "s s", "Dup");
        checkFormatFail("2002 2002", "dddd dddd", "Dup");
        checkFormatFail("1 1", "d d", "Dup");
    }


    @Test
    public void allMethods() throws ParseException, IOException {
        HdTimeSpan min = HdTimeSpan.MIN_VALUE;
        HdTimeSpan max = HdTimeSpan.MAX_VALUE;
        HdTimeSpan zero = HdTimeSpan.ZERO;

        HdTimeSpan.fromUnderlying(123456789);
        HdTimeSpan.fromUnderlying(123456789123456789L);
        HdTimeSpan.newInstance(0, 0,0);
        HdTimeSpan.newInstance(0, 0, 0,0);
        HdTimeSpan.newInstance(0, 0, 0,0, 0);

        HdTimeSpan.fromDays(0L);
        HdTimeSpan.fromHours(0L);
        HdTimeSpan.fromMinutes(0L);
        HdTimeSpan.fromSeconds(0L);
        HdTimeSpan.fromMilliseconds(0L);
        HdTimeSpan.fromMicroseconds(0L);
        HdTimeSpan.fromNanoseconds(0L);

        HdTimeSpan.fromDays(0.0);
        HdTimeSpan.fromHours(0.0);
        HdTimeSpan.fromMinutes(0.0);
        HdTimeSpan.fromSeconds(0.0);
        HdTimeSpan.fromMilliseconds(0.0);
        HdTimeSpan.fromMicroseconds(0.0);
        
        zero.getDays();
        zero.getHours();
        zero.getMinutes();
        zero.getSeconds();
        zero.getNanoseconds();

        zero.totalWeeks();
        zero.totalDays();
        zero.totalHours();
        zero.totalMinutes();
        zero.totalSeconds();
        zero.totalMilliseconds();
        zero.totalMicroseconds();
        zero.totalNanoseconds();

        HdTimeSpan a = HdTimeSpan.fromSeconds(-1), b = HdTimeSpan.fromSeconds(1);

        a.compareTo(b);
        HdTimeSpan.compare(a, b);
        HdTimeSpan.isLess(a, b);
        HdTimeSpan.isLessOrEqual(a, b);
        HdTimeSpan.isGreater(a, b);
        HdTimeSpan.isGreaterOrEqual(a, b);
        a.isLess(b);
        a.isLessOrEqual(b);
        a.isGreater(b);
        a.isGreaterOrEqual(b);
        a.isZero();
        a.isPositive();
        a.isNegative();
        a.roundTo(Resolution.MILLISECOND);
        a.roundTo(HdTimeSpan.fromMinutes(5));

        a.add(b);
        a.addUnchecked(b);
        a.subtract(b);
        a.subtractUnchecked(b);
        a.negate();
        a.duration();
        a.addDays(1);
        a.addHours(1);
        a.addMinutes(1);
        a.addSeconds(1);
        a.addMilliseconds(1);
        a.addMicroseconds(1);
        a.addNanoseconds(1);

        a.hashCode();
        a.equals("Nothing");
        a.equals(b);
        HdTimeSpan.equals(a, b);

        a.toString();
        a.toString("H:mm:ss,fffffffff");
        StringBuilder sb = new StringBuilder();
        a.appendTo(sb);
        a.appendTo(sb, "H:m:s");

        HdTimeSpan.parse("42 11:11:11.111111111");
        HdTimeSpan.parse("42", "d");
    }

    private void check(HdTimeSpan a, long b,
                       BiFunction<HdTimeSpan, Long, HdTimeSpan> f1,
                       BiFunction<HdTimeSpan, Long, HdTimeSpan> f2)  {

        HdTimeSpan x1 = HdTimeSpan.NULL, x2 = x1;
        Throwable e1 = null, e2 = null;
        try {
            x1 = f1.apply(a, b);
        }
        catch (Throwable e) {
            e1 = e;
        }

        try {
            x2 = f2.apply(a, b);
        }
        catch (Throwable e) {
            e2 = e;
        }

        if (e1 != null) {
            if (e2 != null) {
                if (e1.getClass() == e2.getClass())
                    return;

                Assert.fail(String.format("%s throws %s, while %s throws %s", f1, e1, f2, x2));
            }

            Assert.fail(String.format("%s throws %s, while %s returns %s", f1, e1, f2, x2));
        } else if (e2 != null) {
            Assert.fail(String.format("%s returns %s, while %s throws %s", f1, x1, f2, e2));
        }

        Assert.assertEquals(x1, x2);
    }


    @Test
    public void testAddXXBasic() {
        HdTimeSpan a = HdDateTime.now().subtract(HdDateTime.fromEpochMilliseconds(0));
        Assert.assertEquals(a.add(HdTimeSpan.fromDays(-123)), a.addDays(-123));
        Assert.assertEquals(a.add(HdTimeSpan.fromHours(321)), a.addHours(321));
        Assert.assertEquals(a.add(HdTimeSpan.fromMinutes(432)), a.addMinutes(432));
        Assert.assertEquals(a.add(HdTimeSpan.fromSeconds(-543)), a.addSeconds(-543));
        Assert.assertEquals(a.add(HdTimeSpan.fromMilliseconds(987654321)), a.addMilliseconds(987654321));
        Assert.assertEquals(a.add(HdTimeSpan.fromMicroseconds(-987654321)), a.addMicroseconds(-987654321));
        Assert.assertEquals(a.add(HdTimeSpan.fromNanoseconds(-123456789987654321L)), a.addNanoseconds(-123456789987654321L));
    }
}