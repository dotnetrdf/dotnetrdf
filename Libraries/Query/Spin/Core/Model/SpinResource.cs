/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved.
 *******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using VDS.RDF.Query.Spin.Utility;

namespace VDS.RDF.Query.Spin.Model
{
    /// <summary>
    /// A utility class that wraps dotNetRDF Nodes with the same interface as the Jena Resource classes
    /// </summary>
    public class SpinResource : IResource, IComparable<IResource>, IEquatable<IResource>
    {
        #region "Basic resource wrapper implementation "

        private INode _sourceNode;
        private SpinModel _model;

        internal static SpinResource Get(INode node, SpinModel spinModel)
        {
            if (node == null) return null;
            if (node is SpinResource && ((IResource)node).GetModel() == spinModel)
            {
                return (SpinResource)node;
            }
            return new SpinResource(node, spinModel);
        }

        protected SpinResource(INode node, SpinModel spinModel)
        {
            _sourceNode = node;
            _model = spinModel;
            if (node is SpinResource)
            {
                _sourceNode = ((SpinResource)node).AsNode();
            }
        }

        public bool IsBlank()
        {
            return _sourceNode is IBlankNode;
        }

        public bool IsUri()
        {
            return _sourceNode is IUriNode;
        }

        public bool IsLiteral()
        {
            return _sourceNode is ILiteralNode;
        }

        public String UniqueIdentifier
        {
            get
            {
                return GetModel().GetHashCode().ToString() + "#" + HttpUtility.UrlEncode(Uri.ToString());
            }
        }

        public Uri Uri
        {
            get
            {
                if (IsUri())
                {
                    return ((IUriNode)_sourceNode).Uri;
                }
                else if (IsBlank())
                {
                    return UriFactory.Create("_:" + ((IBlankNode)_sourceNode).InternalID);
                }
                return null;
            }
        }

        public INode AsNode()
        {
            return _sourceNode;
        }

        public SpinModel GetModel()
        {
            return _model;
        }

        public bool CanAs(INode cls)
        {
            return _model.ContainsTriple(_sourceNode, RDF.PropertyType, cls);
        }

        // A constructor cache to optimize half of the reflection work
        private static Dictionary<Type, ConstructorInfo> constructors = new Dictionary<Type, ConstructorInfo>();

        public IResource As(Type cls)
        {
            ConstructorInfo constructor;
            if (constructors.ContainsKey(cls))
            {
                constructor = constructors[cls];
            }
            else
            {
                constructor = cls.GetConstructor(new Type[] { typeof(INode), typeof(SpinModel) });
                constructors[cls] = constructor;
            }
            return (IResource)constructor.Invoke(new object[] { _sourceNode, _model });
        }

        public IEnumerable<IResource> GetObjects(INode property)
        {
            return _model.GetTriplesWithSubjectPredicate(this, property).Select(t => SpinResource.Get(t.Object, _model));
        }

        public List<IResource> AsList()
        {
            List<IResource> result = new List<IResource>();
            INode listRoot = this;
            Triple step = _model.GetTriplesWithSubjectPredicate(listRoot, RDF.PropertyFirst).FirstOrDefault();
            if (step != null)
            {
                while (step != null)
                {
                    if (!RDFHelper.SameTerm(RDF.Nil, step.Object))
                    {
                        result.Add(SpinResource.Get(step.Object, _model));
                    }
                    step = _model.GetTriplesWithSubjectPredicate(listRoot, RDF.PropertyRest).FirstOrDefault();
                    if (step != null)
                    {
                        if (RDFHelper.SameTerm(RDF.Nil, step.Object))
                        {
                            break;
                        }
                        listRoot = step.Object;
                        step = _model.GetTriplesWithSubjectPredicate(listRoot, RDF.PropertyFirst).FirstOrDefault();
                    }
                }
            }
            else
            {
                result.Add(this);
            }
            return result;
        }

        public void AddProperty(INode predicate, INode value)
        {
            if (predicate == null || value == null)
            {
                return;
            }
            if (predicate is IResource)
            {
                predicate = ((IResource)predicate).AsNode();
            }
            if (value is IResource)
            {
                value = ((IResource)value).AsNode();
            }
            _sourceNode.Graph.Assert(_sourceNode, predicate, value);
        }

        // TODO check whether we need to reference Model or _model
        public IEnumerable<Triple> ListProperties()
        {
            return _model.GetTriplesWithSubject(_sourceNode);
        }

        public IEnumerable<Triple> ListProperties(INode property)
        {
            return _model.GetTriplesWithSubjectPredicate(_sourceNode, property);
        }

        public bool HasProperty(INode property)
        {
            return _model.GetTriplesWithSubjectPredicate(_sourceNode, property).Any();
        }

        public bool HasProperty(INode property, INode value)
        {
            return _model.ContainsTriple(_sourceNode, property, value);
        }

        /* To simplify subsequent code and calls, we consider chaining cases where property is null */

        public Triple GetProperty(INode property)
        {
            if (property != null)
            {
                return _model.GetTriplesWithSubjectPredicate(_sourceNode, property).FirstOrDefault();
            }
            return null;
        }

        public IResource GetObject(INode property)
        {
            Triple t = GetProperty(property);
            if (t != null)
            {
                return SpinResource.Get(t.Object, _model);
            }
            else
            {
                return null;
            }
        }

        public IResource GetResource(INode property)
        {
            IResource obj = GetObject(property);
            if (obj != null && !obj.IsLiteral())
            {
                return obj;
            }
            return null;
        }

        public ILiteralNode GetLiteral(INode property)
        {
            IResource obj = GetObject(property);
            if (obj != null && obj.IsLiteral())
            {
                return (ILiteralNode)obj.AsNode();
            }
            return null;
        }

        public bool? GetBoolean(INode property)
        {
            return RDFHelper.AsBoolean(GetLiteral(property));
        }

        public int? GetInteger(INode property)
        {
            return RDFHelper.AsInteger(GetLiteral(property));
        }

        public long? GetLong(INode property)
        {
            return RDFHelper.AsLong(GetLiteral(property));
        }

        public String GetString(INode property)
        {
            return RDFHelper.AsString(GetLiteral(property));
        }

        public IResource InferRDFNode(INode property)
        {
            IResource existing = GetObject(property);
            if (existing != null)
            {
                return existing;
            }
            else
            {
                return null;
            }
        }

        #endregion "Basic resource wrapper implementation "

        #region "INode implementation "

        public NodeType NodeType
        {
            get
            {
                return _sourceNode.NodeType;
            }
        }

        public IGraph Graph
        {
            get
            {
                return _sourceNode.Graph;
            }
        }

        public Uri GraphUri
        {
            get
            {
                return _sourceNode.GraphUri;
            }
            set
            {
                _sourceNode.GraphUri = value;
            }
        }

        public string ToString(Writing.Formatting.INodeFormatter formatter)
        {
            return _sourceNode.ToString(formatter);
        }

        public string ToString(Writing.Formatting.INodeFormatter formatter, Writing.TripleSegment segment)
        {
            return _sourceNode.ToString(formatter, segment);
        }

        public int CompareTo(INode other)
        {
            if (other is IResource) return CompareTo((IResource)other);
            return _sourceNode.CompareTo(other);
        }

        public int CompareTo(IResource other)
        {
            if (other == null) return 1;
            return _sourceNode.CompareTo(other.AsNode());
        }

        public int CompareTo(IBlankNode other)
        {
            return _sourceNode.CompareTo(other);
        }

        public int CompareTo(IGraphLiteralNode other)
        {
            return _sourceNode.CompareTo(other);
        }

        public int CompareTo(ILiteralNode other)
        {
            return _sourceNode.CompareTo(other);
        }

        public int CompareTo(IUriNode other)
        {
            return _sourceNode.CompareTo(other);
        }

        public int CompareTo(IVariableNode other)
        {
            return _sourceNode.CompareTo(other);
        }

        public bool Equals(INode other)
        {
            if (other is IResource) return Equals((IResource)other);
            return _sourceNode.Equals(other);
        }

        /// <summary>
        /// Resources are equals if they refer to the sameNode in the same Model.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(IResource other)
        {
            if (other == null) return false;
            return _sourceNode.Equals(other.AsNode()) && _model.Equals(other.GetModel());
        }

        public bool Equals(IBlankNode other)
        {
            return _sourceNode.Equals(other);
        }

        public bool Equals(IGraphLiteralNode other)
        {
            return _sourceNode.Equals(other);
        }

        public bool Equals(ILiteralNode other)
        {
            return _sourceNode.Equals(other);
        }

        public bool Equals(IUriNode other)
        {
            return _sourceNode.Equals(other);
        }

        public bool Equals(IVariableNode other)
        {
            return _sourceNode.Equals(other);
        }

        public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            _sourceNode.GetObjectData(info, context);
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return _sourceNode.GetSchema();
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            _sourceNode.ReadXml(reader);
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            _sourceNode.WriteXml(writer);
        }

        #endregion "INode implementation "
    }
}