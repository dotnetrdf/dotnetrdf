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

#if !NO_STORAGE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Update;

namespace VDS.RDF.Storage
{
    //REQ: Implement the In-Memory Manager

    /// <summary>
    /// Provides a wrapper around an in-memory store
    /// </summary>
    /// <remarks>
    /// <strong>Not yet implemented</strong>
    /// <para>
    /// Useful if you want to test out some code using temporary in-memory data before you run the code against a real store or if you are using some code that requires an <see cref="IGenericIOManager">IGenericIOManager</see> interface but you need the results of that code to be available directly in-memory.
    /// </para>
    /// </remarks>
    public class InMemoryManager : IUpdateableGenericIOManager
    {
        private ISparqlDataset _dataset;
        private SparqlQueryParser _queryParser;
        private SparqlUpdateParser _updateParser;
        private LeviathanQueryProcessor _queryProcessor;
        private LeviathanUpdateProcessor _updateProcessor;

        public InMemoryManager(IInMemoryQueryableStore store)
            : this(new InMemoryDataset(store)) { }

        public InMemoryManager(ISparqlDataset dataset)
        {
            this._dataset = dataset;
        }

        #region IGenericIOManager Members

        public void LoadGraph(IGraph g, Uri graphUri)
        {
            if (this._dataset.HasGraph(graphUri))
            {
                g.Merge(this._dataset[graphUri]);
            }
        }

        public void LoadGraph(IGraph g, string graphUri)
        {
            if (graphUri == null)
            {
                this.LoadGraph(g, (Uri)null);
            }
            else if (graphUri.Equals(String.Empty))
            {
                this.LoadGraph(g, (Uri)null);
            }
            else
            {
                this.LoadGraph(g, new Uri(graphUri));
            }
        }

        public void SaveGraph(IGraph g)
        {
            if (this._dataset.HasGraph(g.BaseUri))
            {
                this._dataset.RemoveGraph(g.BaseUri);
            }
            this._dataset.AddGraph(g);
            this._dataset.Flush();
        }

        public void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            if (!this._dataset.HasGraph(graphUri))
            {
                Graph temp = new Graph();
                temp.BaseUri = graphUri;
                this._dataset.AddGraph(temp);
            }

            if ((additions != null && additions.Any()) || (removals != null && removals.Any()))
            {
                IGraph g = this._dataset.GetModifiableGraph(graphUri);
                if (additions != null && additions.Any()) g.Assert(additions);
                if (removals != null && removals.Any()) g.Retract(removals);
                g.Dispose();
            }

            this._dataset.Flush();
        }

        public void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            if (graphUri == null)
            {
                this.UpdateGraph((Uri)null, additions, removals);
            }
            else if (graphUri.Equals(String.Empty))
            {
                this.UpdateGraph((Uri)null, additions, removals);
            }
            else
            {
                this.UpdateGraph(new Uri(graphUri), additions, removals);
            }
        }

        public bool UpdateSupported
        {
            get 
            {
                return true; 
            }
        }

        public void DeleteGraph(Uri graphUri)
        {
            if (graphUri == null)
            {
                IGraph g = this._dataset.GetModifiableGraph(graphUri);
                g.Clear();
                g.Dispose();
            }
            else
            {
                this._dataset.RemoveGraph(graphUri);
            }
            this._dataset.Flush();
        }

        public void DeleteGraph(string graphUri)
        {
            if (graphUri == null)
            {
                this.DeleteGraph((Uri)null);
            }
            else if (graphUri.Equals(String.Empty))
            {
                this.DeleteGraph((Uri)null);
            }
            else
            {
                this.DeleteGraph(new Uri(graphUri));
            }
        }

        public bool DeleteSupported
        {
            get 
            {
                return true; 
            }
        }

        public bool IsReady
        {
            get 
            {
                return true; 
            }
        }

        public bool IsReadOnly
        {
            get 
            {
                return false; 
            }
        }

        public Object Query(String sparqlQuery)
        {
            if (this._queryParser == null) this._queryParser = new SparqlQueryParser();
            SparqlQuery q = this._queryParser.ParseFromString(sparqlQuery);

            if (this._queryProcessor == null) this._queryProcessor = new LeviathanQueryProcessor(this._dataset);
            return this._queryProcessor.ProcessQuery(q);
        }

        public void Update(String sparqlUpdate)
        {
            if (this._updateParser == null) this._updateParser = new SparqlUpdateParser();
            SparqlUpdateCommandSet cmds = this._updateParser.ParseFromString(sparqlUpdate);

            if (this._updateProcessor == null) this._updateProcessor = new LeviathanUpdateProcessor(this._dataset);
            this._updateProcessor.ProcessCommandSet(cmds);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            this._dataset.Flush();
        }

        #endregion
    }
}

#endif