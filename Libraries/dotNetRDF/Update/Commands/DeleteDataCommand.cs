/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
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
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query.Construct;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Update.Commands
{
    /// <summary>
    /// Represents a SPARQL Update DELETE DATA command
    /// </summary>
    public class DeleteDataCommand : SparqlUpdateCommand
    {
        private GraphPattern _pattern;

        /// <summary>
        /// Creates a new DELETE DATA command
        /// </summary>
        /// <param name="pattern">Pattern composed of concrete Triples to delete</param>
        public DeleteDataCommand(GraphPattern pattern)
            : base(SparqlUpdateCommandType.DeleteData) 
        {
            if (!IsValidDataPattern(pattern, true)) throw new SparqlUpdateException("Cannot create a DELETE DATA command where any of the Triple Patterns are not concrete triples (Variables/Blank Nodes are not permitted) or a GRAPH clause has nested Graph Patterns");
            _pattern = pattern;
        }

        /// <summary>
        /// Determines whether a Graph Pattern is valid for use in an DELETE DATA command
        /// </summary>
        /// <param name="p">Graph Pattern</param>
        /// <param name="top">Is this the top level pattern?</param>
        /// <returns></returns>
        private bool IsValidDataPattern(GraphPattern p, bool top)
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
        /// Gets the Data Pattern containing Triples to delete
        /// </summary>
        public GraphPattern DataPattern
        {
            get
            {
                return _pattern;
            }
        }

        /// <summary>
        /// Gets whether the Command affects a single Graph
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
                    List<String> affectedUris = new List<string>();
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
        /// Gets whether the Command affects a given Graph
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        public override bool AffectsGraph(Uri graphUri)
        {
            List<String> affectedUris = new List<string>();
            if (_pattern.IsGraph)
            {
                affectedUris.Add(_pattern.GraphSpecifier.Value);
            }
            else
            {
                affectedUris.Add(String.Empty);
            }
            if (_pattern.HasChildGraphPatterns)
            {
                affectedUris.AddRange(from p in _pattern.ChildGraphPatterns
                                      where p.IsGraph
                                      select p.GraphSpecifier.Value);
            }
            if (affectedUris.Any(u => u != null)) affectedUris.Add(String.Empty);

            return affectedUris.Contains(graphUri.ToSafeString());
        }

        /// <summary>
        /// Evaluates the Command in the given Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        public override void Evaluate(SparqlUpdateEvaluationContext context)
        {
            // Split the Pattern into the set of Graph Patterns
            List<GraphPattern> patterns = new List<GraphPattern>();
            if (_pattern.IsGraph)
            {
                patterns.Add(_pattern);
            }
            else if (_pattern.TriplePatterns.Count > 0 || _pattern.HasChildGraphPatterns)
            {
                if (_pattern.TriplePatterns.Count > 0)
                {
                    patterns.Add(new GraphPattern());
                    _pattern.TriplePatterns.ForEach(tp => patterns[0].AddTriplePattern(tp));
                }
                _pattern.ChildGraphPatterns.ForEach(gp => patterns.Add(gp));
            }
            else
            {
                // If no Triple Patterns and No Child Graph Patterns nothing to do
                return;
            }

            foreach (GraphPattern pattern in patterns)
            {
                if (!IsValidDataPattern(pattern, false)) throw new SparqlUpdateException("Cannot evaluate a DELETE DATA command where any of the Triple Patterns are not concrete triples (variables are not permitted) or any of the GRAPH clauses have nested Graph Patterns");

                // Get the Target Graph
                IGraph target;
                Uri graphUri;
                if (pattern.IsGraph)
                {
                    switch (pattern.GraphSpecifier.TokenType)
                    {
                        case Token.QNAME:
                            throw new NotSupportedException("Graph Specifiers as QNames for DELETE DATA Commands are not supported - please specify an absolute URI instead");
                        case Token.URI:
                            graphUri = UriFactory.Create(pattern.GraphSpecifier.Value);
                            break;
                        default:
                            throw new SparqlUpdateException("Cannot evaluate an DELETE DATA Command as the Graph Specifier is not a QName/URI");
                    }
                }
                else
                {
                    graphUri = null;
                }

                // If the Pattern affects a non-existent Graph then nothing to DELETE
                if (!context.Data.HasGraph(graphUri)) continue;
                target = context.Data.GetModifiableGraph(graphUri);

                // Delete the actual Triples
                INode subj, pred, obj;

                ConstructContext constructContext = new ConstructContext(target, null, false);
                foreach (IConstructTriplePattern p in pattern.TriplePatterns.OfType<IConstructTriplePattern>())
                {
                    subj = p.Subject.Construct(constructContext);
                    pred = p.Predicate.Construct(constructContext);
                    obj = p.Object.Construct(constructContext);

                    target.Retract(new Triple(subj, pred, obj));
                }
            }
        }

        /// <summary>
        /// Processes the Command using the given Update Processor
        /// </summary>
        /// <param name="processor">SPARQL Update Processor</param>
        public override void Process(ISparqlUpdateProcessor processor)
        {
            processor.ProcessDeleteDataCommand(this);
        }

        /// <summary>
        /// Gets the String representation of the Command
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine("DELETE DATA");
            if (_pattern.IsGraph) output.AppendLine("{");
            output.AppendLine(_pattern.ToString());
            if (_pattern.IsGraph) output.AppendLine("}");
            return output.ToString();
        }
    }
}
