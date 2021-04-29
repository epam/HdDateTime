package com.epam.deltix.hdtime;

public interface Formattable {
    /**
     *
     * @param to destination character array. Should be big enough to hold the resulting value
     * @param ofs write offset
     * @param components date/time components
     * @return new write offset value
     */
    int format(char[] to, int ofs, Components components);
}
