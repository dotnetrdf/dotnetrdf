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
using System.Data;
using System.Threading;

namespace dotNetRDFStore
{
    public class StoreCompacter
    {
        private BaseDBConnection _connection;
        private bool _fullCompact = false;
        private Thread _compacter;

        public StoreCompacter(BaseDBConnection connection, bool fullCompact)
        {
            this._connection = connection;
            this._fullCompact = fullCompact;
        }

        public int OperationsRequired()
        {
            int required = 0;
            //Count Unused Triples
            required += this.GetCount("SELECT COUNT(T.tripleID) AS UnusedTriples FROM TRIPLES T LEFT OUTER JOIN GRAPH_TRIPLES G ON T.tripleID=G.tripleID WHERE graphID IS NULL");
            //Count 1 for Unused Nodes - Hard to get an accurate count without first removing Unused Triples
            required += 1;
            if (this._fullCompact)
            {
                //Count Unused Namespace Prefixes
                required += this.GetCount("SELECT COUNT(P.nsPrefixID) AS UnusedPrefixes FROM NS_PREFIXES P LEFT OUTER JOIN NAMESPACES N ON P.nsPrefixID=N.nsPrefixID WHERE nsID IS NULL");
                //Count Unused Namespace URIs
                required += this.GetCount("SELECT COUNT(U.nsUriID) As UnusuedURIs FROM NS_URIS U LEFT OUTER JOIN NAMESPACES N ON U.nsUriID=N.nsUriID WHERE nsID IS NULL");
                //Count Empty Graphs
                String findEmptyGraphs = "SELECT G.graphID, COUNT(tripleID) AS GraphTriples FROM GRAPHS G INNER JOIN GRAPH_TRIPLES T ON G.graphID=T.graphID GROUP BY G.graphID";
                DataTable graphs = this._connection.ExecuteQuery(findEmptyGraphs);
                foreach (DataRow r in graphs.Rows)
                {
                    int tripleCount = Int32.Parse(r["GraphTriples"].ToString());
                    if (tripleCount == 0)
                    {
                        required++;
                    }
                }
            }
            return required;
        }

        private int GetCount(String sqlCmd)
        {
            Object count = this._connection.ExecuteScalar(sqlCmd);
            if (count != null)
            {
                return Int32.Parse(count.ToString());
            }
            else
            {
                return 0;
            }
        }

        public void Compact()
        {
            this._compacter = new Thread(new ThreadStart(this.DoCompact));
            this._compacter.Start();
        }

        private void DoCompact()
        {
            try
            {
                int completed = 0;

                //First Delete Unused Triples
                String findUnusedTriples = "SELECT T.tripleID FROM TRIPLES T LEFT OUTER JOIN GRAPH_TRIPLES G ON T.tripleID=G.tripleID WHERE graphID IS NULL";
                DataTable triples = this._connection.ExecuteQuery(findUnusedTriples);
                foreach (DataRow r in triples.Rows)
                {
                    this._connection.ExecuteNonQuery("DELETE FROM TRIPLES WHERE tripleID=" + r["tripleID"].ToString());
                    completed++;
                    this.OnProgress(completed);
                }
                triples.Dispose();

                //Then Delete Unused Nodes
                String findUnusedNodes = "SELECT nodeID FROM ((NODES N LEFT OUTER JOIN TRIPLES T ON nodeID=T.tripleSubject) LEFT OUTER JOIN TRIPLES S ON nodeID=S.triplePredicate) LEFT OUTER JOIN TRIPLES U ON nodeID=U.tripleObject WHERE T.tripleID IS NULL AND S.tripleID IS NULL AND U.tripleID IS NULL";
                DataTable nodes = this._connection.ExecuteQuery(findUnusedNodes);
                foreach (DataRow r in nodes.Rows)
                {
                    this._connection.ExecuteNonQuery("DELETE FROM NODES WHERE nodeID=" + r["nodeID"].ToString());
                }
                completed++;
                this.OnProgress(completed);
                nodes.Dispose();

                if (this._fullCompact)
                {
                    //Delete Unusued Namespaces
                    String findUnusedPrefixes = "SELECT P.nsPrefixID FROM NS_PREFIXES P LEFT OUTER JOIN NAMESPACES N ON P.nsPrefixID=N.nsPrefixID WHERE nsID IS NULL";
                    DataTable prefixes = this._connection.ExecuteQuery(findUnusedPrefixes);
                    foreach (DataRow r in prefixes.Rows)
                    {
                        this._connection.ExecuteNonQuery("DELETE FROM NS_PREFIXES WHERE nsPrefixID=" + r["prefixID"].ToString());
                        completed++;
                        this.OnProgress(completed);
                    }
                    prefixes.Dispose();

                    String findUnusedURIs = "SELECT U.nsUriID FROM NS_URIS U LEFT OUTER JOIN NAMESPACES N ON U.nsUriID=N.nsUriID WHERE nsID IS NULL";
                    DataTable uris = this._connection.ExecuteQuery(findUnusedURIs);
                    foreach (DataRow r in uris.Rows)
                    {
                        this._connection.ExecuteQuery("DELETE FROM NS_URIS WHERE nsUriID=" + r["nsUriID"].ToString());
                        completed++;
                        this.OnProgress(completed);
                    }

                    //Delete Empty Graphs
                    String findEmptyGraphs = "SELECT G.graphID, COUNT(tripleID) AS GraphTriples FROM GRAPHS G INNER JOIN GRAPH_TRIPLES T ON G.graphID=T.graphID GROUP BY G.graphID";
                    DataTable graphs = this._connection.ExecuteQuery(findEmptyGraphs);
                    foreach (DataRow r in graphs.Rows)
                    {
                        int tripleCount = Int32.Parse(r["GraphTriples"].ToString());
                        if (tripleCount == 0)
                        {
                            this._connection.ExecuteNonQuery("DELETE FROM GRAPHS WHERE graphID=" + r["graphID"].ToString());
                            completed++;
                            this.OnProgress(completed);
                        }
                    }
                    graphs.Dispose();
                }
            }
            catch (Exception ex)
            {
                this.OnError(ex);
            }
        }

        private void OnProgress(int operationsCompleted)
        {
            if (this.Progress != null)
            {
                this.Progress(operationsCompleted);
            }
        }

        private void OnError(Exception ex)
        {
            if (this.Error != null)
            {
                this.Error(ex);
            }
        }

        public event OperationProgress Progress;

        public event OperationError Error;
    }
}
