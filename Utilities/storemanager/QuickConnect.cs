/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
