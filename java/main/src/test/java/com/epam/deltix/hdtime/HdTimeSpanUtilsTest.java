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

import java.util.Random;

public class HdTimeSpanUtilsTest {
    @Test
    public void testRandomTimeSpans() {
        Random rnd = new Random(System.currentTimeMillis());
        for (int i = 0; i < 100_000; ++i) {
            int days = rnd.nextInt(80000) - 40000; // Guaranteed to fit month/year
            int hours = rnd.nextInt(20000) - 10000;
            int minutes = rnd.nextInt(20000) - 10000;
            int seconds = rnd.nextInt(20000) - 10000;
            int nanoseconds = rnd.nextInt();

            long dt = timeSpan(days, hours, minutes, seconds, nanoseconds);

            // Check getters
            Assert.assertEquals(days, HdTimeSpanUtils.getDays(timeSpan(days, 0, 0, 0, 0)));
            Assert.assertEquals(hours % 24, HdTimeSpanUtils.getHours(timeSpan(0, hours, 0, 0, 0)));
            Assert.assertEquals(minutes % 60, HdTimeSpanUtils.getMinutes(timeSpan(0, 0, minutes, 0, 0)));
            Assert.assertEquals(seconds % 60, HdTimeSpanUtils.getSeconds(timeSpan(0, 0, 0, seconds, 0)));
            Assert.assertEquals(nanoseconds % 1000000000, HdTimeSpanUtils.getNanoseconds(timeSpan(0, 0, 0, 0, nanoseconds)));

            Assert.assertEquals((double)days, HdTimeSpanUtils.totalDays(timeSpan(days, 0, 0, 0, 0)), 1E-9);
            Assert.assertEquals((double)hours, HdTimeSpanUtils.totalHours(timeSpan(0, hours, 0, 0, 0)), 1E-9);
            Assert.assertEquals((double)minutes, HdTimeSpanUtils.totalMinutes(timeSpan(0, 0, minutes, 0, 0)), 1E-9);
            Assert.assertEquals((double)seconds, HdTimeSpanUtils.totalSeconds(timeSpan(0, 0, 0, seconds, 0)), 1E-9);
            Assert.assertEquals((double)nanoseconds, HdTimeSpanUtils.totalNanoseconds(timeSpan(0, 0, 0, 0, nanoseconds)), 1E-9);
        }
    }

    private long timeSpan(int days, int hours, int minutes, int seconds, int nanoseconds) {
        return HdTimeSpanUtils.newInstance(days, hours, minutes, seconds, nanoseconds);
    }
}