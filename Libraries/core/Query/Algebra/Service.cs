using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Represents a Service Clause
    /// </summary>
    public class Service : ISparqlAlgebra
    {
        private IToken _endpointSpecifier;
        private GraphPattern _pattern;

        /// <summary>
        /// Creates a new Service clause with the given Endpoint Specifier and Graph Pattern
        /// </summary>
        /// <param name="endpointSpecifier">Endpoint Specifier</param>
        /// <param name="pattern">Graph Pattern</param>
        public Service(IToken endpointSpecifier, GraphPattern pattern)
        {
            this._endpointSpecifier = endpointSpecifier;
            this._pattern = pattern;
        }

        /// <summary>
        /// Evaluates the Service Clause by generating instance(s) of <see cref="SparqlRemoteEndpoint">SparqlRemoteEndpoint</see> as required and issuing the query to the remote endpoint(s)
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            try
            {
                SparqlRemoteEndpoint endpoint;
                Uri endpointUri;
                String baseUri = (context.Query.BaseUri == null) ? String.Empty : context.Query.BaseUri.ToString();
                SparqlParameterizedString sparqlQuery = new SparqlParameterizedString("SELECT * WHERE " + this._pattern.ToString());

                //Pass through LIMIT and OFFSET to the remote service
                if (context.Query.Limit >= 0)
                {
                    //Calculate a LIMIT which is the LIMIT plus the OFFSET
                    //We'll apply OFFSET locally so don't pass that through explicitly
                    int limit = context.Query.Limit;
                    if (context.Query.Offset > 0) limit += context.Query.Offset;
                    sparqlQuery.QueryText += " LIMIT " + limit;
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
                    List<UriNode> services = new List<UriNode>();
                    foreach (Set s in context.InputMultiset.Sets)
                    {
                        if (s.ContainsVariable(var))
                        {
                            if (s[var].NodeType == NodeType.Uri)
                            {
                                services.Add((UriNode)s[var]);
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
                    HashSet<Set> bindings = new HashSet<Set>();
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
                        foreach (Set s in context.InputMultiset.Sets)
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
                    foreach (Set s in bindings)
                    {
                        foreach (String var in s.Variables)
                        {
                            sparqlQuery.SetVariable(var, s[var]);
                        }
                        SparqlResultSet results = endpoint.QueryWithResultSet(sparqlQuery.ToString());

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

            }
            catch (Exception ex)
            {
                throw new RdfQueryException("Query execution failed because evaluating a SERVICE clause failed - this may be due to an error with the remote service", ex);
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

        public SparqlQuery ToQuery()
        {
            SparqlQuery q = new SparqlQuery();
            q.RootGraphPattern = this.ToGraphPattern();
            q.Optimise();
            return q;
        }

        public GraphPattern ToGraphPattern()
        {
            GraphPattern p = this._pattern;
            if (!p.IsService)
            {
                p.IsService = true;
                p.GraphSpecifier = this._endpointSpecifier;
            }
            return p;
        }
    }
}
