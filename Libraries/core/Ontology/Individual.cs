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
    /// Represents an Individual i.e. an instance of some class in an ontology
    /// </summary>
    /// <remarks>
    /// <para>
    /// See <a href="http://www.dotnetrdf.org/content.asp?pageID=Ontology%20API">Using the Ontology API</a> for some informal documentation on the use of the Ontology namespace
    /// </para>
    /// </remarks>
    public class Individual : OntologyResource
    {
        private List<OntologyClass> _classes = new List<OntologyClass>();

        /// <summary>
        /// Gets an Individual from the Graph
        /// </summary>
        /// <param name="resource">Resource that represents the Individual</param>
        /// <param name="graph">Graph the Individual is in</param>
        /// <remarks>
        /// Requires that an individual (a resource which is the subject of at least one triple where the predicate is <strong>rdf:type</strong>) is already present in the Graph
        /// </remarks>
        public Individual(INode resource, IGraph graph)
            : base(resource, graph) 
        {
            this.IntialiseClasses();
            if (this._classes.Count == 0) throw new RdfOntologyException("Cannot create an individual when the given resource has no types associated with it");
        }

        /// <summary>
        /// Gets/Creates an Individual from the Graph
        /// </summary>
        /// <param name="resource">Resource that represents the Individual</param>
        /// <param name="resourceClass">Class to create/add the Individual to</param>
        /// <param name="graph">Graph the Individual is in</param>
        /// <remarks>
        /// Allows for creating new Individuals in the Graph or adding existing resources to another Class.  If the resource for the Individual or the given Class are new then they will be added to the Graph
        /// </remarks>
        public Individual(INode resource, INode resourceClass, IGraph graph)
            : base(resource, graph)
        {
            this.AddResourceProperty(OntologyHelper.PropertyType, resourceClass, true);
            this.IntialiseClasses();
            if (this._classes.Count == 0) throw new RdfOntologyException("Failed to create a new individual");
        }

        /// <summary>
        /// Helper method which finds all the Types given for this Resource
        /// </summary>
        private void IntialiseClasses() 
        {
            IUriNode rdfType = this._graph.CreateUriNode(new Uri(OntologyHelper.PropertyType));
            foreach (Triple t in this._graph.GetTriplesWithSubjectPredicate(this._resource, rdfType))
            {
                OntologyClass c = new OntologyClass(t.Object, this._graph);
                this._classes.Add(c);
            }
        }

        /// <summary>
        /// Gets all the Classes that this resource belongs to
        /// </summary>
        public IEnumerable<OntologyClass> Classes
        {
            get
            {
                return this._classes;
            }
        }

        /// <summary>
        /// Gets whether the Individual belongs to a specific class
        /// </summary>
        /// <param name="class">Class</param>
        /// <returns></returns>
        public bool HasClass(OntologyClass @class)
        {
            return this._classes.Any(c => c.Equals(@class));
        }

        /// <summary>
        /// Gets whether the Individual belongs to a class identified by the given resource
        /// </summary>
        /// <param name="class">Class</param>
        /// <returns></returns>
        public bool HasClass(INode @class)
        {
            return this._classes.Any(c => c.Resource.Equals(@class));
        }

        /// <summary>
        /// Gets whether the Individual belongs to a class identified by the given URI
        /// </summary>
        /// <param name="class">Class URI</param>
        /// <returns></returns>
        public bool HasClass(Uri @class)
        {
            return this.HasClass(this._graph.CreateUriNode(@class));
        }

    }
}
