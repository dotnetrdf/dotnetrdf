/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Spin.Model;
using System.Reflection;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query.Spin.Util
{
    /// <summary>
    /// A utility class that wraps dotNetRDF Nodes with the same interface as the Jena Resource classes
    /// </summary>
    internal class Resource : IResource, IComparable<IResource>, IEquatable<IResource>
    {

        #region "Basic resource wrapper implementation "

        private INode _source;
        private SpinProcessor _model;

        //TODO DO NOT USE THIS ONE !!!
        //internal static Resource Get(INode node)
        //{
        //    if (node is Resource)
        //    {
        //        return (Resource)node;
        //    }
        //    return new Resource(node, SpinWrapperDataset.currentModel);
        //}

        internal static Resource Get(INode node, SpinProcessor spinModel)
        {
            if (node == null) return null;
            if (node is Resource && ((IResource)node).getModel() == spinModel)
            {
                return (Resource)node;
            }
            return new Resource(node, spinModel);
        }

        protected Resource(INode node, SpinProcessor spinModel)
        {
            _source = node;
            _model = spinModel;
            if (node is Resource)
            {
                _source = ((Resource)node).getSource();
            }
        }

        public bool isBlank()
        {
            return _source is IBlankNode;
        }

        public bool isUri()
        {
            return _source is IUriNode;
        }

        public bool isLiteral()
        {
            return _source is ILiteralNode;
        }

        public Uri Uri
        {
            get
            {
                if (isUri())
                {
                    return ((IUriNode)_source).Uri;
                }
                return null;
            }
        }

        public INode getSource()
        {
            return _source;
        }

        public SpinProcessor getModel()
        {
            return _model;
        }

        public bool canAs(INode cls)
        {
            return _model.ContainsTriple(_source, RDF.PropertyType, cls);
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
                constructor = cls.GetConstructor(new Type[] { typeof(INode), typeof(SpinProcessor) });
                constructors[cls] = constructor;
            }
            return (IResource)constructor.Invoke(new object[] { _source, _model });
        }

        public IEnumerable<IResource> getObjects(INode property)
        {
            return _model.GetTriplesWithSubjectPredicate(this, property).Select(t => Resource.Get(t.Object, _model));
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
                    if (!RDFUtil.sameTerm(RDF.Nil, step.Object))
                    {
                        result.Add(Resource.Get(step.Object, _model));
                    }
                    step = _model.GetTriplesWithSubjectPredicate(listRoot, RDF.PropertyRest).FirstOrDefault();
                    if (step != null)
                    {
                        if (RDFUtil.sameTerm(RDF.Nil, step.Object))
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
                predicate = ((IResource)predicate).getSource();
            }
            if (value is IResource)
            {
                value = ((IResource)value).getSource();
            }
            _source.Graph.Assert(_source, predicate, value);
        }

        // TODO check whether we need to reference Model or _model
        public IEnumerable<Triple> listProperties()
        {
            return _model.GetTriplesWithSubject(_source);
        }

        public IEnumerable<Triple> listProperties(INode property)
        {
            return _model.GetTriplesWithSubjectPredicate(_source, property);
        }

        public bool hasProperty(INode property)
        {
            return _model.GetTriplesWithSubjectPredicate(_source, property).Any();
        }

        public bool hasProperty(INode property, INode value)
        {
            return _model.ContainsTriple(_source, property, value);
        }

        /* To simplify subsequent code and calls, we consider chaining cases where property is null */
        public Triple getProperty(INode property)
        {
            if (property != null)
            {
                return _model.GetTriplesWithSubjectPredicate(_source, property).FirstOrDefault();
            }
            return null;
        }

        public IResource getObject(INode property)
        {
            Triple t = getProperty(property);
            if (t != null)
            {
                return Resource.Get(t.Object, _model);
            }
            else
            {
                return null;
            }
        }

        public IResource getResource(INode property)
        {
            IResource obj = getObject(property);
            if (obj != null && !obj.isLiteral())
            {
                return obj;
            }
            return null;
        }

        public ILiteralNode getLiteral(INode property)
        {
            IResource obj = getObject(property);
            if (obj != null && obj.isLiteral())
            {
                return (ILiteralNode)obj.getSource();
            }
            return null;
        }

        public bool? getBoolean(INode property)
        {
            ILiteralNode obj = getLiteral(property);
            if (obj != null && obj is IValuedNode)
            {
                return ((IValuedNode)obj).AsBoolean();
            }
            return null;
        }

        public int? getInteger(INode property)
        {
            ILiteralNode obj = getLiteral(property);
            if (obj != null && obj is IValuedNode)
            {
                return (int)((IValuedNode)obj).AsInteger();
            }
            return null;
        }
        public long? getLong(INode property)
        {
            ILiteralNode obj = getLiteral(property);
            if (obj != null && obj is IValuedNode)
            {
                return ((IValuedNode)obj).AsInteger();
            }
            return null;
        }

        public String getString(INode property)
        {
            ILiteralNode obj = getLiteral(property);
            if (obj != null)
            {
                return obj.Value;
            }
            return null;
        }


        public IResource inferRDFNode(INode property)
        {
            IResource existing = getObject(property);
            if (existing != null)
            {
                return existing;
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region "INode implementation "

        public NodeType NodeType
        {
            get
            {
                return _source.NodeType;
            }
        }

        public IGraph Graph
        {
            get
            {
                return _source.Graph;
            }
        }

        public Uri GraphUri
        {
            get
            {
                return _source.GraphUri;
            }
            set
            {
                _source.GraphUri = value;
            }
        }

        public string ToString(Writing.Formatting.INodeFormatter formatter)
        {
            return _source.ToString(formatter);
        }

        public string ToString(Writing.Formatting.INodeFormatter formatter, Writing.TripleSegment segment)
        {
            return _source.ToString(formatter, segment);
        }

        public int CompareTo(INode other)
        {
            if (other is IResource) return CompareTo((IResource)other);
            return _source.CompareTo(other);
        }

        public int CompareTo(IResource other)
        {
            if (other == null) return 1;
            return _source.CompareTo(other.getSource());
        }

        public int CompareTo(IBlankNode other)
        {
            return _source.CompareTo(other);
        }

        public int CompareTo(IGraphLiteralNode other)
        {
            return _source.CompareTo(other);
        }

        public int CompareTo(ILiteralNode other)
        {
            return _source.CompareTo(other);
        }

        public int CompareTo(IUriNode other)
        {
            return _source.CompareTo(other);
        }

        public int CompareTo(IVariableNode other)
        {
            return _source.CompareTo(other);
        }

        public bool Equals(INode other)
        {
            if (other is IResource) return Equals((IResource)other);
            return _source.Equals(other);
        }

        public bool Equals(IResource other)
        {
            if (other == null) return false;
            return _source.Equals(other.getSource());
        }

        public bool Equals(IBlankNode other)
        {
            return _source.Equals(other);
        }

        public bool Equals(IGraphLiteralNode other)
        {
            return _source.Equals(other);
        }

        public bool Equals(ILiteralNode other)
        {
            return _source.Equals(other);
        }

        public bool Equals(IUriNode other)
        {
            return _source.Equals(other);
        }

        public bool Equals(IVariableNode other)
        {
            return _source.Equals(other);
        }

        public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            _source.GetObjectData(info, context);
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return _source.GetSchema();
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            _source.ReadXml(reader);
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            _source.WriteXml(writer);
        }

        #endregion
    }
}