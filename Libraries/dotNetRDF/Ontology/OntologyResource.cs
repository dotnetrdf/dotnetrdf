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
using VDS.RDF.Query;

namespace VDS.RDF.Ontology
{
    /// <summary>
    /// Base class for representing a resource in an Ontology
    /// </summary>
    /// <remarks>
    /// <para>
    /// See <a href="http://www.dotnetrdf.org/content.asp?pageID=Ontology%20API">Using the Ontology API</a> for some informal documentation on the use of the Ontology namespace
    /// </para>
    /// </remarks>
    public class OntologyResource
    {
        /// <summary>
        /// Storage of Literal Properties
        /// </summary>
        protected Dictionary<String, List<ILiteralNode>> _literalProperties = new Dictionary<string, List<ILiteralNode>>();
        /// <summary>
        /// Storage of Resource Properties
        /// </summary>
        protected Dictionary<String, HashSet<INode>> _resourceProperties = new Dictionary<string, HashSet<INode>>();
        /// <summary>
        /// The Node which this Resource is a wrapper around
        /// </summary>
        protected INode _resource;
        /// <summary>
        /// The Graph from which this Resource originates
        /// </summary>
        protected IGraph _graph;

        /// <summary>
        /// Creates a new Ontology Resource for the given Resource in the given Graph
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <param name="graph">Graph</param>
        protected internal OntologyResource(INode resource, IGraph graph)
        {
            if (resource == null) throw new RdfOntologyException("Cannot create an Ontology Resource for a null Resource");
            if (graph == null) throw new RdfOntologyException("Cannot create an Ontology Resource in a null Graph");

            _resource = resource;
            _graph = graph;

            // Find the relevant Properties and populate them
            IntialiseProperty(OntologyHelper.PropertyComment, true);
            IntialiseProperty(OntologyHelper.PropertyLabel, true);
            IntialiseProperty(OntologyHelper.PropertySameAs, false);
            IntialiseProperty(OntologyHelper.PropertyIsDefinedBy, false);
            IntialiseProperty(OntologyHelper.PropertySeeAlso, false);
            IntialiseProperty(OntologyHelper.PropertyDifferentFrom, false);
            IntialiseProperty(OntologyHelper.PropertyType, false);
            IntialiseProperty(OntologyHelper.PropertyVersionInfo, true);
        }

        /// <summary>
        /// Creates a new Ontology Resource for the given Resource in the given Graph
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <param name="graph">Graph</param>
        protected internal OntologyResource(Uri resource, IGraph graph)
            : this(graph.CreateUriNode(resource), graph) { }

        /// <summary>
        /// Gets the Resource that this Ontology Resource refers to
        /// </summary>
        public INode Resource
        {
            get
            {
                return _resource;
            }
        }

        /// <summary>
        /// Gets the Graph that this Ontology Resource is from
        /// </summary>
        public IGraph Graph
        {
            get
            {
                return _graph;
            }
        }

        /// <summary>
        /// Retrieves all the Triples which have the Resource as the subject and the given property URI as the predicate from the Graph and stores the values locally
        /// </summary>
        /// <param name="propertyUri">Property URI</param>
        /// <param name="requireLiteral">Whether only Literal values are acceptable</param>
        protected void IntialiseProperty(String propertyUri, bool requireLiteral)
        {
            IUriNode prop = _graph.CreateUriNode(UriFactory.Create(propertyUri));
            foreach (Triple t in _graph.GetTriplesWithSubjectPredicate(_resource, prop))
            {
                if (requireLiteral)
                {
                    if (t.Object.NodeType == NodeType.Literal)
                    {
                        AddLiteralProperty(propertyUri, (ILiteralNode)t.Object, false);
                    }
                }
                else
                {
                    AddResourceProperty(propertyUri, t.Object, false);
                }
            }
        }

        #region Property Setting Helper Methods

        /// <summary>
        /// Adds a new literal value for a property
        /// </summary>
        /// <param name="propertyUri">Property URI</param>
        /// <param name="value">Literal Value</param>
        /// <param name="persist">Whether the new value should be added to the Graph</param>
        public bool AddLiteralProperty(String propertyUri, ILiteralNode value, bool persist) 
        {
            if (_literalProperties.ContainsKey(propertyUri))
            {
                if (!_literalProperties[propertyUri].Contains(value))
                {
                    _literalProperties[propertyUri].Add(value);
                    if (persist) _graph.Assert(new Triple(_resource, _graph.CreateUriNode(UriFactory.Create(propertyUri)), value));
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                _literalProperties.Add(propertyUri, new List<ILiteralNode>() { value });
                if (persist) _graph.Assert(new Triple(_resource, _graph.CreateUriNode(UriFactory.Create(propertyUri)), value));
                return true;
            }
        }

        /// <summary>
        /// Adds a new literal value for a property
        /// </summary>
        /// <param name="propertyUri">Property URI</param>
        /// <param name="value">Literal Value</param>
        /// <param name="persist">Whether the new value should be added to the Graph</param>
        public bool AddLiteralProperty(Uri propertyUri, ILiteralNode value, bool persist)
        {
            if (propertyUri == null) throw new ArgumentNullException("propertyUri");
            return AddLiteralProperty(propertyUri.AbsoluteUri, value, persist);
        }

        /// <summary>
        /// Adds a new value for a property
        /// </summary>
        /// <param name="propertyUri">Property URI</param>
        /// <param name="value">Literal Value</param>
        /// <param name="persist">Whether the new value should be added to the Graph</param>
        public bool AddResourceProperty(String propertyUri, INode value, bool persist)
        {
            if (_resourceProperties.ContainsKey(propertyUri))
            {
                if (!_resourceProperties[propertyUri].Contains(value))
                {
                    _resourceProperties[propertyUri].Add(value);
                    if (persist) _graph.Assert(new Triple(_resource, _graph.CreateUriNode(UriFactory.Create(propertyUri)), value));
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                _resourceProperties.Add(propertyUri, new HashSet<INode>() { value });
                if (persist) _graph.Assert(new Triple(_resource, _graph.CreateUriNode(UriFactory.Create(propertyUri)), value));
                return true;
            }
        }

        /// <summary>
        /// Adds a new value for a property
        /// </summary>
        /// <param name="propertyUri">Property URI</param>
        /// <param name="value">Literal Value</param>
        /// <param name="persist">Whether the new value should be added to the Graph</param>
        public bool AddResourceProperty(Uri propertyUri, INode value, bool persist)
        {
            if (propertyUri == null) throw new ArgumentNullException("propertyUri");
            return AddResourceProperty(propertyUri.AbsoluteUri, value, persist);
        }

        /// <summary>
        /// Clears all values for a Literal Property
        /// </summary>
        /// <param name="propertyUri">Property URI</param>
        /// <param name="persist">Whether the removed values are removed from the Graph</param>
        public bool ClearLiteralProperty(String propertyUri, bool persist)
        {
            if (_literalProperties.ContainsKey(propertyUri))
            {
                _literalProperties[propertyUri].Clear();
                if (persist) _graph.Retract(_graph.GetTriplesWithSubjectPredicate(_resource, _graph.CreateUriNode(UriFactory.Create(propertyUri))).ToList());
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Clears all values for a Literal Property
        /// </summary>
        /// <param name="propertyUri">Property URI</param>
        /// <param name="persist">Whether the removed values are removed from the Graph</param>
        public bool ClearLiteralProperty(Uri propertyUri, bool persist)
        {
            if (propertyUri == null) throw new ArgumentNullException("propertyUri");
            return ClearLiteralProperty(propertyUri.AbsoluteUri, persist);
        }

        /// <summary>
        /// Clears all values for a Resource Property
        /// </summary>
        /// <param name="propertyUri">Property URI</param>
        /// <param name="persist">Whether the removed values are removed from the Graph</param>
        public bool ClearResourceProperty(String propertyUri, bool persist)
        {
            if (_resourceProperties.ContainsKey(propertyUri))
            {
                _resourceProperties[propertyUri].Clear();
                if (persist) _graph.Retract(_graph.GetTriplesWithSubjectPredicate(_resource, _graph.CreateUriNode(UriFactory.Create(propertyUri))).ToList());
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Clears all values for a Resource Property
        /// </summary>
        /// <param name="propertyUri">Property URI</param>
        /// <param name="persist">Whether the removed values are removed from the Graph</param>
        public bool ClearResourceProperty(Uri propertyUri, bool persist)
        {
            if (propertyUri == null) throw new ArgumentNullException("propertyUri");
            return ClearResourceProperty(propertyUri.AbsoluteUri, persist);
        }

        /// <summary>
        /// Removes a literal value for a property
        /// </summary>
        /// <param name="propertyUri">Property URI</param>
        /// <param name="value">Value to remove</param>
        /// <param name="persist">Whether the removed value is removed from the Graph</param>
        public bool RemoveLiteralProperty(String propertyUri, ILiteralNode value, bool persist)
        {
            if (_literalProperties.ContainsKey(propertyUri))
            {
                if (_literalProperties[propertyUri].Contains(value))
                {
                    _literalProperties[propertyUri].Remove(value);
                    if (persist) _graph.Retract(new Triple(_resource, _graph.CreateUriNode(UriFactory.Create(propertyUri)), value));
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Removes a literal value for a property
        /// </summary>
        /// <param name="propertyUri">Property URI</param>
        /// <param name="value">Value to remove</param>
        /// <param name="persist">Whether the removed value is removed from the Graph</param>
        public bool RemoveLiteralProperty(Uri propertyUri, ILiteralNode value, bool persist)
        {
            if (propertyUri == null) throw new ArgumentNullException("propertyUri");
            return RemoveLiteralProperty(propertyUri.AbsoluteUri, value, persist);
        }

        /// <summary>
        /// Removes a value for a property
        /// </summary>
        /// <param name="propertyUri">Property URI</param>
        /// <param name="value">Value to remove</param>
        /// <param name="persist">Whether the removed value is removed from the Graph</param>
        public bool RemoveResourceProperty(String propertyUri, INode value, bool persist)
        {
            if (_resourceProperties.ContainsKey(propertyUri))
            {
                if (_resourceProperties[propertyUri].Contains(value))
                {
                    _resourceProperties[propertyUri].Remove(value);
                    if (persist) _graph.Retract(new Triple(_resource, _graph.CreateUriNode(UriFactory.Create(propertyUri)), value));
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Removes a value for a property
        /// </summary>
        /// <param name="propertyUri">Property URI</param>
        /// <param name="value">Value to remove</param>
        /// <param name="persist">Whether the removed value is removed from the Graph</param>
        public bool RemoveResourceProperty(Uri propertyUri, INode value, bool persist)
        {
            if (propertyUri == null) throw new ArgumentException("propertyUri");
            return RemoveResourceProperty(propertyUri.AbsoluteUri, value, persist);
        }

#endregion

        #region Specific Property Setting Methods

        /// <summary>
        /// Adds a comment for this resource
        /// </summary>
        /// <param name="comment">Comment</param>
        /// <returns></returns>
        public bool AddComment(String comment)
        {
            return AddLiteralProperty(OntologyHelper.PropertyComment, _graph.CreateLiteralNode(comment), true);
        }

        /// <summary>
        /// Adds a comment in a specific language for this resource
        /// </summary>
        /// <param name="comment">Comment</param>
        /// <param name="lang">Language</param>
        /// <returns></returns>
        public bool AddComment(String comment, String lang)
        {
            return AddLiteralProperty(OntologyHelper.PropertyComment, _graph.CreateLiteralNode(comment, lang), true);
        }

        /// <summary>
        /// Removes all comments for this resource
        /// </summary>
        /// <returns></returns>
        public bool ClearComments()
        {
            return ClearLiteralProperty(OntologyHelper.PropertyComment, true);
        }

        /// <summary>
        /// Removes a comment for this resource
        /// </summary>
        /// <param name="comment">Comment</param>
        /// <returns></returns>
        public bool RemoveComment(ILiteralNode comment)
        {
            return RemoveLiteralProperty(OntologyHelper.PropertyComment, (ILiteralNode)comment.CopyNode(_graph), true);
        }

        /// <summary>
        /// Removes a comment for this resource
        /// </summary>
        /// <param name="comment">Comment</param>
        /// <returns></returns>
        public bool RemoveComment(String comment)
        {
            return RemoveComment(_graph.CreateLiteralNode(comment));
        }

        /// <summary>
        /// Removes a comment in a specific language for this resource
        /// </summary>
        /// <param name="comment">Comment</param>
        /// <param name="lang">Language</param>
        /// <returns></returns>
        public bool RemoveComment(String comment, String lang)
        {
            return RemoveComment(_graph.CreateLiteralNode(comment, lang));
        }

        /// <summary>
        /// Adds a new <em>owl:differentFrom</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddDifferentFrom(INode resource)
        {
            return AddResourceProperty(OntologyHelper.PropertyDifferentFrom, resource.CopyNode(_graph), true);
        }

        /// <summary>
        /// Adds a new <em>owl:differentFrom</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddDifferentFrom(Uri resource)
        {
            return AddDifferentFrom(_graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Adds a new <em>owl:differentFrom</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        /// <remarks>
        /// This overload also adds this resource as different from the given resource
        /// </remarks>
        public bool AddDifferentFrom(OntologyResource resource)
        {
            bool a = AddDifferentFrom(resource.Resource);
            bool b = resource.AddDifferentFrom(_resource);
            return (a || b);
        }

        /// <summary>
        /// Clears all <em>owl:differentFrom</em> triples for the resource
        /// </summary>
        /// <returns></returns>
        public bool ClearDifferentFrom()
        {
            INode diffFrom = _graph.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyDifferentFrom));
            _graph.Retract(_graph.GetTriplesWithSubjectPredicate(_resource, diffFrom).ToList());
            _graph.Retract(_graph.GetTriplesWithPredicateObject(diffFrom, _resource).ToList());
            return ClearResourceProperty(OntologyHelper.PropertyDifferentFrom, true);
        }

        /// <summary>
        /// Removes a <em>owl:differentFrom</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveDifferentFrom(INode resource)
        {
            return RemoveResourceProperty(OntologyHelper.PropertyDifferentFrom, resource.CopyNode(_graph), true);
        }

        /// <summary>
        /// Removes a <em>owl:differentFrom</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveDifferentFrom(Uri resource)
        {
            return RemoveDifferentFrom(_graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Removes a <em>owl:differentFrom</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        /// <remarks>
        /// This overload also removes this resource as different from the given resource
        /// </remarks>
        public bool RemoveDifferentFrom(OntologyResource resource)
        {
            bool a = RemoveDifferentFrom(resource.Resource);
            bool b = resource.RemoveDifferentFrom(_resource);
            return (a || b);
        }

        /// <summary>
        /// Adds a new <em>rdfs:isDefinedBy</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddIsDefinedBy(INode resource)
        {
            return AddResourceProperty(OntologyHelper.PropertyIsDefinedBy, resource.CopyNode(_graph), true);
        }

        /// <summary>
        /// Adds a new <em>rdfs:isDefinedBy</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddIsDefinedBy(Uri resource)
        {
            return AddIsDefinedBy(_graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Adds a new <em>rdfs:isDefinedBy</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddIsDefinedBy(OntologyResource resource)
        {
            return AddIsDefinedBy(resource.Resource);
        }

        /// <summary>
        /// Removes all <em>rdfs:isDefinedBy</em> triples for the resource
        /// </summary>
        /// <returns></returns>
        public bool ClearIsDefinedBy()
        {
            return ClearResourceProperty(OntologyHelper.PropertyIsDefinedBy, true);
        }

        /// <summary>
        /// Removes a <em>rdfs:isDefinedBy</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveIsDefinedBy(INode resource)
        {
            return RemoveResourceProperty(OntologyHelper.PropertyIsDefinedBy, resource.CopyNode(_graph), true);
        }

        /// <summary>
        /// Removes a <em>rdfs:isDefinedBy</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveIsDefinedBy(Uri resource)
        {
            return RemoveIsDefinedBy(_graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Removes a <em>rdfs:isDefinedBy</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveIsDefinedBy(OntologyResource resource)
        {
            return RemoveIsDefinedBy(resource.Resource);
        }

        /// <summary>
        /// Adds a label for the resource
        /// </summary>
        /// <param name="label">Label</param>
        /// <returns></returns>
        public bool AddLabel(String label)
        {
            return AddLiteralProperty(OntologyHelper.PropertyLabel, _graph.CreateLiteralNode(label), true);
        }

        /// <summary>
        /// Adds a label in a specific language for a resource
        /// </summary>
        /// <param name="label">Label</param>
        /// <param name="lang">Language</param>
        /// <returns></returns>
        public bool AddLabel(String label, String lang)
        {
            return AddLiteralProperty(OntologyHelper.PropertyLabel, _graph.CreateLiteralNode(label, lang), true);
        }

        /// <summary>
        /// Clears all labels for a resource
        /// </summary>
        /// <returns></returns>
        public bool ClearLabels()
        {
            return ClearLiteralProperty(OntologyHelper.PropertyLabel, true);
        }

        /// <summary>
        /// Removes a specific label for a resource
        /// </summary>
        /// <param name="label">Label</param>
        /// <returns></returns>
        public bool RemoveLabel(ILiteralNode label)
        {
            return RemoveLiteralProperty(OntologyHelper.PropertyLabel, (ILiteralNode)label.CopyNode(_graph), true);
        }

        /// <summary>
        /// Removes a label for a resource
        /// </summary>
        /// <param name="label">Label</param>
        /// <returns></returns>
        public bool RemoveLabel(String label)
        {
            return RemoveLabel(_graph.CreateLiteralNode(label));
        }

        /// <summary>
        /// Removes a label in a specific language for a resource
        /// </summary>
        /// <param name="label">Label</param>
        /// <param name="lang">Language</param>
        /// <returns></returns>
        public bool RemoveLabel(String label, String lang)
        {
            return RemoveLabel(_graph.CreateLiteralNode(label, lang));
        }

        /// <summary>
        /// Adds a new <em>owl:sameAs</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddSameAs(INode resource)
        {
            return AddResourceProperty(OntologyHelper.PropertySameAs, resource.CopyNode(_graph), true);
        }

        /// <summary>
        /// Adds a new <em>owl:sameAs</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddSameAs(Uri resource)
        {
            return AddSameAs(_graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Adds a new <em>owl:sameAs</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        /// <remarks>
        /// This overload also adds this resource as an <em>owl:sameAs</em> triple for the given resource
        /// </remarks>
        public bool AddSameAs(OntologyResource resource)
        {
            bool a = AddSameAs(resource.Resource);
            bool b = resource.AddSameAs(_resource);
            return (a || b);
        }

        /// <summary>
        /// Removes all <em>owl:sameAs</em> triples for the resource
        /// </summary>
        /// <returns></returns>
        public bool ClearSameAs()
        {
            INode sameAs = _graph.CreateUriNode(UriFactory.Create(OntologyHelper.PropertySameAs));
            _graph.Retract(_graph.GetTriplesWithSubjectPredicate(_resource, sameAs).ToList());
            _graph.Retract(_graph.GetTriplesWithPredicateObject(sameAs, _resource).ToList());
            return ClearResourceProperty(OntologyHelper.PropertySameAs, true);
        }

        /// <summary>
        /// Removes a <em>owl:sameAs</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveSameAs(INode resource)
        {
            return RemoveResourceProperty(OntologyHelper.PropertySameAs, resource.CopyNode(_graph), true);
        }

        /// <summary>
        /// Removes a <em>owl:sameAs</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveSameAs(Uri resource)
        {
            return RemoveSameAs(_graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Removes a <em>owl:sameAs</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        /// <remarks>
        /// This overload also removes the <em>owl:sameAs</em> triple for the given resource
        /// </remarks>
        public bool RemoveSameAs(OntologyResource resource)
        {
            bool a = RemoveSameAs(resource.Resource);
            bool b = resource.RemoveSameAs(_resource);
            return (a || b);
        }

        /// <summary>
        /// Adds a new <em>rdfs:seeAlso</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddSeeAlso(INode resource)
        {
            return AddResourceProperty(OntologyHelper.PropertySeeAlso, resource.CopyNode(_graph), true);
        }

        /// <summary>
        /// Adds a new <em>rdfs:seeAlso</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddSeeAlso(Uri resource)
        {
            return AddSeeAlso(_graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Adds a new <em>rdfs:seeAlso</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddSeeAlso(OntologyResource resource)
        {
            return AddSeeAlso(resource.Resource);
        }

        /// <summary>
        /// Removes all <em>rdfs:seeAlso</em> triples for the resource
        /// </summary>
        /// <returns></returns>
        public bool ClearSeeAlso()
        {
            return ClearResourceProperty(OntologyHelper.PropertySeeAlso, true);
        }

        /// <summary>
        /// Removes a <em>rdfs:seeAlso</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveSeeAlso(INode resource)
        {
            return RemoveResourceProperty(OntologyHelper.PropertySeeAlso, resource.CopyNode(_graph), true);
        }

        /// <summary>
        /// Removes a <em>rdfs:seeAlso</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveSeeAlso(Uri resource)
        {
            return RemoveSeeAlso(_graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Removes a <em>rdfs:seeAlso</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveSeeAlso(OntologyResource resource)
        {
            return RemoveSeeAlso(resource.Resource);
        }

        /// <summary>
        /// Adds a new <em>rdf:type</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddType(INode resource)
        {
            return AddResourceProperty(OntologyHelper.PropertyType, resource.CopyNode(_graph), true);
        }

        /// <summary>
        /// Adds a new <em>rdf:type</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddType(Uri resource)
        {
            return AddType(_graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Adds a new <em>rdf:type</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddType(OntologyResource resource)
        {
            return AddType(resource.Resource);
        }

        /// <summary>
        /// Removes all <em>rdf:type</em> triples for the resource
        /// </summary>
        /// <returns></returns>
        public bool ClearTypes()
        {
            return ClearResourceProperty(OntologyHelper.PropertyType, true);
        }

        /// <summary>
        /// Removes a <em>rdf:type</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveType(INode resource)
        {
            return RemoveResourceProperty(OntologyHelper.PropertyType, resource.CopyNode(_graph), true);
        }

        /// <summary>
        /// Removes a <em>rdf:type</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveType(Uri resource)
        {
            return RemoveType(_graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Removes a <em>rdf:type</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveType(OntologyResource resource)
        {
            return RemoveType(resource.Resource);
        }

        /// <summary>
        /// Adds version information for the resource
        /// </summary>
        /// <param name="info">Version Information</param>
        /// <returns></returns>
        public bool AddVersionInfo(String info)
        {
            return AddLiteralProperty(OntologyHelper.PropertyVersionInfo, _graph.CreateLiteralNode(info), true);
        }

        /// <summary>
        /// Clears version information for the resource
        /// </summary>
        /// <returns></returns>
        public bool ClearVersionInfo()
        {
            return ClearLiteralProperty(OntologyHelper.PropertyVersionInfo, true);
        }

        /// <summary>
        /// Remove version information for the resource
        /// </summary>
        /// <param name="info">Version Information</param>
        /// <returns></returns>
        public bool RemoveVersionInfo(ILiteralNode info)
        {
            return RemoveLiteralProperty(OntologyHelper.PropertyVersionInfo, (ILiteralNode)info.CopyNode(_graph), true);
        }

        /// <summary>
        /// Remove version information for the resource
        /// </summary>
        /// <param name="info">Version Information</param>
        /// <returns></returns>
        public bool RemoveVersionInfo(String info)
        {
            return RemoveVersionInfo(_graph.CreateLiteralNode(info));
        }

        #endregion

        #region Property Retrieval

        /// <summary>
        /// Gets the values for a property which is restricted to literals
        /// </summary>
        /// <param name="propertyUri">Property URI</param>
        /// <returns></returns>
        public IEnumerable<ILiteralNode> GetLiteralProperty(String propertyUri)
        {
            if (_literalProperties.ContainsKey(propertyUri))
            {
                return _literalProperties[propertyUri];
            }
            else
            {
                return Enumerable.Empty<ILiteralNode>();
            }
        }

        /// <summary>
        /// Gets the values for a property which is restricted to literals
        /// </summary>
        /// <param name="propertyUri">Property URI</param>
        /// <returns></returns>
        public IEnumerable<ILiteralNode> GetLiteralProperty(Uri propertyUri)
        {
            if (propertyUri == null) throw new ArgumentNullException("propertyUri");
            return GetLiteralProperty(propertyUri.AbsoluteUri);
        }

        /// <summary>
        /// Gets the values for a property which can be any node type
        /// </summary>
        /// <param name="propertyUri">Property URI</param>
        /// <returns></returns>
        public IEnumerable<INode> GetResourceProperty(String propertyUri)
        {
            if (_resourceProperties.ContainsKey(propertyUri))
            {
                return _resourceProperties[propertyUri];
            }
            else
            {
                return Enumerable.Empty<INode>();
            }
        }

        /// <summary>
        /// Gets the values for a property which can be any node type
        /// </summary>
        /// <param name="propertyUri">Property URI</param>
        /// <returns></returns>
        public IEnumerable<INode> GetResourceProperty(Uri propertyUri)
        {
            if (propertyUri == null) throw new ArgumentNullException("propertyUri");
            return GetResourceProperty(propertyUri.AbsoluteUri);
        }

        /// <summary>
        /// Gets the Version Information for the Resource
        /// </summary>
        public IEnumerable<ILiteralNode> VersionInfo
        {
            get
            {
                return GetLiteralProperty(OntologyHelper.PropertyVersionInfo);
            }
        }

        /// <summary>
        /// Gets the Comment(s) for the Resource
        /// </summary>
        public IEnumerable<ILiteralNode> Comment
        {
            get
            {
                return GetLiteralProperty(OntologyHelper.PropertyComment);
            }
        }

        /// <summary>
        /// Gets the Label(s) for the Resource
        /// </summary>
        public IEnumerable<ILiteralNode> Label
        {
            get
            {
                return GetLiteralProperty(OntologyHelper.PropertyLabel);
            }
        }

        /// <summary>
        /// Gets the See Also(s) for the Resource
        /// </summary>
        public IEnumerable<INode> SeeAlso
        {
            get
            {
                return GetResourceProperty(OntologyHelper.PropertySeeAlso);
            }
        }

        /// <summary>
        /// Gets the Same As('s) for the Resource
        /// </summary>
        public IEnumerable<INode> SameAs
        {
            get
            {
                return GetResourceProperty(OntologyHelper.PropertySameAs);
            }
        }

        /// <summary>
        /// Gets the Is Defined By(s) for the Resource
        /// </summary>
        public IEnumerable<INode> IsDefinedBy
        {
            get
            {
                return GetResourceProperty(OntologyHelper.PropertyIsDefinedBy);
            }
        }

        /// <summary>
        /// Gets the Different From(s) for the Resource
        /// </summary>
        public IEnumerable<INode> DifferentFrom
        {
            get
            {
                return GetResourceProperty(OntologyHelper.PropertyDifferentFrom);
            }
        }

        /// <summary>
        /// Gets the rdf:type's for the Resource
        /// </summary>
        public IEnumerable<INode> Types
        {
            get
            {
                return GetResourceProperty(OntologyHelper.PropertyType);
            }
        }

        #endregion

        #region Properties returning Triples

        /// <summary>
        /// Gets all the Triples from the Graph where the Resource occurs as the Subject
        /// </summary>
        public IEnumerable<Triple> TriplesWithSubject
        {
            get
            {
                return _graph.GetTriplesWithSubject(_resource);
            }
        }

        /// <summary>
        /// Gets all the Triples from the Graph where the Resource occurs as the Object
        /// </summary>
        public IEnumerable<Triple> TriplesWithObject
        {
            get
            {
                return _graph.GetTriplesWithObject(_resource);
            }
        }

        /// <summary>
        /// Gets all the Triples from the Graph where the Resource occurs as the Predicate
        /// </summary>
        public IEnumerable<Triple> TriplesWithPredicate
        {
            get
            {
                return _graph.GetTriplesWithPredicate(_resource);
            }
        }

        /// <summary>
        /// Gets all the Triples where the Resource occurs in any position
        /// </summary>
        public IEnumerable<Triple> Triples
        {
            get
            {
                return _graph.GetTriplesWithSubject(_resource).Concat(_graph.GetTriplesWithPredicate(_resource)).Concat(_graph.GetTriplesWithObject(_resource));
            }
        }

        #endregion

        /// <summary>
        /// Gets the String representation of the Resource
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This is either the first label (if any are declared) or the string representation of the <see cref="INode">INode</see> that this resource wraps
        /// </remarks>
        public override string ToString()
        {
            if (Label.Any())
            {
                return Label.First().ToString();
            }
            else
            {
                return _resource.ToString();
            }
        }

        /// <summary>
        /// Casts a Resource into an Ontology Class
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Anything may be cast to a <see cref="OntologyClass"/> regardless of whether it actually represents a class in the ontology
        /// </remarks>
        public OntologyClass AsClass()
        {
            return new OntologyClass(_resource, _graph);
        }

        /// <summary>
        /// Casts a Resource into an Ontology Property
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Anything may be cast to a <see cref="OntologyProperty"/> regardless of whether it actually represents a property in the ontology
        /// </remarks>
        public OntologyProperty AsProperty()
        {
            return new OntologyProperty(_resource, _graph);
        }

        /// <summary>
        /// Casts a Resource into a Graph
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        /// <remarks>
        /// Equivalent to doing a SPARQL DESCRIBE query on this resource
        /// </remarks>
        public static explicit operator Graph(OntologyResource resource)
        {
            SparqlParameterizedString describe = new SparqlParameterizedString("DESCRIBE @resource");
            describe.SetParameter("resource", resource.Resource);
            Object results = resource.Graph.ExecuteQuery(describe.ToString());
            if (results is Graph)
            {
                return (Graph)results;
            }
            else if (results is IGraph)
            {
                Graph g = new Graph();
                g.Merge((IGraph)results);
                return g;
            }
            else
            {
                throw new InvalidCastException("Unable to cast this Resource to a valid Graph");
            }
        }
    }
}
