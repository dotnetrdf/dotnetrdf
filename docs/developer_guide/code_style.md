# Code Style

This document covers the Code Style used in dotNetRDF, if you are contributing patches, bug fixes or other code to dotNetRDF we much prefer it if you keep to the following guidelines and we will likely reformat code that does not match these guidelines.

Broadly speaking we follow the standard [Design Guidelines for Developing Class Libraries](http://msdn.microsoft.com/en-us/library/ms229042.aspx) but there are some deviations as noted on this page.

## Naming Conventions

### Interfaces

Interfaces must always start with a `I` in their name e.g. `IGraph`

### Abstract Base Classes

Abstract Base classes should be named with a leading `Base` - this is contrary to the standard .Net guidelines but is our preference.

### Method and Property Names

All method and property names should start with an upper case character

### Field Names

Field names should start with an underscore

## Class Coding Conventions

### Extension and Interface definitions

The extension and interface definitions should be on a separate line to the class name and indented e.g.

```csharp
public class Example
	: IExampleInterface
```

### Auto-implemented vs Manually implemented Properties

Historically we tended to manually implement all properties, with newer code we now use auto-implemented properties wherever appropriate.

## General Coding Conventions

### Brace Style

When using braces e.g. if blocks, for loops etc we indent as follows:

```csharp

if (condition)
{
  //Code
}
else if (condition)
{
  //Code
}
else
{
  //Code
}

foreach (X x in xs)
{
  //Code
}

while (x > 0)
{
  //Code
}

do
{
  //Code
} while (condition);
```

Braces are omitted from if statements only when there is no else/else if branches and the action to take if the condition evaluates to true is a single line statement given on the same line e.g.

```csharp
if (condition) statement;
```