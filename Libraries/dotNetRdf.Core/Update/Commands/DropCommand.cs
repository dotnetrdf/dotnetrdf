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
/// Represents a SPARQL Update DROP command.
/// </summary>
public class DropCommand : SparqlUpdateCommand
{
    /// <summary>
    /// Creates a new DROP command.
    /// </summary>
    /// <param name="graphName">Name of the Graph to DROP.</param>
    /// <param name="mode">DROP Mode to use.</param>
    /// <param name="silent">Whether the DROP should be done silently.</param>
    public DropCommand(IRefNode graphName = null, ClearMode mode = ClearMode.Graph, bool silent = false) : base(SparqlUpdateCommandType.Drop)
    {
        TargetGraphName = graphName;
        Mode = mode;
        if (TargetGraphName == null && Mode == ClearMode.Graph) Mode = ClearMode.Default;
        if (Mode == ClearMode.Default) TargetGraphName = null;
        Silent = silent;
    }

    /// <summary>
    /// Creates a new DROP command.
    /// </summary>
    /// <param name="graphUri">URI of the Graph to DROP.</param>
    /// <param name="mode">DROP Mode to use.</param>
    /// <param name="silent">Whether the DROP should be done silently.</param>
    [Obsolete("Replaced by DropCommand(IRefNode, ClearMode, bool)")]
    public DropCommand(Uri graphUri, ClearMode mode, bool silent)
        : this(graphUri == null ? null : new UriNode(graphUri), mode, silent)
    {
    }

    /// <summary>
    /// Creates a new DROP command.
    /// </summary>
    /// <param name="graphUri">URI of the Graph to DROP.</param>
    /// <param name="mode">DROP Mode to use.</param>
    [Obsolete("Replaced by DropCommand(IRefNode, ClearMode, bool)")]
    public DropCommand(Uri graphUri, ClearMode mode)
        : this(graphUri, mode, false) { }

    /// <summary>
    /// Creates a new DROP command.
    /// </summary>
    /// <param name="graphUri">URI of the Graph to DROP.</param>
    [Obsolete("Replaced by DropCommand(IRefNode, ClearMode, bool)")]
    public DropCommand(Uri graphUri)
        : this(graphUri, ClearMode.Graph, false) { }

    /// <summary>
    /// Creates a new DROP command which drops the Default Graph.
    /// </summary>
    [Obsolete("Replaced by DropCommand(IRefNode, ClearMode, bool)")]
    public DropCommand()
        : this(null, ClearMode.Default) { }

    /// <summary>
    /// Creates a new DROP command which performs a specific clear mode drop operation.
    /// </summary>
    /// <param name="mode">Clear Mode.</param>
    [Obsolete("Replaced by DropCommand(IRefNode, ClearMode, bool)")]
    public DropCommand(ClearMode mode)
        : this(mode, false) { }

    /// <summary>
    /// Creates a new DROP command which performs a specific clear mode drop operation.
    /// </summary>
    /// <param name="mode">Clear Mode.</param>
    /// <param name="silent">Whether errors should be suppressed.</param>
    public DropCommand(ClearMode mode, bool silent)
        : this((IRefNode)null, mode, silent) { }

    /// <summary>
    /// Gets whether the Command affects a single Graph.
    /// </summary>
    public override bool AffectsSingleGraph
    {
        get
        {
            return Mode == ClearMode.Graph || Mode == ClearMode.Default;
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
        return AffectsGraph(graphUri == null ? null : new UriNode(graphUri));
    }

    /// <summary>
    /// Gets whether the Command will potentially affect the given Graph.
    /// </summary>
    /// <param name="graphName">Graph name.</param>
    /// <returns></returns>
    public override bool AffectsGraph(IRefNode graphName)
    {
        switch (Mode)
        {
            case ClearMode.All:
                return true;
            case ClearMode.Default:
                return graphName == null;
            case ClearMode.Named:
                return graphName != null;
            case ClearMode.Graph:
                if (TargetGraphName == null)
                {
                    return true;
                }
                else
                {
                    return TargetGraphName.Equals(graphName);
                }
            default:
                // No Other Clear Modes but have to keep the compiler happy
                return true;
        }
    }

    /// <summary>
    /// Gets the URI of the Graph to be dropped.
    /// </summary>
    [Obsolete("Replaced by TargetGraphName")]
    public Uri TargetUri => ((IUriNode)TargetGraphName)?.Uri;

    /// <summary>
    /// Get the name of the graph to be dropped.
    /// </summary>
    public IRefNode TargetGraphName { get; }

    /// <summary>
    /// Gets whether the Drop should be done silently.
    /// </summary>
    public bool Silent { get; }

    /// <summary>
    /// Gets the type of DROP operation to perform.
    /// </summary>
    public ClearMode Mode { get; }

    /// <summary>
    /// Processes the Command using the given Update Processor.
    /// </summary>
    /// <param name="processor">SPARQL Update Processor.</param>
    public override void Process(ISparqlUpdateProcessor processor)
    {
        processor.ProcessDropCommand(this);
    }

    /// <summary>
    /// Gets the String representation of the command.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var output = new StringBuilder();
        output.Append("DROP ");
        if (Silent) output.Append("SILENT ");
        switch (Mode)
        {
            case ClearMode.All:
                output.Append("ALL");
                break;
            case ClearMode.Default:
                output.Append("DEFAULT");
                break;
            case ClearMode.Named:
                output.Append("NAMED");
                break;
            case ClearMode.Graph:
                var formatter = new SparqlFormatter();
                output.Append("GRAPH ");
                output.Append(formatter.Format(TargetGraphName));
                break;
        }
        return output.ToString();
    }

}
