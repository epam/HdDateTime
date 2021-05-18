# HdDateTime

Date & Time Classes for Java & .NET
Have nanosecond precision, mostly match the functionality of .NET DateTime/TimeSpan, but have no timezone support and greater performance.

## Implementation details

### HdDateTime
Represents UTC Date/Time relative to Unix epoch (1970-01-01 00:00:00 UTC) as a single 64-bit long value.

Range: 1678-01-01 00:00:00.000000000 - 2261-12-31 23:59:59.999999999

The values outside this range are reserved, in particular, `Long.MIN_VALUE`

### HdTimeSpan
Represents signed time offset in nanoseconds as a single 64-bit long value.

`Long.MIN_VALUE` is reserved.

### Java & ValueType agent

Java APIs are implemented as immutable classes `HdDateTime` / `HdTimeSpan` and as classes `HdDateTimeUtils` / `HdTimeSpanUtils` that only have static methods that operate on `long` values.

ValueType Java agent is able to transform the code working with `HdDateTime` / `HdTimeSpan` instances into code working with 'long' basic type via `HdDateTimeUtils` / `HdTimeSpanUtils`.


### Formatting and parsing
Java and C# currently have different but mostly compatible implementations of formatting and parsing methods.

#### HdTimeSpan/HdDateTime formatting/parsing (.NET)

The implementation delegates parsing/formatting to standard DateTime/TimeSpan methods but extends `f` field maximum length to 9 digits.

#### HdTimeSpan formatting/parsing (Java)

Java formatter is a custom high performance implementation that supports simplified format strings that are mostly compatible with both .NET `TimeSpan` and Java 8 `DateTimeFormatter`. The feature not supported by standard implementations is the automatic sign field insertion.

The following methods are supported:
* `String toString()`
* `String toString(String format)`
* `Appendable appendTo(Appendable appendable)`
* `Appendable appendTo(Appendable appendable, String format)`
* `static HdTimeSpan parse(CharSequence text) throws ParseException`
* `static HdTimeSpan parse(CharSequence text, String fmt) throws ParseException`


The following letters are supported:

* `d` - days count 0..MAX_VALUE.
* `h` - hours [0..23]
* `m` - minutes [0..59]
* `s` - seconds [0..59]
* `f`/`S` - fractions of second, length: [1..9], constant length, always zero padded. Field length over 9 is an error.
* `'` lets you directly include any text.
* `''` adds `'` character.
* TODO: `\'` also adds `'` or any other character, including '\'. For .NET compatibility.

Any unrecognized characters are included literally. Printed by the formatter, expected by the parser.

Sign field is automatically inserted before the *first* non-static field of the format string. If necessary, it is put *before any zero padding* before the field, *even if it is specified as a string*. The sign field is an optional `-` character.

Single occurrence of any format character means field of arbitrary length. Formatted value is printed with actual length, parsed value is read until 1st non-digit character.

2 or more occurrences of any format character means fixed length. Printed fields are zero-padded, parsed fields are expected to contain the exact amount of characters specified. The length can be arbitrary, but any extra characters will expected to be `0` by the parser and printed as `0`. If the printed value (except fractions) is longer than the specified length, it is *not* truncated. For `HdTimeSpan` this only applies to days count.

A complete TimeSpan can be constructed from any single field that parses at least a single digit.

#### HdDateTime formatting/parsing (Java)
Java formatter is a custom high performance implementation that supports simplified format strings that are mostly compatible with both .NET `DateTime` and Java 8 `DateTimeFormatter`.

The following methods are supported:
* `String toString()`
* `String toString(String format)`
* `Appendable appendTo(Appendable appendable)`
* `Appendable appendTo(Appendable appendable, String format)`
* `static HdDateTime parse(CharSequence text) throws ParseException`
* `static HdDateTime parse(CharSequence text, String fmt) throws ParseException`

The following letters are supported:

* `G`/`g` - epoch. **`AD`** is always printed
* `y` - year. Due to `HdDateTime` range limitations, it always has 4 significant digits, unless `'yy'` is specified in which case only the last 2 digits are printed and only years 2000-2099 are parsed.
* `M` - month of year [1..12] Length of 3 produces short month name. Length of 4 produces long month name. Length above 4 produces long month name optionally left-padded with spaces to the specified minimum length.
* `d` - day of month [1..31]

The rest are the same as in `HdTimeSpan`, except there is no sign field.
When parsing `HdDateTime`, every format string must include year, every format string that includes day of month must also include month and year, etc. Basically, for any specific field to be present, the higher magnitude fields must also be present. The order in which the fields are included doesn't matter.

Exception will be thrown if it is impossible to construct a valid date or if there are conflicting (repeated) fields.


#### Known limitations for DateTime and TimeSpan parsing/formatting (Java)
* No way to specify non-zero-padded fractions field, it will always be padded by zeroes from the right.
* No way to omit fractions part together with decimal separator if it does not present.
* Always 24 hour UTC time.
* No localization of any kind.
* No week, day of year printing/formatting.
* No way to omit days/hours/etc. fields and optional separators, if these fields are zero.
* No option to skip whitespace in the parser, or ignore case when comparing the input with the format string.
* TimeSpan: No manual specification for sign field, no fixed-length sign field. It is either `-` or nothing.
* TimeSpan: If higher order fields are absent from the format string, such as days, the range limits for the lower order fields remain. If there is no 'days' field, you still can't specify '100 hours'.
* DateTime: Parsing is somewhat limited, does not parse textual month names.

#### HdDateTime parse format string examples
TimeSpan:
* `ss.ffff` - two digit seconds, dot, 4 digit fractional part
* `s.fffffffff` - seconds(least one digit) a dot character and 9 digit fractional part (nanoseconds)
* `d'T'H:m:s.ffffff` - days(least one digit), 'T' character, hours, minutes, seconds (1-2 digits), a dot cahracter, 6 digit fraction (microseconds)
* `ddHHmmssffffff` - days, hours, minutes, seconds (exactly 2 digits), 6 digit fraction (microseconds) without separators. There still can be optional `-` in the beginning.
