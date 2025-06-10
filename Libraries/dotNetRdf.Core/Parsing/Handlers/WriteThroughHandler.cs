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
using System.IO;
using System.Linq;
using System.Reflection;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Parsing.Handlers;

/// <summary>
/// A RDF Handler which writes the handled Triples out to a <see cref="TextWriter">TextWriter</see> using a provided <see cref="IQuadFormatter">IQuadFormatter</see>.
/// </summary>
public class WriteThroughHandler
    : BaseRdfHandler
{
    private readonly Type _formatterType;
    private IQuadFormatter _formatter;
    private ITripleFormatter _tripleFormatter;
    private TextWriter _writer;
    private readonly bool _closeOnEnd;
    private INamespaceMapper _formattingMapper = new QNameOutputMapper();
    private int _written;

    private const int FlushInterval = 50000;

    /// <summary>
    /// Creates a new Write-Through Handler.
    /// </summary>
    /// <param name="formatter">Triple formatter to use.</param>
    /// <param name="writer">Text write to write to.</param>
    /// <param name="closeOnEnd">Whether to close the writer at the end of RDF handling.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public WriteThroughHandler(ITripleFormatter formatter, TextWriter writer, bool closeOnEnd)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer), "Cannot use a null TextWriter with the WriteThroughHandler");
        _formatter = null;
        _tripleFormatter = formatter ?? new NTriples11Formatter();
        _closeOnEnd = closeOnEnd;
    }
    /// <summary>
    /// Creates a new Write-Through Handler.
    /// </summary>
    /// <param name="formatter">Quad Formatter to use.</param>
    /// <param name="writer">Text Writer to write to.</param>
    /// <param name="closeOnEnd">Whether to close the writer at the end of RDF handling.</param>
    public WriteThroughHandler(IQuadFormatter formatter, TextWriter writer, bool closeOnEnd)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer), "Cannot use a null TextWriter with the Write Through Handler");
        _formatter = formatter ?? new NQuads11Formatter();
        _tripleFormatter = null;
        _closeOnEnd = closeOnEnd;
    }

    /// <summary>
    /// Creates a new Write-Through Handler.
    /// </summary>
    /// <param name="formatter">Triple Formatter to use.</param>
    /// <param name="writer">Text Writer to write to.</param>
    public WriteThroughHandler(IQuadFormatter formatter, TextWriter writer)
        : this(formatter, writer, true) { }

    /// <summary>
    /// Creates a new Write-Through Handler.
    /// </summary>
    /// <param name="formatterType">Type of the formatter to create.</param>
    /// <param name="writer">Text Writer to write to.</param>
    /// <param name="closeOnEnd">Whether to close the writer at the end of RDF handling.</param>
    public WriteThroughHandler(Type formatterType, TextWriter writer, bool closeOnEnd)
    {
        _formatterType = formatterType ?? throw new ArgumentNullException(nameof(formatterType), "Cannot use a null formatter type");
        _writer = writer ?? throw new ArgumentNullException(nameof(writer), "Cannot use a null TextWriter with the Write Through Handler");
        _closeOnEnd = closeOnEnd;
    }

    /// <summary>
    /// Creates a new Write-Through Handler.
    /// </summary>
    /// <param name="formatterType">Type of the formatter to create.</param>
    /// <param name="writer">Text Writer to write to.</param>
    public WriteThroughHandler(Type formatterType, TextWriter writer)
        : this(formatterType, writer, true) { }

    /// <summary>
    /// Starts RDF Handling instantiating a Triple Formatter if necessary.
    /// </summary>
    protected override void StartRdfInternal()
    {
        if (_closeOnEnd && _writer == null) throw new RdfParseException("Cannot use this WriteThroughHandler as an RDF Handler for parsing as you set closeOnEnd to true and you have already used this Handler and so the provided TextWriter was closed");

        if (_formatterType != null)
        {
            _formatter = null;
            _formattingMapper = new QNameOutputMapper();

            // Instantiate a new Formatter
            ConstructorInfo[] cs = _formatterType.GetConstructors();
            Type qNameMapperType = typeof(QNameOutputMapper);
            Type nsMapperType = typeof(INamespaceMapper);
            object formatter = null;
            foreach (ConstructorInfo c in cs.OrderByDescending(c => c.GetParameters().Count()))
            {
                ParameterInfo[] ps = c.GetParameters();
                try
                {
                    if (ps.Length == 1)
                    {
                        if (ps[0].ParameterType == qNameMapperType)
                        {
                            formatter = Activator.CreateInstance(_formatterType, _formattingMapper);
                        }
                        else if (ps[0].ParameterType == nsMapperType)
                        {
                            formatter = Activator.CreateInstance(_formatterType, _formattingMapper);
                        }
                    }
                    else if (ps.Length == 0)
                    {
                        formatter = Activator.CreateInstance(_formatterType);
                    }

                    if (formatter != null) break;
                }
                catch
                {
                    // Suppress errors since we'll throw later if necessary
                }
            }

            switch (formatter)
            {
                case IQuadFormatter quadFormatter:
                    _formatter = quadFormatter;
                    break;
                case ITripleFormatter tripleFormatter:
                    _tripleFormatter = tripleFormatter;
                    break;
                default:
                    throw new RdfParseException(
                        "Unable to instantiate an ITripleFormatter or IQuadFormatter from teh given formatter type " +
                        _formatterType);
            }
        }

        if (_tripleFormatter is IGraphFormatter graphFormatter)
        {
            _writer.WriteLine(graphFormatter.FormatGraphHeader(_formattingMapper));
        }
        _written = 0;
    }

    /// <summary>
    /// Ends RDF Handling closing the <see cref="TextWriter">TextWriter</see> being used if the setting is enabled.
    /// </summary>
    /// <param name="ok">Indicates whether parsing completed without error.</param>
    protected override void EndRdfInternal(bool ok)
    {
        if (_tripleFormatter is IGraphFormatter)
        {
            _writer.WriteLine(((IGraphFormatter)_tripleFormatter).FormatGraphFooter());
        }
        if (_closeOnEnd)
        {
            _writer.Close();
            _writer = null;
        }
    }

    /// <summary>
    /// Handles Namespace Declarations passing them to the underlying formatter if applicable.
    /// </summary>
    /// <param name="prefix">Namespace Prefix.</param>
    /// <param name="namespaceUri">Namespace URI.</param>
    /// <returns></returns>
    protected override bool HandleNamespaceInternal(string prefix, Uri namespaceUri)
    {
        if (_formattingMapper != null)
        {
            _formattingMapper.AddNamespace(prefix, namespaceUri);
        }

        if (_tripleFormatter is INamespaceFormatter nsFormatter)
        {
            _writer.WriteLine(nsFormatter.FormatNamespace(prefix, namespaceUri));
        }

        return true;
    }

    /// <summary>
    /// Handles Base URI Declarations passing them to the underlying formatter if applicable.
    /// </summary>
    /// <param name="baseUri">Base URI.</param>
    /// <returns></returns>
    protected override bool HandleBaseUriInternal(Uri baseUri)
    {
        if (_tripleFormatter is IBaseUriFormatter baseUriFormatter)
        {
            _writer.WriteLine(baseUriFormatter.FormatBaseUri(baseUri));
        }

        return true;
    }

    /// <summary>
    /// Handles Triples by writing them using the underlying formatter.
    /// </summary>
    /// <param name="t">Triple.</param>
    /// <returns></returns>
    protected override bool HandleTripleInternal(Triple t)
    {
        _written++;
        _writer.WriteLine(_tripleFormatter != null ? _tripleFormatter.Format(t) : _formatter.Format(t, null));
        MaybeFlush();
        return true;
    }

    /// <summary>
    /// Handles Triples by writing them using the underlying formatter.
    /// </summary>
    /// <param name="t">Triple.</param>
    /// <param name="graph">The name of the graph containing the triple.</param>
    /// <returns></returns>
    protected override bool HandleQuadInternal(Triple t, IRefNode graph)
    {
        if (_tripleFormatter != null)
        {
            if (graph == null)
            {
                return HandleTripleInternal(t);
            }

            throw new RdfParseException(
                "A quad with a named graph component cannot be handled by the triple formatter.");
        }

        _written++;
        _writer.WriteLine(_formatter.Format(t, graph));
        MaybeFlush();
        return true;
    }

    /// <summary>
    /// Handles Comments passing them to the underlying formatter if applicable.
    /// </summary>
    /// <param name="text">Comment text.</param>
    /// <returns></returns>
    protected override bool HandleCommentInternal(string text)
    {
        if (_tripleFormatter is ICommentFormatter commentFormatter)
        {
            _writer.WriteLine(commentFormatter.FormatComment(text));
        }

        return true;
    }

    private void MaybeFlush()
    {
        if (_written < FlushInterval)
        {
            return;
        }

        _written = 0;
        _writer.Flush();
    }

    /// <summary>
    /// Gets that the Handler accepts all Triples.
    /// </summary>
    public override bool AcceptsAll
    {
        get 
        {
            return true;
        }
    }
}
