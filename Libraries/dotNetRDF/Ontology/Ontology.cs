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
    /// Represents the meta-information about an Ontology
    /// </summary>
    /// <remarks>
    /// <para>
    /// See <a href="http://www.dotnetrdf.org/content.asp?pageID=Ontology%20API">Using the Ontology API</a> for some informal documentation on the use of the Ontology namespace
    /// </para>
    /// </remarks>
    public class Ontology : OntologyResource
    {
        /// <summary>
        /// Creates a new Ontology for the given resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <param name="graph">Graph</param>
        public Ontology(INode resource, IGraph graph)
            : base(resource, graph)
        {
            // Assert the rdf:type owl:Ontology triple
            _graph.Assert(new Triple(resource, _graph.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyType)), _graph.CreateUriNode(UriFactory.Create(OntologyHelper.OwlOntology))));

            IntialiseProperty(OntologyHelper.PropertyBackwardCompatibleWith, false);
            IntialiseProperty(OntologyHelper.PropertyIncompatibleWith, false);
            IntialiseProperty(OntologyHelper.PropertyPriorVersion, false);
            IntialiseProperty(OntologyHelper.PropertyImports, false);
        }

        /// <summary>
        /// Adds a new <em>owl:backwardsCompatibleWith</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddBackwardsCompatibleWith(INode resource)
        {
            return AddResourceProperty(OntologyHelper.PropertyBackwardCompatibleWith, resource.CopyNode(_graph), true);
        }

        /// <summary>
        /// Adds a new <em>owl:backwardsCompatibleWith</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddBackwardsCompatibleWith(Uri resource)
        {
            return AddBackwardsCompatibleWith(_graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Adds a new <em>owl:backwardsCompatibleWith</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddBackwardsCompatibleWith(OntologyResource resource)
        {
            return AddBackwardsCompatibleWith(resource.Resource);
        }

        /// <summary>
        /// Removes all <em>owl:backwardsCompatibleWith</em> triples for this Ontology
        /// </summary>
        /// <returns></returns>
        public bool ClearBackwardsCompatibleWith()
        {
            return ClearResourceProperty(OntologyHelper.PropertyBackwardCompatibleWith, true);
        }

        /// <summary>
        /// Removes a <em>owl:backwardsCompatibleWith</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveBackwardsCompatibleWith(INode resource)
        {
            return RemoveResourceProperty(OntologyHelper.PropertyBackwardCompatibleWith, resource.CopyNode(_graph), true);
        }

        /// <summary>
        /// Removes a <em>owl:backwardsCompatibleWith</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveBackwardsCompatibleWith(Uri resource)
        {
            return RemoveBackwardsCompatibleWith(_graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Removes a <em>owl:backwardsCompatibleWith</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveBackwardsCompatibleWith(OntologyResource resource)
        {
            return RemoveBackwardsCompatibleWith(resource.Resource);
        }

        /// <summary>
        /// Adds a new <em>owl:incompatibleWith</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddIncompatibleWith(INode resource)
        {
            return AddResourceProperty(OntologyHelper.PropertyIncompatibleWith, resource.CopyNode(_graph), true);
        }

        /// <summary>
        /// Adds a new <em>owl:incompatibleWith</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddIncompatibleWith(Uri resource)
        {
            return AddIncompatibleWith(_graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Adds a new <em>owl:incompatibleWith</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddIncompatibleWith(OntologyResource resource)
        {
            return AddIncompatibleWith(resource.Resource);
        }

        /// <summary>
        /// Removes all <em>owl:incompatibleWith</em> triples for this Ontology
        /// </summary>
        /// <returns></returns>
        public bool ClearIncompatibleWith()
        {
            return ClearResourceProperty(OntologyHelper.PropertyIncompatibleWith, true);
        }

        /// <summary>
        /// Removes a <em>owl:incompatibleWith</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveIncompatibleWith(INode resource)
        {
            return RemoveResourceProperty(OntologyHelper.PropertyIncompatibleWith, resource.CopyNode(_graph), true);
        }

        /// <summary>
        /// Removes a <em>owl:incompatibleWith</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveIncompatibleWith(Uri resource)
        {
            return RemoveIncompatibleWith(_graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Removes a <em>owl:incompatibleWith</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveIncompatibleWith(OntologyResource resource)
        {
            return RemoveIncompatibleWith(resource.Resource);
        }

        /// <summary>
        /// Adds a new <em>owl:imports</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddImports(INode resource)
        {
            return AddResourceProperty(OntologyHelper.PropertyImports, resource.CopyNode(_graph), true);
        }

        /// <summary>
        /// Adds a new <em>owl:imports</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddImports(Uri resource)
        {
            return AddImports(_graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Adds a new <em>owl:imports</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddImports(OntologyResource resource)
        {
            return AddImports(resource.Resource);
        }

        /// <summary>
        /// Removes all <em>owl:imports</em> triples for this Ontology
        /// </summary>
        /// <returns></returns>
        public bool ClearImports()
        {
            return ClearResourceProperty(OntologyHelper.PropertyImports, true);
        }

        /// <summary>
        /// Removes a <em>owl:imports</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveImports(INode resource)
        {
            return RemoveResourceProperty(OntologyHelper.PropertyImports, resource.CopyNode(_graph), true);
        }

        /// <summary>
        /// Removes a <em>owl:imports</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveImports(Uri resource)
        {
            return RemoveImports(_graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Removes a <em>owl:imports</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveImports(OntologyResource resource)
        {
            return RemoveImports(resource.Resource);
        }

        /// <summary>
        /// Adds a new <em>owl:priorVersion</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddPriorVersion(INode resource)
        {
            return AddResourceProperty(OntologyHelper.PropertyPriorVersion, resource.CopyNode(_graph), true);
        }

        /// <summary>
        /// Adds a new <em>owl:priorVersion</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddPriorVersion(Uri resource)
        {
            return AddPriorVersion(_graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Adds a new <em>owl:priorVersion</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddPriorVersion(OntologyResource resource)
        {
            return AddPriorVersion(resource.Resource);
        }

        /// <summary>
        /// Removes all <em>owl:priorVersion</em> triples for this Ontology
        /// </summary>
        /// <returns></returns>
        public bool ClearPriorVersions()
        {
            return ClearResourceProperty(OntologyHelper.PropertyPriorVersion, true);
        }

        /// <summary>
        /// Removes a <em>owl:priorVersion</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemovePriorVersion(INode resource)
        {
            return RemoveResourceProperty(OntologyHelper.PropertyPriorVersion, resource.CopyNode(_graph), true);
        }

        /// <summary>
        /// Removes a <em>owl:priorVersion</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemovePriorVersion(Uri resource)
        {
            return RemovePriorVersion(_graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Removes a <em>owl:priorVersion</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemovePriorVersion(OntologyResource resource)
        {
            return RemovePriorVersion(resource.Resource);
        }

        /// <summary>
        /// Gets all the Ontologies that this Ontology is backwards compatible with
        /// </summary>
        public IEnumerable<Ontology> BackwardsCompatibleWith
        {
            get
            {
                return GetResourceProperty(OntologyHelper.PropertyBackwardCompatibleWith).Select(r => new Ontology(r, _graph));
            }
        }

        /// <summary>
        /// Gets all the Ontologies that this Ontology is incompatible with
        /// </summary>
        public IEnumerable<Ontology> IncompatibleWith
        {
            get
            {
                return GetResourceProperty(OntologyHelper.PropertyIncompatibleWith).Select(r => new Ontology(r, _graph));
            }
        }

        /// <summary>
        /// Gets all the Ontologies that this Ontology imports
        /// </summary>
        public IEnumerable<Ontology> Imports
        {
            get
            {
                return GetResourceProperty(OntologyHelper.PropertyImports).Select(r => new Ontology(r, _graph));
            }
        }

        /// <summary>
        /// Gets all the Ontologies that are prior versions of this Ontology
        /// </summary>
        public IEnumerable<Ontology> PriorVersions
        {
            get
            {
                return GetResourceProperty(OntologyHelper.PropertyPriorVersion).Select(r => new Ontology(r, _graph));
            }
        }
    }
}
