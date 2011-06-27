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

If this license is not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/


using System;
using System.Collections.Generic;
using System.Linq;
using SemWeb;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Functions;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Patterns;
using SW = SemWeb;

namespace VDS.RDF.Interop.SemWeb
{
    /// <summary>
    /// Provides a dotNetRDF In-memory store to SemWeb as both a StatementSource and a StatementSink
    /// </summary>
    public class InMemoryStoreSource : StatementSink, StatementSource, SelectableSource, ModifiableSource, QueryableSource
    {
        private IInMemoryQueryableStore _store;
        private Dictionary<int, SemWebMapping> _mappings = new Dictionary<int, SemWebMapping>();

        /// <summary>
        /// Creates a new In-,emory Store Source
        /// </summary>
        /// <param name="store">In-memory Store</param>
        public InMemoryStoreSource(IInMemoryQueryableStore store)
        {
            this._store = store;
            this._mappings = new Dictionary<int, SemWebMapping>();
        }

        /// <summary>
        /// Returns that Statements returned are not necessarily distinct
        /// </summary>
        public bool Distinct
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the Store that this Source is a wrapper around
        /// </summary>
        public IInMemoryQueryableStore Store
        {
            get
            {
                return this._store;
            }
        }

        /// <summary>
        /// Selects all statements from this source and streams them into the given Sink
        /// </summary>
        /// <param name="sink">Statement Sink</param>
        public void Select(StatementSink sink)
        {
            foreach (IGraph g in this._store.Graphs)
            {
                //Get the Hash Code of the Graphs URI and create a new empty mapping if necessary
                Entity graphUri;
                int hash;
                if (g.BaseUri == null)
                {
                    graphUri = new Entity(GraphCollection.DefaultGraphUri);
                    hash = new Uri(GraphCollection.DefaultGraphUri).GetEnhancedHashCode();
                }
                else
                {
                    graphUri = new Entity(g.BaseUri.ToString());
                    hash = g.BaseUri.GetEnhancedHashCode();
                }

                SemWebMapping mapping = this.GetMapping(hash, g);
                foreach (Triple t in g.Triples)
                {
                    Statement stmt = SemWebConverter.ToSemWeb(t, mapping);
                    stmt.Meta = graphUri;
                    if (!sink.Add(stmt)) return;
                }
            }
        }

        /// <summary>
        /// Adds a statement to this Source
        /// </summary>
        /// <param name="statement">Statement</param>
        /// <returns></returns>
        public bool Add(Statement statement)
        {
            IGraph g;
            Uri graphUri;
            int hash;

            //Set the Graph URI based on the Statement Meta field
            if (statement.Meta != Statement.DefaultMeta && statement.Meta.Uri != null)
            {
                if (statement.Meta.Uri.Equals(GraphCollection.DefaultGraphUri))
                {
                    graphUri = null;
                    hash = new Uri(GraphCollection.DefaultGraphUri).GetEnhancedHashCode();
                }
                else
                {
                    graphUri = new Uri(statement.Meta.Uri);
                    hash = graphUri.GetEnhancedHashCode();
                }
            }
            else
            {
                graphUri = null;
                hash = new Uri(GraphCollection.DefaultGraphUri).GetEnhancedHashCode();
            }
            if (!this._store.HasGraph(graphUri))
            {
                g = new Graph();
                if (graphUri != null) g.BaseUri = graphUri;
                this._store.Add(g);
            }
            else
            {
                g = this._store.Graph(graphUri);
            }

            //Assert into the appropriate Graph
            g.Assert(SemWebConverter.FromSemWeb(statement, this.GetMapping(hash, g)));

            //We never ask the sink to stop streaming statements so we always return true
            return true;
        }

        /// <summary>
        /// Checks whether the Source contains Triples matching the given Template
        /// </summary>
        /// <param name="template">Statement Template</param>
        /// <returns></returns>
        public bool Contains(Statement template)
        {
            if (template.Meta != null)
            {
                //Check against the specific Graph if it exists
                if (template.Meta.Uri != null)
                {
                    Uri graphUri = new Uri(template.Meta.Uri);
                    if (this._store.HasGraph(graphUri))
                    {
                        return this.TemplateToEnumerable(template, this._store.Graph(graphUri)).Any();
                    }
                }
            }
            else
            {
                //Have to check against each Graph until we find a match
                foreach (IGraph g in this._store.Graphs)
                {
                    //Convert the Template to an Enumerable and call Any() on it to see if it is non-empty
                    if (this.TemplateToEnumerable(template, g).Any()) return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns whether the Source contains any Statement which contains the given Resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool Contains(Resource resource)
        {
            //Have to check against each Graph until we find a match
            foreach (IGraph g in this._store.Graphs)
            {
                //First we need to convert the Resource to a Node
                INode n = SemWebConverter.FromSemWeb(resource, this.GetMapping(g));

                if (g.Nodes.Contains(n))
                {
                    //If it's in the Node collection check that a Triple with the given Node exists
                    if (g.Triples.Any(t => t.Involves(n))) return true;
                }
            }

            //If none of the Graphs contained it we return false
            return false;
        }

        /// <summary>
        /// Selects Statements from the Source based on a Filter
        /// </summary>
        /// <param name="filter">Statement Filter</param>
        /// <param name="sink">Sink to stream results to</param>
        public void Select(SelectFilter filter, StatementSink sink)
        {
            IEnumerable<Triple> ts = Enumerable.Empty<Triple>();
            if (filter.Metas != null)
            {
                //This applies over some Graphs
                foreach (Entity meta in filter.Metas)
                {
                    if (meta.Uri != null)
                    {
                        Uri graphUri = new Uri(meta.Uri);
                        if (this._store.HasGraph(graphUri))
                        {
                            ts = ts.Concat(this.FilterToEnumerable(filter, this._store.Graph(graphUri)));
                        }
                    }
                }
            }
            else
            {
                //This applies over all Graphs
                foreach (IGraph g in this._store.Graphs)
                {
                    ts = ts.Concat(this.FilterToEnumerable(filter, g));
                }
            }

            int count = 0;
            foreach (Triple t in ts)
            {
                //Apply limit if applicable
                if (filter.Limit > 0 && count >= filter.Limit) return;

                Statement stmt = SemWebConverter.ToSemWeb(t, this.GetMapping(t.Graph));
                stmt.Meta = new Entity(t.GraphUri.ToString());

                if (filter.LiteralFilters != null)
                {
                    if (LiteralFilter.MatchesFilters(stmt.Object, filter.LiteralFilters, this))
                    {
                        //If the Object matched the filters then we return the Triple and stop
                        //streaming if the sink tells us to
                        if (!sink.Add(stmt)) return;
                        count++;
                    }
                }
                else
                {
                    //Just add the statement and stop if the sink tells us to stop streaming
                    if (!sink.Add(stmt)) return;
                    count++;
                }
            }
        }

        /// <summary>
        /// Selects Statements from the Source based on a Template
        /// </summary>
        /// <param name="template">Statement Template</param>
        /// <param name="sink">Sink to stream results to</param>
        public void Select(Statement template, StatementSink sink)
        {
            //Convert Template to an Enumerable
            IEnumerable<Triple> ts = Enumerable.Empty<Triple>();
            int hash;

            if (template.Meta != Statement.DefaultMeta && template.Meta != null)
            {
                //Select from the specific Graph if it exists
                Uri graphUri;
                if (template.Meta.Uri == null)
                {
                    hash = new Uri(GraphCollection.DefaultGraphUri).GetEnhancedHashCode();
                    graphUri = null;
                } 
                else 
                {
                    graphUri = new Uri(template.Meta.Uri);
                    hash = graphUri.GetEnhancedHashCode();
                }
                if (this._store.HasGraph(graphUri))
                {
                    ts = this.TemplateToEnumerable(template, this._store.Graph(graphUri));
                    SemWebMapping mapping = this.GetMapping(hash, this._store.Graph(graphUri));

                    foreach (Triple t in ts)
                    {
                        //Keep streaming Triples until the sink tells us to stop
                        Statement stmt = SemWebConverter.ToSemWeb(t, mapping);
                        if (!sink.Add(stmt)) return;
                    }
                }
            }
            else
            {
                //Output the results from each Graph in turn
                foreach (IGraph g in this._store.Graphs)
                {
                    Entity graphUri;
                    if (g.BaseUri == null) 
                    {
                        hash = new Uri(GraphCollection.DefaultGraphUri).GetEnhancedHashCode();
                        graphUri = new Entity(GraphCollection.DefaultGraphUri);
                    } 
                    else 
                    {
                        hash = g.BaseUri.GetEnhancedHashCode();
                        graphUri = new Entity(g.BaseUri.ToString());
                    }
                    SemWebMapping mapping = this.GetMapping(hash, g);
                    foreach (Triple t in this.TemplateToEnumerable(template, g))
                    {
                        Statement stmt = SemWebConverter.ToSemWeb(t, mapping);
                        stmt.Meta = graphUri;
                        if (!sink.Add(stmt)) return;
                    }
                }
            }
        }

        #region Helper Methods

        private SemWebMapping GetMapping(int hash, IGraph g)
        {
            if (!this._mappings.ContainsKey(hash)) this._mappings.Add(hash, new SemWebMapping(g));
            return this._mappings[hash];
        }

        private SemWebMapping GetMapping(IGraph g)
        {
            if (g == null) return new SemWebMapping(new Graph());

            int hash = (g.BaseUri == null) ? new Uri(GraphCollection.DefaultGraphUri).GetEnhancedHashCode() : g.BaseUri.GetEnhancedHashCode();
            return this.GetMapping(hash, g);
        }

        /// <summary>
        /// Converts a SemWeb Statement Template to an IEnumerable of Triples
        /// </summary>
        /// <param name="template">Statement Template</param>
        /// <param name="g">Graph the Template should be created for</param>
        /// <returns></returns>
        private IEnumerable<Triple> TemplateToEnumerable(Statement template, IGraph g)
        {
            INode s, p, o;
            int hash;
            if (g.BaseUri == null)
            {
                hash = new Uri(GraphCollection.DefaultGraphUri).GetEnhancedHashCode();
            }
            else
            {
                hash = g.BaseUri.GetEnhancedHashCode();
            }
            if (template.Subject is Variable)
            {
                if (template.Predicate is Variable)
                {
                    if (template.Object is Variable)
                    {
                        //All three things are variables so this just checks that some Triple(s) are present
                        return g.Triples;
                    }
                    else
                    {
                        //Subject & Predicate are Variables
                        //Convert the Object and do a WithObject().Any() call
                        o = SemWebConverter.FromSemWeb(template.Object, this.GetMapping(hash, g));

                        return g.GetTriplesWithObject(o);
                    }
                }
                else if (template.Object is Variable)
                {
                    //Subject & Object are variables
                    //Convert the Predicate and do a WithPredicate() call
                    p = SemWebConverter.FromSemWeb(template.Predicate, this.GetMapping(hash, g));

                    return g.GetTriplesWithPredicate(p);
                }
                else
                {
                    //Subject is a Variable
                    //Convert the Predicate and Object and do a WithPredicateObject() call
                    p = SemWebConverter.FromSemWeb(template.Predicate, this.GetMapping(hash, g));
                    o = SemWebConverter.FromSemWeb(template.Object, this.GetMapping(hash, g));

                    return g.GetTriplesWithPredicateObject(p, o);
                }
            }
            else if (template.Predicate is Variable)
            {
                if (template.Object is Variable)
                {
                    //Predicate & Object are Variables
                    //Convert the Subject and do a WithSubject() call
                    s = SemWebConverter.FromSemWeb(template.Subject, this.GetMapping(hash, g));

                    return g.GetTriplesWithSubject(s);
                }
                else
                {
                    //Predicate is a Variable
                    //Convert the Subject and Object and do a WithSubjectObject() call
                    s = SemWebConverter.FromSemWeb(template.Subject, this.GetMapping(hash, g));
                    o = SemWebConverter.FromSemWeb(template.Object, this.GetMapping(hash, g));

                    return g.GetTriplesWithSubjectObject(s, o);
                }
            }
            else if (template.Object is Variable)
            {
                //Object is a Variable
                //Convert the Subject and Predicate and do a WithSubjectPredicate() call
                s = SemWebConverter.FromSemWeb(template.Subject, this.GetMapping(hash, g));
                p = SemWebConverter.FromSemWeb(template.Predicate, this.GetMapping(hash, g));

                return g.GetTriplesWithSubjectPredicate(s, p);
            }
            else
            {
                //Just convert the Triple and do a Contains() call
                Triple t = SemWebConverter.FromSemWeb(template, this.GetMapping(hash, g));
                if (g.ContainsTriple(t))
                {
                    return t.AsEnumerable();
                }
                else
                {
                    return Enumerable.Empty<Triple>();
                }
            }
        }

        private IEnumerable<Triple> FilterToEnumerable(SelectFilter filter, IGraph g)
        {
            //Want to build an IEnumerable based on the Filter
            IEnumerable<Triple> ts = Enumerable.Empty<Triple>();
            INode s, p, o;
            int hash = (g.BaseUri == null) ? new Uri(GraphCollection.DefaultGraphUri).GetEnhancedHashCode() : g.BaseUri.GetEnhancedHashCode();

            if (filter.Subjects != null)
            {
                if (filter.Predicates != null)
                {
                    //Subject-Predicate filter
                    foreach (Entity subj in filter.Subjects)
                    {
                        s = SemWebConverter.FromSemWeb(subj, this.GetMapping(hash, g));
                        foreach (Entity pred in filter.Predicates)
                        {
                            p = SemWebConverter.FromSemWeb(pred, this.GetMapping(hash, g));
                            ts = ts.Concat(g.GetTriplesWithSubjectPredicate(s, p));
                        }
                    }
                }
                else if (filter.Objects != null)
                {
                    //Subject-Object filter
                    foreach (Entity subj in filter.Subjects)
                    {
                        s = SemWebConverter.FromSemWeb(subj, this.GetMapping(hash, g));
                        foreach (Resource obj in filter.Objects)
                        {
                            o = SemWebConverter.FromSemWeb(obj, this.GetMapping(hash, g));
                            ts = ts.Concat(g.GetTriplesWithSubjectObject(s, o));
                        }
                    }
                }
                else
                {
                    //Subjects filter
                    foreach (Entity subj in filter.Subjects)
                    {
                        s = SemWebConverter.FromSemWeb(subj, this.GetMapping(hash, g));
                        ts = ts.Concat(g.GetTriplesWithSubject(s));
                    }
                }
            }
            else if (filter.Predicates != null)
            {
                if (filter.Objects != null)
                {
                    //Predicate-Object Filter
                    foreach (Entity pred in filter.Predicates)
                    {
                        p = SemWebConverter.FromSemWeb(pred, this.GetMapping(hash, g));
                        foreach (Resource obj in filter.Objects)
                        {
                            o = SemWebConverter.FromSemWeb(obj, this.GetMapping(hash, g));
                            ts = ts.Concat(g.GetTriplesWithPredicateObject(p, o));
                        }
                    }
                }
                else
                {
                    //Predicate Filter
                    foreach (Entity pred in filter.Predicates)
                    {
                        p = SemWebConverter.FromSemWeb(pred, this.GetMapping(hash, g));
                        ts = ts.Concat(g.GetTriplesWithPredicate(p));
                    }
                }
            }
            else if (filter.Objects != null)
            {
                //Object Filter
                foreach (Resource obj in filter.Objects)
                {
                    o = SemWebConverter.FromSemWeb(obj, this.GetMapping(hash, g));
                    ts = ts.Concat(g.GetTriplesWithObject(o));
                }
            }
            else
            {
                //Everything is null so this is a Select All
                ts = g.Triples;
            }

            return ts;
        }

        /// <summary>
        /// Helper method which converts a SemWeb resource into a PatternItem for use in a SPARQL Triple Pattern
        /// </summary>
        /// <param name="r">Resource</param>
        /// <param name="mapping">Mapping of Variables &amp; Blank Nodes to Pattern Items</param>
        /// <param name="g">Graph</param>
        /// <returns></returns>
        private PatternItem FromSemWeb(Resource r, IGraph g, Dictionary<String, PatternItem> mapping)
        {
            if (r is Variable)
            {
                if (mapping.ContainsKey(r.ToString()))
                {
                    return mapping[r.ToString()];
                }
                else
                {
                    PatternItem temp = new VariablePattern(r.ToString());
                    mapping.Add(r.ToString(), temp);
                    return temp;
                }
            }
            else if (r is BNode)
            {
                if (mapping.ContainsKey(r.ToString()))
                {
                    return mapping[r.ToString()];
                }
                else
                {
                    PatternItem temp = new BlankNodePattern(r.ToString().Substring(2));
                    mapping.Add(r.ToString(), temp);
                    return temp;
                }
            }
            else
            {
                return new NodeMatchPattern(SemWebConverter.FromSemWeb(r, this.GetMapping(g)));
            }
        }

        /// <summary>
        /// Helper method which transforms a set of Statement Templates to an Algebra BGP
        /// </summary>
        /// <param name="statements">Statement Templates</param>
        /// <returns></returns>
        private ISparqlAlgebra ToAlgebra(Statement[] statements)
        {
            Graph g = new Graph();

            List<ITriplePattern> patterns = new List<ITriplePattern>();
            Dictionary<String, PatternItem> mapping = new Dictionary<string, PatternItem>();

            foreach (Statement stmt in statements)
            {
                //Build a Triple Pattern for each part of the Statement
                PatternItem s, p, o;
                s = this.FromSemWeb(stmt.Subject, g, mapping);
                p = this.FromSemWeb(stmt.Predicate, g, mapping);
                o = this.FromSemWeb(stmt.Object, g, mapping);
                patterns.Add(new TriplePattern(s, p, o));

                //If any of the parts were a Blank Node apply an IsBlank() FILTER
                if (stmt.Subject is BNode && !(stmt.Subject is Variable)) patterns.Add(new FilterPattern(new UnaryExpressionFilter(new IsBlankFunction(new VariableExpressionTerm(s.VariableName)))));
                if (stmt.Predicate is BNode && !(stmt.Predicate is Variable)) patterns.Add(new FilterPattern(new UnaryExpressionFilter(new IsBlankFunction(new VariableExpressionTerm(p.VariableName)))));
                if (stmt.Object is BNode && !(stmt.Object is Variable)) patterns.Add(new FilterPattern(new UnaryExpressionFilter(new IsBlankFunction(new VariableExpressionTerm(o.VariableName)))));
            }

            return new Bgp(patterns);
        }

        #endregion

        #region ModifiableSource Members

        /// <summary>
        /// Clears the underlying Store
        /// </summary>
        public void Clear()
        {
            this._store.Dispose();
        }

        /// <summary>
        /// Adds the contents of the given Statement Source to the Store
        /// </summary>
        /// <param name="source">Statement Source</param>
        public void Import(StatementSource source)
        {
            source.Select(this);
        }

        /// <summary>
        /// Removes Triples that match the template from the underlying Store
        /// </summary>
        /// <param name="template">Template</param>
        public void Remove(Statement template)
        {
            //Get the Graphs over which the Remove will operate
            IEnumerable<IGraph> gs;
            if (template.Meta == null)
            {
                gs = this._store.Graphs;
            }
            else
            {
                Uri graphUri = new Uri(template.Meta.Uri);
                if (this._store.HasGraph(graphUri))
                {
                    gs = this._store.Graph(graphUri).AsEnumerable();
                }
                else
                {
                    gs = Enumerable.Empty<IGraph>();
                }
            }

            //Retract the Triples which match the Template in each affected Graph
            foreach (IGraph g in gs)
            {
                IEnumerable<Triple> ts = this.TemplateToEnumerable(template, g);
                g.Retract(ts);
            }
        }

        /// <summary>
        /// Removes Triples which match any of the templates from the underlying Store
        /// </summary>
        /// <param name="templates">Templates</param>
        public void RemoveAll(Statement[] templates)
        {
            foreach (Statement template in templates)
            {
                this.Remove(template);
            }
        }

        /// <summary>
        /// Throws an error since the Replace operation is not valid in the dotNetRDF API model
        /// </summary>
        /// <param name="find">Find Template</param>
        /// <param name="replacement">Replace Template</param>
        public void Replace(Statement find, Statement replacement)
        {
            throw new RdfException("The SemWeb Replace operation is not valid for dotNetRDF Data Sources");
        }

        /// <summary>
        /// Throws an error since the Replace operation is not valid in the dotNetRDF API model
        /// </summary>
        /// <param name="find">Find Entity</param>
        /// <param name="replacement">Replace Entity</param>
        public void Replace(Entity find, Entity replacement)
        {
            throw new RdfException("The SemWeb Replace operation is not valid for dotNetRDF Data Sources");
        }

        #endregion

        #region QueryableSource Members

        /// <summary>
        /// Returns a Meta Result that says that the Query is supported
        /// </summary>
        /// <param name="graph">Graph Pattern</param>
        /// <param name="options">Query Options</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// The method does not actually do any analysis of the query, it assumes that all Graph Patterns expressible in SemWeb statement templates can be transformed to a SPARQL Algebra BGP (which generally they can)
        /// </para>
        /// </remarks>
        public SW.Query.MetaQueryResult MetaQuery(Statement[] graph, SW.Query.QueryOptions options)
        {
            SW.Query.MetaQueryResult metaResult = new SW.Query.MetaQueryResult();
            metaResult.QuerySupported = true;
            return metaResult;
        }

        /// <summary>
        /// Executes a Graph Pattern style query against the Source
        /// </summary>
        /// <param name="graph">Graph Pattern</param>
        /// <param name="options">Query Options</param>
        /// <param name="sink">Results Sink</param>
        /// <remarks>
        /// <para>
        /// This is implemented by transforming the Graph Pattern which is a set of SemWeb Statement templates into a SPARQL Algebra BGP.  The resulting algebra is then executed using the Leviathan engine and the results converted into VariableBindings for SemWeb
        /// </para>
        /// <para>
        /// The only Query Option that is supported is the Limit option
        /// </para>
        /// </remarks>
        public void Query(Statement[] graph, SW.Query.QueryOptions options, SW.Query.QueryResultSink sink)
        {
            ISparqlAlgebra algebra = this.ToAlgebra(graph);

            SparqlEvaluationContext context = new SparqlEvaluationContext(null, new InMemoryDataset(this._store));
            BaseMultiset results = context.Evaluate(algebra);//algebra.Evaluate(context);

            sink.Init(results.Variables.Select(v => new Variable(v)).ToArray());
            if (results.Count > 0)
            {
                int c = 0;
                foreach (ISet s in results.Sets)
                {
                    //Apply Limit if applicable
                    if (options.Limit > 0 && c >= options.Limit)
                    {
                        sink.Finished();
                        return;
                    }

                    //Convert the Set to VariableBindings for SemWeb
                    Variable[] vars = s.Variables.Select(v => new Variable(v)).ToArray();
                    Resource[] resources = s.Variables.Select(v => SemWebConverter.ToSemWeb(s[v], this.GetMapping(s[v].Graph))).ToArray();
                    SW.Query.VariableBindings bindings = new SW.Query.VariableBindings(vars, resources);

                    //Keep adding results until the sink tells us to stop
                    if (!sink.Add(bindings))
                    {
                        sink.Finished();
                        return;
                    }
                    c++;
                }
                sink.Finished();
            }
            else
            {
                sink.Finished();
            }
        }


        #endregion
    }
}
