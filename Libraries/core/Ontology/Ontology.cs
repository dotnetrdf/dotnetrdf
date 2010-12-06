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
            //Assert the rdf:type owl:Ontology triple
            this._graph.Assert(new Triple(resource, this._graph.CreateUriNode(new Uri(OntologyHelper.PropertyType)), this._graph.CreateUriNode(new Uri(OntologyHelper.OwlOntology))));

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
