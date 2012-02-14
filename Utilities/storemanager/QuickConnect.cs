/*

Copyright Robert Vesse 2009-12
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
using System.Text;
using System.Windows.Forms;
using VDS.RDF;
using VDS.RDF.Configuration;
using VDS.RDF.Storage;
using VDS.RDF.Writing;

namespace VDS.RDF.Utilities.StoreManager
{
    /// <summary>
    /// Class for quickly loading a Connection when the Configuration Graph and Object Node are known
    /// </summary>
    public class QuickConnect
    {
        private IGraph _g;
        private INode _objNode;

        /// <summary>
        /// Creates a new Quick Connection
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        public QuickConnect(IGraph g, INode objNode)
        {
            this._g = g;
            this._objNode = objNode;
        }

        public QuickConnect(IGraph g, INode objNode, String label)
            : this(g, objNode)
        {
            this.Label = label;
        }

        /// <summary>
        /// Attempts to load the Connection
        /// </summary>
        /// <returns></returns>
        public IGenericIOManager GetConnection()
        {
            Object temp = ConfigurationLoader.LoadObject(this._g, this._objNode);
            if (temp is IGenericIOManager)
            {
                return (IGenericIOManager)temp;
            }
            else
            {
                throw new DotNetRdfConfigurationException("Connection Configuration appears to be invalid as could not load an Object which implements the IGenericIOManager interface from the given Configuration Graph");
            }
        }

        /// <summary>
        /// Gets the Node that this Connection should be loaded from
        /// </summary>
        public INode ObjectNode
        {
            get
            {
                return this._objNode;
            }
        }

        public String Label
        {
            get;
            private set;
        }

        public override string ToString()
        {
            if (this.Label != null)
            {
                return this.Label;
            }
            else
            {
                return this.ObjectNode.ToString();
            }
        }
    }

    public class QuickRemove
    {
        private ToolStripMenuItem _menu;
        private IGraph _g;
        private INode _objNode;
        private String _file;

        public QuickRemove(ToolStripMenuItem menu, IGraph g, INode objNode, String file)
        {
            this._menu = menu;
            this._g = g;
            this._objNode = objNode;
            this._file = file;
        }

        public void Remove()
        {
            this._g.Retract(this._g.GetTriplesWithSubject(this._objNode));

            if (this._file != null)
            {
                CompressingTurtleWriter ttlwriter = new CompressingTurtleWriter();
                ttlwriter.Save(this._g, this._file);
            }
        }

        public ToolStripMenuItem Menu
        {
            get
            {
                return this._menu;
            }
        }

        /// <summary>
        /// Gets the Node for this Connection
        /// </summary>
        public INode ObjectNode
        {
            get
            {
                return this._objNode;
            }
        }
    }
}
