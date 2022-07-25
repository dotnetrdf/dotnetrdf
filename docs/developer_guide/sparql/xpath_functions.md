# XPath Function Library

The XPath Function Library is a library of functions which form part of the W3C specification for XPath. Please follow the appropriate links to see the documentation for the function, we have noted where we don't support all forms of a function. As a general rule for string functions which have forms which support a collation argument we do not support that form.

**Note:** Many of these functions now have built-in equivalents in SPARQL 1.1 which often offer better functionality since they are slightly modified to work with RDF where appropriate.

# Namespace

The namespace for XPath functions is `http://www.w3.org/2005/xpath-functions/#` and the preferred prefix is `fn`

# Available Functions

dotNetRDF supports a subset of this library which is as follows:

## Aggregate Functions

| XPath Function | SPARQL Equivalent | SPARQL Version | Description |
|----------------|-------------------|----------------|-------------|
| [fn:string-join()](http://www.w3.org/2005/xpath-functions/#string-join) | `GROUP_CONCAT()` | 1.1 | Joins the members of a group into a string with the given selector |

## Boolean Functions

| XPath Function | SPARQL Equivalent | SPARQL Version | Description |
|----------------|-------------------|----------------|-------------|
| [fn:not()](http://www.w3.org/2005/xpath-functions/#not) | `!` | 1.0 | Equivalent to the `!` operator i.e. logical negation of the expression it is applied to |
| [fn:boolean()](http://www.w3.org/2005/xpath-functions/#boolean) | | 1.0 | Computes the effective boolean value of the expression it is applied to.  SPARQL automatically computes effective boolean value when necessary |
| [fn:true()](http://www.w3.org/2005/xpath-functions/#true) | `true` | 1.0 | Equivalent to the plain literal `true`
| [fn:false()](http://www.w3.org/2005/xpath-functions/#false) | `false` | 1.0 | Equivalent to the plain literal `false` |

## Date Time Functions

| XPath Function | SPARQL Equivalent | SPARQL Version | Description |
|----------------|-------------------|----------------|-------------|
| [fn:year-from-dateTime()](http://www.w3.org/2005/xpath-functions/#year-from-dateTime) | `YEAR()` | 1.1 | Returns the year portion of a Date Time |
| [fn:month-from-dateTime()](http://www.w3.org/2005/xpath-functions/#month-from-dateTime) | `MONTH()` | 1.1 | Returns the month portion of a Date Time |
| [fn:day-from-dateTime()](http://www.w3.org/2005/xpath-functions/#day-from-dateTime) | `DAY()` | 1.1 | Returns the day portion of a Date Time |
| [fn:hours-from-dateTime()](http://www.w3.org/2005/xpath-functions/#hours-from-dateTime) | `HOURS()` | 1.1 | Returns the hours portion of a Date Time |
| [fn:minutes-from-dateTime()](http://www.w3.org/2005/xpath-functions/#minutes-from-dateTime) | `MINUTES()` | 1.1 | Returns the minutes portion of a Date Time |
| [fn:seconds-from-dateTime()](http://www.w3.org/2005/xpath-functions/#seconds-from-dateTime) | `SECONDS()` | 1.1 | Returns the seconds portion of a Date Time |
| [fn:timezone-from-dateTime()](http://www.w3.org/2005/xpath-functions/#timezone-from-dateTime) | `TIMEZONE()` and `TZ()` | 1.1 | Returns the time zone of a Date Time |

## Numeric Functions

| XPath Function | SPARQL Equivalent | SPARQL Version | Description |
|----------------|-------------------|----------------|-------------|
| [fn:abs()](http://www.w3.org/2005/xpath-functions/#abs) | `ABS()` | 1.1 | Returns the absolute value of the expression |
| [fn:ceiling()](http://www.w3.org/2005/xpath-functions/#ceiling) | `CEIL()` | 1.1 | Returns the value of the expression rounded up to a whole number |
| [fn:floor()](http://www.w3.org/2005/xpath-functions/#floor) | `FLOOR()` | 1.1 | Returns the value of the expression rounded down to a whole number |
| [fn:round()](http://www.w3.org/2005/xpath-functions/#round) | `ROUND()` | 1.1 | Returns the value of the expression rounded to the nearest whole number |
| [fn:round-half-to-even()](http://www.w3.org/2005/xpath-functions/#round-half-to-even) | | | Returns the value of the expression rounded to the nearest whole number, where the number is exactly between two whole numbers it rounds to the nearest even number. |

## String Functions

| XPath Function | SPARQL Equivalent | SPARQL Version | Description |
|----------------|-------------------|----------------|-------------|
| [fn:matches()](http://www.w3.org/2005/xpath-functions/#matches) | `REGEX()` | 1.0 | Equivalent to the `REGEX` function |
| [fn:contains()](http://www.w3.org/2005/xpath-functions/#contains) | `CONTAINS()` | 1.1 | Determines whether a given string contains another string. |
| [fn:starts-with()](http://www.w3.org/2005/xpath-functions/#starts-with) | `STRSTARTS()` | 1.1 | Determines whether a given string starts with another string. |
| [fn:ends-with()](http://www.w3.org/2005/xpath-functions/#ends-with) | `STRENDS()` | 1.1 | Determines whether a given string ends with another string. |
| [fn:string-length()](http://www.w3.org/2005/xpath-functions/#string-length) | `STRLEN()` | 1.1 | Returns the length of the given string. |
| [fn:concat()](http://www.w3.org/2005/xpath-functions/#concat) | `CONCAT()` | 1.1 | Concatenates any number of string arguments without a separator |
| [fn:substring()](http://www.w3.org/2005/xpath-functions/#substring) | `SUBSTR()` | 1.1 | Returns a substring from a string, uses a 1 based index |
| [fn:substring-before()](http://www.w3.org/2005/xpath-functions/#substring-before) | `STRBEFORE()` | 1.1 | Gets the part of the string that occurs before a given string |
| [fn:substring-after()](http://www.w3.org/2005/xpath-functions/#substring-after) | `STRAFTER()` | 1.1 | Gets the part of the string that occurs before a given string |
| [fn:normalize-space()](http://www.w3.org/2005/xpath-functions/#normalize-space) | | | Normalizes space in a string |
| [fn:normalize-unicode()](http://www.w3.org/2005/xpath-functions/#normalize-unicode) | | | Normalizes a string to a given Unicode normalization form, not all forms are supported |
| [fn:upper-case()](http://www.w3.org/2005/xpath-functions/#upper-case) | `UCASE()` | 1.1 | Converts a string to upper case |
| [fn:lower-case()](http://www.w3.org/2005/xpath-functions/#lower-case) | `LCASE()` | 1.1 | Converts a string to lower case |
| [fn:encode-for-uri()](http://www.w3.org/2005/xpath-functions/#encode-for-uri) | `ENCODE_FOR_URI()` | 1.1 | Encodes a String for use in a URI |
| [fn:escape-html-uri()](http://www.w3.org/2005/xpath-functions/#escape-html-uri) | | | Escapes a URI for insertion in HTML |
| [fn:replace()](http://www.w3.org/2005/xpath-functions/#replace) | `REPLACE()` | 1.1 | Performs a regular expression based replace on strings |
| [fn:compare()](http://www.w3.org/2005/xpath-functions/#compare) | | | Compares two strings and returns -1, 0 or 1 to indicate their relative ordering |