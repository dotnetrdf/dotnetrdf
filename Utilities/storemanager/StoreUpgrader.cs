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
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager
{
    public class StoreUpgrader
    {
        private BaseDBConnection _connection;
        private Thread _upgrader;
        private Graph _g = new Graph();

        /// <summary>
        /// Defines a Regular Expression for Valid Language Specifiers to aid decoding of Literal Node values from the Database
        /// </summary>
        protected static Regex _validLangSpecifier = new Regex("@[A-Za-z]{2}(\\-[A-Za-z]{2})?$");

        public StoreUpgrader(BaseDBConnection connection)
        {
            this._connection = connection;
        }

        public bool UpgradeRequired
        {
            get
            {
                return (this._connection.IsDotNetRDFStore() && this._connection.Version().Equals("0.1.0"));
            }
        }

        public int OperationsRequired()
        {
            int required = 0;
            if (this._connection.Version().Equals("0.1.1"))
            {
                return 0;
            }
            else if (this._connection.Version().Equals("0.1.0"))
            {
                //Need to run the Upgrade script
                required = 1;
                //Need to compute the Hash Code for each Graph Uri
                required += this.GetCount("SELECT COUNT(graphID) AS TotalGraphs FROM GRAPHS");
                //Need to compute the Hash Code for each Namespace Uri
                required += this.GetCount("SELECT COUNT(nsUriID) AS TotalNamespaceURIs FROM NS_URIS");
                //Need to compute the Hash Code for each Node
                required += this.GetCount("SELECT COUNT(nodeID) AS TotalNodes FROM NODES");
                //Need to compute the Hash Code for each Triple
                required += this.GetCount("SELECT COUNT(tripleID) AS TotalTriples FROM TRIPLES");
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

        #region Upgrading

        public void Upgrade()
        {
            if (this.UpgradeRequired)
            {
                //Create a Thread for the Upgrade
                this._upgrader = new Thread(new ThreadStart(this.DoUpgrade));
                this._upgrader.Start();
            }
            else
            {
                throw new RdfStorageException("No Upgrade is required for the current Database or the current Database is not a dotNetRDF Store");
            }
        }

        private void DoUpgrade()
        {
            int completed = 0;
            try
            {
                //First run the Upgrade Script
                //Need to read it in from dotNetRDF assembly
                StreamReader reader = new StreamReader(Assembly.GetAssembly(typeof(VDS.RDF.Graph)).GetManifestResourceStream("VDS.RDF.Storage.UpgradeMSSQLStore_010_011.sql"));
                String upgrade = reader.ReadToEnd();
                reader.Close();

                this._connection.ExecuteNonQuery(upgrade);
                completed++;
                this.OnProgress(completed);

                //Generate Hash Codes for the Graph URIs
                DataTable graphs = this._connection.ExecuteQuery("SELECT graphID, graphUri FROM GRAPHS");
                foreach (DataRow g in graphs.Rows)
                {
                    Uri graphUri = new Uri(g["graphUri"].ToString());
                    int graphHash = graphUri.GetEnhancedHashCode();
                    this._connection.ExecuteNonQuery("UPDATE GRAPHS SET graphHash=" + graphHash + " WHERE graphID=" + g["graphID"].ToString());
                    completed++;
                    this.OnProgress(completed);
                }

                //Generate Hash Codes for the Namespace URIs
                DataTable namespaces = this._connection.ExecuteQuery("SELECT nsUriID, nsUri FROM NS_URIS");
                foreach (DataRow n in namespaces.Rows)
                {
                    Uri nsUri = new Uri(n["nsUri"].ToString());
                    int nsUriHash = nsUri.GetEnhancedHashCode();
                    this._connection.ExecuteNonQuery("UPDATE NS_URIS SET nsUriHash=" + nsUriHash + " WHERE nsUriID=" + n["nsUriID"].ToString());
                    completed++;
                    this.OnProgress(completed);
                }

                //Generate Hash Codes for Nodes
                DataTable nodes = this._connection.ExecuteQuery("SELECT nodeID, nodeType, nodeValue FROM NODES");
                foreach (DataRow n in nodes.Rows)
                {
                    INode temp = this.DecodeNode(n);
                    this._connection.ExecuteNonQuery("UPDATE NODES SET nodeHash=" + temp.GetHashCode() + " WHERE nodeID=" + n["nodeID"].ToString());
                    completed++;
                    this.OnProgress(completed);
                }

                //Generate Hash Codes for Triples
                String getTriples = "SELECT * FROM TRIPLES T INNER JOIN NODES S ON T.tripleSubject=S.nodeID INNER JOIN NODES P ON T.triplePredicate=P.nodeID INNER JOIN NODES O ON T.tripleObject=O.nodeID";
                DataTable triples = this._connection.ExecuteQuery(getTriples);
                foreach (DataRow t in triples.Rows)
                {
                    int tripleHash = (t["nodeHash"].ToString() + t["nodeHash1"].ToString() + t["nodeHash2"].ToString()).GetHashCode();
                    this._connection.ExecuteNonQuery("UPDATE TRIPLES SET tripleHash=" + tripleHash + " WHERE tripleID=" + t["tripleID"].ToString());
                    completed++;
                    this.OnProgress(completed);
                }
            }
            catch (Exception ex)
            {
                this.OnError(ex);
            }
        }

        private INode DecodeNode(DataRow node) {
            int type;
            String value;
            INode n;
            type = Int32.Parse(node["nodeType"].ToString());
            value = node["nodeValue"].ToString().Normalize();

            //Parse the Node Value based on the Node Type
            switch ((NodeType)type)
            {
                case NodeType.Blank:
                    //Ignore the first two characters which will be _:
                    value = value.Substring(2);
                    n = this._g.CreateBlankNode(value);
                    break;

                case NodeType.Literal:
                    //Extract Data Type or Language Specifier as appropriate
                    String lit, typeorlang;
                    if (value.Contains("^^"))
                    {
                        lit = value.Substring(0, value.LastIndexOf("^^"));
                        typeorlang = value.Substring(value.LastIndexOf("^^") + 2);
                        n = this._g.CreateLiteralNode(lit, new Uri(typeorlang));
                    }
                    else if (_validLangSpecifier.IsMatch(value))
                    {
                        lit = value.Substring(0, value.LastIndexOf("@"));
                        typeorlang = value.Substring(value.LastIndexOf("@") + 1);
                        n = this._g.CreateLiteralNode(lit, typeorlang);
                    }
                    else
                    {
                        n = this._g.CreateLiteralNode(value);
                    }

                    break;
                case NodeType.Uri:
                    //Uri is the Value
                    n = this._g.CreateUriNode(new Uri(value));
                    break;
                default:
                    throw new RdfParseException("The Node Record is for a Node of Unknown Type");
            }

            return n;
        }

        #endregion

        #region Events

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

        #endregion
    }
}
