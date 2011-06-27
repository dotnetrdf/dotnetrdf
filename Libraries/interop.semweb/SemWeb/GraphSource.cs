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
    /// Provides a dotNetRDF Graph to SemWeb as both a StatementSource and a StatementSink
    /// </summary>
    public class GraphSource : StatementSource, StatementSink, SelectableSource, QueryableSource, ModifiableSource
    {
        private IGraph _g;
        private IInMemoryQueryableStore _store;
        private SemWebMapping _mapping;

        /// <summary>
        /// Creates a new Graph Source
        /// </summary>
        /// <param name="g">Graph</param>
        public GraphSource(IGraph g)
        {
            this._g = g;
            this._mapping = new SemWebMapping(this._g);
        }

        /// <summary>
        /// Returns that Statements returned are distinct
        /// </summary>
        public bool Distinct
        {
            get 
            { 
                return true; 
            }
        }

        /// <summary>
        /// Gets the Graph this Source is a wrapper around
        /// </summary>
        public IGraph Graph
        {
            get
            {
                return this._g;
            }
        }

        /// <summary>
        /// Selects all statements from this source and streams them into the given Sink
        /// </summary>
        /// <param name="sink">Statement Sink</param>
        /// <remarks>
        /// This is essentially the same code as the <see cref="SemWebConverter.ToSemWeb">ToSemWeb(IGraph g, StatementSink sink)</see> function but we need to maintain a consistent mapping of BNodes for the source
        /// </remarks>
        public void Select(StatementSink sink)
        {
            foreach (Triple t in this._g.Triples)
            {
                Statement stmt = SemWebConverter.ToSemWeb(t, this._mapping);
                if (!sink.Add(stmt)) return;                
            }
        }

        /// <summary>
        /// Adds a statement to this Source
        /// </summary>
        /// <param name="statement">Statement</param>
        /// <returns></returns>
        public bool Add(Statement statement)
        {
            this._g.Assert(SemWebConverter.FromSemWeb(statement, this._mapping));
            //We never ask the sink to stop streaming statements so we always return true
            return true;
        }

        #region SelectableSource Members

        /// <summary>
        /// Checks whether the Source contains Triples matching the given Template
        /// </summary>
        /// <param name="template">Statement Template</param>
        /// <returns></returns>
        public bool Contains(Statement template)
        {
            //Convert the Template to an Enumerable and call Any() on it to see if it is non-empty
            return this.TemplateToEnumerable(template).Any();
        }

        /// <summary>
        /// Returns whether the Source contains any Statement which contains the given Resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool Contains(Resource resource)
        {
            //First we need to convert the Resource to a Node
            INode n = SemWebConverter.FromSemWeb(resource, this._mapping);

            if (this._g.Nodes.Contains(n))
            {
                //If it's in the Node collection check that a Triple with the given Node exists
                return this._g.Triples.Any(t => t.Involves(n));
            }
            else
            {
                //If it's not in the Node collection it can't be in the Graph
                return false;
            }
        }

        /// <summary>
        /// Selects Statements from the Source based on a Filter
        /// </summary>
        /// <param name="filter">Statement Filter</param>
        /// <param name="sink">Sink to stream results to</param>
        public void Select(SelectFilter filter, StatementSink sink)
        {
            //Don't support filters on Metas for the Graph Source
            if (filter.Metas != null)
            {
                throw new RdfException("The dotNetRDF GraphSource does not support SemWeb filters which use Meta filters");
            }

            //Want to build an IEnumerable based on the Filter
            IEnumerable<Triple> ts = Enumerable.Empty<Triple>();
            INode s, p, o;

            if (filter.Subjects != null)
            {
                if (filter.Predicates != null)
                {
                    //Subject-Predicate filter
                    foreach (Entity subj in filter.Subjects)
                    {
                        s = SemWebConverter.FromSemWeb(subj, this._mapping);
                        foreach (Entity pred in filter.Predicates)
                        {
                            p = SemWebConverter.FromSemWeb(pred, this._mapping);
                            ts = ts.Concat(this._g.GetTriplesWithSubjectPredicate(s, p));
                        }
                    }
                }
                else if (filter.Objects != null)
                {
                    //Subject-Object filter
                    foreach (Entity subj in filter.Subjects)
                    {
                        s = SemWebConverter.FromSemWeb(subj, this._mapping);
                        foreach (Resource obj in filter.Objects)
                        {
                            o = SemWebConverter.FromSemWeb(obj, this._mapping);
                            ts = ts.Concat(this._g.GetTriplesWithSubjectObject(s, o));
                        }
                    }
                }
                else
                {
                    //Subjects filter
                    foreach (Entity subj in filter.Subjects)
                    {
                        s = SemWebConverter.FromSemWeb(subj, this._mapping);
                        ts = ts.Concat(this._g.GetTriplesWithSubject(s));
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
                        p = SemWebConverter.FromSemWeb(pred, this._mapping);
                        foreach (Resource obj in filter.Objects)
                        {
                            o = SemWebConverter.FromSemWeb(obj, this._mapping);
                            ts = ts.Concat(this._g.GetTriplesWithPredicateObject(p,o));
                        }
                    }
                }
                else
                {
                    //Predicate Filter
                    foreach (Entity pred in filter.Predicates)
                    {
                        p = SemWebConverter.FromSemWeb(pred, this._mapping);
                        ts = ts.Concat(this._g.GetTriplesWithPredicate(p));
                    }
                }
            }
            else if (filter.Objects != null)
            {
                //Object Filter
                foreach (Resource obj in filter.Objects)
                {
                    o = SemWebConverter.FromSemWeb(obj, this._mapping);
                    ts = ts.Concat(this._g.GetTriplesWithObject(o));
                }
            }
            else
            {
                //Everything is null so this is a Select All
                ts = this._g.Triples;
            }

            int count = 0;
            foreach (Triple t in ts) 
            {
                //Apply limit if applicable
                if (filter.Limit > 0 && count >= filter.Limit) return;

                //Convert to a Statement and apply applicable Literal Filters
                Statement stmt = SemWebConverter.ToSemWeb(t, this._mapping);
                if (filter.LiteralFilters != null)
                {
                    if (LiteralFilter.MatchesFilters(stmt.Object, filter.LiteralFilters, this))
                    {
                        //If the Object matched the filters then we return the Triple and stop
                        //streaming if the sink tells us to
                        if (!sink.Add(stmt)) return;
                        count++;
                    }
                    //If it doesn't match the filter it is ignored
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
            IEnumerable<Triple> ts = this.TemplateToEnumerable(template);

            foreach (Triple t in ts)
            {
                //Keep streaming Triples until the sink tells us to stop
                Statement stmt = SemWebConverter.ToSemWeb(t, this._mapping);
                if (!sink.Add(stmt)) return;
            }
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
            if (this._store == null) 
            {
                this._store = new TripleStore();
                this._store.Add(this._g);
            }

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
                    Resource[] resources = s.Variables.Select(v => SemWebConverter.ToSemWeb(s[v], this._mapping)).ToArray();
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

        #region ModifiableSource Members

        /// <summary>
        /// Clears the underlying Graph of Triples
        /// </summary>
        public void Clear()
        {
            this._g.Triples.Dispose();
            this._g.Nodes.Dispose();
        }

        /// <summary>
        /// Imports the contents of a Source into the underlying Graph
        /// </summary>
        /// <param name="source"></param>
        public void Import(StatementSource source)
        {
            source.Select(this);
        }

        /// <summary>
        /// Removes Triples that match the template from the underlying Graph
        /// </summary>
        /// <param name="template">Template</param>
        public void Remove(Statement template)
        {
            IEnumerable<Triple> ts = this.TemplateToEnumerable(template);
            this._g.Retract(ts);
        }

        /// <summary>
        /// Removes Triples which match any of the templates from the underlying Graph
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

        #region Helper Methods

        /// <summary>
        /// Converts a SemWeb Statement Template to an IEnumerable of Triples
        /// </summary>
        /// <param name="template">Statement Template</param>
        /// <returns></returns>
        private IEnumerable<Triple> TemplateToEnumerable(Statement template)
        {
            INode s, p, o;
            if (template.Subject is Variable)
            {
                if (template.Predicate is Variable)
                {
                    if (template.Object is Variable)
                    {
                        //All three things are variables so this just checks that some Triple(s) are present
                        return this._g.Triples;
                    }
                    else
                    {
                        //Subject & Predicate are Variables
                        //Convert the Object and do a WithObject().Any() call
                        o = SemWebConverter.FromSemWeb(template.Object, this._mapping);

                        return this._g.GetTriplesWithObject(o);
                    }
                }
                else if (template.Object is Variable)
                {
                    //Subject & Object are variables
                    //Convert the Predicate and do a WithPredicate() call
                    p = SemWebConverter.FromSemWeb(template.Predicate, this._mapping);

                    return this._g.GetTriplesWithPredicate(p);
                }
                else
                {
                    //Subject is a Variable
                    //Convert the Predicate and Object and do a WithPredicateObject() call
                    p = SemWebConverter.FromSemWeb(template.Predicate, this._mapping);
                    o = SemWebConverter.FromSemWeb(template.Object, this._mapping);

                    return this._g.GetTriplesWithPredicateObject(p, o);
                }
            }
            else if (template.Predicate is Variable)
            {
                if (template.Object is Variable)
                {
                    //Predicate & Object are Variables
                    //Convert the Subject and do a WithSubject() call
                    s = SemWebConverter.FromSemWeb(template.Subject, this._mapping);

                    return this._g.GetTriplesWithSubject(s);
                }
                else
                {
                    //Predicate is a Variable
                    //Convert the Subject and Object and do a WithSubjectObject() call
                    s = SemWebConverter.FromSemWeb(template.Subject, this._mapping);
                    o = SemWebConverter.FromSemWeb(template.Object, this._mapping);

                    return this._g.GetTriplesWithSubjectObject(s, o);
                }
            }
            else if (template.Object is Variable)
            {
                //Object is a Variable
                //Convert the Subject and Predicate and do a WithSubjectPredicate() call
                s = SemWebConverter.FromSemWeb(template.Subject, this._mapping);
                p = SemWebConverter.FromSemWeb(template.Predicate, this._mapping);

                return this._g.GetTriplesWithSubjectPredicate(s, p);
            }
            else
            {
                //Just convert the Triple and do a Contains() call
                Triple t = SemWebConverter.FromSemWeb(template, this._mapping);
                if (this._g.ContainsTriple(t))
                {
                    return t.AsEnumerable();
                }
                else
                {
                    return Enumerable.Empty<Triple>();
                }
            }
        }

        /// <summary>
        /// Helper method which transforms a set of Statement Templates to an Algebra BGP
        /// </summary>
        /// <param name="statements">Statement Templates</param>
        /// <returns></returns>
        private ISparqlAlgebra ToAlgebra(Statement[] statements)
        {
            List<ITriplePattern> patterns = new List<ITriplePattern>();
            Dictionary<String, PatternItem> mapping = new Dictionary<string, PatternItem>();

            foreach (Statement stmt in statements)
            {
                //Build a Triple Pattern for each part of the Statement
                PatternItem s, p, o;
                s = this.FromSemWeb(stmt.Subject, mapping);
                p = this.FromSemWeb(stmt.Predicate, mapping);
                o = this.FromSemWeb(stmt.Object, mapping);
                patterns.Add(new TriplePattern(s, p, o));

                //If any of the parts were a Blank Node apply an IsBlank() FILTER
                if (stmt.Subject is BNode && !(stmt.Subject is Variable)) patterns.Add(new FilterPattern(new UnaryExpressionFilter(new IsBlankFunction(new VariableExpressionTerm(s.VariableName)))));
                if (stmt.Predicate is BNode && !(stmt.Predicate is Variable)) patterns.Add(new FilterPattern(new UnaryExpressionFilter(new IsBlankFunction(new VariableExpressionTerm(p.VariableName)))));
                if (stmt.Object is BNode && !(stmt.Object is Variable)) patterns.Add(new FilterPattern(new UnaryExpressionFilter(new IsBlankFunction(new VariableExpressionTerm(o.VariableName)))));
            }

            return new Bgp(patterns);
        }

        /// <summary>
        /// Helper method which converts a SemWeb resource into a PatternItem for use in a SPARQL Triple Pattern
        /// </summary>
        /// <param name="r">Resource</param>
        /// <param name="mapping">Mapping of Variables &amp; Blank Nodes to Pattern Items</param>
        /// <returns></returns>
        private PatternItem FromSemWeb(Resource r, Dictionary<String, PatternItem> mapping)
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
                return new NodeMatchPattern(SemWebConverter.FromSemWeb(r, this._mapping));
            }
        }

        #endregion
    }
}
