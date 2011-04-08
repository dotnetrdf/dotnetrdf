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

namespace VDS.RDF.Ontology
{
    /// <summary>
    /// Class for representing a property in an Ontology
    /// </summary>
    /// <remarks>
    /// <para>
    /// See <a href="http://www.dotnetrdf.org/content.asp?pageID=Ontology%20API">Using the Ontology API</a> for some informal documentation on the use of the Ontology namespace
    /// </para>
    /// </remarks>
    public class OntologyProperty : OntologyResource
    {
        private const String PropertyDerivedProperty = "derivedProperty";

        /// <summary>
        /// Creates a new Ontology Property for the given resource in the given Graph
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <param name="graph">Graph</param>
        public OntologyProperty(INode resource, IGraph graph)
            : base(resource, graph) 
        {
            //Q: Assert that this resource is a property?
            //UriNode rdfType = graph.CreateUriNode(new Uri(OntologyHelper.PropertyType));
            //graph.Assert(new Triple(resource, rdfType, graph.CreateUriNode(new Uri(OntologyHelper.RdfsProperty))));

            this.IntialiseProperty(OntologyHelper.PropertyDomain, false);
            this.IntialiseProperty(OntologyHelper.PropertyRange, false);
            this.IntialiseProperty(OntologyHelper.PropertyEquivalentProperty, false);
            this.IntialiseProperty(OntologyHelper.PropertySubPropertyOf, false);
            this.IntialiseProperty(OntologyHelper.PropertyInverseOf, false);

            //Find derived properties
            IUriNode subPropertyOf = this._graph.CreateUriNode(new Uri(OntologyHelper.PropertySubPropertyOf));
            this._resourceProperties.Add(PropertyDerivedProperty, new List<INode>());
            foreach (Triple t in this._graph.GetTriplesWithPredicateObject(subPropertyOf, this._resource))
            {
                if (!this._resourceProperties[PropertyDerivedProperty].Contains(t.Subject)) this._resourceProperties[PropertyDerivedProperty].Add(t.Subject);
            }
            int c = 0;
            do
            {
                c = this._resourceProperties[PropertyDerivedProperty].Count;
                foreach (INode n in this._resourceProperties[PropertyDerivedProperty].ToList())
                {
                    foreach (Triple t in this._graph.GetTriplesWithPredicateObject(subPropertyOf, n))
                    {
                        if (!this._resourceProperties[PropertyDerivedProperty].Contains(t.Subject)) this._resourceProperties[PropertyDerivedProperty].Add(t.Subject);
                    }
                }
            } while (c < this._resourceProperties[PropertyDerivedProperty].Count);

            //Find additional super properties
            if (this._resourceProperties.ContainsKey(OntologyHelper.PropertySubPropertyOf))
            {
                do
                {
                    c = this._resourceProperties[OntologyHelper.PropertySubPropertyOf].Count;
                    foreach (INode n in this._resourceProperties[OntologyHelper.PropertySubPropertyOf].ToList())
                    {
                        foreach (Triple t in this._graph.GetTriplesWithSubjectPredicate(n, subPropertyOf))
                        {
                            if (!this._resourceProperties[OntologyHelper.PropertySubPropertyOf].Contains(t.Object)) this._resourceProperties[OntologyHelper.PropertySubPropertyOf].Add(t.Object);
                        }
                    }
                } while (c < this._resourceProperties[OntologyHelper.PropertySubPropertyOf].Count);
            }

            //Find additional inverses
            if (!this._resourceProperties.ContainsKey(OntologyHelper.PropertyInverseOf)) this._resourceProperties.Add(OntologyHelper.PropertyInverseOf, new List<INode>());
            foreach (Triple t in this._graph.GetTriplesWithPredicateObject(graph.CreateUriNode(new Uri(OntologyHelper.PropertyInverseOf)), this._resource))
            {
                if (!this._resourceProperties[OntologyHelper.PropertyInverseOf].Contains(t.Subject)) this._resourceProperties[OntologyHelper.PropertyInverseOf].Add(t.Subject);
            }
        }

        /// <summary>
        /// Creates a new RDFS Ontology Property for the given resource in the given Graph
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <param name="graph">Graph</param>
        public OntologyProperty(Uri resource, IGraph graph)
            : this(graph.CreateUriNode(resource), graph) { }

        /// <summary>
        /// Adds a new domain for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddDomain(INode resource)
        {
            return this.AddResourceProperty(OntologyHelper.PropertyDomain, resource.CopyNode(this._graph), true);
        }

        /// <summary>
        /// Adds a new domain for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddDomain(Uri resource)
        {
            return this.AddDomain(this._graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Adds a new domain for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddDomain(OntologyResource resource)
        {
            return this.AddDomain(resource.Resource);
        }

        /// <summary>
        /// Clears all domains for the property
        /// </summary>
        /// <returns></returns>
        public bool ClearDomains()
        {
            return this.ClearResourceProperty(OntologyHelper.PropertyDomain, true);
        }

        /// <summary>
        /// Removes a domain for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveDomain(INode resource)
        {
            return this.RemoveResourceProperty(OntologyHelper.PropertyDomain, resource.CopyNode(this._graph), true);
        }

        /// <summary>
        /// Removes a domain for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveDomain(Uri resource)
        {
            return this.RemoveDomain(this._graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Removes a domain for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveDomain(OntologyResource resource)
        {
            return this.RemoveDomain(resource.Resource);
        }

        /// <summary>
        /// Adds a new range for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddRange(INode resource)
        {
            return this.AddResourceProperty(OntologyHelper.PropertyRange, resource.CopyNode(this._graph), true);
        }

        /// <summary>
        /// Adds a new range for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddRange(Uri resource)
        {
            return this.AddRange(this._graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Adds a new range for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddRange(OntologyResource resource)
        {
            return this.AddRange(resource.Resource);
        }

        /// <summary>
        /// Clears all ranges for the property
        /// </summary>
        /// <returns></returns>
        public bool ClearRanges()
        {
            return this.ClearResourceProperty(OntologyHelper.PropertyRange, true);
        }

        /// <summary>
        /// Removes a range for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveRange(INode resource)
        {
            return this.RemoveResourceProperty(OntologyHelper.PropertyRange, resource, true);
        }

        /// <summary>
        /// Removes a range for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveRange(Uri resource)
        {
            return this.RemoveRange(this._graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Removes a range for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveRange(OntologyResource resource)
        {
            return this.RemoveRange(resource.Resource);
        }

        /// <summary>
        /// Adds a new equivalent property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddEquivalentProperty(INode resource)
        {
            return this.AddResourceProperty(OntologyHelper.PropertyEquivalentProperty, resource.CopyNode(this._graph), true);
        }

        /// <summary>
        /// Adds a new equivalent property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddEquivalentProperty(Uri resource)
        {
            return this.AddEquivalentProperty(this._graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Adds a new equivalent property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddEquivalentProperty(OntologyResource resource)
        {
            return this.AddEquivalentProperty(resource.Resource);
        }

        /// <summary>
        /// Adds a new equivalent property for the property
        /// </summary>
        /// <param name="property">Property</param>
        /// <returns></returns>
        /// <remarks>
        /// This overload also adds this property as an equivalent property of the given property
        /// </remarks>
        public bool AddEquivalentProperty(OntologyProperty property)
        {
            bool a = this.AddEquivalentProperty(property.Resource);
            bool b = property.AddEquivalentProperty(this._resource);
            return (a || b);
        }

        /// <summary>
        /// Clears all equivalent properties for this property
        /// </summary>
        /// <returns></returns>
        public bool ClearEquivalentProperties()
        {
            INode equivProp = this._graph.CreateUriNode(new Uri(OntologyHelper.PropertyEquivalentProperty));
            this._graph.Retract(this._graph.GetTriplesWithSubjectPredicate(this._resource, equivProp));
            this._graph.Retract(this._graph.GetTriplesWithPredicateObject(equivProp, this._resource));
            return this.ClearResourceProperty(OntologyHelper.PropertyEquivalentProperty, true);
        }

        /// <summary>
        /// Removes an equivalent property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveEquivalentProperty(INode resource)
        {
            return this.RemoveResourceProperty(OntologyHelper.PropertyEquivalentProperty, resource.CopyNode(this._graph), true);
        }

        /// <summary>
        /// Removes an equivalent property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveEquivalentProperty(Uri resource)
        {
            return this.RemoveEquivalentProperty(this._graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Removes an equivalent property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveEquivalentProperty(OntologyResource resource)
        {
            return this.RemoveEquivalentProperty(resource.Resource);
        }

        /// <summary>
        /// Removes an equivalent property for the property
        /// </summary>
        /// <param name="property">Property</param>
        /// <returns></returns>
        /// <remarks>
        /// This overload also removes this property as an equivalent property of the given property
        /// </remarks>
        public bool RemoveEquivalentProperty(OntologyProperty property)
        {
            bool a = this.RemoveEquivalentProperty(property.Resource);
            bool b = property.RemoveEquivalentProperty(this._resource);
            return (a || b);
        }

        /// <summary>
        /// Adds an inverse property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddInverseProperty(INode resource)
        {
            return this.AddResourceProperty(OntologyHelper.PropertyInverseOf, resource.CopyNode(this._graph), true);
        }

        /// <summary>
        /// Adds an inverse property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddInverseProperty(Uri resource)
        {
            return this.AddInverseProperty(this._graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Adds an inverse property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddInverseProperty(OntologyResource resource)
        {
            return this.AddInverseProperty(resource.Resource);
        }

        /// <summary>
        /// Adds an inverse property for the property
        /// </summary>
        /// <param name="property">Property</param>
        /// <returns></returns>
        /// <remarks>
        /// This overload also adds this property as an inverse property of the given property
        /// </remarks>
        public bool AddInverseProperty(OntologyProperty property)
        {
            bool a = this.AddInverseProperty(property.Resource);
            bool b = property.AddInverseProperty(this._resource);
            return (a || b);
        }

        /// <summary>
        /// Removes all inverse properties for this property
        /// </summary>
        /// <returns></returns>
        public bool ClearInverseProperties()
        {
            INode inverseOf = this._graph.CreateUriNode(new Uri(OntologyHelper.PropertyInverseOf));
            this._graph.Retract(this._graph.GetTriplesWithSubjectPredicate(this._resource, inverseOf));
            this._graph.Retract(this._graph.GetTriplesWithPredicateObject(inverseOf, this._resource));
            return this.ClearResourceProperty(OntologyHelper.PropertyInverseOf, true);
        }

        /// <summary>
        /// Removes an inverse property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveInverseProperty(INode resource)
        {
            return this.RemoveResourceProperty(OntologyHelper.PropertyInverseOf, resource.CopyNode(this._graph), true);
        }

        /// <summary>
        /// Removes an inverse property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveInverseProperty(Uri resource)
        {
            return this.RemoveInverseProperty(this._graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Removes an inverse property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveInverseProperty(OntologyResource resource)
        {
            return this.RemoveInverseProperty(resource.Resource);
        }

        /// <summary>
        /// Removes an inverse property for the property
        /// </summary>
        /// <param name="property">Property</param>
        /// <returns></returns>
        /// <remarks>
        /// This overload also removes this property as an inverse property of the given property
        /// </remarks>
        public bool RemoveInverseProperty(OntologyProperty property)
        {
            bool a = this.RemoveInverseProperty(property.Resource);
            bool b = property.RemoveInverseProperty(this._resource);
            return (a || b);
        }

        /// <summary>
        /// Adds a sub-property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddSubProperty(INode resource)
        {
            return this.AddResourceProperty(PropertyDerivedProperty, resource.CopyNode(this._graph), false);
        }

        /// <summary>
        /// Adds a sub-property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddSubProperty(Uri resource)
        {
            return this.AddSubProperty(this._graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Adds a sub-property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddSubProperty(OntologyResource resource)
        {
            return this.AddSubProperty(resource.Resource);
        }

        /// <summary>
        /// Adds a sub-property for the property
        /// </summary>
        /// <param name="property">Property</param>
        /// <returns></returns>
        /// <remarks>
        /// This overload also adds this property as a super-property of the given property
        /// </remarks>
        public bool AddSubProperty(OntologyProperty property)
        {
            bool a = this.AddSubProperty(property.Resource);
            bool b = property.AddSuperProperty(this._resource);
            return (a || b);
        }

        /// <summary>
        /// Clears all sub-properties of this property
        /// </summary>
        /// <returns></returns>
        public bool ClearSubProperties()
        {
            this._graph.Retract(this._graph.GetTriplesWithPredicateObject(this._graph.CreateUriNode(new Uri(OntologyHelper.PropertySubPropertyOf)), this._resource));
            return this.ClearResourceProperty(PropertyDerivedProperty, false);
        }

        /// <summary>
        /// Removes a sub-property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveSubProperty(INode resource)
        {
            return this.RemoveResourceProperty(PropertyDerivedProperty, resource.CopyNode(this._graph), false);
        }

        /// <summary>
        /// Removes a sub-property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveSubProperty(Uri resource)
        {
            return this.RemoveSubProperty(this._graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Removes a sub-property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveSubProperty(OntologyResource resource)
        {
            return this.RemoveSubProperty(resource.Resource);
        }

        /// <summary>
        /// Removes a sub-property for the property
        /// </summary>
        /// <param name="property">Property</param>
        /// <returns></returns>
        /// <remarks>
        /// This overload also removes this property as a super-property of the given property
        /// </remarks>
        public bool RemoveSubProperty(OntologyProperty property)
        {
            bool a = this.RemoveSubProperty(property.Resource);
            bool b = property.RemoveSuperProperty(this._resource);
            return (a || b);
        }

        /// <summary>
        /// Adds a super-property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddSuperProperty(INode resource)
        {
            return this.AddResourceProperty(OntologyHelper.PropertySubPropertyOf, resource.CopyNode(this._graph), true);
        }

        /// <summary>
        /// Adds a super-property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddSuperProperty(Uri resource)
        {
            return this.AddSuperProperty(this._graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Adds a super-property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddSuperProperty(OntologyResource resource)
        {
            return this.AddSuperProperty(resource.Resource);
        }

        /// <summary>
        /// Adds a super-property for the property
        /// </summary>
        /// <param name="property">Property</param>
        /// <returns></returns>
        /// <remarks>
        /// This overload also adds this property as a sub-property of the given property
        /// </remarks>
        public bool AddSuperProperty(OntologyProperty property)
        {
            bool a = this.AddSuperProperty(property.Resource);
            bool b = property.AddSubProperty(this._resource);
            return (a || b);
        }

        /// <summary>
        /// Removes all super-properties of this property
        /// </summary>
        /// <returns></returns>
        public bool ClearSuperProperties()
        {
            this._graph.Retract(this._graph.GetTriplesWithSubjectPredicate(this._resource, this._graph.CreateUriNode(new Uri(OntologyHelper.PropertySubPropertyOf))));
            return this.ClearResourceProperty(OntologyHelper.PropertySubPropertyOf, true);
        }

        /// <summary>
        /// Removes a super-property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveSuperProperty(INode resource)
        {
            return this.RemoveResourceProperty(OntologyHelper.PropertySubPropertyOf, resource.CopyNode(this._graph), true);
        }

        /// <summary>
        /// Removes a super-property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveSuperProperty(Uri resource)
        {
            return this.RemoveSuperProperty(this._graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Removes a super-property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveSuperProperty(OntologyResource resource)
        {
            return this.RemoveSuperProperty(resource.Resource);
        }

        /// <summary>
        /// Removes a super-property for the property
        /// </summary>
        /// <param name="property">Property</param>
        /// <returns></returns>
        /// <remarks>
        /// This overload also removes this property as a sub-property of the given property
        /// </remarks>
        public bool RemoveSuperProperty(OntologyProperty property)
        {
            bool a = this.RemoveSuperProperty(property.Resource);
            bool b = property.RemoveSubProperty(this._resource);
            return (a || b);
        }

        /// <summary>
        /// Gets all the Classes which are in the properties Domain
        /// </summary>
        public IEnumerable<OntologyClass> Domains
        {
            get
            {
                return this.GetResourceProperty(OntologyHelper.PropertyDomain).Select(r => new OntologyClass(r, this._graph));
            }
        }

        /// <summary>
        /// Gets all the Classes which are in this properties Range
        /// </summary>
        public IEnumerable<OntologyClass> Ranges
        {
            get
            {
                return this.GetResourceProperty(OntologyHelper.PropertyRange).Select(r => new OntologyClass(r, this._graph));
            }
        }

        /// <summary>
        /// Gets all the equivalent properties of this property
        /// </summary>
        public IEnumerable<OntologyProperty> EquivalentProperties
        {
            get
            {
                return this.GetResourceProperty(OntologyHelper.PropertyEquivalentProperty).Select(r => new OntologyProperty(r, this._graph));
            }
        }

        /// <summary>
        /// Gets the sub-properties of this property (both direct and indirect)
        /// </summary>
        public IEnumerable<OntologyProperty> SubProperties
        {
            get
            {
                return this.GetResourceProperty(PropertyDerivedProperty).Select(c => new OntologyProperty(c, this._graph));
            }
        }

        /// <summary>
        /// Gets the super-properties of this property (both direct and indirect)
        /// </summary>
        public IEnumerable<OntologyProperty> SuperProperties
        {
            get
            {
                return this.GetResourceProperty(OntologyHelper.PropertySubPropertyOf).Select(c => new OntologyProperty(c, this._graph));
            }
        }

        /// <summary>
        /// Gets all the inverse properties of this property
        /// </summary>
        public IEnumerable<OntologyProperty> InverseProperties
        {
            get
            {
                return this.GetResourceProperty(OntologyHelper.PropertyInverseOf).Select(r => new OntologyProperty(r, this._graph));
            }
        }

        /// <summary>
        /// Gets all the resources that use this property
        /// </summary>
        public IEnumerable<OntologyResource> UsedBy
        {
            get
            {
                return (from t in this._graph.GetTriplesWithPredicate(this._resource)
                        select t.Subject).Distinct().Select(r => new OntologyResource(r, this._graph));
            }
        }
    }
}
