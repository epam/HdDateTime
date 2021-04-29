package com.epam.deltix.hdtime;

public class ParsedDateTimeValue extends ParsedValue {
    int year;       // Year, always 4 digit integer
    int month;      // Month of year, [0..11] !!
    int day;        // Day of month. [1..31]

    @Override
    long get() {
        return Convert.DateTime.from(year, month, day) + x;
    }

    @Override
    public void reset() {
        x = 0;
        year = month = day = 1;
    }
}

