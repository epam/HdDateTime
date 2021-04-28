package com.epam.deltix.hdtime;

// Formatable components
class Components {
    int sign;       // Sign mask (0 or -1)
    int year;       // Year, always 4 digit integer
    int month;      // Month of year, [1..12]
    int day;        // Day of month. [1..31]
    int hour;       // Hour of day [0..24]
    int minute;     // Minute of hour [0..59]
    int second;     // Seconds of minute [0..59]
    int nanosecond; // Nanosecond of second [0..999999999]
}