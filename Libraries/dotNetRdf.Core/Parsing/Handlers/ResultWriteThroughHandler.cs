/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2026 dotNetRDF Project (http://dotnetrdf.org/)
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
using System.IO;
using System.Linq;
using System.Reflection;
using VDS.RDF.Query;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Parsing.Handlers;

/// <summary>
/// A Results Handler which writes the handled Results out to a <see cref="TextWriter">TextWriter</see> using a provided <see cref="IResultFormatter">IResultFormatter</see>.
/// </summary>
public class ResultWriteThroughHandler 
    : BaseResultsHandler
{
    private readonly Type _formatterType;
    private IResultFormatter _formatter;
    private TextWriter _writer;
    private readonly bool _closeOnEnd;
    private INamespaceMapper _formattingMapper = new QNameOutputMapper();
    private SparqlResultsType _currentType = SparqlResultsType.Boolean;
    private List<string> _currVariables = [];
    private bool _headerWritten;

    /// <summary>
    /// Creates a new Write-Through Handler.
    /// </summary>
    /// <param name="formatter">Triple Formatter to use.</param>
    /// <param name="writer">Text Writer to write to.</param>
    /// <param name="closeOnEnd">Whether to close the writer at the end of RDF handling.</param>
    public ResultWriteThroughHandler(IResultFormatter formatter, TextWriter writer, bool closeOnEnd)
    {
        _formatter = formatter ?? new NTriplesFormatter();
        _writer = writer ?? throw new ArgumentNullException(nameof(writer), "Cannot use a null TextWriter with the Result Write Through Handler");
        _closeOnEnd = closeOnEnd;
    }

    /// <summary>
    /// Creates a new Write-Through Handler.
    /// </summary>
    /// <param name="formatter">Triple Formatter to use.</param>
    /// <param name="writer">Text Writer to write to.</param>
    public ResultWriteThroughHandler(IResultFormatter formatter, TextWriter writer)
        : this(formatter, writer, true) { }

    /// <summary>
    /// Creates a new Write-Through Handler.
    /// </summary>
    /// <param name="formatterType">Type of the formatter to create.</param>
    /// <param name="writer">Text Writer to write to.</param>
    /// <param name="closeOnEnd">Whether to close the writer at the end of RDF handling.</param>
    public ResultWriteThroughHandler(Type formatterType, TextWriter writer, bool closeOnEnd)
    {
        _formatterType = formatterType ?? throw new ArgumentNullException(nameof(formatterType), "Cannot use a null formatter type");
        _writer = writer ?? throw new ArgumentNullException(nameof(writer), "Cannot use a null TextWriter with the Result Write Through Handler");
        _closeOnEnd = closeOnEnd;
    }

    /// <summary>
    /// Creates a new Write-Through Handler.
    /// </summary>
    /// <param name="formatterType">Type of the formatter to create.</param>
    /// <param name="writer">Text Writer to write to.</param>
    public ResultWriteThroughHandler(Type formatterType, TextWriter writer)
        : this(formatterType, writer, true) { }

    /// <summary>
    /// Starts writing results.
    /// </summary>
    protected override void StartResultsInternal()
    {
        if (_closeOnEnd && _writer == null) throw new RdfParseException("Cannot use this ResultWriteThroughHandler as an Results Handler for parsing as you set closeOnEnd to true and you have already used this Handler and so the provided TextWriter was closed");
        _currentType = SparqlResultsType.Unknown;
        _currVariables.Clear();
        _headerWritten = false;

        if (_formatterType != null)
        {
            _formatter = null;
            _formattingMapper = new QNameOutputMapper();

            // Instantiate a new Formatter
            ConstructorInfo[] cs = _formatterType.GetConstructors();
            Type qNameMapperType = typeof(QNameOutputMapper);
            Type nsMapperType = typeof(INamespaceMapper);
            foreach (ConstructorInfo c in cs.OrderByDescending(c => c.GetParameters().Count()))
            {
                ParameterInfo[] ps = c.GetParameters();
                try
                {
                    if (ps.Length == 1)
                    {
                        if (ps[0].ParameterType == qNameMapperType)
                        {
                            _formatter = Activator.CreateInstance(_formatterType, [_formattingMapper]) as IResultFormatter;
                        }
                        else if (ps[0].ParameterType == nsMapperType)
                        {
                            _formatter = Activator.CreateInstance(_formatterType, [_formattingMapper]) as IResultFormatter;
                        }
                    }
                    else if (ps.Length == 0)
                    {
                        _formatter = Activator.CreateInstance(_formatterType) as IResultFormatter;
                    }

                    if (_formatter != null) break;
                }
                catch
                {
                    // Suppress errors since we'll throw later if necessary
                }
            }

            // If we get out here and the formatter is null then we throw an error
            if (_formatter == null) throw new RdfParseException("Unable to instantiate a IResultFormatter from the given Formatter Type " + _formatterType.FullName);
        }
    }

    /// <summary>
    /// Ends the writing of results closing the <see cref="TextWriter">TextWriter</see> depending on the option set when this instance was instantiated.
    /// </summary>
    /// <param name="ok"></param>
    protected override void EndResultsInternal(bool ok)
    {
        if (_formatter is IResultSetFormatter resultSetFormatter)
        {
            _writer.WriteLine(resultSetFormatter.FormatResultSetFooter());
        }
        if (_closeOnEnd)
        {
            _writer.Close();
            _writer = null;
        }
        _currentType = SparqlResultsType.Unknown;
        _currVariables.Clear();
        _headerWritten = false;
    }

    /// <summary>
    /// Writes a Boolean Result to the output.
    /// </summary>
    /// <param name="result">Boolean Result.</param>
    protected override void HandleBooleanResultInternal(bool result)
    {
        if (_currentType != SparqlResultsType.Unknown) throw new RdfParseException("Cannot handle a Boolean Result when the handler has already handled other types of results");
        _currentType = SparqlResultsType.Boolean;
        if (!_headerWritten && _formatter is IResultSetFormatter resultSetFormatter)
        {
            _writer.WriteLine(resultSetFormatter.FormatResultSetHeader());
            _headerWritten = true;
        }

        _writer.WriteLine(_formatter.FormatBooleanResult(result));
    }

    /// <summary>
    /// Writes a Variable declaration to the output.
    /// </summary>
    /// <param name="var">Variable Name.</param>
    /// <returns></returns>
    protected override bool HandleVariableInternal(string var)
    {
        if (_currentType == SparqlResultsType.Boolean) throw new RdfParseException("Cannot handler a Variable when the handler has already handled a boolean result");
        _currentType = SparqlResultsType.VariableBindings;
        _currVariables.Add(var);
        return true;
    }

    /// <summary>
    /// Writes a Result to the output.
    /// </summary>
    /// <param name="result">SPARQL Result.</param>
    /// <returns></returns>
    protected override bool HandleResultInternal(ISparqlResult result)
    {
        if (_currentType == SparqlResultsType.Boolean) throw new RdfParseException("Cannot handle a Result when the handler has already handled a boolean result");
        _currentType = SparqlResultsType.VariableBindings;
        if (!_headerWritten && _formatter is IResultSetFormatter resultSetFormatter)
        {
            _writer.WriteLine(resultSetFormatter.FormatResultSetHeader(_currVariables.Distinct()));
            _headerWritten = true;
        }
        _writer.WriteLine(_formatter.Format(result));
        return true;
    }
}