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

import java.time.DayOfWeek;
import java.time.Month;
import java.util.Calendar;
import java.util.Random;

public class HdDateTimeUtilsTest {
    @Test
    public void testGetDateGetTime() {
        checkGetDateGetTime(1970, Month.JANUARY, 1, 0, 0, 0, 0);

        Random rnd = new Random(System.currentTimeMillis());
        for (int i = 0; i < 100_000; ++i) {
            int year = 1970 + rnd.nextInt(200) - 100;
            Month month = Month.of(rnd.nextInt(12) + 1);
            int day = 1 + rnd.nextInt(27); // Guaranteed to fit month/year
            int hour = rnd.nextInt(24);
            int minute = rnd.nextInt(60);
            int second = rnd.nextInt(60);
            //int millisecond = rnd.nextInt(1000);
            int nanosecond = rnd.nextInt(1000000000);
            //int microsecond = rnd.nextInt(100);
            checkGetDateGetTime(year, month, day, hour, minute, second, nanosecond);

            // Now check getters
            long dt = dateTime(year, month, day, hour, minute, second, nanosecond);
            Assert.assertEquals(year, HdDateTimeUtils.getYear(dt));
            Assert.assertEquals(month, HdDateTimeUtils.getMonth(dt));
            Assert.assertEquals(day, HdDateTimeUtils.getDayOfMonth(dt));
            Assert.assertEquals(hour, HdDateTimeUtils.getHour(dt));
            Assert.assertEquals(minute, HdDateTimeUtils.getMinute(dt));
            Assert.assertEquals(second, HdDateTimeUtils.getSecond(dt));
            Assert.assertEquals(nanosecond, HdDateTimeUtils.getNanosecond(dt));
        }
    }

    @Test
    public void testMonthImpl() {
        Assert.assertEquals(Calendar.JANUARY, Month.JANUARY.getValue() - 1);
        Assert.assertEquals(Calendar.FEBRUARY, Month.FEBRUARY.getValue() - 1);
        Assert.assertEquals(Calendar.MARCH, Month.MARCH.getValue() - 1);
        Assert.assertEquals(Calendar.APRIL, Month.APRIL.getValue() - 1);
        Assert.assertEquals(Calendar.MAY, Month.MAY.getValue() - 1);
        Assert.assertEquals(Calendar.JUNE, Month.JUNE.getValue() - 1);
        Assert.assertEquals(Calendar.JULY, Month.JULY.getValue() - 1);
        Assert.assertEquals(Calendar.AUGUST, Month.AUGUST.getValue() - 1);
        Assert.assertEquals(Calendar.SEPTEMBER, Month.SEPTEMBER.getValue() - 1);
        Assert.assertEquals(Calendar.OCTOBER, Month.OCTOBER.getValue() - 1);
        Assert.assertEquals(Calendar.NOVEMBER, Month.NOVEMBER.getValue() - 1);
        Assert.assertEquals(Calendar.DECEMBER, Month.DECEMBER.getValue() - 1);
    }

    @Test
    public void testDayOfWeekImpl() {
        Assert.assertEquals(Calendar.SUNDAY, Convert.DateTime.fromDayOfWeek[DayOfWeek.SUNDAY.getValue() - 1]);
        Assert.assertEquals(Calendar.MONDAY, Convert.DateTime.fromDayOfWeek[DayOfWeek.MONDAY.getValue() - 1]);
        Assert.assertEquals(Calendar.TUESDAY, Convert.DateTime.fromDayOfWeek[DayOfWeek.TUESDAY.getValue() - 1]);
        Assert.assertEquals(Calendar.WEDNESDAY, Convert.DateTime.fromDayOfWeek[DayOfWeek.WEDNESDAY.getValue() - 1]);
        Assert.assertEquals(Calendar.THURSDAY, Convert.DateTime.fromDayOfWeek[DayOfWeek.THURSDAY.getValue() - 1]);
        Assert.assertEquals(Calendar.FRIDAY, Convert.DateTime.fromDayOfWeek[DayOfWeek.FRIDAY.getValue() - 1]);
        Assert.assertEquals(Calendar.SATURDAY, Convert.DateTime.fromDayOfWeek[DayOfWeek.SATURDAY.getValue() - 1]);
        
        Assert.assertEquals(DayOfWeek.SUNDAY, Convert.DateTime.toDayOfWeek[Calendar.SUNDAY - 1]);
        Assert.assertEquals(DayOfWeek.MONDAY, Convert.DateTime.toDayOfWeek[Calendar.MONDAY - 1]);
        Assert.assertEquals(DayOfWeek.TUESDAY, Convert.DateTime.toDayOfWeek[Calendar.TUESDAY - 1]);
        Assert.assertEquals(DayOfWeek.WEDNESDAY, Convert.DateTime.toDayOfWeek[Calendar.WEDNESDAY - 1]);
        Assert.assertEquals(DayOfWeek.THURSDAY, Convert.DateTime.toDayOfWeek[Calendar.THURSDAY - 1]);
        Assert.assertEquals(DayOfWeek.FRIDAY, Convert.DateTime.toDayOfWeek[Calendar.FRIDAY - 1]);
        Assert.assertEquals(DayOfWeek.SATURDAY, Convert.DateTime.toDayOfWeek[Calendar.SATURDAY - 1]);
    }

    private long dateTime(int year, Month month, int day, int hour, int minute, int second, int nanosecond) {
        return HdDateTimeUtils.newInstance(year, month, day, hour, minute, second, nanosecond);
    }

    private void checkGetDateGetTime(int year, Month month, int day, int hour, int minute, int second, int nanosecond) {
        long dateTime = HdDateTimeUtils.newInstance(year, month, day, hour, minute, second, nanosecond);
        long expectedDate = HdDateTimeUtils.newInstance(year, month, day);
        long expectedTime = HdTimeSpanUtils.newInstance(0, hour, minute, second, nanosecond);
        Assert.assertEquals("getDate", expectedDate, HdDateTimeUtils.getDate(dateTime));
        Assert.assertEquals("getTime", expectedTime, HdDateTimeUtils.getTimeOfDay(dateTime));
        Assert.assertEquals("from Date and Time", dateTime, HdDateTimeUtils.setTime(expectedDate, expectedTime));
        Assert.assertEquals("add Date and Time", dateTime, HdDateTimeUtils.add(expectedDate, expectedTime));
        Assert.assertEquals("add/sub Date and Time", expectedDate,
                HdDateTimeUtils.subtractTimeSpan(HdDateTimeUtils.add(expectedDate, expectedTime), expectedTime));
        Assert.assertEquals("add/sub Date and Time", expectedTime,
                HdDateTimeUtils.subtractDateTime(HdDateTimeUtils.add(expectedDate, expectedTime), expectedDate));
    }
}