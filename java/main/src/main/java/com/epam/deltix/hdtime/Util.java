package com.epam.deltix.hdtime;

class Util {
    static void throwOutOfRange() {
        throw new IllegalArgumentException();
    }

    static void additionOverflow() {
        throw new ArithmeticException("Signed Time Interval addition overflow");
    }

    static void subtractionOverflow() {
        throw new ArithmeticException("Signed Time Interval subtraction overflow");
    }

    static boolean inRange(final int x, final int lowerBound, final int upperBound) {
        //return x >= lowerBound && x <= upperBound;
        return (x + (Integer.MIN_VALUE - lowerBound)) < (Integer.MIN_VALUE + 1 + upperBound - lowerBound);
    }

    // Range check, closed range is used
    static boolean inRange(final long x, final long lowerBound, final long upperBound) {
        //return x >= lowerBound && x <= upperBound;
        return (x + (Long.MIN_VALUE - lowerBound)) < (Long.MIN_VALUE + 1 + upperBound - lowerBound);
    }

/*
    static long null2sign(long a) {
        assert (Convert.NULL & -Convert.NULL) < 0;
        return -a & a;
    }

    static long null2sign(long a, long b) {
        assert (Convert.NULL & -Convert.NULL) < 0;
        return (-a & a) | (-b & b);
    }

    static long null2sign(long a, long b, long c) {
        assert (Convert.NULL & -Convert.NULL) < 0;
        return (-a & a) | (-b & b) | (-c & c);
    }

    static boolean anyIsNull(long a, long b) {
        // a == NULL || b == NULL
        return null2sign(a, b) < 0;
    }

    static boolean anyIsNull(long a, long b, long c) {
        // a == NULL || b == NULL || c == NULL
        return null2sign(a, b, c) < 0;
    }
    */

    // Subtract with basic overflow check. Can return Long.MIN_VALUE if it was obtained without overflow
    static long subtract(long a, long b) {
        long x = a - b;
        if (((a ^ b) & (a ^ x)) < 0)
            subtractionOverflow();

        return x;
    }

    static long subtractToDt(long a, long b) {
        long x = a - b;
        if (((a ^ b) & (a ^ x)) < 0 | !Convert.DateTime.isValidNanos(x))
            subtractionOverflow();

        return x;
    }

    static long subtractToTs(long a, long b) {
        long x = a - b;
        if (((a ^ b) & (a ^ x) | (x & -x)) < 0)
            subtractionOverflow();

        return x;
    }

    private static void checkSubtractOppositeSigns(long result, long a) {
        if (((a | result) & (-a | -result)) < 0)
            subtractionOverflow();
    }

    // Subtract 2 DateTime or 2 TimeSpan values, returning TimeSpan. Will never return Long.MIN_VALUE
    // Optimized for the case where arguments have the same sign bit (it can't cause overflow or return NULL_VALUE)
    static long subtractToTsOld(long a, long b) {
        long x = a - b;
        if ((a ^ b) < 0)
            checkSubtractOppositeSigns(x, a);

        return x;
    }

    static long add(long a, long b) {
        long x = a + b;
        if (((a ^ ~b) & (a ^ x)) < 0)
            additionOverflow();

        return x;
    }

    static long addToDt(long a, long b) {
        long x = a + b;
        if (((a ^ ~b) & (a ^ x)) < 0 | !Convert.DateTime.isValidNanos(x))
            additionOverflow();

        return x;
    }

    static long addToTs(long a, long b) {
        long x = a + b;
        if (((a ^ ~b) & (a ^ x) | x & -x) < 0)
            additionOverflow();

        return x;
    }

    static long addSpecial(long a, long b) {
        long x = a + b;
        if (x < a)
            throwOutOfRange();

        return x;
    }
}
