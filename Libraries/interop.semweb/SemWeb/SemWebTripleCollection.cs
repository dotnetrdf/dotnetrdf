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


using System.Collections.Generic;
using System.Linq;
using SemWeb;
using VDS.RDF.Storage;

namespace VDS.RDF.Interop.SemWeb
{
    /// <summary>
    /// Provides a SemWeb StatementSink and StatementSource as a Triple Collection
    /// </summary>
    public class SemWebTripleCollection : BaseTripleCollection
    {
        private IGraph _g;
        private StatementSink _sink;
        private StatementSource _source;
        private SemWebMapping _mapping;

        /// <summary>
        /// Creates a new Triple Collection as a wrapper around a SemWeb StatementSink and StatementSource
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="sink">Sink</param>
        /// <param name="source">Source</param>
        public SemWebTripleCollection(IGraph g, StatementSink sink, StatementSource source) 
        {
            this._g = g;
            this._sink = sink;
            this._source = source;
            this._mapping = new SemWebMapping(this._g);
        }

        /// <summary>
        /// Adds a Triple to the Triple Collection
        /// </summary>
        /// <param name="t">Triple to add</param>
        protected override void Add(Triple t)
        {
            Statement stmt = SemWebConverter.ToSemWeb(t, this._mapping);
            this._sink.Add(stmt);
        }

        /// <summary>
        /// Checks whether the Triple exists in the Collection
        /// </summary>
        /// <param name="t">Triple to check for</param>
        /// <returns></returns>
        /// <exception cref="RdfStorageException">Thrown if the underlying StatementSource is not a SelectableSource</exception>
        public override bool Contains(Triple t)
        {
            if (this._source is SelectableSource)
            {
                Statement stmt = SemWebConverter.ToSemWeb(t, this._mapping);
                return ((SelectableSource)this._source).Contains(stmt);
            }
            else
            {
                throw new RdfStorageException("The underlying StatementSource does not support the Contains() operation");
            }
        }

        /// <summary>
        /// Returns either the Count of Triples
        /// </summary>
        /// <exception cref="RdfStorageException">Thrown if the underlying StatementSource is not IEnumerable</exception>
        public override int Count
        {
            get 
            {
                if (this._source is IEnumerable<Statement>)
                {
                    return ((IEnumerable<Statement>)this._source).Count();
                }
                else
                {
                    throw new RdfStorageException("The underlying StatementSource does not support counting");
                }
            }
        }

        /// <summary>
        /// Removes a Triple from the underlying SemWeb StatementSource if the Source is modifiable
        /// </summary>
        /// <param name="t">Triple to remove</param>
        /// <exception cref="RdfStorageException">Thrown if the underlying SemWeb data source is not modifiable</exception>
        protected override void Delete(Triple t)
        {
            if (this._source is ModifiableSource)
            {
                Statement stmt = SemWebConverter.ToSemWeb(t, this._mapping);
                ((ModifiableSource)this._source).Remove(stmt);
            }
            else
            {
                throw new RdfStorageException("The underlying StatementSource does not support the Remove() operation");
            }
        }

        /// <summary>
        /// Selects a specific Triple from the Triple Collection
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        public override Triple this[Triple t]
        {
            get 
            {
                if (this.Contains(t))
                {
                    return t;
                }
                else
                {
                    throw new KeyNotFoundException("The given Triple does not exist in the Triple Collection");
                }
            }
        }

        /// <summary>
        /// Gets the Nodes which are the Objects of Triples in the underlying data source
        /// </summary>
        public override IEnumerable<INode> ObjectNodes
        {
            get 
            {
                if (this._source is IEnumerable<Statement>)
                {
                    IEnumerable<Statement> stmts = (IEnumerable<Statement>)this._source;
                    return (from stmt in stmts
                            select SemWebConverter.FromSemWeb(stmt.Object, this._mapping));
                }
                else
                {
                    throw new RdfStorageException("The underlying StatementSource is not enumerable");
                }
            }
        }

        /// <summary>
        /// Gets the Nodes which are the Predicates of Triples in the underlying data source
        /// </summary>
        public override IEnumerable<INode> PredicateNodes
        {
            get
            {
                if (this._source is IEnumerable<Statement>)
                {
                    IEnumerable<Statement> stmts = (IEnumerable<Statement>)this._source;
                    return (from stmt in stmts
                            select SemWebConverter.FromSemWeb(stmt.Predicate, this._mapping));
                }
                else
                {
                    throw new RdfStorageException("The underlying StatementSource is not enumerable");
                }
            }
        }

        /// <summary>
        /// Gets the Nodes which are the Subjects of Triples in the underlying data source
        /// </summary>
        public override IEnumerable<INode> SubjectNodes
        {
            get
            {
                if (this._source is IEnumerable<Statement>)
                {
                    IEnumerable<Statement> stmts = (IEnumerable<Statement>)this._source;
                    return (from stmt in stmts
                            select SemWebConverter.FromSemWeb(stmt.Subject, this._mapping));
                }
                else
                {
                    throw new RdfStorageException("The underlying StatementSource is not enumerable");
                }
            }
        }

        /// <summary>
        /// Disposes of the Triple Collection
        /// </summary>
        public override void Dispose()
        {
            //Don't do anything
        }

        /// <summary>
        /// Gets the Enumerator for this Triple Collection
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<Triple> GetEnumerator()
        {
            if (this._source is IEnumerable<Statement>)
            {
                IEnumerable<Statement> stmts = (IEnumerable<Statement>)this._source;
                return (from stmt in stmts
                        select SemWebConverter.FromSemWeb(stmt, this._mapping)).GetEnumerator();
            }
            else
            {
                throw new RdfStorageException("Underlying StatementSource does not support enumeration");
            }
        }

        /// <summary>
        /// Gets all the Triples with a specific Object
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithObject(INode obj)
        {
            if (this._source is SelectableSource)
            {
                EnumerationSink sink = new EnumerationSink(this._mapping);
                Statement stmt = new Statement(null, null, SemWebConverter.ToSemWeb(obj, this._mapping));
                ((SelectableSource)this._source).Select(stmt, sink);
                return sink;
            }
            else
            {
                return base.WithObject(obj);
            }
        }

        /// <summary>
        /// Gets all the Triples with a specific Predicate
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithPredicate(INode pred)
        {
            if (this._source is SelectableSource)
            {
                EnumerationSink sink = new EnumerationSink(this._mapping);
                Statement stmt = new Statement(null, SemWebConverter.ToSemWebEntity(pred, this._mapping), null);
                ((SelectableSource)this._source).Select(stmt, sink);
                return sink;
            }
            else
            {
                return base.WithPredicate(pred);
            }
        }

        /// <summary>
        /// Gets all the Triples with a specific Predicate and Object
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithPredicateObject(INode pred, INode obj)
        {
            if (this._source is SelectableSource)
            {
                EnumerationSink sink = new EnumerationSink(this._mapping);
                Statement stmt = new Statement(null, SemWebConverter.ToSemWebEntity(pred, this._mapping), SemWebConverter.ToSemWeb(obj, this._mapping));
                ((SelectableSource)this._source).Select(stmt, sink);
                return sink;
            }
            else
            {
                return base.WithPredicateObject(pred, obj);
            }
        }

        /// <summary>
        /// Gets all the Triples with a specific Subject
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithSubject(INode subj)
        {
            if (this._source is SelectableSource)
            {
                EnumerationSink sink = new EnumerationSink(this._mapping);
                Statement stmt = new Statement(SemWebConverter.ToSemWebEntity(subj, this._mapping), null, null);
                ((SelectableSource)this._source).Select(stmt, sink);
                return sink;
            }
            else
            {
                return base.WithSubject(subj);
            }
        }

        /// <summary>
        /// Gets all the Triples with a specific Subject and Object
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithSubjectObject(INode subj, INode obj)
        {
            if (this._source is SelectableSource)
            {
                EnumerationSink sink = new EnumerationSink(this._mapping);
                Statement stmt = new Statement(SemWebConverter.ToSemWebEntity(subj, this._mapping), null, SemWebConverter.ToSemWeb(obj, this._mapping));
                ((SelectableSource)this._source).Select(stmt, sink);
                return sink;
            }
            else
            {
                return base.WithSubjectObject(subj, obj);
            }
        }

        /// <summary>
        /// Gets all the Triples with a specific Subject and Predicate
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithSubjectPredicate(INode subj, INode pred)
        {
            if (this._source is SelectableSource)
            {
                EnumerationSink sink = new EnumerationSink(this._mapping);
                Statement stmt = new Statement(SemWebConverter.ToSemWebEntity(subj, this._mapping), SemWebConverter.ToSemWebEntity(pred, this._mapping), null);
                ((SelectableSource)this._source).Select(stmt, sink);
                return sink;
            }
            else
            {
                return base.WithSubjectPredicate(subj, pred);
            }
        }
    }
}
