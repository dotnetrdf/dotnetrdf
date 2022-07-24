# Leviathan Function Library

The Leviathan Function Library is a library of extension functions for SPARQL which embody a large number of numeric functions and a few string functions that can be used in queries executed by dotNetRDF.

## Namespace

The namespace for the Leviathan Function Library is `http://www.dotnetrdf.org/leviathan` and the recommended prefix is `lfn`

The following is an example query using a function from the library to calculate the MD5 Hashes of each subject in the data you are querying:

```sparql
PREFIX lfn: <http://www.dotnetrdf.org/leviathan#>

SELECT lfn:md5hash(STR(?s)) AS ?SubjectHash WHERE {?s ?p ?o} GROUP BY ?s
```

# Available Functions

## Aggregate Functions

The library contains the following custom aggregate functions. Not that if a query does not contain a `GROUP BY` clause there is a single implicit group which is the entire result set.

### lfn:all(expr)

Returns true/false as a boolean literal to indicate whether the expression evaluates to true for all results in the group

### lfn:any(expr)

Returns true/false as a boolean literal to indicate whether the expression evaluates to true for any result in the group

### lfn:median(expr)

Returns the Node which is the median value for the expression according to SPARQL sort order. Results where the expression results in an error are ignored and the result may be a null if the expression errors for all results or if it returns null for more than half the results in the group.

This is not a true numeric median, in the event that there is an even number of results the two middle values are not averaged because there is no way to do this as the middle values may not even be Literal Nodes.

### lfn:mode(expr)

Returns the Node which is the most popular value for the expression. This can be null if the expression evaluates to null/errors for the majority of results in the group.

### lfn:nmax(expr)

Returns a numeric typed Literal Node which represents the maximum numeric value of the expression as evaluated for the results in the group. If none of the results in the group returns a numeric value then an error will occur.

### lfn:nmin(expr)

Returns a numeric typed Literal Node which represents the minimum numeric value of the expression as evaluated for the results in the group. If none of the results in the group returns a numeric value then an error will occur.

### lfn:none(expr)

Returns true/false as a boolean literal to indicate whether the expression evaluates to false for all results in the group i.e. this is the inverse of lfn:all()

## Numeric Functions

The library provides an extensive range of numeric functions which are detailed below:

### cartesian(x1,y1,x2,y2)

Calculates the distance between two pairs of points assuming a cartesian coordinate system. There is also a 3D version of this function which takes the form `cartesian(x1,y1,z1,x2,y2,z2)`

### cube(expr)

Calculates the cube of an expression i.e. `expr * expr * expr`

### e(expr)

Calculates `e` (the natural logarithm root) raised to the power of the expression i.e. `e^expr`

### factorial(expr)

Calculates the factorial of the given expression

### log(expr)

Calculates the base 10 logarithm of the given expression, there is also a two argument version that takes an arbitrary base `log(expr,base)`

### ln(expr)

Calculate the natural logarithm (`log` to the base `e`) of the expression

### pow(expr,pow)

Calculates the expression raised to the given power i.e. `expr^pow`

### pythagoras(expr,expr)

Calculates the hypotenuse of a right angle triangle, the arguments represent the length of the two other sides of the triangle.

### reciprocal(expr)

Calculates the reciprocal of an expression i.e. `1 / expr`

### rnd()

Returns a random number in the range `0`-`1` as an `xsd:double`, superceded by the `RAND()` function in SPARQL 1.1

### rnd(max)

Returns a random number in the range `0`-`max` as an `xsd:double`

### rnd(min,max)

Returns a random number in the range `min`-`max` as an `xsd:double`

### root(expr,n)

Calculates the nth root of the expression

### sq(expr)

Calculates the square of the expression i.e. `expr * expr`

### sqrt(expr)

Calculates the square root of the expression

### ten(expr)

Calculates 10 raised to the power of the expresison i.e. `10^expr`

## Trigonometric Functions

The Library also provides a suite of trigonometric functions. All these functions operate in radians but we provide conversion functions between radians and degrees that can be used if necessary.

### cos(expr)

Calculates the cosine of the expression

### cos-1(expr)

Calculates the inverse cosine of the expression

### cosec(expr)

Calculates the cosecant of the expression

### cosec-1(expr)

Calculates the inverse cosecant of the expression

### cotan(expr) 

Calculates the cotangent of the expression

### cotan-1(expr)

Calculates the inverse cotangent of the expression

### degrees-to-radians(expr)

Converts the given expression from a number in degrees to radians

### radians-to-degrees(expr)

Converts the given expression from a number in radians to degrees

### sec(expr)

Calculates the secant of the expression

### sec-1(expr)

Calculates the inverse secant of the expression

### sin(expr)

Calculates the sine of the expression

### sin-1(expr)

Calculates the inverse sine of the expression

### tan(expr)

Calculates the tangent of the expression

### tan-1(expr)

Calculates the inverse tangent of the expression

## String Functions

We also provide a couple of potentially useful string functions:

### md5hash(expr)

Calculates the MD5 Hash of the expression provided the input is an `xsd:string`, superseded by the `MD5()` function in SPARQL 1.1

### sha256hash(expr)

Calculates the SHA256 Hash of the expression provided the input is an `xsd:string`, superseded by the `SHA256()` function in SPARQL 1.1