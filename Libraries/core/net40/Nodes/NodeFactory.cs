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
using VDS.RDF.Graphs;

namespace VDS.RDF.Nodes
{
    /// <summary>
    /// A default implementation of a Node Factory
    /// </summary>
    public class NodeFactory
        : INodeFactory
    {
        /// <summary>
        /// Creates a new Node Factory
        /// </summary>
        public NodeFactory()
        { }

        /// <summary>
        /// Indicates whether this factory produces RDF 1.1 literals
        /// </summary>
        /// <remarks>
        /// <para>
        /// If true then calling <see cref="CreateLiteralNode(string)"/> will produce a literal typed as xsd:string and calling <see cref="CreateLiteralNode(string,string)"/> will produce a literal typed as rdf:langString.  If false then literals are created only with the fields provided.
        /// </para>
        /// <para>
        /// This instance respects the global <see cref="Options.Rdf11"/> setting and thus its return value is dictated by the value of that setting
        /// </para>
        /// </remarks>
        public virtual bool CreatesImplicitlyTypedLiterals
        {
            get { return Options.Rdf11; }
        }
        
        /// <summary>
        /// Creates a Blank Node with a new automatically generated ID
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// A factory should always return a fresh blank node when this method is invoked
        /// </remarks>
        public virtual INode CreateBlankNode()
        {
            return new BlankNode(Guid.NewGuid());
        }

        /// <summary>
        /// Creates a new blank node with the given ID
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>Blank node</returns>
        public virtual INode CreateBlankNode(Guid id)
        {
            return new BlankNode(id);
        }

        /// <summary>
        /// Creates a Graph Literal Node which represents the empty Subgraph
        /// </summary>
        /// <returns></returns>
        public virtual INode CreateGraphLiteralNode()
        {
            return new GraphLiteralNode(null);
        }

        /// <summary>
        /// Creates a Graph Literal Node which represents the given Subgraph
        /// </summary>
        /// <param name="subgraph">Subgraph</param>
        /// <returns></returns>
        public virtual INode CreateGraphLiteralNode(IGraph subgraph)
        {
            return new GraphLiteralNode(subgraph);
        }

        /// <summary>
        /// Creates a Literal Node with the given Value and Data Type
        /// </summary>
        /// <param name="literal">Value of the Literal</param>
        /// <param name="datatype">Data Type URI of the Literal</param>
        /// <returns></returns>
        public virtual INode CreateLiteralNode(string literal, Uri datatype)
        {
            return new LiteralNode(literal, datatype);
        }

        /// <summary>
        /// Creates a Literal Node with the given Value
        /// </summary>
        /// <param name="literal">Value of the Literal</param>
        /// <returns></returns>
        public virtual INode CreateLiteralNode(string literal)
        {
            return new LiteralNode(literal);
        }

        /// <summary>
        /// Creates a Literal Node with the given Value and Language
        /// </summary>
        /// <param name="literal">Value of the Literal</param>
        /// <param name="langspec">Language Specifier for the Literal</param>
        /// <returns></returns>
        public virtual INode CreateLiteralNode(string literal, string langspec)
        {
            return new LiteralNode(literal, langspec);
        }

        /// <summary>
        /// Creates a URI Node for the given URI
        /// </summary>
        /// <param name="uri">URI</param>
        /// <returns></returns>
        public virtual INode CreateUriNode(Uri uri)
        {
            return new UriNode(uri);
        }

        /// <summary>
        /// Creates a Variable Node for the given Variable Name
        /// </summary>
        /// <param name="varname"></param>
        /// <returns></returns>
        public virtual INode CreateVariableNode(string varname)
        {
            return new VariableNode(varname);
        }
    }
}
