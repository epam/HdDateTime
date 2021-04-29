package com.epam.deltix.hdtime;

interface Parseable {
    int parse(CharSequence from, int ofs, ParsedValue dst) throws ParseException;
}
