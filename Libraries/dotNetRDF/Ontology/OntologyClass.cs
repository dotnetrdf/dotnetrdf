/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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

            this.IntialiseProperty(OntologyHelper.PropertySubClassOf, false);
            this.IntialiseProperty(OntologyHelper.PropertyEquivalentClass, false);
            this.IntialiseProperty(OntologyHelper.PropertyDisjointWith, false);

            // Find derived classes
            IUriNode subClassOf = this._graph.CreateUriNode(UriFactory.Create(OntologyHelper.PropertySubClassOf));
            this._resourceProperties.Add(PropertyDerivedClass, new List<INode>());
            this._resourceProperties.Add(PropertyDirectSubClass, new List<INode>());
            foreach (Triple t in this._graph.GetTriplesWithPredicateObject(subClassOf, this._resource))
            {
                if (!this._resourceProperties[PropertyDerivedClass].Contains(t.Subject)) this._resourceProperties[PropertyDerivedClass].Add(t.Subject);
                if (!this._resourceProperties[PropertyDirectSubClass].Contains(t.Subject)) this._resourceProperties[PropertyDirectSubClass].Add(t.Subject);
            }
            int c = 0; 
            do
            {
                c = this._resourceProperties[PropertyDerivedClass].Count;
                foreach (INode n in this._resourceProperties[PropertyDerivedClass].ToList())
                {
                    foreach (Triple t in this._graph.GetTriplesWithPredicateObject(subClassOf, n))
                    {
                        if (!this._resourceProperties[PropertyDerivedClass].Contains(t.Subject)) this._resourceProperties[PropertyDerivedClass].Add(t.Subject);
                    }
                }
            } while (c < this._resourceProperties[PropertyDerivedClass].Count);

            // Find additional super classes
            this._resourceProperties.Add(PropertyDirectSuperClass, new List<INode>());
            if (this._resourceProperties.ContainsKey(OntologyHelper.PropertySubClassOf))
            {
                this._resourceProperties[PropertyDirectSuperClass].AddRange(this._resourceProperties[OntologyHelper.PropertySubClassOf]);

                do
                {
                    c = this._resourceProperties[OntologyHelper.PropertySubClassOf].Count;
                    foreach (INode n in this._resourceProperties[OntologyHelper.PropertySubClassOf].ToList())
                    {
                        foreach (Triple t in this._graph.GetTriplesWithSubjectPredicate(n, subClassOf))
                        {
                            if (!this._resourceProperties[OntologyHelper.PropertySubClassOf].Contains(t.Object)) this._resourceProperties[OntologyHelper.PropertySubClassOf].Add(t.Object);
                        }
                    }
                } while (c < this._resourceProperties[OntologyHelper.PropertySubClassOf].Count);
            }
        }

        /// <summary>
        /// Adds a new sub-class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddSubClass(INode resource)
        {
            return this.AddResourceProperty(PropertyDerivedClass, resource.CopyNode(this._graph), false);
        }

        /// <summary>
        /// Adds a new sub-class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddSubClass(Uri resource)
        {
            return this.AddSubClass(this._graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Adds a new sub-class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddSubClass(OntologyResource resource)
        {
            return this.AddSuperClass(resource.Resource);
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
            bool a = this.AddSubClass(@class.Resource);
            bool b = @class.AddSuperClass(this._resource);
            return (a || b);
        }

        /// <summary>
        /// Removes all sub-classes for this class
        /// </summary>
        /// <returns></returns>
        public bool ClearSubClasses()
        {
            this._graph.Retract(this._graph.GetTriplesWithPredicateObject(this._graph.CreateUriNode(UriFactory.Create(OntologyHelper.PropertySubClassOf)), this._resource).ToList());
            return this.ClearResourceProperty(PropertyDerivedClass, false);
        }

        /// <summary>
        /// Removes a sub-class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveSubClass(INode resource)
        {
            return this.RemoveResourceProperty(PropertyDerivedClass, resource.CopyNode(this._graph), false);
        }

        /// <summary>
        /// Removes a sub-class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveSubClass(Uri resource)
        {
            return this.RemoveSubClass(this._graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Removes a sub-class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveSubClass(OntologyResource resource)
        {
            return this.RemoveSubClass(resource.Resource);
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
            bool a = this.RemoveSubClass(@class.Resource);
            bool b = @class.RemoveSuperClass(this._resource);
            return (a || b);
        }

        /// <summary>
        /// Adds a new super-class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddSuperClass(INode resource)
        {
            return this.AddResourceProperty(OntologyHelper.PropertySubClassOf, resource.CopyNode(this._graph), true);
        }

        /// <summary>
        /// Adds a new super-class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddSuperClass(Uri resource)
        {
            return this.AddSuperClass(this._graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Adds a new super-class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddSuperClass(OntologyResource resource)
        {
            return this.AddSuperClass(resource.Resource);
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
            bool a = this.AddSuperClass(@class.Resource);
            bool b = @class.AddSubClass(this._resource);
            return (a || b);
        }

        /// <summary>
        /// Removes all super-classes
        /// </summary>
        /// <returns></returns>
        public bool ClearSuperClasses()
        {
            this._graph.Retract(this._graph.GetTriplesWithSubjectPredicate(this._resource, this._graph.CreateUriNode(UriFactory.Create(OntologyHelper.PropertySubClassOf))).ToList());
            return this.ClearResourceProperty(OntologyHelper.PropertySubClassOf, true);
        }

        /// <summary>
        /// Removes a super-class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveSuperClass(INode resource)
        {
            return this.RemoveResourceProperty(OntologyHelper.PropertySubClassOf, resource.CopyNode(this._graph), true);
        }

        /// <summary>
        /// Removes a super-class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveSuperClass(Uri resource)
        {
            return this.RemoveSuperClass(this._graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Removes a super-class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveSuperClass(OntologyResource resource)
        {
            return this.RemoveSuperClass(resource.Resource);
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
            bool a = this.RemoveSuperClass(@class.Resource);
            bool b = @class.RemoveSubClass(this._resource);
            return (a || b);
        }

        /// <summary>
        /// Adds an equivalent class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddEquivalentClass(INode resource)
        {
            return this.AddResourceProperty(OntologyHelper.PropertyEquivalentClass, resource.CopyNode(this._graph), true);
        }

        /// <summary>
        /// Adds an equivalent class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddEquivalentClass(Uri resource)
        {
            return this.AddEquivalentClass(this._graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Adds an equivalent class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddEquivalentClass(OntologyResource resource)
        {
            return this.AddEquivalentClass(resource.Resource);
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
            bool a = this.AddEquivalentClass(@class.Resource);
            bool b = @class.AddEquivalentClass(this._resource);
            return (a || b);
        }

        /// <summary>
        /// Removes all equivalent classes for this class
        /// </summary>
        /// <returns></returns>
        public bool ClearEquivalentClasses()
        {
            INode equivClass = this._graph.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyEquivalentClass));
            this._graph.Retract(this._graph.GetTriplesWithSubjectPredicate(this._resource, equivClass).ToList());
            this._graph.Retract(this._graph.GetTriplesWithPredicateObject(equivClass, this._resource).ToList());
            return this.ClearResourceProperty(OntologyHelper.PropertyEquivalentClass, true);
        }

        /// <summary>
        /// Removes an equivalent class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveEquivalentClass(INode resource)
        {
            return this.RemoveResourceProperty(OntologyHelper.PropertyEquivalentClass, resource.CopyNode(this._graph), true);
        }

        /// <summary>
        /// Removes an equivalent class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveEquivalentClass(Uri resource)
        {
            return this.RemoveEquivalentClass(this._graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Removes an equivalent class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveEquivalentClass(OntologyResource resource)
        {
            return this.RemoveEquivalentClass(resource.Resource);
        }

        /// <summary>
        /// Removes an equivalent class for this class
        /// </summary>
        /// <param name="class">Class</param>
        /// <returns></returns>
        public bool RemoveEquivalentClass(OntologyClass @class)
        {
            bool a = this.RemoveEquivalentClass(@class.Resource);
            bool b = @class.RemoveEquivalentClass(this._resource);
            return (a || b);
        }

        /// <summary>
        /// Adds a new disjoint class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddDisjointClass(INode resource)
        {
            return this.AddResourceProperty(OntologyHelper.PropertyDisjointWith, resource.CopyNode(this._graph), true);
        }

        /// <summary>
        /// Adds a new disjoint class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddDisjointClass(Uri resource)
        {
            return this.AddDisjointClass(this._graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Adds a new disjoint class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddDisjointClass(OntologyResource resource)
        {
            return this.AddDisjointClass(resource.Resource);
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
            bool a = this.AddDisjointClass(@class.Resource);
            bool b = @class.AddDisjointClass(this._resource);
            return (a || b);
        }

        /// <summary>
        /// Removes all disjoint classes for this class
        /// </summary>
        /// <returns></returns>
        public bool ClearDisjointClasses()
        {
            INode disjointClass = this._graph.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyDisjointWith));
            this._graph.Retract(this._graph.GetTriplesWithSubjectPredicate(this._resource, disjointClass).ToList());
            this._graph.Retract(this._graph.GetTriplesWithPredicateObject(disjointClass, this._resource).ToList());
            return this.ClearResourceProperty(OntologyHelper.PropertyDisjointWith, true);
        }

        /// <summary>
        /// Removes a disjoint class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveDisjointClass(INode resource)
        {
            return this.RemoveResourceProperty(OntologyHelper.PropertyDisjointWith, resource.CopyNode(this._graph), true);
        }

        /// <summary>
        /// Removes a disjoint class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveDisjointClass(Uri resource)
        {
            return this.RemoveDisjointClass(this._graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Removes a disjoint class for this class
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveDisjointClass(OntologyResource resource)
        {
            return this.RemoveDisjointClass(resource.Resource);
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
            bool a = this.RemoveDisjointClass(@class.Resource);
            bool b = @class.RemoveDisjointClass(this._resource);
            return (a || b);
        }

        /// <summary>
        /// Gets the sub-classes of this class (both direct and indirect)
        /// </summary>
        public IEnumerable<OntologyClass> SubClasses
        {
            get
            {
                return this.GetResourceProperty(PropertyDerivedClass).Select(c => new OntologyClass(c, this._graph));
            }
        }

        /// <summary>
        /// Gets the direct sub-classes of this class
        /// </summary>
        public IEnumerable<OntologyClass> DirectSubClasses
        {
            get
            {
                return this.GetResourceProperty(PropertyDirectSubClass).Select(c => new OntologyClass(c, this._graph));
            }
        }

        /// <summary>
        /// Gets the indirect sub-classes of this class
        /// </summary>
        public IEnumerable<OntologyClass> IndirectSubClasses
        {
            get
            {
                return (from c in this.GetResourceProperty(PropertyDerivedClass)
                        where !this.GetResourceProperty(PropertyDirectSubClass).Contains(c)
                        select new OntologyClass(c, this._graph));
            }
        }

        /// <summary>
        /// Gets the super-classes of this class (both direct and indirect)
        /// </summary>
        public IEnumerable<OntologyClass> SuperClasses
        {
            get
            {
                return this.GetResourceProperty(OntologyHelper.PropertySubClassOf).Select(c => new OntologyClass(c, this._graph));
            }
        }

        /// <summary>
        /// Gets the direct super-classes of this class
        /// </summary>
        public IEnumerable<OntologyClass> DirectSuperClasses
        {
            get
            {
                return this.GetResourceProperty(PropertyDirectSuperClass).Select(c => new OntologyClass(c, this._graph));
            }
        }

        /// <summary>
        /// Gets the indirect super-classes of this class
        /// </summary>
        public IEnumerable<OntologyClass> IndirectSuperClasses
        {
            get
            {
                return (from c in this.GetResourceProperty(OntologyHelper.PropertySubClassOf)
                        where !this.GetResourceProperty(PropertyDirectSuperClass).Contains(c)
                        select new OntologyClass(c, this._graph));
            }
        }

        /// <summary>
        /// Gets the Sibling classes of this class, if this class is the root of the ontology nothing is returned even if there are multiple root classes
        /// </summary>
        public IEnumerable<OntologyClass> Siblings
        {
            get
            {
                return this.GetResourceProperty(PropertyDirectSuperClass)
                       .Select(c => new OntologyClass(c, this._graph))
                       .SelectMany(c => c.DirectSubClasses)
                       .Where(c => !c.Resource.Equals(this._resource)).Distinct();
            }
        }

        /// <summary>
        /// Gets the equivalent classes of this class
        /// </summary>
        public IEnumerable<OntologyClass> EquivalentClasses
        {
            get
            {
                return this.GetResourceProperty(OntologyHelper.PropertyEquivalentClass).Select(c => new OntologyClass(c, this._graph));
            }
        }

        /// <summary>
        /// Gets the disjoint classes of this class
        /// </summary>
        public IEnumerable<OntologyClass> DisjointClasses
        {
            get
            {
                return this.GetResourceProperty(OntologyHelper.PropertyDisjointWith).Select(c => new OntologyClass(c, this._graph));
            }
        }

        /// <summary>
        /// Gets the instances (individuals) of this class
        /// </summary>
        public IEnumerable<OntologyResource> Instances
        {
            get
            {
                return (from t in this._graph.GetTriplesWithPredicateObject(this._graph.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyType)), this._resource)
                        select new OntologyResource(t.Subject, this._graph));
            }
        }

        /// <summary>
        /// Gets the properties which have this class as a domain
        /// </summary>
        public IEnumerable<OntologyProperty> IsDomainOf
        {
            get
            {
                INode domain = this._graph.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "domain"));
                return (from t in this._graph.GetTriplesWithPredicateObject(domain, this._resource)
                        select new OntologyProperty(t.Subject, this._graph));
            }
        }

        /// <summary>
        /// Gets the properties which have this class as a range
        /// </summary>
        public IEnumerable<OntologyProperty> IsRangeOf
        {
            get
            {
                INode range = this._graph.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "range"));
                return (from t in this._graph.GetTriplesWithPredicateObject(range, this._resource)
                        select new OntologyProperty(t.Subject, this._graph));
            }
        }

        /// <summary>
        /// Gets whether something is a Top Class i.e. has no super classes
        /// </summary>
        public bool IsTopClass
        {
            get
            {
                return !this.SuperClasses.Any();
            }
        }

        /// <summary>
        /// Gets whether something is a Bottom Class i.e. has no sub classes
        /// </summary>
        public bool IsBottomClass
        {
            get
            {
                return !this.SubClasses.Any();
            }
        }

        /// <summary>
        /// Gets/Creates an Individual of this class
        /// </summary>
        /// <param name="resource">Resource identifying the individual</param>
        /// <returns></returns>
        public Individual CreateIndividual(Uri resource)
        {
            return new Individual(this._graph.CreateUriNode(resource), this._resource, this._graph);
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
                return other.Resource.Equals(this._resource) && ReferenceEquals(other.Graph, this._graph);
            }
            else
            {
                return false;
            }
        }
    }
}
