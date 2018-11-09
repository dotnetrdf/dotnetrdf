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
    /// Class for representing a class in an Ontology
    /// </summary>
    /// <remarks>
    /// <para>
    /// See <a href="http://www.dotnetrdf.org/content.asp?pageID=Ontology%20API">Using the Ontology API</a> for some informal documentation on the use of the Ontology namespace
    /// </para>
    /// </remarks>
    public class OntologyClass
        : OntologyResource
    {
        private const String PropertyDerivedClass = "derivedClass";
        private const String PropertyDirectSubClass = "directSubClass";
        private const String PropertyDirectSuperClass = "directSuperClass";

        /// <summary>
        /// Creates a new representation of a Class in the given Ontology Mode
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <param name="graph">Graph</param>
        public OntologyClass(INode resource, IGraph graph)
            : base(resource, graph)
        {
            // Q: Assert that this resource is a Class?
            // UriNode rdfType = graph.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyType));
            // graph.Assert(new Triple(resource, rdfType, graph.CreateUriNode(new Uri(OntologyHelper.RdfsClass))));

            IntialiseProperty(OntologyHelper.PropertySubClassOf, false);
            IntialiseProperty(OntologyHelper.PropertyEquivalentClass, false);
            IntialiseProperty(OntologyHelper.PropertyDisjointWith, false);

            // Find derived classes
            IUriNode subClassOf = _graph.CreateUriNode(UriFactory.Create(OntologyHelper.PropertySubClassOf));
            _resourceProperties.Add(PropertyDerivedClass, new HashSet<INode>());
            _resourceProperties.Add(PropertyDirectSubClass, new HashSet<INode>());
            foreach (Triple t in _graph.GetTriplesWithPredicateObject(subClassOf, _resource))
            {
                _resourceProperties[PropertyDerivedClass].Add(t.Subject);
                _resourceProperties[PropertyDirectSubClass].Add(t.Subject);
            }
            int c = 0; 
            do
            {
                c = _resourceProperties[PropertyDerivedClass].Count;
                foreach (INode n in _resourceProperties[PropertyDerivedClass].ToList())
                {
                    foreach (Triple t in _graph.GetTriplesWithPredicateObject(subClassOf, n))
                    {
                        _resourceProperties[PropertyDerivedClass].Add(t.Subject);
                    }
                }
            } while (c < _resourceProperties[PropertyDerivedClass].Count);

            // Find additional super classes
            _resourceProperties.Add(PropertyDirectSuperClass, new HashSet<INode>());
            if (_resourceProperties.ContainsKey(OntologyHelper.PropertySubClassOf))
            {
              foreach (var node in _resourceProperties[OntologyHelper.PropertySubClassOf])
              {
                _resourceProperties[PropertyDirectSuperClass].Add(node);
              }

                do
                {
                    c = _resourceProperties[OntologyHelper.PropertySubClassOf].Count;
                    foreach (INode n in _resourceProperties[OntologyHelper.PropertySubClassOf].ToList())
                    {
                        foreach (Triple t in _graph.GetTriplesWithSubjectPredicate(n, subClassOf))
                        {
                            _resourceProperties[OntologyHelper.PropertySubClassOf].Add(t.Object);
                        }
                    }
                } while (c < _resourceProperties[OntologyHelper.PropertySubClassOf].Count);
            }
        }

        /// <summary>
        /// Adds a new sub-class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddSubClass(INode resource)
        {
            return AddResourceProperty(PropertyDerivedClass, resource.CopyNode(_graph), false);
        }

        /// <summary>
        /// Adds a new sub-class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddSubClass(Uri resource)
        {
            return AddSubClass(_graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Adds a new sub-class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddSubClass(OntologyResource resource)
        {
            return AddSuperClass(resource.Resource);
        }

        /// <summary>
        /// Adds a new sub-class for this class
        /// </summary>
        /// <param name="class">Class</param>
        /// <returns></returns>
        /// <remarks>
        /// This overload also adds this class as a super-class of the given class
        /// </remarks>
        public bool AddSubClass(OntologyClass @class)
        {
            bool a = AddSubClass(@class.Resource);
            bool b = @class.AddSuperClass(_resource);
            return (a || b);
        }

        /// <summary>
        /// Removes all sub-classes for this class
        /// </summary>
        /// <returns></returns>
        public bool ClearSubClasses()
        {
            _graph.Retract(_graph.GetTriplesWithPredicateObject(_graph.CreateUriNode(UriFactory.Create(OntologyHelper.PropertySubClassOf)), _resource).ToList());
            return ClearResourceProperty(PropertyDerivedClass, false);
        }

        /// <summary>
        /// Removes a sub-class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveSubClass(INode resource)
        {
            return RemoveResourceProperty(PropertyDerivedClass, resource.CopyNode(_graph), false);
        }

        /// <summary>
        /// Removes a sub-class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveSubClass(Uri resource)
        {
            return RemoveSubClass(_graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Removes a sub-class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveSubClass(OntologyResource resource)
        {
            return RemoveSubClass(resource.Resource);
        }

        /// <summary>
        /// Removes a sub-class for this class
        /// </summary>
        /// <param name="class">Class</param>
        /// <returns></returns>
        /// <remarks>
        /// This overload also removes this class from being a super-class of the given class
        /// </remarks>
        public bool RemoveSubClass(OntologyClass @class)
        {
            bool a = RemoveSubClass(@class.Resource);
            bool b = @class.RemoveSuperClass(_resource);
            return (a || b);
        }

        /// <summary>
        /// Adds a new super-class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddSuperClass(INode resource)
        {
            return AddResourceProperty(OntologyHelper.PropertySubClassOf, resource.CopyNode(_graph), true);
        }

        /// <summary>
        /// Adds a new super-class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddSuperClass(Uri resource)
        {
            return AddSuperClass(_graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Adds a new super-class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddSuperClass(OntologyResource resource)
        {
            return AddSuperClass(resource.Resource);
        }

        /// <summary>
        /// Adds a new super-class for this class
        /// </summary>
        /// <param name="class">Class</param>
        /// <returns></returns>
        /// <remarks>
        /// This overload also adds this class as a sub-class of the given class
        /// </remarks>
        public bool AddSuperClass(OntologyClass @class)
        {
            bool a = AddSuperClass(@class.Resource);
            bool b = @class.AddSubClass(_resource);
            return (a || b);
        }

        /// <summary>
        /// Removes all super-classes
        /// </summary>
        /// <returns></returns>
        public bool ClearSuperClasses()
        {
            _graph.Retract(_graph.GetTriplesWithSubjectPredicate(_resource, _graph.CreateUriNode(UriFactory.Create(OntologyHelper.PropertySubClassOf))).ToList());
            return ClearResourceProperty(OntologyHelper.PropertySubClassOf, true);
        }

        /// <summary>
        /// Removes a super-class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveSuperClass(INode resource)
        {
            return RemoveResourceProperty(OntologyHelper.PropertySubClassOf, resource.CopyNode(_graph), true);
        }

        /// <summary>
        /// Removes a super-class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveSuperClass(Uri resource)
        {
            return RemoveSuperClass(_graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Removes a super-class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveSuperClass(OntologyResource resource)
        {
            return RemoveSuperClass(resource.Resource);
        }

        /// <summary>
        /// Removes a super-class for this class
        /// </summary>
        /// <param name="class">Class</param>
        /// <returns></returns>
        /// <remarks>
        /// This overload also removes this class as a sub-class of the given class
        /// </remarks>
        public bool RemoveSuperClass(OntologyClass @class)
        {
            bool a = RemoveSuperClass(@class.Resource);
            bool b = @class.RemoveSubClass(_resource);
            return (a || b);
        }

        /// <summary>
        /// Adds an equivalent class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddEquivalentClass(INode resource)
        {
            return AddResourceProperty(OntologyHelper.PropertyEquivalentClass, resource.CopyNode(_graph), true);
        }

        /// <summary>
        /// Adds an equivalent class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddEquivalentClass(Uri resource)
        {
            return AddEquivalentClass(_graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Adds an equivalent class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddEquivalentClass(OntologyResource resource)
        {
            return AddEquivalentClass(resource.Resource);
        }

        /// <summary>
        /// Adds an equivalent class for this class
        /// </summary>
        /// <param name="class">Class</param>
        /// <returns></returns>
        /// <remarks>
        /// This overload also adds this class as an equivalent class of the given class
        /// </remarks>
        public bool AddEquivalentClass(OntologyClass @class)
        {
            bool a = AddEquivalentClass(@class.Resource);
            bool b = @class.AddEquivalentClass(_resource);
            return (a || b);
        }

        /// <summary>
        /// Removes all equivalent classes for this class
        /// </summary>
        /// <returns></returns>
        public bool ClearEquivalentClasses()
        {
            INode equivClass = _graph.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyEquivalentClass));
            _graph.Retract(_graph.GetTriplesWithSubjectPredicate(_resource, equivClass).ToList());
            _graph.Retract(_graph.GetTriplesWithPredicateObject(equivClass, _resource).ToList());
            return ClearResourceProperty(OntologyHelper.PropertyEquivalentClass, true);
        }

        /// <summary>
        /// Removes an equivalent class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveEquivalentClass(INode resource)
        {
            return RemoveResourceProperty(OntologyHelper.PropertyEquivalentClass, resource.CopyNode(_graph), true);
        }

        /// <summary>
        /// Removes an equivalent class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveEquivalentClass(Uri resource)
        {
            return RemoveEquivalentClass(_graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Removes an equivalent class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveEquivalentClass(OntologyResource resource)
        {
            return RemoveEquivalentClass(resource.Resource);
        }

        /// <summary>
        /// Removes an equivalent class for this class
        /// </summary>
        /// <param name="class">Class</param>
        /// <returns></returns>
        public bool RemoveEquivalentClass(OntologyClass @class)
        {
            bool a = RemoveEquivalentClass(@class.Resource);
            bool b = @class.RemoveEquivalentClass(_resource);
            return (a || b);
        }

        /// <summary>
        /// Adds a new disjoint class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddDisjointClass(INode resource)
        {
            return AddResourceProperty(OntologyHelper.PropertyDisjointWith, resource.CopyNode(_graph), true);
        }

        /// <summary>
        /// Adds a new disjoint class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddDisjointClass(Uri resource)
        {
            return AddDisjointClass(_graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Adds a new disjoint class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddDisjointClass(OntologyResource resource)
        {
            return AddDisjointClass(resource.Resource);
        }

        /// <summary>
        /// Adds a new disjoint class for this class
        /// </summary>
        /// <param name="class">Class</param>
        /// <returns></returns>
        /// <remarks>
        /// This overload also adds this class as a disjoint class of the given class
        /// </remarks>
        public bool AddDisjointClass(OntologyClass @class)
        {
            bool a = AddDisjointClass(@class.Resource);
            bool b = @class.AddDisjointClass(_resource);
            return (a || b);
        }

        /// <summary>
        /// Removes all disjoint classes for this class
        /// </summary>
        /// <returns></returns>
        public bool ClearDisjointClasses()
        {
            INode disjointClass = _graph.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyDisjointWith));
            _graph.Retract(_graph.GetTriplesWithSubjectPredicate(_resource, disjointClass).ToList());
            _graph.Retract(_graph.GetTriplesWithPredicateObject(disjointClass, _resource).ToList());
            return ClearResourceProperty(OntologyHelper.PropertyDisjointWith, true);
        }

        /// <summary>
        /// Removes a disjoint class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveDisjointClass(INode resource)
        {
            return RemoveResourceProperty(OntologyHelper.PropertyDisjointWith, resource.CopyNode(_graph), true);
        }

        /// <summary>
        /// Removes a disjoint class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveDisjointClass(Uri resource)
        {
            return RemoveDisjointClass(_graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Removes a disjoint class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveDisjointClass(OntologyResource resource)
        {
            return RemoveDisjointClass(resource.Resource);
        }

        /// <summary>
        /// Removes a disjoint class for this class
        /// </summary>
        /// <param name="class">Class</param>
        /// <returns></returns>
        /// <remarks>
        /// This overload also removes this class as a disjoint class of the given class
        /// </remarks>
        public bool RemoveDisjointClass(OntologyClass @class)
        {
            bool a = RemoveDisjointClass(@class.Resource);
            bool b = @class.RemoveDisjointClass(_resource);
            return (a || b);
        }

        /// <summary>
        /// Gets the sub-classes of this class (both direct and indirect)
        /// </summary>
        public IEnumerable<OntologyClass> SubClasses
        {
            get
            {
                return GetResourceProperty(PropertyDerivedClass).Select(c => new OntologyClass(c, _graph));
            }
        }

        /// <summary>
        /// Gets the direct sub-classes of this class
        /// </summary>
        public IEnumerable<OntologyClass> DirectSubClasses
        {
            get
            {
                return GetResourceProperty(PropertyDirectSubClass).Select(c => new OntologyClass(c, _graph));
            }
        }

        /// <summary>
        /// Gets the indirect sub-classes of this class
        /// </summary>
        public IEnumerable<OntologyClass> IndirectSubClasses
        {
            get
            {
                return (from c in GetResourceProperty(PropertyDerivedClass)
                        where !GetResourceProperty(PropertyDirectSubClass).Contains(c)
                        select new OntologyClass(c, _graph));
            }
        }

        /// <summary>
        /// Gets the super-classes of this class (both direct and indirect)
        /// </summary>
        public IEnumerable<OntologyClass> SuperClasses
        {
            get
            {
                return GetResourceProperty(OntologyHelper.PropertySubClassOf).Select(c => new OntologyClass(c, _graph));
            }
        }

        /// <summary>
        /// Gets the direct super-classes of this class
        /// </summary>
        public IEnumerable<OntologyClass> DirectSuperClasses
        {
            get
            {
                return GetResourceProperty(PropertyDirectSuperClass).Select(c => new OntologyClass(c, _graph));
            }
        }

        /// <summary>
        /// Gets the indirect super-classes of this class
        /// </summary>
        public IEnumerable<OntologyClass> IndirectSuperClasses
        {
            get
            {
                return (from c in GetResourceProperty(OntologyHelper.PropertySubClassOf)
                        where !GetResourceProperty(PropertyDirectSuperClass).Contains(c)
                        select new OntologyClass(c, _graph));
            }
        }

        /// <summary>
        /// Gets the Sibling classes of this class, if this class is the root of the ontology nothing is returned even if there are multiple root classes
        /// </summary>
        public IEnumerable<OntologyClass> Siblings
        {
            get
            {
                return GetResourceProperty(PropertyDirectSuperClass)
                       .Select(c => new OntologyClass(c, _graph))
                       .SelectMany(c => c.DirectSubClasses)
                       .Where(c => !c.Resource.Equals(_resource)).Distinct();
            }
        }

        /// <summary>
        /// Gets the equivalent classes of this class
        /// </summary>
        public IEnumerable<OntologyClass> EquivalentClasses
        {
            get
            {
                return GetResourceProperty(OntologyHelper.PropertyEquivalentClass).Select(c => new OntologyClass(c, _graph));
            }
        }

        /// <summary>
        /// Gets the disjoint classes of this class
        /// </summary>
        public IEnumerable<OntologyClass> DisjointClasses
        {
            get
            {
                return GetResourceProperty(OntologyHelper.PropertyDisjointWith).Select(c => new OntologyClass(c, _graph));
            }
        }

        /// <summary>
        /// Gets the instances (individuals) of this class
        /// </summary>
        public IEnumerable<OntologyResource> Instances
        {
            get
            {
                return (from t in _graph.GetTriplesWithPredicateObject(_graph.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyType)), _resource)
                        select new OntologyResource(t.Subject, _graph));
            }
        }

        /// <summary>
        /// Gets the properties which have this class as a domain
        /// </summary>
        public IEnumerable<OntologyProperty> IsDomainOf
        {
            get
            {
                INode domain = _graph.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "domain"));
                return (from t in _graph.GetTriplesWithPredicateObject(domain, _resource)
                        select new OntologyProperty(t.Subject, _graph));
            }
        }

        /// <summary>
        /// Gets the properties which have this class as a range
        /// </summary>
        public IEnumerable<OntologyProperty> IsRangeOf
        {
            get
            {
                INode range = _graph.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "range"));
                return (from t in _graph.GetTriplesWithPredicateObject(range, _resource)
                        select new OntologyProperty(t.Subject, _graph));
            }
        }

        /// <summary>
        /// Gets whether something is a Top Class i.e. has no super classes
        /// </summary>
        public bool IsTopClass
        {
            get
            {
                return !SuperClasses.Any();
            }
        }

        /// <summary>
        /// Gets whether something is a Bottom Class i.e. has no sub classes
        /// </summary>
        public bool IsBottomClass
        {
            get
            {
                return !SubClasses.Any();
            }
        }

        /// <summary>
        /// Gets/Creates an Individual of this class
        /// </summary>
        /// <param name="resource">Resource identifying the individual</param>
        /// <returns></returns>
        public Individual CreateIndividual(Uri resource)
        {
            return new Individual(_graph.CreateUriNode(resource), _resource, _graph);
        }

        /// <summary>
        /// Gets whether this Class is equal to another Class
        /// </summary>
        /// <param name="obj">Object to test</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is OntologyClass)
            {
                OntologyClass other = (OntologyClass)obj;
                return other.Resource.Equals(_resource) && ReferenceEquals(other.Graph, _graph);
            }
            else
            {
                return false;
            }
        }
    }
}
