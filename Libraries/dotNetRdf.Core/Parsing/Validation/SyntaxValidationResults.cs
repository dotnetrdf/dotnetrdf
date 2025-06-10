/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.Collections.Generic;

namespace VDS.RDF.Parsing.Validation;

/// <summary>
/// Represents Syntax Validation Results.
/// </summary>
public class SyntaxValidationResults : ISyntaxValidationResults
{
    private bool _valid;
    private string _message;
    private List<string> _warnings = new List<string>();
    private Exception _error;
    private object _result;

    /// <summary>
    /// Creates new Syntax Validation Results.
    /// </summary>
    /// <param name="valid">Whether the Syntax was valid.</param>
    /// <param name="message">Validation Message.</param>
    public SyntaxValidationResults(bool valid, string message)
    {
        _valid = valid;
        _message = message;
    }

    /// <summary>
    /// Creates new Syntax Validation Results.
    /// </summary>
    /// <param name="valid">Whether the Syntax was valid.</param>
    /// <param name="message">Validation Message.</param>
    /// <param name="result">Results Object.</param>
    public SyntaxValidationResults(bool valid, string message, object result)
        : this(valid, message)
    {
        _result = result;
    }

    /// <summary>
    /// Creates new Syntax Validation Results.
    /// </summary>
    /// <param name="valid">Whether the Syntax was valid.</param>
    /// <param name="message">Validation Message.</param>
    /// <param name="result">Results Object.</param>
    /// <param name="warnings">Enumeration of Warnings.</param>
    public SyntaxValidationResults(bool valid, string message, object result, IEnumerable<string> warnings)
        : this(valid, message, result)
    {
        _warnings.AddRange(warnings);
    }

    /// <summary>
    /// Creates new Syntax Validation Results.
    /// </summary>
    /// <param name="valid">Whether the Syntax was valid.</param>
    /// <param name="message">Validation Message.</param>
    /// <param name="result">Results Object.</param>
    /// <param name="warnings">Enumeration of Warnings.</param>
    /// <param name="error">Error that occurred.</param>
    public SyntaxValidationResults(bool valid, string message, object result, IEnumerable<string> warnings, Exception error)
        : this(valid, message, result, warnings)
    {
        _error = error;
    }

    /// <summary>
    /// Creates new Syntax Validation Results.
    /// </summary>
    /// <param name="valid">Whether the Syntax was valid.</param>
    /// <param name="message">Validation Message.</param>
    /// <param name="error">Error that occurred.</param>
    public SyntaxValidationResults(bool valid, string message, Exception error)
        : this(valid, message)
    {
        _error = error;
    }

    /// <summary>
    /// Creates new Syntax Validation Results.
    /// </summary>
    /// <param name="message">Validation Message.</param>
    /// <param name="error">Error that occurred.</param>
    public SyntaxValidationResults(string message, Exception error)
        : this(error == null, message, error) { }

    /// <summary>
    /// Whether the Syntax was valid.
    /// </summary>
    public bool IsValid
    {
        get 
        {
            return _valid; 
        }
    }

    /// <summary>
    /// Gets the Validation Message.
    /// </summary>
    public string Message
    {
        get 
        {
            return _message;
        }
    }

    /// <summary>
    /// Gets the Warnings that were produced.
    /// </summary>
    public IEnumerable<string> Warnings
    {
        get 
        {
            return _warnings;
        }
    }

    /// <summary>
    /// Gets the Error that occurred.
    /// </summary>
    public Exception Error
    {
        get 
        {
            return _error; 
        }
    }

    /// <summary>
    /// Gets the Result Object that was produced.
    /// </summary>
    public object Result
    {
        get
        {
            return _result;
        }
    }
}
