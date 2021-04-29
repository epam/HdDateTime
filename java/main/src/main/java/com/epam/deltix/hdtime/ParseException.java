package com.epam.deltix.hdtime;

class ParseException extends Exception {
    public ParseException(String string, int i) {
        super(i < string.length()
                ? String.format("Unable to parse: '%s[%s]'", string.substring(0, i), string.substring(i))
                : String.format("Unable to parse: '%s' at index %d", string, i));

    }
}
