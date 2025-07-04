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

using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF.Query;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing;

/// <summary>
/// Class for saving SPARQL Result Sets to TSV format (not a standardised format).
/// </summary>
public class SparqlTsvWriter : ISparqlResultsWriter
{
    private TsvFormatter _formatter = new TsvFormatter();

    /// <summary>
    /// Saves a SPARQL Result Set to TSV format.
    /// </summary>
    /// <param name="results">Result Set.</param>
    /// <param name="filename">File to save to.</param>
    /// <remarks>The output file is encoded in UTF-8 with no byte-order mark.</remarks>
    public void Save(SparqlResultSet results, string filename)
    {
        Save(results, filename,
#pragma warning disable CS0618 // Type or member is obsolete
                new UTF8Encoding(Options.UseBomForUtf8) //new UTF8Encoding(false)
#pragma warning restore CS0618 // Type or member is obsolete
            );
    }

    /// <summary>
    /// Saves a SPARQL Result Set to TSV format.
    /// </summary>
    /// <param name="results">Result Set.</param>
    /// <param name="filename">File to save to.</param>
    /// <param name="fileEncoding">The text encoding to use for the file.</param>
    public void Save(SparqlResultSet results, string filename, Encoding fileEncoding)
    {
        using FileStream stream = File.Open(filename, FileMode.Create);
        Save(results, new StreamWriter(stream, fileEncoding));
    }

    /// <summary>
    /// Saves a SPARQL Result Set to TSV format.
    /// </summary>
    /// <param name="results">Result Set.</param>
    /// <param name="output">Writer to save to.</param>
    public void Save(SparqlResultSet results, TextWriter output)
    {
        try
        {
            if (results.ResultsType == SparqlResultsType.VariableBindings)
            {
                // Output Variables first
                var vars = results.Variables.ToArray();
                for (var i = 0; i < vars.Length; i++)
                {
                    output.Write("?" + vars[i]);
                    if (i < vars.Length - 1) output.Write('\t');
                }
                output.Write('\n');

                foreach (SparqlResult result in results)
                {
                    for (var i = 0; i < vars.Length; i++)
                    {
                        if (result.HasValue(vars[i]))
                        {
                            INode temp = result[vars[i]];
                            if (temp != null)
                            {
                                switch (temp.NodeType)
                                {
                                    case NodeType.GraphLiteral:
                                        throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("SPARQL TSV"));
                                    case NodeType.Blank:
                                    case NodeType.Literal:
                                    case NodeType.Uri:
                                        output.Write(_formatter.Format(temp));
                                        break;
                                    default:
                                        throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("SPARQL TSV"));
                                }
                            }
                        }
                        if (i < vars.Length - 1) output.Write('\t');
                    }
                    output.Write('\n');
                }
            }
            else
            {
                output.Write(results.Result.ToString());
            }

            output.Close();
        }
        catch
        {
            try
            {
                output.Close();
            }
            catch
            {
                // No error handling, just trying to clean up
            }
            throw;
        }
    }

    /// <summary>
    /// Helper Method which raises the Warning event when a non-fatal issue with the SPARQL Results being written is detected.
    /// </summary>
    /// <param name="message">Warning Message.</param>
    private void RaiseWarning(string message)
    {
        SparqlWarning d = Warning;
        if (d != null)
        {
            d(message);
        }
    }

    /// <summary>
    /// Event raised when a non-fatal issue with the SPARQL Results being written is detected
    /// </summary>
    public event SparqlWarning Warning;

    /// <summary>
    /// Gets the String representation of the writer which is a description of the syntax it produces.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return "SPARQL Results TSV";
    }
}
