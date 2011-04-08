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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
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
        protected Dictionary<String, List<INode>> _resourceProperties = new Dictionary<string, List<INode>>();
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

            this._resource = resource;
            this._graph = graph;

            //Find the relevant Properties and populate them
            this.IntialiseProperty(OntologyHelper.PropertyComment, true);
            this.IntialiseProperty(OntologyHelper.PropertyLabel, true);
            this.IntialiseProperty(OntologyHelper.PropertySameAs, false);
            this.IntialiseProperty(OntologyHelper.PropertyIsDefinedBy, false);
            this.IntialiseProperty(OntologyHelper.PropertySeeAlso, false);
            this.IntialiseProperty(OntologyHelper.PropertyDifferentFrom, false);
            this.IntialiseProperty(OntologyHelper.PropertyType, false);
            this.IntialiseProperty(OntologyHelper.PropertyVersionInfo, true);
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
                return this._resource;
            }
        }

        /// <summary>
        /// Gets the Graph that this Ontology Resource is from
        /// </summary>
        public IGraph Graph
        {
            get
            {
                return this._graph;
            }
        }

        /// <summary>
        /// Retrieves all the Triples which have the Resource as the subject and the given property URI as the predicate from the Graph and stores the values locally
        /// </summary>
        /// <param name="propertyUri">Property URI</param>
        /// <param name="requireLiteral">Whether only Literal values are acceptable</param>
        protected void IntialiseProperty(String propertyUri, bool requireLiteral)
        {
            IUriNode prop = this._graph.CreateUriNode(new Uri(propertyUri));
            foreach (Triple t in this._graph.GetTriplesWithSubjectPredicate(this._resource, prop))
            {
                if (requireLiteral)
                {
                    if (t.Object.NodeType == NodeType.Literal)
                    {
                        this.AddLiteralProperty(propertyUri, (ILiteralNode)t.Object, false);
                    }
                }
                else
                {
                    this.AddResourceProperty(propertyUri, t.Object, false);
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
            if (this._literalProperties.ContainsKey(propertyUri))
            {
                if (!this._literalProperties[propertyUri].Contains(value))
                {
                    this._literalProperties[propertyUri].Add(value);
                    if (persist) this._graph.Assert(new Triple(this._resource, this._graph.CreateUriNode(new Uri(propertyUri)), value));
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                this._literalProperties.Add(propertyUri, new List<ILiteralNode>() { value });
                if (persist) this._graph.Assert(new Triple(this._resource, this._graph.CreateUriNode(new Uri(propertyUri)), value));
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
            return this.AddLiteralProperty(propertyUri.ToString(), value, persist);
        }

        /// <summary>
        /// Adds a new value for a property
        /// </summary>
        /// <param name="propertyUri">Property URI</param>
        /// <param name="value">Literal Value</param>
        /// <param name="persist">Whether the new value should be added to the Graph</param>
        public bool AddResourceProperty(String propertyUri, INode value, bool persist)
        {
            if (this._resourceProperties.ContainsKey(propertyUri))
            {
                if (!this._resourceProperties[propertyUri].Contains(value))
                {
                    this._resourceProperties[propertyUri].Add(value);
                    if (persist) this._graph.Assert(new Triple(this._resource, this._graph.CreateUriNode(new Uri(propertyUri)), value));
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                this._resourceProperties.Add(propertyUri, new List<INode>() { value });
                if (persist) this._graph.Assert(new Triple(this._resource, this._graph.CreateUriNode(new Uri(propertyUri)), value));
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
            return this.AddResourceProperty(propertyUri.ToString(), value, persist);
        }

        /// <summary>
        /// Clears all values for a Literal Property
        /// </summary>
        /// <param name="propertyUri">Property URI</param>
        /// <param name="persist">Whether the removed values are removed from the Graph</param>
        public bool ClearLiteralProperty(String propertyUri, bool persist)
        {
            if (this._literalProperties.ContainsKey(propertyUri))
            {
                this._literalProperties[propertyUri].Clear();
                if (persist) this._graph.Retract(this._graph.GetTriplesWithSubjectPredicate(this._resource, this._graph.CreateUriNode(new Uri(propertyUri))));
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
            return this.ClearLiteralProperty(propertyUri.ToString(), persist);
        }

        /// <summary>
        /// Clears all values for a Resource Property
        /// </summary>
        /// <param name="propertyUri">Property URI</param>
        /// <param name="persist">Whether the removed values are removed from the Graph</param>
        public bool ClearResourceProperty(String propertyUri, bool persist)
        {
            if (this._resourceProperties.ContainsKey(propertyUri))
            {
                this._resourceProperties[propertyUri].Clear();
                if (persist) this._graph.Retract(this._graph.GetTriplesWithSubjectPredicate(this._resource, this._graph.CreateUriNode(new Uri(propertyUri))));
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
            return this.ClearResourceProperty(propertyUri.ToString(), persist);
        }

        /// <summary>
        /// Removes a literal value for a property
        /// </summary>
        /// <param name="propertyUri">Property URI</param>
        /// <param name="value">Value to remove</param>
        /// <param name="persist">Whether the removed value is removed from the Graph</param>
        public bool RemoveLiteralProperty(String propertyUri, ILiteralNode value, bool persist)
        {
            if (this._literalProperties.ContainsKey(propertyUri))
            {
                if (this._literalProperties[propertyUri].Contains(value))
                {
                    this._literalProperties[propertyUri].Remove(value);
                    if (persist) this._graph.Retract(new Triple(this._resource, this._graph.CreateUriNode(new Uri(propertyUri)), value));
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
            return this.RemoveLiteralProperty(propertyUri.ToString(), value, persist);
        }

        /// <summary>
        /// Removes a value for a property
        /// </summary>
        /// <param name="propertyUri">Property URI</param>
        /// <param name="value">Value to remove</param>
        /// <param name="persist">Whether the removed value is removed from the Graph</param>
        public bool RemoveResourceProperty(String propertyUri, INode value, bool persist)
        {
            if (this._resourceProperties.ContainsKey(propertyUri))
            {
                if (this._resourceProperties[propertyUri].Contains(value))
                {
                    this._resourceProperties[propertyUri].Remove(value);
                    if (persist) this._graph.Retract(new Triple(this._resource, this._graph.CreateUriNode(new Uri(propertyUri)), value));
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
            return this.RemoveResourceProperty(propertyUri.ToString(), value, persist);
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
            return this.AddLiteralProperty(OntologyHelper.PropertyComment, this._graph.CreateLiteralNode(comment), true);
        }

        /// <summary>
        /// Adds a comment in a specific language for this resource
        /// </summary>
        /// <param name="comment">Comment</param>
        /// <param name="lang">Language</param>
        /// <returns></returns>
        public bool AddComment(String comment, String lang)
        {
            return this.AddLiteralProperty(OntologyHelper.PropertyComment, this._graph.CreateLiteralNode(comment, lang), true);
        }

        /// <summary>
        /// Removes all comments for this resource
        /// </summary>
        /// <returns></returns>
        public bool ClearComments()
        {
            return this.ClearLiteralProperty(OntologyHelper.PropertyComment, true);
        }

        /// <summary>
        /// Removes a comment for this resource
        /// </summary>
        /// <param name="comment">Comment</param>
        /// <returns></returns>
        public bool RemoveComment(ILiteralNode comment)
        {
            return this.RemoveLiteralProperty(OntologyHelper.PropertyComment, (ILiteralNode)comment.CopyNode(this._graph), true);
        }

        /// <summary>
        /// Removes a comment for this resource
        /// </summary>
        /// <param name="comment">Comment</param>
        /// <returns></returns>
        public bool RemoveComment(String comment)
        {
            return this.RemoveComment(this._graph.CreateLiteralNode(comment));
        }

        /// <summary>
        /// Removes a comment in a specific language for this resource
        /// </summary>
        /// <param name="comment">Comment</param>
        /// <param name="lang">Language</param>
        /// <returns></returns>
        public bool RemoveComment(String comment, String lang)
        {
            return this.RemoveComment(this._graph.CreateLiteralNode(comment, lang));
        }

        /// <summary>
        /// Adds a new <em>owl:differentFrom</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddDifferentFrom(INode resource)
        {
            return this.AddResourceProperty(OntologyHelper.PropertyDifferentFrom, resource.CopyNode(this._graph), true);
        }

        /// <summary>
        /// Adds a new <em>owl:differentFrom</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddDifferentFrom(Uri resource)
        {
            return this.AddDifferentFrom(this._graph.CreateUriNode(resource));
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
            bool a = this.AddDifferentFrom(resource.Resource);
            bool b = resource.AddDifferentFrom(this._resource);
            return (a || b);
        }

        /// <summary>
        /// Clears all <em>owl:differentFrom</em> triples for the resource
        /// </summary>
        /// <returns></returns>
        public bool ClearDifferentFrom()
        {
            INode diffFrom = this._graph.CreateUriNode(new Uri(OntologyHelper.PropertyDifferentFrom));
            this._graph.Retract(this._graph.GetTriplesWithSubjectPredicate(this._resource, diffFrom));
            this._graph.Retract(this._graph.GetTriplesWithPredicateObject(diffFrom, this._resource));
            return this.ClearResourceProperty(OntologyHelper.PropertyDifferentFrom, true);
        }

        /// <summary>
        /// Removes a <em>owl:differentFrom</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveDifferentFrom(INode resource)
        {
            return this.RemoveResourceProperty(OntologyHelper.PropertyDifferentFrom, resource.CopyNode(this._graph), true);
        }

        /// <summary>
        /// Removes a <em>owl:differentFrom</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveDifferentFrom(Uri resource)
        {
            return this.RemoveDifferentFrom(this._graph.CreateUriNode(resource));
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
            bool a = this.RemoveDifferentFrom(resource.Resource);
            bool b = resource.RemoveDifferentFrom(this._resource);
            return (a || b);
        }

        /// <summary>
        /// Adds a new <em>rdfs:isDefinedBy</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddIsDefinedBy(INode resource)
        {
            return this.AddResourceProperty(OntologyHelper.PropertyIsDefinedBy, resource.CopyNode(this._graph), true);
        }

        /// <summary>
        /// Adds a new <em>rdfs:isDefinedBy</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddIsDefinedBy(Uri resource)
        {
            return this.AddIsDefinedBy(this._graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Adds a new <em>rdfs:isDefinedBy</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddIsDefinedBy(OntologyResource resource)
        {
            return this.AddIsDefinedBy(resource.Resource);
        }

        /// <summary>
        /// Removes all <em>rdfs:isDefinedBy</em> triples for the resource
        /// </summary>
        /// <returns></returns>
        public bool ClearIsDefinedBy()
        {
            return this.ClearResourceProperty(OntologyHelper.PropertyIsDefinedBy, true);
        }

        /// <summary>
        /// Removes a <em>rdfs:isDefinedBy</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveIsDefinedBy(INode resource)
        {
            return this.RemoveResourceProperty(OntologyHelper.PropertyIsDefinedBy, resource.CopyNode(this._graph), true);
        }

        /// <summary>
        /// Removes a <em>rdfs:isDefinedBy</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveIsDefinedBy(Uri resource)
        {
            return this.RemoveIsDefinedBy(this._graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Removes a <em>rdfs:isDefinedBy</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveIsDefinedBy(OntologyResource resource)
        {
            return this.RemoveIsDefinedBy(resource.Resource);
        }

        /// <summary>
        /// Adds a label for the resource
        /// </summary>
        /// <param name="label">Label</param>
        /// <returns></returns>
        public bool AddLabel(String label)
        {
            return this.AddLiteralProperty(OntologyHelper.PropertyLabel, this._graph.CreateLiteralNode(label), true);
        }

        /// <summary>
        /// Adds a label in a specific language for a resource
        /// </summary>
        /// <param name="label">Label</param>
        /// <param name="lang">Language</param>
        /// <returns></returns>
        public bool AddLabel(String label, String lang)
        {
            return this.AddLiteralProperty(OntologyHelper.PropertyLabel, this._graph.CreateLiteralNode(label, lang), true);
        }

        /// <summary>
        /// Clears all labels for a resource
        /// </summary>
        /// <returns></returns>
        public bool ClearLabels()
        {
            return this.ClearLiteralProperty(OntologyHelper.PropertyLabel, true);
        }

        /// <summary>
        /// Removes a specific label for a resource
        /// </summary>
        /// <param name="label">Label</param>
        /// <returns></returns>
        public bool RemoveLabel(ILiteralNode label)
        {
            return this.RemoveLiteralProperty(OntologyHelper.PropertyLabel, (ILiteralNode)label.CopyNode(this._graph), true);
        }

        /// <summary>
        /// Removes a label for a resource
        /// </summary>
        /// <param name="label">Label</param>
        /// <returns></returns>
        public bool RemoveLabel(String label)
        {
            return this.RemoveLabel(this._graph.CreateLiteralNode(label));
        }

        /// <summary>
        /// Removes a label in a specific language for a resource
        /// </summary>
        /// <param name="label">Label</param>
        /// <param name="lang">Language</param>
        /// <returns></returns>
        public bool RemoveLabel(String label, String lang)
        {
            return this.RemoveLabel(this._graph.CreateLiteralNode(label, lang));
        }

        /// <summary>
        /// Adds a new <em>owl:sameAs</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddSameAs(INode resource)
        {
            return this.AddResourceProperty(OntologyHelper.PropertySameAs, resource.CopyNode(this._graph), true);
        }

        /// <summary>
        /// Adds a new <em>owl:sameAs</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddSameAs(Uri resource)
        {
            return this.AddSameAs(this._graph.CreateUriNode(resource));
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
            bool a = this.AddSameAs(resource.Resource);
            bool b = resource.AddSameAs(this._resource);
            return (a || b);
        }

        /// <summary>
        /// Removes all <em>owl:sameAs</em> triples for the resource
        /// </summary>
        /// <returns></returns>
        public bool ClearSameAs()
        {
            INode sameAs = this._graph.CreateUriNode(new Uri(OntologyHelper.PropertySameAs));
            this._graph.Retract(this._graph.GetTriplesWithSubjectPredicate(this._resource, sameAs));
            this._graph.Retract(this._graph.GetTriplesWithPredicateObject(sameAs, this._resource));
            return this.ClearResourceProperty(OntologyHelper.PropertySameAs, true);
        }

        /// <summary>
        /// Removes a <em>owl:sameAs</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveSameAs(INode resource)
        {
            return this.RemoveResourceProperty(OntologyHelper.PropertySameAs, resource.CopyNode(this._graph), true);
        }

        /// <summary>
        /// Removes a <em>owl:sameAs</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveSameAs(Uri resource)
        {
            return this.RemoveSameAs(this._graph.CreateUriNode(resource));
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
            bool a = this.RemoveSameAs(resource.Resource);
            bool b = resource.RemoveSameAs(this._resource);
            return (a || b);
        }

        /// <summary>
        /// Adds a new <em>rdfs:seeAlso</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddSeeAlso(INode resource)
        {
            return this.AddResourceProperty(OntologyHelper.PropertySeeAlso, resource.CopyNode(this._graph), true);
        }

        /// <summary>
        /// Adds a new <em>rdfs:seeAlso</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddSeeAlso(Uri resource)
        {
            return this.AddSeeAlso(this._graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Adds a new <em>rdfs:seeAlso</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddSeeAlso(OntologyResource resource)
        {
            return this.AddSeeAlso(resource.Resource);
        }

        /// <summary>
        /// Removes all <em>rdfs:seeAlso</em> triples for the resource
        /// </summary>
        /// <returns></returns>
        public bool ClearSeeAlso()
        {
            return this.ClearResourceProperty(OntologyHelper.PropertySeeAlso, true);
        }

        /// <summary>
        /// Removes a <em>rdfs:seeAlso</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveSeeAlso(INode resource)
        {
            return this.RemoveResourceProperty(OntologyHelper.PropertySeeAlso, resource.CopyNode(this._graph), true);
        }

        /// <summary>
        /// Removes a <em>rdfs:seeAlso</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveSeeAlso(Uri resource)
        {
            return this.RemoveSeeAlso(this._graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Removes a <em>rdfs:seeAlso</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveSeeAlso(OntologyResource resource)
        {
            return this.RemoveSeeAlso(resource.Resource);
        }

        /// <summary>
        /// Adds a new <em>rdf:type</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddType(INode resource)
        {
            return this.AddResourceProperty(OntologyHelper.PropertyType, resource.CopyNode(this._graph), true);
        }

        /// <summary>
        /// Adds a new <em>rdf:type</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddType(Uri resource)
        {
            return this.AddType(this._graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Adds a new <em>rdf:type</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddType(OntologyResource resource)
        {
            return this.AddType(resource.Resource);
        }

        /// <summary>
        /// Removes all <em>rdf:type</em> triples for the resource
        /// </summary>
        /// <returns></returns>
        public bool ClearTypes()
        {
            return this.ClearResourceProperty(OntologyHelper.PropertyType, true);
        }

        /// <summary>
        /// Removes a <em>rdf:type</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveType(INode resource)
        {
            return this.RemoveResourceProperty(OntologyHelper.PropertyType, resource.CopyNode(this._graph), true);
        }

        /// <summary>
        /// Removes a <em>rdf:type</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveType(Uri resource)
        {
            return this.RemoveType(this._graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Removes a <em>rdf:type</em> triple for the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveType(OntologyResource resource)
        {
            return this.RemoveType(resource.Resource);
        }

        /// <summary>
        /// Adds version information for the resource
        /// </summary>
        /// <param name="info">Version Information</param>
        /// <returns></returns>
        public bool AddVersionInfo(String info)
        {
            return this.AddLiteralProperty(OntologyHelper.PropertyVersionInfo, this._graph.CreateLiteralNode(info), true);
        }

        /// <summary>
        /// Clears version information for the resource
        /// </summary>
        /// <returns></returns>
        public bool ClearVersionInfo()
        {
            return this.ClearLiteralProperty(OntologyHelper.PropertyVersionInfo, true);
        }

        /// <summary>
        /// Remove version information for the resource
        /// </summary>
        /// <param name="info">Version Information</param>
        /// <returns></returns>
        public bool RemoveVersionInfo(ILiteralNode info)
        {
            return this.RemoveLiteralProperty(OntologyHelper.PropertyVersionInfo, (ILiteralNode)info.CopyNode(this._graph), true);
        }

        /// <summary>
        /// Remove version information for the resource
        /// </summary>
        /// <param name="info">Version Information</param>
        /// <returns></returns>
        public bool RemoveVersionInfo(String info)
        {
            return this.RemoveVersionInfo(this._graph.CreateLiteralNode(info));
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
            if (this._literalProperties.ContainsKey(propertyUri))
            {
                return this._literalProperties[propertyUri];
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
            return this.GetLiteralProperty(propertyUri.ToString());
        }

        /// <summary>
        /// Gets the values for a property which can be any node type
        /// </summary>
        /// <param name="propertyUri">Property URI</param>
        /// <returns></returns>
        public IEnumerable<INode> GetResourceProperty(String propertyUri)
        {
            if (this._resourceProperties.ContainsKey(propertyUri))
            {
                return this._resourceProperties[propertyUri];
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
            return this.GetResourceProperty(propertyUri.ToString());
        }

        /// <summary>
        /// Gets the Version Information for the Resource
        /// </summary>
        public IEnumerable<ILiteralNode> VersionInfo
        {
            get
            {
                return this.GetLiteralProperty(OntologyHelper.PropertyVersionInfo);
            }
        }

        /// <summary>
        /// Gets the Comment(s) for the Resource
        /// </summary>
        public IEnumerable<ILiteralNode> Comment
        {
            get
            {
                return this.GetLiteralProperty(OntologyHelper.PropertyComment);
            }
        }

        /// <summary>
        /// Gets the Label(s) for the Resource
        /// </summary>
        public IEnumerable<ILiteralNode> Label
        {
            get
            {
                return this.GetLiteralProperty(OntologyHelper.PropertyLabel);
            }
        }

        /// <summary>
        /// Gets the See Also(s) for the Resource
        /// </summary>
        public IEnumerable<INode> SeeAlso
        {
            get
            {
                return this.GetResourceProperty(OntologyHelper.PropertySeeAlso);
            }
        }

        /// <summary>
        /// Gets the Same As('s) for the Resource
        /// </summary>
        public IEnumerable<INode> SameAs
        {
            get
            {
                return this.GetResourceProperty(OntologyHelper.PropertySameAs);
            }
        }

        /// <summary>
        /// Gets the Is Defined By(s) for the Resource
        /// </summary>
        public IEnumerable<INode> IsDefinedBy
        {
            get
            {
                return this.GetResourceProperty(OntologyHelper.PropertyIsDefinedBy);
            }
        }

        /// <summary>
        /// Gets the Different From(s) for the Resource
        /// </summary>
        public IEnumerable<INode> DifferentFrom
        {
            get
            {
                return this.GetResourceProperty(OntologyHelper.PropertyDifferentFrom);
            }
        }

        /// <summary>
        /// Gets the rdf:type's for the Resource
        /// </summary>
        public IEnumerable<INode> Types
        {
            get
            {
                return this.GetResourceProperty(OntologyHelper.PropertyType);
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
                return this._graph.GetTriplesWithSubject(this._resource);
            }
        }

        /// <summary>
        /// Gets all the Triples from the Graph where the Resource occurs as the Object
        /// </summary>
        public IEnumerable<Triple> TriplesWithObject
        {
            get
            {
                return this._graph.GetTriplesWithObject(this._resource);
            }
        }

        /// <summary>
        /// Gets all the Triples from the Graph where the Resource occurs as the Predicate
        /// </summary>
        public IEnumerable<Triple> TriplesWithPredicate
        {
            get
            {
                return this._graph.GetTriplesWithPredicate(this._resource);
            }
        }

        /// <summary>
        /// Gets all the Triples where the Resource occurs in any position
        /// </summary>
        public IEnumerable<Triple> Triples
        {
            get
            {
                return this._graph.GetTriplesWithSubject(this._resource).Concat(this._graph.GetTriplesWithPredicate(this._resource)).Concat(this._graph.GetTriplesWithObject(this._resource));
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
            if (this.Label.Any())
            {
                return this.Label.First().ToString();
            }
            else
            {
                return this._resource.ToString();
            }
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
