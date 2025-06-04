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
/// Abstract Base Class for SPARQL Update Commands which move data between Graphs.
/// </summary>
public abstract class BaseTransferCommand : SparqlUpdateCommand
{
    /// <summary>
    /// Whether errors should be suppressed.
    /// </summary>
    protected bool _silent;

    /// <summary>
    /// Creates a new Transfer Command.
    /// </summary>
    /// <param name="type">Command Type.</param>
    /// <param name="sourceUri">Source Graph URI.</param>
    /// <param name="destUri">Destination Graph URI.</param>
    /// <param name="silent">Whether errors should be suppressed.</param>
    [Obsolete("Replaced by BaseTransferCommand(SparqlUpdateCommandType, IRefNode, IRefNode, bool)")]
    protected BaseTransferCommand(SparqlUpdateCommandType type, Uri sourceUri, Uri destUri, bool silent)
        : this(type, sourceUri == null ? null : new UriNode(sourceUri),
            destUri == null ? null : new UriNode(destUri), silent)
    {
    }

    /// <summary>
    /// Creates a new Transfer Command.
    /// </summary>
    /// <param name="type">Command Type.</param>
    /// <param name="sourceUri">Source Graph URI.</param>
    /// <param name="destUri">Destination Graph URI.</param>
    [Obsolete("Replaced by BaseTransferCommand(SparqlUpdateCommandType, IRefNode, IRefNode, bool)")]
    protected BaseTransferCommand(SparqlUpdateCommandType type, Uri sourceUri, Uri destUri)
        : this(type, sourceUri == null ? null : new UriNode(sourceUri),
            destUri == null ? null : new UriNode(destUri))
    {
    }

    /// <summary>
    /// Creates a new Transfer Command.
    /// </summary>
    /// <param name="type">Command Type.</param>
    /// <param name="sourceGraphName">Source Graph name.</param>
    /// <param name="destGraphName">Destination Graph name.</param>
    /// <param name="silent">Whether errors should be suppressed.</param>
    protected BaseTransferCommand(SparqlUpdateCommandType type, IRefNode sourceGraphName, IRefNode destGraphName, bool silent = false)
    : base(type)
    {
        SourceGraphName = sourceGraphName;
        DestinationGraphName = destGraphName;
        _silent = silent;
    }

    /// <summary>
    /// URI of the Source Graph.
    /// </summary>
    [Obsolete("Replaced by SourceGraphName")]
    public Uri SourceUri
    {
        get
        {
            return ((IUriNode) SourceGraphName)?.Uri;
        }
    }

    /// <summary>
    /// Name of the source graph.
    /// </summary>
    public IRefNode SourceGraphName { get; }

    /// <summary>
    /// URI of the Destination Graph.
    /// </summary>
    [Obsolete("Replaced by DestinationGraphName")]
    public Uri DestinationUri
    {
        get
        {
            return ((IUriNode)DestinationGraphName)?.Uri;
        }
    }

    /// <summary>
    /// Name of the destination graph.
    /// </summary>
    public IRefNode DestinationGraphName { get; }

    /// <summary>
    /// Whether errors during evaluation should be suppressed.
    /// </summary>
    public bool Silent
    {
        get
        {
            return _silent;
        }
    }

    /// <summary>
    /// Gets whether the Command affects a Single Graph.
    /// </summary>
    public override bool AffectsSingleGraph
    {
        get
        {
            return false;
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
        if (graphName == null) return SourceGraphName == null || DestinationGraphName == null;
        return graphName.Equals(SourceGraphName) || graphName.Equals(DestinationGraphName);
    }

    /// <summary>
    /// Gets the String representation of the Command.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var output = new StringBuilder();
        var formatter = new SparqlFormatter();
        switch (CommandType)
        {
            case SparqlUpdateCommandType.Add:
                output.Append("ADD");
                break;
            case SparqlUpdateCommandType.Copy:
                output.Append("COPY");
                break;
            case SparqlUpdateCommandType.Move:
                output.Append("MOVE");
                break;
            default:
                throw new RdfException("Cannot display the String for this Transfer command as it is not one of the valid transfer commands (ADD/COPY/MOVE)");
        }

        if (_silent) output.Append(" SILENT");

        if (SourceGraphName == null)
        {
            output.Append(" DEFAULT");
        }
        else
        {
            output.Append(" GRAPH ");
            output.Append(formatter.Format(SourceGraphName));
        }
        output.Append(" TO ");
        if (DestinationGraphName == null)
        {
            output.Append(" DEFAULT");
        }
        else
        {
            output.Append(" GRAPH ");
            output.Append(formatter.Format(DestinationGraphName));
        }

        return output.ToString();
    }
}
