/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

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
        private IToken _endpointSpecifier;
        private GraphPattern _pattern;
        private bool _silent = false;

        /// <summary>
        /// Creates a new Service clause with the given Endpoint Specifier and Graph Pattern
        /// </summary>
        /// <param name="endpointSpecifier">Endpoint Specifier</param>
        /// <param name="pattern">Graph Pattern</param>
        /// <param name="silent">Whether Evaluation Errors are suppressed</param>
        public Service(IToken endpointSpecifier, GraphPattern pattern, bool silent)
        {
            this._endpointSpecifier = endpointSpecifier;
            this._pattern = pattern;
            this._silent = silent;
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
#if SILVERLIGHT
                throw new PlatformNotSupportedException("SERVICE is not currently supported under Silverlight");
#else
                SparqlRemoteEndpoint endpoint;
                Uri endpointUri;
                String baseUri = (context.Query.BaseUri == null) ? String.Empty : context.Query.BaseUri.ToString();
                SparqlParameterizedString sparqlQuery = new SparqlParameterizedString("SELECT * WHERE ");

                String pattern = this._pattern.ToString();
                pattern = pattern.Substring(pattern.IndexOf('{'));
                sparqlQuery.CommandText += pattern;

                //Pass through LIMIT and OFFSET to the remote service
                if (context.Query.Limit >= 0)
                {
                    //Calculate a LIMIT which is the LIMIT plus the OFFSET
                    //We'll apply OFFSET locally so don't pass that through explicitly
                    int limit = context.Query.Limit;
                    if (context.Query.Offset > 0) limit += context.Query.Offset;
                    sparqlQuery.CommandText += " LIMIT " + limit;
                }

                //Select which service to use
                if (this._endpointSpecifier.TokenType == Token.URI)
                {
                    endpointUri = new Uri(Tools.ResolveUri(this._endpointSpecifier.Value, baseUri));
                    endpoint = new SparqlRemoteEndpoint(endpointUri);
                }
                else if (this._endpointSpecifier.TokenType == Token.VARIABLE)
                {
                    //Get all the URIs that are bound to this Variable in the Input
                    String var = this._endpointSpecifier.Value.Substring(1);
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

                    //Now generate a Federated Remote Endpoint
                    List<SparqlRemoteEndpoint> serviceEndpoints = new List<SparqlRemoteEndpoint>();
                    services.ForEach(u => serviceEndpoints.Add(new SparqlRemoteEndpoint(u.Uri)));
                    endpoint = new FederatedSparqlRemoteEndpoint(serviceEndpoints);
                }
                else
                {
                    //Note that we must bypass the SILENT operator in this case as this is not an evaluation failure
                    //but a query syntax error
                    bypassSilent = true;
                    throw new RdfQueryException("SERVICE Specifier must be a URI/Variable Token but a " + this._endpointSpecifier.GetType().ToString() + " Token was provided");
                }

                //Where possible do substitution and execution to get accurate and correct SERVICE results
                context.OutputMultiset = new Multiset();
                List<String> existingVars = (from v in this._pattern.Variables
                                             where context.InputMultiset.ContainsVariable(v)
                                             select v).ToList();

                if (existingVars.Any() || context.Query.Bindings != null)
                {
                    //Pre-bound variables/BINDINGS clause so do substitution and execution

                    //Build the set of possible bindings
                    HashSet<ISet> bindings = new HashSet<ISet>();
                    if (context.Query.Bindings != null && !this._pattern.Variables.IsDisjoint(context.Query.Bindings.Variables))
                    {
                        //Possible Bindings comes from BINDINGS clause
                        //In this case each possibility is a distinct binding tuple defined in the BINDINGS clause
                        foreach (BindingTuple tuple in context.Query.Bindings.Tuples)
                        {
                            bindings.Add(new Set(tuple));
                        }
                    }
                    else
                    {
                        //Possible Bindings get built from current input (if there was a BINDINGS clause the variables it defines are not in this SERVICE clause)
                        //In this case each possibility only contains Variables bound so far
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

                    //Execute the Query for every possible Binding and build up our Output Multiset from all the results
                    foreach (ISet s in bindings)
                    {
                        //Q: Should we continue processing here if and when we hit an error?

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
                    //No pre-bound variables/BINDINGS clause so just execute the query

                    //Try and get a Result Set from the Service
                    SparqlResultSet results = endpoint.QueryWithResultSet(sparqlQuery.ToString());

                    //Transform this Result Set back into a Multiset
                    foreach (SparqlResult r in results.Results)
                    {
                        context.OutputMultiset.Add(new Set(r));
                    }

                    return context.OutputMultiset;
                }
#endif
            }
            catch (Exception ex)
            {
                if (this._silent && !bypassSilent)
                {

                    //If Evaluation Errors are SILENT is specified then a Multiset containing a single set with all values unbound is returned
                    //Unless some of the SPARQL queries did return results in which we just return the results we did obtain
                    if (context.OutputMultiset.IsEmpty)
                    {
                        Set s = new Set();
                        foreach (String var in this._pattern.Variables.Distinct())
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
                if (this._endpointSpecifier.TokenType == Token.VARIABLE)
                {
                    String serviceVar = ((VariableToken)this._endpointSpecifier).Value.Substring(1);
                    return this._pattern.Variables.Concat(serviceVar.AsEnumerable()).Distinct();
                }
                else
                {
                    return this._pattern.Variables.Distinct();
                }
            }
        }

        /// <summary>
        /// Gets the Endpoint Specifier
        /// </summary>
        public IToken EndpointSpecifier
        {
            get
            {
                return this._endpointSpecifier;
            }
        }

        /// <summary>
        /// Gets the Graph Pattern
        /// </summary>
        public GraphPattern Pattern
        {
            get
            {
                return this._pattern;
            }
        }

        /// <summary>
        /// Gets the String representation of the Algebra
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Service(" + this._endpointSpecifier.Value + ", " + this._pattern.ToAlgebra().ToString() + ")";
        }

        /// <summary>
        /// Converts the Algebra back to a SPARQL Query
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            SparqlQuery q = new SparqlQuery();
            q.RootGraphPattern = this.ToGraphPattern();
            q.Optimise();
            return q;
        }

        /// <summary>
        /// Converts the Algebra into a Graph Pattern
        /// </summary>
        /// <returns></returns>
        public GraphPattern ToGraphPattern()
        {
            GraphPattern p = new GraphPattern(this._pattern);
            if (!p.HasModifier)
            {
                p.IsService = true;
                p.GraphSpecifier = this._endpointSpecifier;
                return p;
            }
            else
            {
                GraphPattern parent = new GraphPattern();
                parent.IsService = true;
                parent.GraphSpecifier = this._endpointSpecifier;
                parent.AddGraphPattern(p);
                return parent;
            }
        }
    }
}
