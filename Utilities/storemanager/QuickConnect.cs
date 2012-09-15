/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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
        private Type _type;

        /// <summary>
        /// Creates a new Quick Connection
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        public QuickConnect(IGraph g, INode objNode)
        {
            this._g = g;
            this._objNode = objNode;
            
            //Get Type
            String typeName = ConfigurationLoader.GetConfigurationString(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyType)));
            if (typeName != null)
            {
                try
                {
                    this._type = System.Type.GetType(typeName);

                    //Force oureslves into the catch block where we try assembly qualifiying the name
                    if (this._type == null) throw new Exception();
                }
                catch
                {
                    //Was it not assembly qualified?  Try adding dotNetRDF to ensure it
                    if (!typeName.Contains(','))
                    {
                        typeName = typeName += ", dotNetRDF";
                        try
                        {
                            this._type = System.Type.GetType(typeName);
                        }
                        catch
                        {
                            this._type = null;
                        }
                    }
                }
            }
            else
            {
                this._type = null;
            }
        }

        /// <summary>
        /// Creates a new Quick Connection
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="label">Label</param>
        public QuickConnect(IGraph g, INode objNode, String label)
            : this(g, objNode)
        {
            this.Label = label;
        }

        /// <summary>
        /// Attempts to load the Connection
        /// </summary>
        /// <returns></returns>
        public IStorageProvider GetConnection()
        {
            Object temp = ConfigurationLoader.LoadObject(this._g, this._objNode);
            if (temp is IStorageProvider)
            {
                return (IStorageProvider)temp;
            }
            else
            {
                throw new DotNetRdfConfigurationException("Connection Configuration appears to be invalid as could not load an Object which implements the IStorageProvider interface from the given Configuration Graph");
            }
        }

        /// <summary>
        /// Gets the Graph that the Connection should be loaded from
        /// </summary>
        public IGraph Graph
        {
            get
            {
                return this._g;
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

        /// <summary>
        /// Gets the Type that this quick connect will generate
        /// </summary>
        public Type Type
        {
            get
            {
                return this._type;
            }
        }

        /// <summary>
        /// Gets the Label
        /// </summary>
        public String Label
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the String representation of the Quick Connect
        /// </summary>
        /// <returns></returns>
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

    /// <summary>
    /// Class for quickly removing a Connection when the Configuration Graph and Object Node are known
    /// </summary>
    public class QuickRemove
    {
        private ToolStripMenuItem _menu;
        private IGraph _g;
        private INode _objNode;
        private String _file;

        /// <summary>
        /// Creates a new Quick Remove
        /// </summary>
        /// <param name="menu">Menu Item</param>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="file">File</param>
        public QuickRemove(ToolStripMenuItem menu, IGraph g, INode objNode, String file)
        {
            this._menu = menu;
            this._g = g;
            this._objNode = objNode;
            this._file = file;
        }

        /// <summary>
        /// Remove the connection from the Configuration Graph
        /// </summary>
        public void Remove()
        {
            this._g.Retract(this._g.GetTriplesWithSubject(this._objNode).ToList());

            if (this._file != null)
            {
                CompressingTurtleWriter ttlwriter = new CompressingTurtleWriter();
                ttlwriter.Save(this._g, this._file);
            }
        }

        /// <summary>
        /// Gets the Menu Item
        /// </summary>
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
