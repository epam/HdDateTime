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

import java.util.function.BinaryOperator;

public class MathTest {

    static String strSubtract(long a, long b) {
        return String.format("%d - %d", a, b);
    }

    static void expectOverflow(BinaryOperator<Long> f, String name, long a, long b) {
        try {
            f.apply(a, b);
        }
        catch (ArithmeticException e) {
            return;
        }

        Assert.fail(name + " was expected to fail");
    }

    public static class SubtractToTs {
        @Test
        public void test() {
            ok(-2, -1, 1);
            ok(HdDateTimeUtils.MIN_VALUE, HdDateTimeUtils.MIN_VALUE, 0);

            ok(0, 0, 0);
            ok(990, 1000, 10);

            ok(HdDateTimeUtils.MAX_VALUE, HdDateTimeUtils.MAX_VALUE, 0);
            ok(-HdDateTimeUtils.MIN_VALUE, 0, HdDateTimeUtils.MIN_VALUE);
            ok(-HdDateTimeUtils.MAX_VALUE, 0, HdDateTimeUtils.MAX_VALUE);

            ok(HdDateTimeUtils.MIN_VALUE + 1, HdDateTimeUtils.MIN_VALUE, -1);
            ok(HdDateTimeUtils.MAX_VALUE + 1, HdDateTimeUtils.MAX_VALUE, -1);
            ok(HdDateTimeUtils.MIN_VALUE - 1, HdDateTimeUtils.MIN_VALUE, 1);
            ok(HdDateTimeUtils.MAX_VALUE - 1, HdDateTimeUtils.MAX_VALUE, 1);

            ok(0, HdDateTimeUtils.MAX_VALUE, HdDateTimeUtils.MAX_VALUE);
            ok(0, HdDateTimeUtils.MIN_VALUE, HdDateTimeUtils.MIN_VALUE);

            // There are no source range checks, we are only concerned about getting valid TimeSpan,
            // therefore these tests don't fail
            // Note that Long.MIN_VALUE should not be passed anyway, we only guarantee it is not returned
            ok(Long.MAX_VALUE, Long.MAX_VALUE, 0);
            ok(-Long.MAX_VALUE, -Long.MAX_VALUE, 0);
            ok(-Long.MAX_VALUE, 0, Long.MAX_VALUE);
            ok(Long.MAX_VALUE, 0, -Long.MAX_VALUE);
            ok(0, Long.MAX_VALUE, Long.MAX_VALUE);
            ok(0, -Long.MAX_VALUE, -Long.MAX_VALUE);
            ok(-Long.MAX_VALUE + 1, 1, Long.MAX_VALUE);
            ok(Long.MAX_VALUE - 1, Long.MAX_VALUE, 1);

            fail(Long.MIN_VALUE, 0);
            fail(Long.MIN_VALUE, 1);
            fail(Long.MIN_VALUE, Long.MAX_VALUE);
            fail(-Long.MAX_VALUE, 1);
            fail(-Long.MAX_VALUE, 2);
            fail(-Long.MAX_VALUE, Long.MAX_VALUE);

            fail(Long.MAX_VALUE, Long.MIN_VALUE);
            fail(Long.MAX_VALUE, -1);
            fail(Long.MAX_VALUE, -Long.MAX_VALUE);
            fail(Long.MAX_VALUE, Long.MIN_VALUE);

            fail(0, Long.MIN_VALUE);
            fail(1, Long.MIN_VALUE);
            fail(-1, Long.MAX_VALUE);
        }

        private void ok(long result, long a, long b) {
            Assert.assertEquals(strSubtract(a, b), result, Util.subtractToTs(a, b));
        }

        private void fail(long a, long b) {
            expectOverflow((aa, bb) -> Util.subtractToTs(aa, bb), strSubtract(a, b), a, b);
        }
    }
}