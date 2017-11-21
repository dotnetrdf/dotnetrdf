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
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Represents a Service Clause
    /// </summary>
    public class Service 
        : ITerminalOperator
    {
        private readonly IToken _endpointSpecifier;
        private readonly GraphPattern _pattern;
        private readonly bool _silent = false;

        /// <summary>
        /// Creates a new Service clause with the given Endpoint Specifier and Graph Pattern
        /// </summary>
        /// <param name="endpointSpecifier">Endpoint Specifier</param>
        /// <param name="pattern">Graph Pattern</param>
        /// <param name="silent">Whether Evaluation Errors are suppressed</param>
        public Service(IToken endpointSpecifier, GraphPattern pattern, bool silent)
        {
            _endpointSpecifier = endpointSpecifier;
            _pattern = pattern;
            _silent = silent;
        }

        /// <summary>
        /// Creates a new Service clause with the given Endpoint Specifier and Graph Pattern
        /// </summary>
        /// <param name="endpointSpecifier">Endpoint Specifier</param>
        /// <param name="pattern">Graph Pattern</param>
        public Service(IToken endpointSpecifier, GraphPattern pattern)
            : this(endpointSpecifier, pattern, false) { }

        /// <summary>
        /// Evaluates the Service Clause by generating instance(s) of <see cref="SparqlRemoteEndpoint">SparqlRemoteEndpoint</see> as required and issuing the query to the remote endpoint(s)
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            bool bypassSilent = false;
            try
            {
                SparqlRemoteEndpoint endpoint;
                Uri endpointUri;
                String baseUri = (context.Query.BaseUri == null) ? String.Empty : context.Query.BaseUri.AbsoluteUri;
                SparqlParameterizedString sparqlQuery = new SparqlParameterizedString("SELECT * WHERE ");

                String pattern = _pattern.ToString();
                pattern = pattern.Substring(pattern.IndexOf('{'));
                sparqlQuery.CommandText += pattern;

                // Pass through LIMIT and OFFSET to the remote service
                if (context.Query.Limit >= 0)
                {
                    // Calculate a LIMIT which is the LIMIT plus the OFFSET
                    // We'll apply OFFSET locally so don't pass that through explicitly
                    int limit = context.Query.Limit;
                    if (context.Query.Offset > 0) limit += context.Query.Offset;
                    sparqlQuery.CommandText += " LIMIT " + limit;
                }

                // Select which service to use
                if (_endpointSpecifier.TokenType == Token.URI)
                {
                    endpointUri = UriFactory.Create(Tools.ResolveUri(_endpointSpecifier.Value, baseUri));
                    endpoint = new SparqlRemoteEndpoint(endpointUri);
                }
                else if (_endpointSpecifier.TokenType == Token.VARIABLE)
                {
                    // Get all the URIs that are bound to this Variable in the Input
                    String var = _endpointSpecifier.Value.Substring(1);
                    if (!context.InputMultiset.ContainsVariable(var)) throw new RdfQueryException("Cannot evaluate a SERVICE clause which uses a Variable as the Service specifier when the Variable is unbound");
                    List<IUriNode> services = new List<IUriNode>();
                    foreach (ISet s in context.InputMultiset.Sets)
                    {
                        if (s.ContainsVariable(var))
                        {
                            if (s[var].NodeType == NodeType.Uri)
                            {
                                services.Add((IUriNode)s[var]);
                            }
                        }
                    }
                    services = services.Distinct().ToList();

                    // Now generate a Federated Remote Endpoint
                    List<SparqlRemoteEndpoint> serviceEndpoints = new List<SparqlRemoteEndpoint>();
                    services.ForEach(u => serviceEndpoints.Add(new SparqlRemoteEndpoint(u.Uri)));
                    endpoint = new FederatedSparqlRemoteEndpoint(serviceEndpoints);
                }
                else
                {
                    // Note that we must bypass the SILENT operator in this case as this is not an evaluation failure
                    // but a query syntax error
                    bypassSilent = true;
                    throw new RdfQueryException("SERVICE Specifier must be a URI/Variable Token but a " + _endpointSpecifier.GetType().ToString() + " Token was provided");
                }

                // Where possible do substitution and execution to get accurate and correct SERVICE results
                context.OutputMultiset = new Multiset();
                List<String> existingVars = (from v in _pattern.Variables
                                             where context.InputMultiset.ContainsVariable(v)
                                             select v).ToList();

                if (existingVars.Any() || context.Query.Bindings != null)
                {
                    // Pre-bound variables/BINDINGS clause so do substitution and execution

                    // Build the set of possible bindings
                    HashSet<ISet> bindings = new HashSet<ISet>();
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

                    // Execute the Query for every possible Binding and build up our Output Multiset from all the results
                    foreach (ISet s in bindings)
                    {
                        // Q: Should we continue processing here if and when we hit an error?

                        foreach (String var in s.Variables)
                        {
                            sparqlQuery.SetVariable(var, s[var]);
                        }
                        SparqlResultSet results = endpoint.QueryWithResultSet(sparqlQuery.ToString());
                        context.CheckTimeout();

                        foreach (SparqlResult r in results)
                        {
                            Set t = new Set(r);
                            foreach (String var in s.Variables)
                            {
                                t.Add(var, s[var]);
                            }
                            context.OutputMultiset.Add(t);
                        }
                    }

                    return context.OutputMultiset;
                }
                else
                {
                    // No pre-bound variables/BINDINGS clause so just execute the query

                    // Try and get a Result Set from the Service
                    SparqlResultSet results = endpoint.QueryWithResultSet(sparqlQuery.ToString());

                    // Transform this Result Set back into a Multiset
                    foreach (SparqlResult r in results.Results)
                    {
                        context.OutputMultiset.Add(new Set(r));
                    }

                    return context.OutputMultiset;
                }
            }
            catch (Exception ex)
            {
                if (_silent && !bypassSilent)
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

        /// <summary>
        /// Gets the Variables used in the Algebra
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
        /// Gets the enumeration of floating variables in the algebra i.e. variables that are not guaranteed to have a bound value
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
        /// Gets the enumeration of fixed variables in the algebra i.e. variables that are guaranteed to have a bound value
        /// </summary>
        public IEnumerable<String> FixedVariables { get { return Enumerable.Empty<String>(); } }

        /// <summary>
        /// Gets the Endpoint Specifier
        /// </summary>
        public IToken EndpointSpecifier
        {
            get
            {
                return _endpointSpecifier;
            }
        }

        /// <summary>
        /// Gets the Graph Pattern
        /// </summary>
        public GraphPattern Pattern
        {
            get
            {
                return _pattern;
            }
        }

        /// <summary>
        /// Gets the String representation of the Algebra
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Service(" + _endpointSpecifier.Value + ", " + _pattern.ToString() + ")";
        }

        /// <summary>
        /// Converts the Algebra back to a SPARQL Query
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
        /// Converts the Algebra into a Graph Pattern
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
