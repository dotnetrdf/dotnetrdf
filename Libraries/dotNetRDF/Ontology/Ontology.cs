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
            this._graph.Assert(new Triple(resource, this._graph.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyType)), this._graph.CreateUriNode(UriFactory.Create(OntologyHelper.OwlOntology))));

            this.IntialiseProperty(OntologyHelper.PropertyBackwardCompatibleWith, false);
            this.IntialiseProperty(OntologyHelper.PropertyIncompatibleWith, false);
            this.IntialiseProperty(OntologyHelper.PropertyPriorVersion, false);
            this.IntialiseProperty(OntologyHelper.PropertyImports, false);
        }

        /// <summary>
        /// Adds a new <em>owl:backwardsCompatibleWith</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddBackwardsCompatibleWith(INode resource)
        {
            return this.AddResourceProperty(OntologyHelper.PropertyBackwardCompatibleWith, resource.CopyNode(this._graph), true);
        }

        /// <summary>
        /// Adds a new <em>owl:backwardsCompatibleWith</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddBackwardsCompatibleWith(Uri resource)
        {
            return this.AddBackwardsCompatibleWith(this._graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Adds a new <em>owl:backwardsCompatibleWith</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddBackwardsCompatibleWith(OntologyResource resource)
        {
            return this.AddBackwardsCompatibleWith(resource.Resource);
        }

        /// <summary>
        /// Removes all <em>owl:backwardsCompatibleWith</em> triples for this Ontology
        /// </summary>
        /// <returns></returns>
        public bool ClearBackwardsCompatibleWith()
        {
            return this.ClearResourceProperty(OntologyHelper.PropertyBackwardCompatibleWith, true);
        }

        /// <summary>
        /// Removes a <em>owl:backwardsCompatibleWith</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveBackwardsCompatibleWith(INode resource)
        {
            return this.RemoveResourceProperty(OntologyHelper.PropertyBackwardCompatibleWith, resource.CopyNode(this._graph), true);
        }

        /// <summary>
        /// Removes a <em>owl:backwardsCompatibleWith</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveBackwardsCompatibleWith(Uri resource)
        {
            return this.RemoveBackwardsCompatibleWith(this._graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Removes a <em>owl:backwardsCompatibleWith</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveBackwardsCompatibleWith(OntologyResource resource)
        {
            return this.RemoveBackwardsCompatibleWith(resource.Resource);
        }

        /// <summary>
        /// Adds a new <em>owl:incompatibleWith</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddIncompatibleWith(INode resource)
        {
            return this.AddResourceProperty(OntologyHelper.PropertyIncompatibleWith, resource.CopyNode(this._graph), true);
        }

        /// <summary>
        /// Adds a new <em>owl:incompatibleWith</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddIncompatibleWith(Uri resource)
        {
            return this.AddIncompatibleWith(this._graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Adds a new <em>owl:incompatibleWith</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddIncompatibleWith(OntologyResource resource)
        {
            return this.AddIncompatibleWith(resource.Resource);
        }

        /// <summary>
        /// Removes all <em>owl:incompatibleWith</em> triples for this Ontology
        /// </summary>
        /// <returns></returns>
        public bool ClearIncompatibleWith()
        {
            return this.ClearResourceProperty(OntologyHelper.PropertyIncompatibleWith, true);
        }

        /// <summary>
        /// Removes a <em>owl:incompatibleWith</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveIncompatibleWith(INode resource)
        {
            return this.RemoveResourceProperty(OntologyHelper.PropertyIncompatibleWith, resource.CopyNode(this._graph), true);
        }

        /// <summary>
        /// Removes a <em>owl:incompatibleWith</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveIncompatibleWith(Uri resource)
        {
            return this.RemoveIncompatibleWith(this._graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Removes a <em>owl:incompatibleWith</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveIncompatibleWith(OntologyResource resource)
        {
            return this.RemoveIncompatibleWith(resource.Resource);
        }

        /// <summary>
        /// Adds a new <em>owl:imports</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddImports(INode resource)
        {
            return this.AddResourceProperty(OntologyHelper.PropertyImports, resource.CopyNode(this._graph), true);
        }

        /// <summary>
        /// Adds a new <em>owl:imports</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddImports(Uri resource)
        {
            return this.AddImports(this._graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Adds a new <em>owl:imports</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddImports(OntologyResource resource)
        {
            return this.AddImports(resource.Resource);
        }

        /// <summary>
        /// Removes all <em>owl:imports</em> triples for this Ontology
        /// </summary>
        /// <returns></returns>
        public bool ClearImports()
        {
            return this.ClearResourceProperty(OntologyHelper.PropertyImports, true);
        }

        /// <summary>
        /// Removes a <em>owl:imports</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveImports(INode resource)
        {
            return this.RemoveResourceProperty(OntologyHelper.PropertyImports, resource.CopyNode(this._graph), true);
        }

        /// <summary>
        /// Removes a <em>owl:imports</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveImports(Uri resource)
        {
            return this.RemoveImports(this._graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Removes a <em>owl:imports</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemoveImports(OntologyResource resource)
        {
            return this.RemoveImports(resource.Resource);
        }

        /// <summary>
        /// Adds a new <em>owl:priorVersion</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddPriorVersion(INode resource)
        {
            return this.AddResourceProperty(OntologyHelper.PropertyPriorVersion, resource.CopyNode(this._graph), true);
        }

        /// <summary>
        /// Adds a new <em>owl:priorVersion</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddPriorVersion(Uri resource)
        {
            return this.AddPriorVersion(this._graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Adds a new <em>owl:priorVersion</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool AddPriorVersion(OntologyResource resource)
        {
            return this.AddPriorVersion(resource.Resource);
        }

        /// <summary>
        /// Removes all <em>owl:priorVersion</em> triples for this Ontology
        /// </summary>
        /// <returns></returns>
        public bool ClearPriorVersions()
        {
            return this.ClearResourceProperty(OntologyHelper.PropertyPriorVersion, true);
        }

        /// <summary>
        /// Removes a <em>owl:priorVersion</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemovePriorVersion(INode resource)
        {
            return this.RemoveResourceProperty(OntologyHelper.PropertyPriorVersion, resource.CopyNode(this._graph), true);
        }

        /// <summary>
        /// Removes a <em>owl:priorVersion</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemovePriorVersion(Uri resource)
        {
            return this.RemovePriorVersion(this._graph.CreateUriNode(resource));
        }

        /// <summary>
        /// Removes a <em>owl:priorVersion</em> triple for this Ontology
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool RemovePriorVersion(OntologyResource resource)
        {
            return this.RemovePriorVersion(resource.Resource);
        }

        /// <summary>
        /// Gets all the Ontologies that this Ontology is backwards compatible with
        /// </summary>
        public IEnumerable<Ontology> BackwardsCompatibleWith
        {
            get
            {
                return this.GetResourceProperty(OntologyHelper.PropertyBackwardCompatibleWith).Select(r => new Ontology(r, this._graph));
            }
        }

        /// <summary>
        /// Gets all the Ontologies that this Ontology is incompatible with
        /// </summary>
        public IEnumerable<Ontology> IncompatibleWith
        {
            get
            {
                return this.GetResourceProperty(OntologyHelper.PropertyIncompatibleWith).Select(r => new Ontology(r, this._graph));
            }
        }

        /// <summary>
        /// Gets all the Ontologies that this Ontology imports
        /// </summary>
        public IEnumerable<Ontology> Imports
        {
            get
            {
                return this.GetResourceProperty(OntologyHelper.PropertyImports).Select(r => new Ontology(r, this._graph));
            }
        }

        /// <summary>
        /// Gets all the Ontologies that are prior versions of this Ontology
        /// </summary>
        public IEnumerable<Ontology> PriorVersions
        {
            get
            {
                return this.GetResourceProperty(OntologyHelper.PropertyPriorVersion).Select(r => new Ontology(r, this._graph));
            }
        }
    }
}
