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

#if !NO_XMLDOM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Internal Writer class which is used by <see cref="RdfXmlTreeWriter">RdfXmlTreeWriter</see> to output the produced XML DOM Tree without having Namespace attributes on every node
    /// </summary>
    class InternalXmlWriter : IPrettyPrintingWriter
    {
        private TextWriter _output;
        private int _tabs = 0;
        private int _currTabs = 0;
        private bool _tabbing = true;
        private bool _unterminatedLine = false;
        private bool _prettyprint = true;

        /// <summary>
        /// Saves an XMLDocument to a given Stream
        /// </summary>
        /// <param name="output">Stream to save to</param>
        /// <param name="doc">XML Document to save</param>
        public void Save(TextWriter output, XmlDocument doc)
        {
            this._output = output;

            this.WriteDocument(doc);
        }

        private void WriteDocument(XmlDocument doc)
        {
            //Reset whenever we are called so start state is always correct
            this._tabs = 0;
            this._currTabs = 0;
            this._tabbing = true;
            this._unterminatedLine = false;

            //Write the XML Declaration
            this.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");

            //Write the DocType (if there is one)
            if (doc.DocumentType != null)
            {
                this.WriteLine(doc.DocumentType.OuterXml);
            }

            //Write the Document Element
            this.WriteDocumentElement(doc.DocumentElement);

            //Write out the Child Nodes
            this.IncrementTab();
            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                this.WriteNode(node);
            }
            this.DecrementTab();

            //Close the Document Element
            this.WriteLine("</" + doc.DocumentElement.Name + ">");
        }

        private void WriteDocumentElement(XmlElement root)
        {
            this.WriteLineStart("<" + root.Name);
            if (root.Attributes != null)
            {
                foreach (XmlAttribute attr in root.Attributes)
                {
                    this.WriteAttribute(attr, true);
                }
            }
            this.WriteLineEnd(">");
        }

        private void WriteNode(XmlNode node) {
            //If a CDATA node output and return
            if (node.NodeType == XmlNodeType.CDATA)
            {
                this.Write("<!--[CDATA[");
                this.Write(node.InnerText);
                this.Write("]]-->");
                return;
            }

            this.WriteLineStart("<" + node.Name);

            //Write appropriate Attributes
            bool literal = false;
            if (node.Attributes != null)
            {
                foreach (XmlAttribute attr in node.Attributes)
                {
                    this.WriteAttribute(attr);
                    if (attr.Name.Equals("rdf:parseType") && attr.Value.Equals("Literal"))
                    {
                        literal = true;
                    }
                }
            }

            if (literal)
            {
                //If Literal use InnerXml instead of Inner Text
                this.WriteLineEnd(">");
                this.IncrementTab();
                this.WriteLine(node.InnerXml);
                this.DecrementTab();
                this.WriteLine("</" + node.Name + ">");
            }
            else
            {
                //Write out Child Nodes
                if (node.HasChildNodes)
                {
                    //If only 1 Node can do specific actions
                    if (node.ChildNodes.Count == 1)
                    {
                        XmlNode onlychild = node.ChildNodes[0];
                        if (onlychild.NodeType == XmlNodeType.Text)
                        {
                            this.Write(">");
                            this.Write(onlychild.InnerText);
                            this.WriteLineEnd("</" + node.Name + ">");
                        }
                        else if (onlychild.NodeType == XmlNodeType.CDATA)
                        {
                            //Write out CDATA nodes properly
                            this.Write(">");
                            this.Write("<!--[CDATA[");
                            this.Write(onlychild.InnerText);
                            this.Write("]]-->");
                            this.WriteLineEnd("</" + node.Name + ">");
                        }
                        else
                        {
                            this.WriteLineEnd(">");
                            this.IncrementTab();
                            this.WriteNode(onlychild);
                            this.DecrementTab();
                            this.WriteLine("</" + node.Name + ">");
                        }
                    }
                    else
                    {
                        //Otherwise write out all children by recursion
                        this.WriteLineEnd(">");
                        this.IncrementTab();
                        foreach (XmlNode child in node.ChildNodes)
                        {
                            this.WriteNode(child);
                        }
                        this.DecrementTab();
                        this.WriteLine("</" + node.Name + ">");
                    }
                }
                else
                {
                    this.WriteLineEnd(" />");
                }
            }
        }

        private void WriteAttribute(XmlAttribute attr)
        {
            this.WriteAttribute(attr, false);
        }

        private void WriteAttribute(XmlAttribute attr, bool allowNamespaceAttributes) {
            if (!allowNamespaceAttributes && (attr.LocalName.Equals("xmlns") || attr.Prefix.Equals("xmlns"))) 
            {
                //Skip
            } 
            else 
            {
                this.Write(" " + attr.Name + "=\"" + attr.InnerXml + "\"");
            }
        }

        #region Write to Stream Methods

        private void WriteLine(String line)
        {
            if (this._unterminatedLine)
            {
                this._output.WriteLine();
                this._unterminatedLine = false;
            }
            this.WriteTabs();
            this._output.WriteLine(line);
        }

        private void WriteLineStart(String lineStart)
        {
            if (this._unterminatedLine)
            {
                this._output.WriteLine();
            }
            this.WriteTabs();
            this._output.Write(lineStart);
            this._unterminatedLine = true;
        }

        private void WriteLineEnd(String lineEnd)
        {
            this._output.WriteLine(lineEnd);
            this._unterminatedLine = false;
        }

        private void Write(String text)
        {
            this._output.Write(text);
        }

        private void WriteTabs()
        {
            if (this._prettyprint)
            {
                this._output.Write(new String('\t', this._tabs));
            }
        }

        private void IncrementTab()
        {
            if (this._tabbing)
            {
                this._tabs++;
            }
        }

        private void DecrementTab()
        {
            if (this._tabbing)
            {
                this._tabs--;
            }
        }

        private void ToggleTabs()
        {
            if (this._tabbing)
            {
                this._currTabs = this._tabs;
                this._tabs = 0;
            }
            else
            {
                this._tabs = this._currTabs;
            }
            this._tabbing = !this._tabbing;
        }

        #endregion

        #region IPrettyPrintingWriter Members

        public bool PrettyPrintMode
        {
            get
            {
                return this._prettyprint;
            }
            set
            {
                this._prettyprint = value;
            }
        }

        #endregion
    }
}

#endif