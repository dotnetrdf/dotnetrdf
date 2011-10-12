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
    /// Represents the SPARQL Update INSERT DATA command
    /// </summary>
    public class InsertDataCommand : SparqlUpdateCommand
    {
        private GraphPattern _pattern;

        /// <summary>
        /// Creates a new INSERT DATA command
        /// </summary>
        /// <param name="pattern">Pattern containing concrete Triples to insert</param>
        public InsertDataCommand(GraphPattern pattern)
            : base(SparqlUpdateCommandType.InsertData) 
        {
            if (!this.IsValidDataPattern(pattern, true)) throw new SparqlUpdateException("Cannot create a INSERT DATA command where any of the Triple Patterns are not concrete triples (variables are not permitted) or a GRAPH clause has nested Graph Patterns");
            this._pattern = pattern;
        }

        /// <summary>
        /// Determines whether a Graph Pattern is valid for use in an INSERT DATA command
        /// </summary>
        /// <param name="p">Graph Pattern</param>
        /// <param name="top">Is this the top level pattern?</param>
        /// <returns></returns>
        private bool IsValidDataPattern(GraphPattern p, bool top)
        {
            if (p.IsGraph)
            {
                //If a GRAPH clause then all triple patterns must be constructable and have no Child Graph Patterns
                return !p.HasChildGraphPatterns && p.TriplePatterns.All(tp => tp is IConstructTriplePattern && ((IConstructTriplePattern)tp).HasNoExplicitVariables);
            }
            else if (p.IsExists || p.IsMinus || p.IsNotExists || p.IsOptional || p.IsService || p.IsSubQuery || p.IsUnion)
            {
                //EXISTS/MINUS/NOT EXISTS/OPTIONAL/SERVICE/Sub queries/UNIONs are not permitted
                return false;
            }
            else
            {
                //For other patterns all Triple patterns must be constructable with no explicit variables
                //If top level then any Child Graph Patterns must be valid
                //Otherwise must have no Child Graph Patterns
                return p.TriplePatterns.All(tp => tp is IConstructTriplePattern && ((IConstructTriplePattern)tp).HasNoExplicitVariables) && ((top && p.ChildGraphPatterns.All(gp => IsValidDataPattern(gp, false))) || !p.HasChildGraphPatterns);
            }
        }

        /// <summary>
        /// Gets the Data Pattern containing Triples to insert
        /// </summary>
        public GraphPattern DataPattern
        {
            get
            {
                return this._pattern;
            }
        }

        /// <summary>
        /// Gets whether the Command affects a single Graph
        /// </summary>
        public override bool AffectsSingleGraph
        {
            get
            {
                if (!this._pattern.HasChildGraphPatterns)
                {
                    return true;
                }
                else
                {
                    List<String> affectedUris = new List<string>();
                    if (this._pattern.IsGraph)
                    {
                        affectedUris.Add(this._pattern.GraphSpecifier.Value);
                    }
                    else
                    {
                        affectedUris.Add(null);
                    }
                    affectedUris.AddRange(from p in this._pattern.ChildGraphPatterns
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
            if (graphUri.ToSafeString().Equals(GraphCollection.DefaultGraphUri)) graphUri = null;

            List<String> affectedUris = new List<string>();
            if (this._pattern.IsGraph)
            {
                affectedUris.Add(this._pattern.GraphSpecifier.Value);
            } 
            else 
            {
                affectedUris.Add(String.Empty);
            }
            if (this._pattern.HasChildGraphPatterns)
            {
                affectedUris.AddRange(from p in this._pattern.ChildGraphPatterns
                                      where p.IsGraph
                                      select p.GraphSpecifier.Value);
            }
            if (affectedUris.Any(u => u != null && u.Equals(GraphCollection.DefaultGraphUri))) affectedUris.Add(String.Empty);

            return affectedUris.Contains(graphUri.ToSafeString());
        }

        /// <summary>
        /// Evaluates the Command in the given Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        public override void Evaluate(SparqlUpdateEvaluationContext context)
        {
            //Split the Pattern into the set of Graph Patterns
            List<GraphPattern> patterns = new List<GraphPattern>();
            if (this._pattern.IsGraph)
            {
                patterns.Add(this._pattern);
            }
            else if (this._pattern.TriplePatterns.Count > 0 || this._pattern.HasChildGraphPatterns)
            {
                if (this._pattern.TriplePatterns.Count > 0)
                {
                    patterns.Add(new GraphPattern());
                    this._pattern.TriplePatterns.ForEach(tp => patterns[0].AddTriplePattern(tp));
                }
                this._pattern.ChildGraphPatterns.ForEach(gp => patterns.Add(gp));
            }
            else
            {
                //If no Triple Patterns and No Child Graph Patterns nothing to do
                return;
            }

            foreach (GraphPattern pattern in patterns)
            {
                if (!this.IsValidDataPattern(pattern, false)) throw new SparqlUpdateException("Cannot evaluate a INSERT DATA command where any of the Triple Patterns are not concrete triples (variables are not permitted) or any of the GRAPH clauses have nested Graph Patterns");

                //Get the Target Graph
                IGraph target;
                Uri graphUri;
                if (pattern.IsGraph)
                {
                    switch (pattern.GraphSpecifier.TokenType)
                    {
                        case Token.QNAME:
                            throw new NotSupportedException("Graph Specifiers as QNames for INSERT DATA Commands are not supported - please specify an absolute URI instead");
                        case Token.URI:
                            graphUri = new Uri(pattern.GraphSpecifier.Value);
                            break;
                        default:
                            throw new SparqlUpdateException("Cannot evaluate an INSERT DATA Command as the Graph Specifier is not a QName/URI");
                    }
                }
                else
                {
                    graphUri = null;
                }
                if (context.Data.HasGraph(graphUri))
                {
                    target = context.Data.GetModifiableGraph(graphUri);
                }
                else
                {
                    //If the Graph does not exist then it must be created
                    target = new Graph();
                    target.BaseUri = graphUri;
                    context.Data.AddGraph(target);
                }

                //Insert the actual Triples
                INode subj, pred, obj;
                ConstructContext constructContext = new ConstructContext(target, null, true);
                foreach (IConstructTriplePattern p in pattern.TriplePatterns.OfType<IConstructTriplePattern>())
                {
                    subj = p.Subject.Construct(constructContext);
                    pred = p.Predicate.Construct(constructContext);
                    obj = p.Object.Construct(constructContext);

                    target.Assert(new Triple(subj, pred, obj));
                }
            }
        }

        /// <summary>
        /// Processes the Command using the given Update Processor
        /// </summary>
        /// <param name="processor">SPARQL Update Processor</param>
        public override void Process(ISparqlUpdateProcessor processor)
        {
            processor.ProcessInsertDataCommand(this);
        }

        /// <summary>
        /// Gets the String representation of the Command
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine("INSERT DATA");
            if (this._pattern.IsGraph) output.AppendLine("{");
            output.AppendLine(this._pattern.ToString());
            if (this._pattern.IsGraph) output.AppendLine("}");
            return output.ToString();
        }
    }
}
