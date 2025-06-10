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
using System.Text;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Update.Commands;

/// <summary>
/// Represents the SPARQL Update CREATE command.
/// </summary>
public class CreateCommand : SparqlUpdateCommand
{
    /// <summary>
    /// Creates a new CREATE command.
    /// </summary>
    /// <param name="graphUri">URI of the Graph to create.</param>
    /// <param name="silent">Whether the create should be done silently.</param>
    public CreateCommand(Uri graphUri, bool silent)
        : this(graphUri == null ? null : new UriNode(graphUri), silent) 
    {
    }

    /// <summary>
    /// Creates a new CREATE command.
    /// </summary>
    /// <param name="graphUri">URI of the Graph to create.</param>
    public CreateCommand(Uri graphUri)
        : this(graphUri == null ? null : new UriNode(graphUri), false) { }

    /// <summary>
    /// Creates a new CREATE command.
    /// </summary>
    /// <param name="graphName">Name of the graph to create.</param>
    /// <param name="silent">Whether the create should be done silently.</param>
    public CreateCommand(IRefNode graphName, bool silent = false) : base(SparqlUpdateCommandType.Create)
    {
        TargetGraphName = graphName ?? throw new ArgumentNullException(nameof(graphName));
        Silent = silent;
    }

    /// <summary>
    /// Gets whether the Command affects a Single Graph.
    /// </summary>
    public override bool AffectsSingleGraph
    {
        get
        {
            return true;
        }
    }

    /// <summary>
    /// Gets whether the Command affects a given Graph.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    /// <returns></returns>
    [Obsolete("Replaced by AffectsGraph(IRefNode)")]
    public override bool AffectsGraph(Uri graphUri)
    {
        return AffectsGraph(graphUri == null ? (IRefNode)null : new UriNode(graphUri));
    }

    /// <summary>
    /// Gets whether the Command will potentially affect the given Graph.
    /// </summary>
    /// <param name="graphName">Graph name.</param>
    /// <returns></returns>
    public override bool AffectsGraph(IRefNode graphName)
    {
        return TargetGraphName == null || TargetGraphName.Equals(graphName);
    }

    /// <summary>
    /// Gets the URI of the Graph to be created.
    /// </summary>
    [Obsolete("Replaced by TargetGraphName")]
    public Uri TargetUri { get { return ((IUriNode)TargetGraphName)?.Uri; } }

    /// <summary>
    /// Gets the name of the graph to be created.
    /// </summary>
    public IRefNode TargetGraphName { get; }

    /// <summary>
    /// Gets whether the Create should be done silently.
    /// </summary>
    public bool Silent { get; }

    /// <summary>
    /// Processes the Command using the given Update Processor.
    /// </summary>
    /// <param name="processor">SPARQL Update Processor.</param>
    public override void Process(ISparqlUpdateProcessor processor)
    {
        processor.ProcessCreateCommand(this);
    }

    /// <summary>
    /// Gets the String representation of the Command.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var output = new StringBuilder();
        var formatter = new SparqlFormatter();
        output.Append("CREATE ");
        if (Silent) output.Append("SILENT ");
        output.Append("GRAPH ");
        formatter.Format(TargetGraphName);
        return output.ToString();
    }
}
