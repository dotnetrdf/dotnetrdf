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
using VDS.RDF.Parsing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Update.Commands;

/// <summary>
/// Represents the SPARQL Update LOAD command.
/// </summary>
public class LoadCommand : SparqlUpdateCommand
{
    /// <summary>
    /// Creates a new LOAD command.
    /// </summary>
    /// <param name="sourceUri">Source URI to load data from.</param>
    /// <param name="graphName">Name of the graph to store data in. If null or omitted, the target is the default graph.</param>
    /// <param name="silent">Whether errors loading should be suppressed.</param>
    /// <param name="loader">The loader to use for retrieving and parsing data.</param>
    public LoadCommand(Uri sourceUri, IRefNode graphName = null, bool silent = false, Loader loader = null)
        : base(SparqlUpdateCommandType.Load)
    {
        if (sourceUri == null) throw new ArgumentNullException(nameof(sourceUri));
        SourceUri = sourceUri;
        TargetGraphName = graphName;
        Silent = silent;
        Loader = loader ?? new Loader();

    }
    /// <summary>
    /// Creates a new LOAD command.
    /// </summary>
    /// <param name="sourceUri">Source URI to load data from.</param>
    /// <param name="graphUri">Target URI for the Graph to store data in.</param>
    /// <param name="silent">Whether errors loading should be suppressed.</param>
    /// <param name="loader">The loader to use for retrieving and parsing data.</param>
    [Obsolete("Replaced by LoadCommand(Uri, IRefNode, bool Loader)")]
    public LoadCommand(Uri sourceUri, Uri graphUri, bool silent, Loader loader = null)
        : this(sourceUri, graphUri == null ? null : new UriNode(graphUri), silent, loader)
    {
    }

    /// <summary>
    /// Creates a new LOAD command.
    /// </summary>
    /// <param name="sourceUri">Source URI to load data from.</param>
    /// <param name="silent">Whether errors loading should be suppressed.</param>
    [Obsolete("Replaced by LoadCommand(Uri, IRefNode, bool Loader)")]
    public LoadCommand(Uri sourceUri, bool silent)
        : this(sourceUri, (IRefNode)null, silent) { }

    /// <summary>
    /// Creates a new LOAD command.
    /// </summary>
    /// <param name="sourceUri">Source URI to load data from.</param>
    /// <param name="targetUri">Target URI for the Graph to store data in.</param>
    [Obsolete("Replaced by LoadCommand(Uri, IRefNode, bool Loader)")]
    public LoadCommand(Uri sourceUri, Uri targetUri)
        : this(sourceUri, targetUri, false) { }

    /// <summary>
    /// Creates a new LOAD command which operates on the Default Graph.
    /// </summary>
    /// <param name="sourceUri">Source URI to load data from.</param>
    [Obsolete("Replaced by LoadCommand(Uri, IRefNode, bool Loader)")]
    public LoadCommand(Uri sourceUri)
        : this(sourceUri, null) { }

    /// <summary>
    /// Gets whether the Command affects a specific Graph.
    /// </summary>
    public override bool AffectsSingleGraph => true;

    /// <summary>
    /// Gets whether the Command affects a given Graph.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    /// <returns></returns>
    [Obsolete("Replaced by AffectsGraph(IRefNode)")]
    public override bool AffectsGraph(Uri graphUri)
    {
        return AffectsGraph(graphUri == null ? null : new UriNode(graphUri));
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
    /// Gets the URI that data is loaded from.
    /// </summary>
    public Uri SourceUri { get; }

    /// <summary>
    /// Gets the URI of the Graph to load data into.
    /// </summary>
    [Obsolete("Replaced by TargetGraphName")]
    public Uri TargetUri => ((IUriNode)TargetGraphName)?.Uri;

    /// <summary>
    /// Get the name of the graph to load data into.
    /// </summary>
    public IRefNode TargetGraphName { get; }

    /// <summary>
    /// Gets whether errors loading the data are suppressed.
    /// </summary>
    public bool Silent { get; }

    /// <summary>
    /// Get the loader to use for retrieving and parsing the data.
    /// </summary>
    public Loader Loader { get; }


    /// <summary>
    /// Processes the Command using the given Update Processor.
    /// </summary>
    /// <param name="processor">SPARQL Update Processor.</param>
    public override void Process(ISparqlUpdateProcessor processor)
    {
        processor.ProcessLoadCommand(this);
    }

    /// <summary>
    /// Gets the String representation of the Command.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var silent = Silent ? "SILENT " : string.Empty;
        var formatter = new SparqlFormatter();
        return TargetGraphName == null
            ? $"LOAD {silent}<{formatter.FormatUri(SourceUri)}>"
            : $"LOAD {silent}<{formatter.FormatUri(SourceUri)}> INTO {formatter.Format(TargetGraphName)}";
    }
}
