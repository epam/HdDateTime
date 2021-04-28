package com.epam.deltix.hdtime;

class ParsedValue {
    long x;         // TimeSpan value without sign
    byte sign;      // Sign mask, 0 or -1

    long get() {
        long x = this.x;
        long mask = this.sign;
        return x + mask ^ mask;
    }

    public void reset() {
        x = 0;
        sign = 0;
    }
}
