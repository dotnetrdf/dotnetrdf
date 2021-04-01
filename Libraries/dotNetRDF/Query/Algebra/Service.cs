/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2021 dotNetRDF Project (http://dotnetrdf.org/)
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
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Represents a Service Clause.
    /// </summary>
    public class Service
        : ITerminalOperator
    {
        private readonly IToken _endpointSpecifier;
        private readonly GraphPattern _pattern;
        private readonly bool _silent = false;

        /// <summary>
        /// Creates a new Service clause with the given Endpoint Specifier and Graph Pattern.
        /// </summary>
        /// <param name="endpointSpecifier">Endpoint Specifier.</param>
        /// <param name="pattern">Graph Pattern.</param>
        /// <param name="silent">Whether Evaluation Errors are suppressed.</param>
        public Service(IToken endpointSpecifier, GraphPattern pattern, bool silent)
        {
            _endpointSpecifier = endpointSpecifier;
            _pattern = pattern;
            _silent = silent;
        }

        /// <summary>
        /// Creates a new Service clause with the given Endpoint Specifier and Graph Pattern.
        /// </summary>
        /// <param name="endpointSpecifier">Endpoint Specifier.</param>
        /// <param name="pattern">Graph Pattern.</param>
        public Service(IToken endpointSpecifier, GraphPattern pattern)
            : this(endpointSpecifier, pattern, false) { }

        /// <summary>
        /// Evaluates the Service Clause by generating instance(s) of <see cref="SparqlRemoteEndpoint">SparqlRemoteEndpoint</see> as required and issuing the query to the remote endpoint(s).
        /// </summary>
        /// <param name="context">Evaluation Context.</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            SparqlRemoteEndpoint endpoint = GetRemoteEndpoint(context);
            try
            {
                context.OutputMultiset = new Multiset();
                foreach (var query in GetRemoteQueries(context, GetBindings(context)))
                {
                    // Try and get a Result Set from the Service
                    SparqlResultSet results = endpoint.QueryWithResultSet(query.ToString());
                    context.CheckTimeout();

                    // Transform this Result Set back into a Multiset
                    foreach (SparqlResult r in results)
                    {
                        context.OutputMultiset.Add(new Set(r));
                    }
                }
                return context.OutputMultiset;
            }
            catch (Exception ex)
            {
                if (_silent)
                {
                    // If Evaluation Errors are SILENT is specified then a Multiset containing a single set with all values unbound is returned
                    // Unless some of the SPARQL queries did return results in which we just return the results we did obtain
                    if (context.OutputMultiset.IsEmpty)
                    {
                        Set s = new Set();
                        foreach (String var in _pattern.Variables.Distinct())
                        {
                            s.Add(var, null);
                        }
                        context.OutputMultiset.Add(s);
                    }
                    return context.OutputMultiset;
                }
                else
                {
                    throw new RdfQueryException("Query execution failed because evaluating a SERVICE clause failed - this may be due to an error with the remote service", ex);
                }
            }
        }

        private SparqlRemoteEndpoint GetRemoteEndpoint(SparqlEvaluationContext context)
        {
            if (_endpointSpecifier.TokenType == Token.URI)
            {
                var baseUri = (context.Query.BaseUri == null) ? String.Empty : context.Query.BaseUri.AbsoluteUri;
                var endpointUri = UriFactory.Create(Tools.ResolveUri(_endpointSpecifier.Value, baseUri));
                return new SparqlRemoteEndpoint(endpointUri);
            }

            if (_endpointSpecifier.TokenType == Token.VARIABLE)
            {
                // Get all the URIs that are bound to this Variable in the Input
                String var = _endpointSpecifier.Value.Substring(1);
                if (!context.InputMultiset.ContainsVariable(var)) throw new RdfQueryException("Cannot evaluate a SERVICE clause which uses a Variable as the Service specifier when the Variable is unbound");

                var serviceEndpoints = context.InputMultiset.Sets
                    .Select(set => set[var])
                    .OfType<IUriNode>()
                    .Distinct()
                    .Select(u => new SparqlRemoteEndpoint(u.Uri));

                return new FederatedSparqlRemoteEndpoint(serviceEndpoints);
            }

            throw new RdfQueryException("SERVICE Specifier must be a URI/Variable Token but a " + _endpointSpecifier.GetType().ToString() + " Token was provided");
        }

        private ISet[] GetBindings(SparqlEvaluationContext context)
        {
            var bindings = new HashSet<ISet>();
            List<String> existingVars = (from v in _pattern.Variables
                                         where context.InputMultiset.ContainsVariable(v)
                                         select v).ToList();

            if (existingVars.Any() || context.Query.Bindings != null)
            {
                // Build the set of possible bindings

                if (context.Query.Bindings != null && !_pattern.Variables.IsDisjoint(context.Query.Bindings.Variables))
                {
                    // Possible Bindings comes from BINDINGS clause
                    // In this case each possibility is a distinct binding tuple defined in the BINDINGS clause
                    foreach (BindingTuple tuple in context.Query.Bindings.Tuples)
                    {
                        bindings.Add(new Set(tuple));
                    }
                }
                else
                {
                    // Possible Bindings get built from current input (if there was a BINDINGS clause the variables it defines are not in this SERVICE clause)
                    // In this case each possibility only contains Variables bound so far
                    foreach (ISet s in context.InputMultiset.Sets)
                    {
                        Set t = new Set();
                        foreach (String var in existingVars)
                        {
                            t.Add(var, s[var]);
                        }
                        bindings.Add(t);
                    }
                }
            }

            return bindings.ToArray();
        }

        private IEnumerable<SparqlQuery> GetRemoteQueries(SparqlEvaluationContext context, ISet[] bindings)
        {
            if (bindings.Length == 0)
            {
                // No pre-bound variables/BINDINGS clause so just return the query
                yield return GetRemoteQuery(context);
            }
            else
            {
                // Split bindings in chunks and inject them
                foreach (var chunk in bindings.ChunkBy(100))
                {
                    var vars = chunk.SelectMany(x => x.Variables).Distinct();
                    var data = new BindingsPattern(vars);
                    foreach (var set in chunk)
                    {
                        var tuple = new BindingTuple(
                            new List<string>(set.Variables),
                            new List<PatternItem>(set.Values.Select(x => new NodeMatchPattern(x))));
                        data.AddTuple(tuple);
                    }
                    var sparqlQuery = GetRemoteQuery(context);
                    sparqlQuery.RootGraphPattern.AddInlineData(data);
                    yield return sparqlQuery;
                }
            }
        }

        private SparqlQuery GetRemoteQuery(SparqlEvaluationContext context)
        {
            // Pass through LIMIT and OFFSET to the remote service

            // Calculate a LIMIT which is the LIMIT plus the OFFSET
            // We'll apply OFFSET locally so don't pass that through explicitly
            var limit = context.Query.Limit;
            if (context.Query.Offset > 0) limit += context.Query.Offset;

            return new SparqlQuery
            {
                QueryType = SparqlQueryType.SelectAll,
                Limit = limit,
                RootGraphPattern = new GraphPattern(_pattern) { IsService = false },
            };
        }

        /// <summary>
        /// Gets the Variables used in the Algebra.
        /// </summary>
        public IEnumerable<String> Variables
        {
            get
            {
                if (_endpointSpecifier.TokenType == Token.VARIABLE)
                {
                    String serviceVar = ((VariableToken)_endpointSpecifier).Value.Substring(1);
                    return _pattern.Variables.Concat(serviceVar.AsEnumerable()).Distinct();
                }
                else
                {
                    return _pattern.Variables.Distinct();
                }
            }
        }

        /// <summary>
        /// Gets the enumeration of floating variables in the algebra i.e. variables that are not guaranteed to have a bound value.
        /// </summary>
        public IEnumerable<String> FloatingVariables
        {
            get
            {
                // Safest to assume all variables are floating as no guarantee the remote service is fully SPARQL compliant
                return Variables;
            }
        }

        /// <summary>
        /// Gets the enumeration of fixed variables in the algebra i.e. variables that are guaranteed to have a bound value.
        /// </summary>
        public IEnumerable<String> FixedVariables { get { return Enumerable.Empty<String>(); } }

        /// <summary>
        /// Gets the Endpoint Specifier.
        /// </summary>
        public IToken EndpointSpecifier
        {
            get
            {
                return _endpointSpecifier;
            }
        }

        /// <summary>
        /// Gets the Graph Pattern.
        /// </summary>
        public GraphPattern Pattern
        {
            get
            {
                return _pattern;
            }
        }

        /// <summary>
        /// Gets the String representation of the Algebra.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Service(" + _endpointSpecifier.Value + ", " + _pattern.ToString() + ")";
        }

        /// <summary>
        /// Converts the Algebra back to a SPARQL Query.
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            SparqlQuery q = new SparqlQuery();
            q.RootGraphPattern = ToGraphPattern();
            q.Optimise();
            return q;
        }

        /// <summary>
        /// Converts the Algebra into a Graph Pattern.
        /// </summary>
        /// <returns></returns>
        public GraphPattern ToGraphPattern()
        {
            GraphPattern p = new GraphPattern(_pattern);
            if (!p.HasModifier)
            {
                p.IsService = true;
                p.GraphSpecifier = _endpointSpecifier;
                return p;
            }
            else
            {
                GraphPattern parent = new GraphPattern();
                parent.IsService = true;
                parent.GraphSpecifier = _endpointSpecifier;
                parent.AddGraphPattern(p);
                return parent;
            }
        }
    }
}
