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
            IntialiseClasses();
            if (_classes.Count == 0) throw new RdfOntologyException("Cannot create an individual when the given resource has no types associated with it");
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
            AddResourceProperty(OntologyHelper.PropertyType, resourceClass, true);
            IntialiseClasses();
            if (_classes.Count == 0) throw new RdfOntologyException("Failed to create a new individual");
        }

        /// <summary>
        /// Helper method which finds all the Types given for this Resource
        /// </summary>
        private void IntialiseClasses() 
        {
            IUriNode rdfType = _graph.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyType));
            foreach (Triple t in _graph.GetTriplesWithSubjectPredicate(_resource, rdfType))
            {
                OntologyClass c = new OntologyClass(t.Object, _graph);
                _classes.Add(c);
            }
        }

        /// <summary>
        /// Gets all the Classes that this resource belongs to
        /// </summary>
        public IEnumerable<OntologyClass> Classes
        {
            get
            {
                return _classes;
            }
        }

        /// <summary>
        /// Gets whether the Individual belongs to a specific class
        /// </summary>
        /// <param name="class">Class</param>
        /// <returns></returns>
        public bool HasClass(OntologyClass @class)
        {
            return _classes.Any(c => c.Equals(@class));
        }

        /// <summary>
        /// Gets whether the Individual belongs to a class identified by the given resource
        /// </summary>
        /// <param name="class">Class</param>
        /// <returns></returns>
        public bool HasClass(INode @class)
        {
            return _classes.Any(c => c.Resource.Equals(@class));
        }

        /// <summary>
        /// Gets whether the Individual belongs to a class identified by the given URI
        /// </summary>
        /// <param name="class">Class URI</param>
        /// <returns></returns>
        public bool HasClass(Uri @class)
        {
            return HasClass(_graph.CreateUriNode(@class));
        }

    }
}
