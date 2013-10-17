using System;
using VDS.RDF.Graphs;

namespace VDS.RDF.Nodes
{
    /// <summary>
    /// A private implementation of a Node Factory which returns mock constants regardless of the inputs
    /// </summary>
    /// <remarks>
    /// <para>
    /// Intended for usage in scenarios where the user of the factory does not care about the values returned, for example it is used internally in the <see cref="VDS.RDF.Parsing.Handlers.CountHandler">CountHandler</see> to speed up processing
    /// </para>
    /// </remarks>
    class MockNodeFactory
        : INodeFactory
    {
        private readonly INode _bnode = new BlankNode(Guid.NewGuid());
        private readonly INode _glit = new GraphLiteralNode(new Graph());
        private readonly INode _lit = new LiteralNode("mock");
        private readonly UriNode _uri = new UriNode(UriFactory.Create("dotnetrdf:mock"));
        private readonly INode _var = new VariableNode("mock");

        public INode CreateBlankNode()
        {
            return this._bnode;
        }

        public INode CreateBlankNode(string nodeId)
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