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
