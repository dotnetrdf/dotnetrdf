using System;

namespace VDS.RDF
{
    /// <summary>
    /// Interface for classes which can create Nodes
    /// </summary>
    public interface INodeFactory
    {
        /// <summary>
        /// Creates a Blank Node with a new automatically generated ID
        /// </summary>
        /// <returns></returns>
        IBlankNode CreateBlankNode();

        /// <summary>
        /// Creates a Blank Node with the given Node ID
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <returns></returns>
        IBlankNode CreateBlankNode(string nodeId);

        /// <summary>
        /// Creates a Graph Literal Node which represents the empty Subgraph
        /// </summary>
        /// <returns></returns>
        IGraphLiteralNode CreateGraphLiteralNode();

        /// <summary>
        /// Creates a Graph Literal Node which represents the given Subgraph
        /// </summary>
        /// <param name="subgraph">Subgraph</param>
        /// <returns></returns>
        IGraphLiteralNode CreateGraphLiteralNode(IGraph subgraph);

        /// <summary>
        /// Creates a Literal Node with the given Value and Data Type
        /// </summary>
        /// <param name="literal">Value of the Literal</param>
        /// <param name="datatype">Data Type URI of the Literal</param>
        /// <returns></returns>
        ILiteralNode CreateLiteralNode(string literal, Uri datatype);

        /// <summary>
        /// Creates a Literal Node with the given Value
        /// </summary>
        /// <param name="literal">Value of the Literal</param>
        /// <returns></returns>
        ILiteralNode CreateLiteralNode(string literal);

        /// <summary>
        /// Creates a Literal Node with the given Value and Language
        /// </summary>
        /// <param name="literal">Value of the Literal</param>
        /// <param name="langspec">Language Specifier for the Literal</param>
        /// <returns></returns>
        ILiteralNode CreateLiteralNode(string literal, string langspec);

        /// <summary>
        /// Creates a URI Node for the given URI
        /// </summary>
        /// <param name="uri">URI</param>
        /// <returns></returns>
        IUriNode CreateUriNode(Uri uri);

        /// <summary>
        /// Creates a Variable Node for the given Variable Name
        /// </summary>
        /// <param name="varname"></param>
        /// <returns></returns>
        IVariableNode CreateVariableNode(string varname);

        /// <summary>
        /// Creates a new unused Blank Node ID and returns it
        /// </summary>
        /// <returns></returns>
        String GetNextBlankNodeID();
    }

    /// <summary>
    /// A Default Implementation of a Node Factory which generates Nodes unrelated to Graphs (wherever possible we suggest using a Graph based implementation instead)
    /// </summary>
    public class NodeFactory : INodeFactory
    {
        private BlankNodeMapper _bnodeMap = new BlankNodeMapper();
        private INamespaceMapper _nsmap;

        public NodeFactory()
        {
            this._nsmap = new NamespaceMapper(true);
        }

        public NodeFactory(INamespaceMapper nsmap)
        {
            this._nsmap = nsmap;
        }

        #region INodeFactory Members

        public IBlankNode CreateBlankNode()
        {
            return new BlankNode(this);
        }

        public IBlankNode CreateBlankNode(string nodeId)
        {
            this._bnodeMap.CheckID(ref nodeId);
            return new BlankNode(null, nodeId);
        }

        public IGraphLiteralNode CreateGraphLiteralNode()
        {
            return new GraphLiteralNode(null);
        }

        public IGraphLiteralNode CreateGraphLiteralNode(IGraph subgraph)
        {
            return new GraphLiteralNode(null, subgraph);
        }

        public ILiteralNode CreateLiteralNode(string literal, Uri datatype)
        {
            return new LiteralNode(null, literal, datatype);
        }

        public ILiteralNode CreateLiteralNode(string literal)
        {
            return new LiteralNode(null, literal);
        }

        public ILiteralNode CreateLiteralNode(string literal, string langspec)
        {
            return new LiteralNode(null, literal, langspec);
        }

        public IUriNode CreateUriNode(Uri uri)
        {
            return new UriNode(null, uri);
        }

        public IVariableNode CreateVariableNode(string varname)
        {
            return new VariableNode(null, varname);
        }

        public string GetNextBlankNodeID()
        {
            return this._bnodeMap.GetNextID();
        }

        #endregion
    }
}
