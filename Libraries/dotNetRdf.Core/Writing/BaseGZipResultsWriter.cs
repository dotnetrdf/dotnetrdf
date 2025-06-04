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
using System.IO.Compression;
using System.Text;
using VDS.RDF.Query;

namespace VDS.RDF.Writing;

/// <summary>
/// Abstract Base class for Results writers which generate GZipped output.
/// </summary>
/// <remarks>
/// <para>
/// While the normal witers can be used with GZip streams directly this class just abstracts the wrapping of file/stream output into a GZip stream if it is not already passed as such.
/// </para>
/// </remarks>
public abstract class BaseGZipResultsWriter
    : ISparqlResultsWriter
{
    private readonly ISparqlResultsWriter _writer;

    /// <summary>
    /// Creates a new GZipped Results writer.
    /// </summary>
    /// <param name="writer">Underlying writer.</param>
    protected BaseGZipResultsWriter(ISparqlResultsWriter writer)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        _writer.Warning += RaiseWarning;
    }

    /// <summary>
    /// Saves a Result Set as GZipped output.
    /// </summary>
    /// <param name="results">Result Set to save.</param>
    /// <param name="filename">File to save to.</param>
    public void Save(SparqlResultSet results, string filename)
    {
        Save(results, filename,
#pragma warning disable CS0618 // Type or member is obsolete
                new UTF8Encoding(Options.UseBomForUtf8) //new UTF8Encoding(false)
#pragma warning restore CS0618 // Type or member is obsolete
            );
    }

    /// <inheritdoc />
    public void Save(SparqlResultSet results, string filename, Encoding fileEncoding)
    {
        if (results == null) throw new ArgumentNullException(nameof(results), "Cannot write a null results sets");
        if (filename == null) throw new ArgumentNullException(nameof(filename), "Cannot write RDF to a null file");
        using var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write);
        Save(results, new StreamWriter(fileStream, fileEncoding));
    }

    /// <summary>
    /// Saves a Result Set as GZipped output.
    /// </summary>
    /// <param name="results">Result Set to save.</param>
    /// <param name="output">Writer to save to.</param>
    public void Save(SparqlResultSet results, TextWriter output)
    {
        if (results == null) throw new RdfOutputException("Cannot write RDF from a null Graph");

        if (output is StreamWriter)
        {
            // Check for inner GZipStream and re-wrap if required
            var streamOutput = (StreamWriter)output;
            if (streamOutput.BaseStream is GZipStream)
            {
                _writer.Save(results, streamOutput);
            }
            else
            {
                streamOutput = new StreamWriter(new GZipStream(streamOutput.BaseStream, CompressionMode.Compress));
                _writer.Save(results, streamOutput);
            }
        }
        else
        {
            throw new RdfOutputException("GZipped Output can only be written to StreamWriter instances");
        }
    }

    /// <summary>
    /// Helper method for raising warning events.
    /// </summary>
    /// <param name="message">Warning message.</param>
    private void RaiseWarning(string message)
    {
        SparqlWarning d = Warning;
        if (d != null) d(message);
    }

    /// <summary>
    /// Event which is raised if non-fatal errors occur writing results
    /// </summary>
    public event SparqlWarning Warning;

    /// <summary>
    /// Gets the description of the writer.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return "GZipped " + _writer;
    }
}

/// <summary>
/// Writer for GZipped SPARQL XML.
/// </summary>
public class GZippedSparqlXmlWriter
    : BaseGZipResultsWriter
{
    /// <summary>
    /// Creates a new GZipped SPARQL XML writer.
    /// </summary>
    public GZippedSparqlXmlWriter()
        : base(new SparqlXmlWriter()) { }
}

/// <summary>
/// Writer for GZipped SPARQL JSON.
/// </summary>
public class GZippedSparqlJsonWriter
    : BaseGZipResultsWriter
{
    /// <summary>
    /// Creates a new GZipped SPARQL JSON writer.
    /// </summary>
    public GZippedSparqlJsonWriter()
        : base(new SparqlJsonWriter()) { }
}

/// <summary>
/// Writer for GZipped SPARQL CSV.
/// </summary>
public class GZippedSparqlCsvWriter
    : BaseGZipResultsWriter
{
    /// <summary>
    /// Creates a new GZipped SPARQL CSV writer.
    /// </summary>
    public GZippedSparqlCsvWriter()
        : base(new SparqlCsvWriter()) { }
}

/// <summary>
/// Writer for GZipped SPARQL TSV.
/// </summary>
public class GZippedSparqlTsvWriter
    : BaseGZipResultsWriter
{
    /// <summary>
    /// Creates a new GZipped SPARQL TSV writer.
    /// </summary>
    public GZippedSparqlTsvWriter()
        : base(new SparqlTsvWriter()) { }
}
