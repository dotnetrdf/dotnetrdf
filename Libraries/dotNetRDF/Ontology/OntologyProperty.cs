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
    public class OntologyProperty 
        : OntologyResource
    {
        private const String PropertyDerivedProperty = "derivedProperty";
        private const String PropertyDirectSubProperty = "directSubProperty";
        private const String PropertyDirectSuperProperty = "directSuperProperty";

        /// <summary>
        /// Creates a new Ontology Property for the given resource in the given Graph
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <param name="graph">Graph</param>
        public OntologyProperty(INode resource, IGraph graph)
            : base(resource, graph) 
        {
            // Q: Assert that this resource is a property?
            // UriNode rdfType = graph.CreateUriNode(new Uri(OntologyHelper.PropertyType));
            // graph.Assert(new Triple(resource, rdfType, graph.CreateUriNode(new Uri(OntologyHelper.RdfProperty))));

            IntialiseProperty(OntologyHelper.PropertyDomain, false);
            IntialiseProperty(OntologyHelper.PropertyRange, false);
            IntialiseProperty(OntologyHelper.PropertyEquivalentProperty, false);
            IntialiseProperty(OntologyHelper.PropertySubPropertyOf, false);
            IntialiseProperty(OntologyHelper.PropertyInverseOf, false);

            // Find derived properties
            IUriNode subPropertyOf = _graph.CreateUriNode(UriFactory.Create(OntologyHelper.PropertySubPropertyOf));
            _resourceProperties.Add(PropertyDerivedProperty, new HashSet<INode>());
            _resourceProperties.Add(PropertyDirectSubProperty, new HashSet<INode>());
            foreach (Triple t in _graph.GetTriplesWithPredicateObject(subPropertyOf, _resource))
            {
                _resourceProperties[PropertyDerivedProperty].Add(t.Subject);
                _resourceProperties[PropertyDirectSubProperty].Add(t.Subject);
            }
            int c = 0;
            do
            {
                c = _resourceProperties[PropertyDerivedProperty].Count;
                foreach (INode n in _resourceProperties[PropertyDerivedProperty].ToList())
                {
                    foreach (Triple t in _graph.GetTriplesWithPredicateObject(subPropertyOf, n))
                    {
                        _resourceProperties[PropertyDerivedProperty].Add(t.Subject);
                    }
                }
            } while (c < _resourceProperties[PropertyDerivedProperty].Count);

            // Find additional super properties
            _resourceProperties.Add(PropertyDirectSuperProperty, new HashSet<INode>());
            if (_resourceProperties.ContainsKey(OntologyHelper.PropertySubPropertyOf))
            {
              foreach (var node in _resourceProperties[OntologyHelper.PropertySubPropertyOf])
              {
                _resourceProperties[PropertyDirectSuperProperty].Add(node);
              }

                do
                {
                    c = _resourceProperties[OntologyHelper.PropertySubPropertyOf].Count;
                    foreach (INode n in _resourceProperties[OntologyHelper.PropertySubPropertyOf].ToList())
                    {
                        foreach (Triple t in _graph.GetTriplesWithSubjectPredicate(n, subPropertyOf))
                        {
                            _resourceProperties[OntologyHelper.PropertySubPropertyOf].Add(t.Object);
                        }
                    }
                } while (c < _resourceProperties[OntologyHelper.PropertySubPropertyOf].Count);
            }

            // Find additional inverses
            if (!_resourceProperties.ContainsKey(OntologyHelper.PropertyInverseOf)) _resourceProperties.Add(OntologyHelper.PropertyInverseOf, new HashSet<INode>());
            foreach (Triple t in _graph.GetTriplesWithPredicateObject(graph.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyInverseOf)), _resource))
            {
                _resourceProperties[OntologyHelper.PropertyInverseOf].Add(t.Subject);
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
            return AddResourceProperty(OntologyHelper.PropertyDomain, resource.CopyNode(_graph), true);
        }

        /// <summary>
        /// Adds a new domain for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddDomain(Uri resource)
        {
            return AddDomain(_graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Adds a new domain for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddDomain(OntologyResource resource)
        {
            return AddDomain(resource.Resource);
        }

        /// <summary>
        /// Clears all domains for the property
        /// </summary>
        /// <returns></returns>
        public bool ClearDomains()
        {
            return ClearResourceProperty(OntologyHelper.PropertyDomain, true);
        }

        /// <summary>
        /// Removes a domain for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveDomain(INode resource)
        {
            return RemoveResourceProperty(OntologyHelper.PropertyDomain, resource.CopyNode(_graph), true);
        }

        /// <summary>
        /// Removes a domain for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveDomain(Uri resource)
        {
            return RemoveDomain(_graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Removes a domain for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveDomain(OntologyResource resource)
        {
            return RemoveDomain(resource.Resource);
        }

        /// <summary>
        /// Adds a new range for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddRange(INode resource)
        {
            return AddResourceProperty(OntologyHelper.PropertyRange, resource.CopyNode(_graph), true);
        }

        /// <summary>
        /// Adds a new range for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddRange(Uri resource)
        {
            return AddRange(_graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Adds a new range for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddRange(OntologyResource resource)
        {
            return AddRange(resource.Resource);
        }

        /// <summary>
        /// Clears all ranges for the property
        /// </summary>
        /// <returns></returns>
        public bool ClearRanges()
        {
            return ClearResourceProperty(OntologyHelper.PropertyRange, true);
        }

        /// <summary>
        /// Removes a range for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveRange(INode resource)
        {
            return RemoveResourceProperty(OntologyHelper.PropertyRange, resource, true);
        }

        /// <summary>
        /// Removes a range for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveRange(Uri resource)
        {
            return RemoveRange(_graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Removes a range for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveRange(OntologyResource resource)
        {
            return RemoveRange(resource.Resource);
        }

        /// <summary>
        /// Adds a new equivalent property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddEquivalentProperty(INode resource)
        {
            return AddResourceProperty(OntologyHelper.PropertyEquivalentProperty, resource.CopyNode(_graph), true);
        }

        /// <summary>
        /// Adds a new equivalent property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddEquivalentProperty(Uri resource)
        {
            return AddEquivalentProperty(_graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Adds a new equivalent property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddEquivalentProperty(OntologyResource resource)
        {
            return AddEquivalentProperty(resource.Resource);
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
            bool a = AddEquivalentProperty(property.Resource);
            bool b = property.AddEquivalentProperty(_resource);
            return (a || b);
        }

        /// <summary>
        /// Clears all equivalent properties for this property
        /// </summary>
        /// <returns></returns>
        public bool ClearEquivalentProperties()
        {
            INode equivProp = _graph.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyEquivalentProperty));
            _graph.Retract(_graph.GetTriplesWithSubjectPredicate(_resource, equivProp).ToList());
            _graph.Retract(_graph.GetTriplesWithPredicateObject(equivProp, _resource).ToList());
            return ClearResourceProperty(OntologyHelper.PropertyEquivalentProperty, true);
        }

        /// <summary>
        /// Removes an equivalent property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveEquivalentProperty(INode resource)
        {
            return RemoveResourceProperty(OntologyHelper.PropertyEquivalentProperty, resource.CopyNode(_graph), true);
        }

        /// <summary>
        /// Removes an equivalent property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveEquivalentProperty(Uri resource)
        {
            return RemoveEquivalentProperty(_graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Removes an equivalent property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveEquivalentProperty(OntologyResource resource)
        {
            return RemoveEquivalentProperty(resource.Resource);
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
            bool a = RemoveEquivalentProperty(property.Resource);
            bool b = property.RemoveEquivalentProperty(_resource);
            return (a || b);
        }

        /// <summary>
        /// Adds an inverse property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddInverseProperty(INode resource)
        {
            return AddResourceProperty(OntologyHelper.PropertyInverseOf, resource.CopyNode(_graph), true);
        }

        /// <summary>
        /// Adds an inverse property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddInverseProperty(Uri resource)
        {
            return AddInverseProperty(_graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Adds an inverse property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddInverseProperty(OntologyResource resource)
        {
            return AddInverseProperty(resource.Resource);
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
            bool a = AddInverseProperty(property.Resource);
            bool b = property.AddInverseProperty(_resource);
            return (a || b);
        }

        /// <summary>
        /// Removes all inverse properties for this property
        /// </summary>
        /// <returns></returns>
        public bool ClearInverseProperties()
        {
            INode inverseOf = _graph.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyInverseOf));
            _graph.Retract(_graph.GetTriplesWithSubjectPredicate(_resource, inverseOf).ToList());
            _graph.Retract(_graph.GetTriplesWithPredicateObject(inverseOf, _resource).ToList());
            return ClearResourceProperty(OntologyHelper.PropertyInverseOf, true);
        }

        /// <summary>
        /// Removes an inverse property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveInverseProperty(INode resource)
        {
            return RemoveResourceProperty(OntologyHelper.PropertyInverseOf, resource.CopyNode(_graph), true);
        }

        /// <summary>
        /// Removes an inverse property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveInverseProperty(Uri resource)
        {
            return RemoveInverseProperty(_graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Removes an inverse property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveInverseProperty(OntologyResource resource)
        {
            return RemoveInverseProperty(resource.Resource);
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
            bool a = RemoveInverseProperty(property.Resource);
            bool b = property.RemoveInverseProperty(_resource);
            return (a || b);
        }

        /// <summary>
        /// Adds a sub-property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddSubProperty(INode resource)
        {
            return AddResourceProperty(PropertyDerivedProperty, resource.CopyNode(_graph), false);
        }

        /// <summary>
        /// Adds a sub-property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddSubProperty(Uri resource)
        {
            return AddSubProperty(_graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Adds a sub-property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddSubProperty(OntologyResource resource)
        {
            return AddSubProperty(resource.Resource);
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
            bool a = AddSubProperty(property.Resource);
            bool b = property.AddSuperProperty(_resource);
            return (a || b);
        }

        /// <summary>
        /// Clears all sub-properties of this property
        /// </summary>
        /// <returns></returns>
        public bool ClearSubProperties()
        {
            _graph.Retract(_graph.GetTriplesWithPredicateObject(_graph.CreateUriNode(UriFactory.Create(OntologyHelper.PropertySubPropertyOf)), _resource).ToList());
            return ClearResourceProperty(PropertyDerivedProperty, false);
        }

        /// <summary>
        /// Removes a sub-property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveSubProperty(INode resource)
        {
            return RemoveResourceProperty(PropertyDerivedProperty, resource.CopyNode(_graph), false);
        }

        /// <summary>
        /// Removes a sub-property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveSubProperty(Uri resource)
        {
            return RemoveSubProperty(_graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Removes a sub-property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveSubProperty(OntologyResource resource)
        {
            return RemoveSubProperty(resource.Resource);
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
            bool a = RemoveSubProperty(property.Resource);
            bool b = property.RemoveSuperProperty(_resource);
            return (a || b);
        }

        /// <summary>
        /// Adds a super-property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddSuperProperty(INode resource)
        {
            return AddResourceProperty(OntologyHelper.PropertySubPropertyOf, resource.CopyNode(_graph), true);
        }

        /// <summary>
        /// Adds a super-property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddSuperProperty(Uri resource)
        {
            return AddSuperProperty(_graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Adds a super-property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddSuperProperty(OntologyResource resource)
        {
            return AddSuperProperty(resource.Resource);
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
            bool a = AddSuperProperty(property.Resource);
            bool b = property.AddSubProperty(_resource);
            return (a || b);
        }

        /// <summary>
        /// Removes all super-properties of this property
        /// </summary>
        /// <returns></returns>
        public bool ClearSuperProperties()
        {
            _graph.Retract(_graph.GetTriplesWithSubjectPredicate(_resource, _graph.CreateUriNode(UriFactory.Create(OntologyHelper.PropertySubPropertyOf))).ToList());
            return ClearResourceProperty(OntologyHelper.PropertySubPropertyOf, true);
        }

        /// <summary>
        /// Removes a super-property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveSuperProperty(INode resource)
        {
            return RemoveResourceProperty(OntologyHelper.PropertySubPropertyOf, resource.CopyNode(_graph), true);
        }

        /// <summary>
        /// Removes a super-property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveSuperProperty(Uri resource)
        {
            return RemoveSuperProperty(_graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Removes a super-property for the property
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveSuperProperty(OntologyResource resource)
        {
            return RemoveSuperProperty(resource.Resource);
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
            bool a = RemoveSuperProperty(property.Resource);
            bool b = property.RemoveSubProperty(_resource);
            return (a || b);
        }

        /// <summary>
        /// Gets all the Classes which are in the properties Domain
        /// </summary>
        public IEnumerable<OntologyClass> Domains
        {
            get
            {
                return GetResourceProperty(OntologyHelper.PropertyDomain).Select(r => new OntologyClass(r, _graph));
            }
        }

        /// <summary>
        /// Gets all the Classes which are in this properties Range
        /// </summary>
        public IEnumerable<OntologyClass> Ranges
        {
            get
            {
                return GetResourceProperty(OntologyHelper.PropertyRange).Select(r => new OntologyClass(r, _graph));
            }
        }

        /// <summary>
        /// Gets all the equivalent properties of this property
        /// </summary>
        public IEnumerable<OntologyProperty> EquivalentProperties
        {
            get
            {
                return GetResourceProperty(OntologyHelper.PropertyEquivalentProperty).Select(r => new OntologyProperty(r, _graph));
            }
        }

        /// <summary>
        /// Gets the sub-properties of this property (both direct and indirect)
        /// </summary>
        public IEnumerable<OntologyProperty> SubProperties
        {
            get
            {
                return GetResourceProperty(PropertyDerivedProperty).Select(c => new OntologyProperty(c, _graph));
            }
        }

        /// <summary>
        /// Gets the direct sub-classes of this class
        /// </summary>
        public IEnumerable<OntologyProperty> DirectSubProperties
        {
            get
            {
                return GetResourceProperty(PropertyDirectSubProperty).Select(p => new OntologyProperty(p, _graph));
            }
        }

        /// <summary>
        /// Gets the indirect sub-classes of this class
        /// </summary>
        public IEnumerable<OntologyProperty> IndirectSubProperties
        {
            get
            {
                return (from c in GetResourceProperty(PropertyDerivedProperty)
                        where !GetResourceProperty(PropertyDirectSubProperty).Contains(c)
                        select new OntologyProperty(c, _graph));
            }
        }

        /// <summary>
        /// Gets the super-properties of this property (both direct and indirect)
        /// </summary>
        public IEnumerable<OntologyProperty> SuperProperties
        {
            get
            {
                return GetResourceProperty(OntologyHelper.PropertySubPropertyOf).Select(c => new OntologyProperty(c, _graph));
            }
        }

        /// <summary>
        /// Gets the direct super-properties of this property
        /// </summary>
        public IEnumerable<OntologyProperty> DirectSuperProperties
        {
            get
            {
                return GetResourceProperty(PropertyDirectSuperProperty).Select(c => new OntologyProperty(c, _graph));
            }
        }

        /// <summary>
        /// Gets the indirect super-properties of this property
        /// </summary>
        public IEnumerable<OntologyProperty> IndirectSuperProperty
        {
            get
            {
                return (from c in GetResourceProperty(OntologyHelper.PropertySubPropertyOf)
                        where !GetResourceProperty(PropertyDirectSuperProperty).Contains(c)
                        select new OntologyProperty(c, _graph));
            }
        }

        /// <summary>
        /// Gets whether this is a top property i.e. has no super properties defined
        /// </summary>
        public bool IsTopProperty
        {
            get
            {
                return !SuperProperties.Any();
            }
        }

        /// <summary>
        /// Gets whether this is a btoom property i.e. has no sub properties defined
        /// </summary>
        public bool IsBottomProperty
        {
            get
            {
                return !SubProperties.Any();
            }
        }

        /// <summary>
        /// Gets the Sibling properties of this property, if this property is the root of the ontology nothing is returned even if there are multiple root properties
        /// </summary>
        public IEnumerable<OntologyProperty> Siblings
        {
            get
            {
                return GetResourceProperty(PropertyDirectSuperProperty)
                       .Select(p => new OntologyProperty(p, _graph))
                       .SelectMany(p => p.DirectSubProperties)
                       .Where(p => !p.Resource.Equals(_resource)).Distinct();
            }
        }

        /// <summary>
        /// Gets all the inverse properties of this property
        /// </summary>
        public IEnumerable<OntologyProperty> InverseProperties
        {
            get
            {
                return GetResourceProperty(OntologyHelper.PropertyInverseOf).Select(r => new OntologyProperty(r, _graph));
            }
        }

        /// <summary>
        /// Gets all the resources that use this property
        /// </summary>
        public IEnumerable<OntologyResource> UsedBy
        {
            get
            {
                return (from t in _graph.GetTriplesWithPredicate(_resource)
                        select t.Subject).Distinct().Select(r => new OntologyResource(r, _graph));
            }
        }
    }
}
