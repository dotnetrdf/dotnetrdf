namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq.Expressions;

    public partial class DynamicNode : WrapperNode, IUriNode, IBlankNode, IDynamicMetaObjectProvider
    {
        private readonly Uri baseUri;

        Uri IUriNode.Uri => this.Node is IUriNode uriNode ? uriNode.Uri : throw new InvalidOperationException("is not a uri node");

        string IBlankNode.InternalID => this.Node is IBlankNode blankNode ? blankNode.InternalID : throw new InvalidOperationException("is not a blank node");

        public Uri BaseUri => this.baseUri ?? this.Graph?.BaseUri;

        // TODO: Make sure all instantiations copy original node to appropriate host graph
        public DynamicNode(INode node, Uri baseUri = null) : base(node)
        {
            if (Graph is null)
            {
                throw new InvalidOperationException("Node must have graph");
            }

            this.baseUri = baseUri;
        }

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
        {
            return new DictionaryMetaObject(parameter, this);
        }

        // TODO: Clean this convert mess
        public void Add(object key, object value)
        {
            var node = null as INode;

            switch (key)
            {
                case string stringKey:
                    node = Convert(Convert(stringKey));
                    break;

                case Uri uriKey:
                    node = Convert(uriKey);
                    break;

                case INode nodeKey:
                    node = nodeKey;
                    break;

                default:
                    // TODO: Make more specific
                    throw new Exception();
            }
            
            this.Add(node, value);
        }

        internal bool Contains(object key, object value)
        {
            var node = null as INode;

            switch (key)
            {
                case string stringKey:
                    node = Convert(Convert(stringKey));
                    break;

                case Uri uriKey:
                    node = Convert(uriKey);
                    break;

                case INode nodeKey:
                    node = nodeKey;
                    break;

                default:
                    // TODO: Make more specific
                    throw new Exception();
            }

            return NodeDictionary.Contains(new KeyValuePair<INode, object>(node, value));
        }

        internal bool Remove(object key, object value)
        {
            var node = null as INode;

            switch (key)
            {
                case string stringKey:
                    node = Convert(Convert(stringKey));
                    break;

                case Uri uriKey:
                    node = Convert(uriKey);
                    break;

                case INode nodeKey:
                    node = nodeKey;
                    break;

                default:
                    // TODO: Make more specific
                    throw new Exception();
            }

            return NodeDictionary.Remove(new KeyValuePair<INode, object>(node, value));
        }
    }
}
