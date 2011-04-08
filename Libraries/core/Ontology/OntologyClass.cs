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
    /// Class for representing a class in an Ontology
    /// </summary>
    /// <remarks>
    /// <para>
    /// See <a href="http://www.dotnetrdf.org/content.asp?pageID=Ontology%20API">Using the Ontology API</a> for some informal documentation on the use of the Ontology namespace
    /// </para>
    /// </remarks>
    public class OntologyClass : OntologyResource
    {
        private const String PropertyDerivedClass = "derivedClass";

        /// <summary>
        /// Creates a new representation of a Class in the given Ontology Mode
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <param name="graph">Graph</param>
        public OntologyClass(INode resource, IGraph graph)
            : base(resource, graph)
        {
            //Q: Assert that this resource is a Class?
            //UriNode rdfType = graph.CreateUriNode(new Uri(OntologyHelper.PropertyType));
            //graph.Assert(new Triple(resource, rdfType, graph.CreateUriNode(new Uri(OntologyHelper.RdfsClass))));

            this.IntialiseProperty(OntologyHelper.PropertySubClassOf, false);
            this.IntialiseProperty(OntologyHelper.PropertyEquivalentClass, false);
            this.IntialiseProperty(OntologyHelper.PropertyDisjointWith, false);

            //Find derived classes
            IUriNode subClassOf = this._graph.CreateUriNode(new Uri(OntologyHelper.PropertySubClassOf));
            this._resourceProperties.Add(PropertyDerivedClass, new List<INode>());
            foreach (Triple t in this._graph.GetTriplesWithPredicateObject(subClassOf, this._resource))
            {
                if (!this._resourceProperties[PropertyDerivedClass].Contains(t.Subject)) this._resourceProperties[PropertyDerivedClass].Add(t.Subject);
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

            //Find additional super classes
            if (this._resourceProperties.ContainsKey(OntologyHelper.PropertySubClassOf))
            {
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
            this._graph.Retract(this._graph.GetTriplesWithPredicateObject(this._graph.CreateUriNode(new Uri(OntologyHelper.PropertySubClassOf)), this._resource));
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
            this._graph.Retract(this._graph.GetTriplesWithSubjectPredicate(this._resource, this._graph.CreateUriNode(new Uri(OntologyHelper.PropertySubClassOf))));
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
            INode equivClass = this._graph.CreateUriNode(new Uri(OntologyHelper.PropertyEquivalentClass));
            this._graph.Retract(this._graph.GetTriplesWithSubjectPredicate(this._resource, equivClass));
            this._graph.Retract(this._graph.GetTriplesWithPredicateObject(equivClass, this._resource));
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
            INode disjointClass = this._graph.CreateUriNode(new Uri(OntologyHelper.PropertyDisjointWith));
            this._graph.Retract(this._graph.GetTriplesWithSubjectPredicate(this._resource, disjointClass));
            this._graph.Retract(this._graph.GetTriplesWithPredicateObject(disjointClass, this._resource));
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
                return (from t in this._graph.GetTriplesWithPredicateObject(this._graph.CreateUriNode(new Uri(OntologyHelper.PropertyType)), this._resource)
                        select new OntologyResource(t.Subject, this._graph));
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
