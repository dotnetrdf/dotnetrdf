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
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Update.Commands;

/// <summary>
/// Represents a SPARQL Update DELETE DATA command.
/// </summary>
public class DeleteDataCommand : SparqlUpdateCommand
{
    private GraphPattern _pattern;

    /// <summary>
    /// Creates a new DELETE DATA command.
    /// </summary>
    /// <param name="pattern">Pattern composed of concrete Triples to delete.</param>
    public DeleteDataCommand(GraphPattern pattern)
        : base(SparqlUpdateCommandType.DeleteData) 
    {
        if (!IsValidDataPattern(pattern, true)) throw new SparqlUpdateException("Cannot create a DELETE DATA command where any of the Triple Patterns are not concrete triples (Variables/Blank Nodes are not permitted) or a GRAPH clause has nested Graph Patterns");
        _pattern = pattern;
    }

    /// <summary>
    /// Determines whether a Graph Pattern is valid for use in an DELETE DATA command.
    /// </summary>
    /// <param name="p">Graph Pattern.</param>
    /// <param name="top">Is this the top level pattern?.</param>
    /// <returns></returns>
    public static bool IsValidDataPattern(GraphPattern p, bool top)
    {
        if (p.IsGraph)
        {
            // If a GRAPH clause then all triple patterns must be constructable and have no Child Graph Patterns
            return !p.HasChildGraphPatterns && p.TriplePatterns.All(tp => tp is IConstructTriplePattern && ((IConstructTriplePattern)tp).HasNoVariables);
        }
        else if (p.IsExists || p.IsMinus || p.IsNotExists || p.IsOptional || p.IsService || p.IsSubQuery || p.IsUnion)
        {
            // EXISTS/MINUS/NOT EXISTS/OPTIONAL/SERVICE/Sub queries/UNIONs are not permitted
            return false;
        }
        else
        {
            // For other patterns all Triple patterns must be constructable with no explicit variables
            // If top level then any Child Graph Patterns must be valid
            // Otherwise must have no Child Graph Patterns
            return p.TriplePatterns.All(tp => tp is IConstructTriplePattern && ((IConstructTriplePattern)tp).HasNoVariables) && ((top && p.ChildGraphPatterns.All(gp => IsValidDataPattern(gp, false))) || !p.HasChildGraphPatterns);
        }
    }

    /// <summary>
    /// Gets the Data Pattern containing Triples to delete.
    /// </summary>
    public GraphPattern DataPattern
    {
        get
        {
            return _pattern;
        }
    }

    /// <summary>
    /// Gets whether the Command affects a single Graph.
    /// </summary>
    public override bool AffectsSingleGraph
    {
        get
        {
            if (!_pattern.HasChildGraphPatterns)
            {
                return true;
            }
            else
            {
                var affectedUris = new List<string>();
                if (_pattern.IsGraph)
                {
                    affectedUris.Add(_pattern.GraphSpecifier.Value);
                }
                else
                {
                    affectedUris.Add(null);
                }
                affectedUris.AddRange(from p in _pattern.ChildGraphPatterns
                                      where p.IsGraph
                                      select p.GraphSpecifier.Value);

                return affectedUris.Distinct().Count() <= 1;
            }
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
        if (_pattern.IsGraph)
        {
            affectedUris.Add(_pattern.GraphSpecifier.Value);
        }
        else
        {
            affectedUris.Add(string.Empty);
        }
        if (_pattern.HasChildGraphPatterns)
        {
            affectedUris.AddRange(from p in _pattern.ChildGraphPatterns
                                  where p.IsGraph
                                  select p.GraphSpecifier.Value);
        }
        if (affectedUris.Any(u => u != null)) affectedUris.Add(string.Empty);

        return affectedUris.Contains(graphUri.ToSafeString());
    }

    /// <summary>
    /// Gets whether the Command affects a given Graph.
    /// </summary>
    /// <param name="graphName">Graph name.</param>
    /// <returns></returns>
    public override bool AffectsGraph(IRefNode graphName)
    {
        var affectedUris = new List<string>();
        if (_pattern.IsGraph)
        {
            affectedUris.Add(_pattern.GraphSpecifier.Value);
        }
        else
        {
            affectedUris.Add(string.Empty);
        }

        if (_pattern.HasChildGraphPatterns)
        {
            affectedUris.AddRange(from p in _pattern.ChildGraphPatterns
                where p.IsGraph
                select p.GraphSpecifier.Value);
        }

        if (affectedUris.Any(u => u != null)) affectedUris.Add(string.Empty);

        return affectedUris.Contains(graphName.ToSafeString());
    }


    /// <summary>
    /// Processes the Command using the given Update Processor.
    /// </summary>
    /// <param name="processor">SPARQL Update Processor.</param>
    public override void Process(ISparqlUpdateProcessor processor)
    {
        processor.ProcessDeleteDataCommand(this);
    }

    /// <summary>
    /// Gets the String representation of the Command.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var output = new StringBuilder();
        output.AppendLine("DELETE DATA");
        if (_pattern.IsGraph) output.AppendLine("{");
        output.AppendLine(_pattern.ToString());
        if (_pattern.IsGraph) output.AppendLine("}");
        return output.ToString();
    }
}
