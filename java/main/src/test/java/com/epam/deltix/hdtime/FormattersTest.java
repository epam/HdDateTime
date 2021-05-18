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

public class FormattersTest {
    @Test
    public void testFieldEquality() {
        Assert.assertEquals(new Formatters.SecondsField2().hashCode(), new Formatters.SecondsField2().hashCode());
        Assert.assertEquals(new Formatters.SecondsField2w02().hashCode(), new Formatters.SecondsField2w02().hashCode());
        Assert.assertNotEquals(new Formatters.SecondsField2w02().hashCode(), new Formatters.SecondsField2().hashCode());

        Assert.assertEquals(new Formatters.StringField("123456abc").hashCode(), new Formatters.StringField("123456abc").hashCode());
        Assert.assertEquals(new Formatters.StringField("123456abc"), new Formatters.StringField("123456abc"));
        Assert.assertNotEquals(new Formatters.StringField("123456Abc").hashCode(), new Formatters.StringField("123456abc").hashCode());
        Assert.assertNotEquals(new Formatters.StringField("123456Abc"), new Formatters.StringField("123456abc"));
    }
}