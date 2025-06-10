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
using System.Linq;
using System.Text;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Update.Commands;

/// <summary>
/// Represents the SPARQL Update DELETE command.
/// </summary>
public class DeleteCommand : BaseModificationCommand
{
    /// <summary>
    /// Creates a new DELETE command.
    /// </summary>
    /// <param name="deletions">Pattern to construct Triples to delete.</param>
    /// <param name="where">Pattern to select data which is then used in evaluating the deletions pattern.</param>
    /// <param name="graphUri">URI of the affected Graph.</param>
    [Obsolete("Replaced by DeleteCommand(GraphPatten, GraphPatter, IRefNode")]
    public DeleteCommand(GraphPattern deletions, GraphPattern where, Uri graphUri)
        : base(SparqlUpdateCommandType.Delete)
    {
        if (!IsValidDeletePattern(deletions, true)) throw new SparqlUpdateException("Cannot create a DELETE command where any of the Triple Patterns are not constructable triple patterns (Blank Node Variables are not permitted) or a GRAPH clause has nested Graph Patterns");

        DeletePattern = deletions;
        WherePattern = where;
        WithGraphName = graphUri == null ? null : new UriNode(graphUri);
    }

    /// <summary>
    /// Creates a new DELETE command.
    /// </summary>
    /// <param name="deletions">Pattern to construct Triples to delete.</param>
    /// <param name="where">Pattern to select data which is then used in evaluating the deletions pattern.</param>
    /// <param name="graphName">Name of the affected Graph.</param>
    public DeleteCommand(GraphPattern deletions, GraphPattern where, IRefNode graphName)
        : base(SparqlUpdateCommandType.Delete)
    {
        if (!IsValidDeletePattern(deletions, true)) throw new SparqlUpdateException("Cannot create a DELETE command where any of the Triple Patterns are not constructable triple patterns (Blank Node Variables are not permitted) or a GRAPH clause has nested Graph Patterns");

        DeletePattern = deletions;
        WherePattern = where;
        WithGraphName = graphName;
    }
    /// <summary>
    /// Creates a new DELETE command which operates on the Default Graph.
    /// </summary>
    /// <param name="deletions">Pattern to construct Triples to delete.</param>
    /// <param name="where">Pattern to select data which is then used in evaluating the deletions pattern.</param>
    public DeleteCommand(GraphPattern deletions, GraphPattern where)
        : base(SparqlUpdateCommandType.Delete)
    {
        if (!IsValidDeletePattern(deletions, true)) throw new SparqlUpdateException("Cannot create a DELETE command where any of the Triple Patterns are not constructable triple patterns (Blank Node Variables are not permitted) or a GRAPH clause has nested Graph Patterns");

        DeletePattern = deletions;
        WherePattern = where;
    }

    /// <summary>
    /// Creates a new DELETE command. 
    /// </summary>
    /// <param name="where">Pattern to construct Triples to delete.</param>
    /// <param name="graphUri">URI of the affected Graph.</param>
    [Obsolete("Replaced by DeleteCommand(GraphPattern, IRefNode)")]
    public DeleteCommand(GraphPattern where, Uri graphUri)
        : this(where, where, graphUri) { }

    /// <summary>
    /// Creates a new DELETE command. 
    /// </summary>
    /// <param name="where">Pattern to construct Triples to delete.</param>
    /// <param name="graphName">Name of the affected Graph.</param>
    public DeleteCommand(GraphPattern where, IRefNode graphName)
        : this(where, where, graphName)
    {
    }

    /// <summary>
    /// Createa a new DELETE command which operates on the Default Graph.
    /// </summary>
    /// <param name="where">Pattern to construct Triples to delete.</param>
    public DeleteCommand(GraphPattern where)
        : this(where, where, (IRefNode)null) { }

    /// <summary>
    /// Gets whether the Command affects a single Graph.
    /// </summary>
    public override bool AffectsSingleGraph
    {
        get
        {
            var affectedUris = new List<string>();
            if (TargetGraph != null)
            {
                affectedUris.Add(TargetGraph.ToString());
            }
            if (DeletePattern.IsGraph) affectedUris.Add(DeletePattern.GraphSpecifier.Value);
            if (DeletePattern.HasChildGraphPatterns)
            {
                affectedUris.AddRange(from p in DeletePattern.ChildGraphPatterns
                                      where p.IsGraph
                                      select p.GraphSpecifier.Value);
            }

            return affectedUris.Distinct().Count() <= 1;
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
        var affectedUris = new List<string>();
        if (TargetUri != null)
        {
            affectedUris.Add(TargetUri.AbsoluteUri);
        }
        else
        {
            affectedUris.Add(string.Empty);
        }
        if (DeletePattern.IsGraph) affectedUris.Add(DeletePattern.GraphSpecifier.Value);
        if (DeletePattern.HasChildGraphPatterns)
        {
            affectedUris.AddRange(from p in DeletePattern.ChildGraphPatterns
                                  where p.IsGraph
                                  select p.GraphSpecifier.Value);
        }
        if (affectedUris.Any(u => u != null)) affectedUris.Add(string.Empty);

        return affectedUris.Contains(graphUri.ToSafeString());
    }

    /// <summary>
    /// Gets whether the Command will potentially affect the given Graph.
    /// </summary>
    /// <param name="graphName">Graph name.</param>
    /// <returns></returns>
    public override bool AffectsGraph(IRefNode graphName)
    {
        var affectedUris = new List<string> {TargetGraph.ToSafeString()};

        if (DeletePattern.IsGraph)
        {
            affectedUris.Add(DeletePattern.GraphSpecifier.Value);
        }
        if (DeletePattern.HasChildGraphPatterns)
        {
            affectedUris.AddRange(from p in DeletePattern.ChildGraphPatterns
                where p.IsGraph
                select p.GraphSpecifier.Value);
        }
        if (affectedUris.Any(u => u != null)) affectedUris.Add(string.Empty);

        return affectedUris.Contains(graphName.ToSafeString());
    }

    /// <summary>
    /// Gets the URI of the Graph the deletions are made from.
    /// </summary>
    [Obsolete("Replaced by TargetGraph")]
    public Uri TargetUri
    {
        get
        {
            return (WithGraphName as UriNode)?.Uri;
        }
    }

    /// <summary>
    /// Gets the name of the graph the deletions are made from.
    /// </summary>
    public IRefNode TargetGraph => WithGraphName;
   
    /// <summary>
    /// Gets the pattern used for Deletions.
    /// </summary>
    public GraphPattern DeletePattern { get; }

    /// <summary>
    /// Gets the pattern used for the WHERE clause.
    /// </summary>
    public GraphPattern WherePattern { get; }

    /// <summary>
    /// Optimises the Commands WHERE pattern.
    /// </summary>
    public override void Optimise(IQueryOptimiser optimiser)
    {
        WherePattern.Optimise(optimiser);
    }


    /// <summary>
    /// Processes the Command using the given Update Processor.
    /// </summary>
    /// <param name="processor">SPARQL Update Processor.</param>
    public override void Process(ISparqlUpdateProcessor processor)
    {
        processor.ProcessDeleteCommand(this);
    }

    /// <summary>
    /// Gets the String representation of the Command.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var output = new StringBuilder();
        var formatter = new SparqlFormatter();
        if (WithGraphName != null)
        {
            output.Append("WITH ");
            output.AppendLine(formatter.Format(WithGraphName));
        }
        output.AppendLine("DELETE");
        if (!ReferenceEquals(DeletePattern, WherePattern)) output.AppendLine(DeletePattern.ToString());
        if (_usingUris != null)
        {
            foreach (Uri u in _usingUris)
            {
                output.AppendLine("USING <" + u.AbsoluteUri.Replace(">", "\\>") + ">");
            }
        }
        if (_usingNamedUris != null)
        {
            foreach (Uri u in _usingNamedUris)
            {
                output.AppendLine("USING NAMED <" + u.AbsoluteUri.Replace(">", "\\>") + ">");
            }
        }
        output.AppendLine("WHERE");
        output.AppendLine(WherePattern.ToString());
        return output.ToString();
    }
}
