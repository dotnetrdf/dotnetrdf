/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

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
    /// An internal implementation of a Node Factory which returns mock constants regardless of the inputs
    /// </summary>
    /// <remarks>
    /// <para>
    /// Intended for usage in scenarios where the user of the factory does not care about the values returned, for example it is used internally in the <see cref="VDS.RDF.Parsing.Handlers.CountHandler">CountHandler</see> to speed up processing
    /// </para>
    /// </remarks>
    internal sealed class MockNodeFactory
        : INodeFactory
    {
        private readonly INode _bnode = new BlankNode(Guid.NewGuid());
        private readonly INode _glit = new GraphLiteralNode(new Graph());
        private readonly INode _lit = new LiteralNode("mock");
        private readonly UriNode _uri = new UriNode(UriFactory.Create("dotnetrdf:mock"));
        private readonly INode _var = new VariableNode("mock");

        public bool CreatesImplicitlyTypedLiterals
        {
            get { return this._lit.HasDataType; }
        }

        public INode CreateBlankNode()
        {
            return this._bnode;
        }

        public INode CreateBlankNode(Guid id)
        {
            return this._bnode;
        }

        public INode CreateGraphLiteralNode()
        {
            return this._glit;
        }

        public INode CreateGraphLiteralNode(IGraph subgraph)
        {
            return this._glit;
        }

        public INode CreateLiteralNode(string literal, Uri datatype)
        {
            return this._lit;
        }

        public INode CreateLiteralNode(string literal)
        {
            return this._lit;
        }

        public INode CreateLiteralNode(string literal, string langspec)
        {
            return this._lit;
        }

        public INode CreateUriNode(Uri uri)
        {
            return this._uri;
        }

        public INode CreateVariableNode(string varname)
        {
            return this._var;
        }
    }
}