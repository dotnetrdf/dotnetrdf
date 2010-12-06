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
    /// Represents a Graph with additional methods for extracting ontology based information from it
    /// </summary>
    /// <remarks>
    /// <para>
    /// See <a href="http://www.dotnetrdf.org/content.asp?pageID=Ontology%20API">Using the Ontology API</a> for some informal documentation on the use of the Ontology namespace
    /// </para>
    /// </remarks>
    public class OntologyGraph : Graph
    {
        /// <summary>
        /// Creates a new Ontology Graph
        /// </summary>
        public OntologyGraph() { }

        /// <summary>
        /// Gets/Creates an ontology resource in the Graph
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public virtual OntologyResource CreateOntologyResource(INode resource)
        {
            return new OntologyResource(resource, this);
        }

        /// <summary>
        /// Gets/Creates an ontology resource in the Graph
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public virtual OntologyResource CreateOntologyResource(Uri resource)
        {
            return this.CreateOntologyResource(this.CreateUriNode(resource));
        }

        /// <summary>
        /// Gets/Creates an anonymous ontology resource in the Graph
        /// </summary>
        /// <returns></returns>
        public virtual OntologyResource CreateOntologyResource()
        {
            return new OntologyResource(this.CreateBlankNode(), this);
        }

        /// <summary>
        /// Gets/Creates an ontology class in the Graph
        /// </summary>
        /// <param name="resource">Class Resource</param>
        /// <returns></returns>
        public virtual OntologyClass CreateOntologyClass(INode resource)
        {
            return new OntologyClass(resource, this);
        }

        /// <summary>
        /// Gets/Creates an ontology class in the Graph
        /// </summary>
        /// <param name="resource">Class Resource</param>
        /// <returns></returns>
        public virtual OntologyClass CreateOntologyClass(Uri resource)
        {
            return this.CreateOntologyClass(this.CreateUriNode(resource));
        }

        /// <summary>
        /// Gets/Creates an anonymous ontology class in the Graph
        /// </summary>
        /// <returns></returns>
        public virtual OntologyClass CreateOntologyClass()
        {
            return new OntologyClass(this.CreateBlankNode(), this);
        }

        /// <summary>
        /// Gets/Creates an ontology property in the Graph
        /// </summary>
        /// <param name="resource">Property Resource</param>
        /// <returns></returns>
        public virtual OntologyProperty CreateOntologyProperty(INode resource)
        {
            return new OntologyProperty(resource, this);
        }

        /// <summary>
        /// Gets/Creates an ontology property in the Graph
        /// </summary>
        /// <param name="resource">Property Resource</param>
        /// <returns></returns>
        public virtual OntologyProperty CreateOntologyProperty(Uri resource)
        {
            return this.CreateOntologyProperty(this.CreateUriNode(resource));
        }

        /// <summary>
        /// Gets an existing individual in the Graph
        /// </summary>
        /// <param name="resource">Individual Resource</param>
        /// <returns></returns>
        public virtual Individual CreateIndividual(INode resource)
        {
            return new Individual(resource, this);
        }

        /// <summary>
        /// Gets/Creates an individual in the Graph of the given class
        /// </summary>
        /// <param name="resource">Individual Resource</param>
        /// <param name="class">Class</param>
        /// <returns></returns>
        public virtual Individual CreateIndividual(INode resource, INode @class)
        {
            return new Individual(resource, @class, this);
        }

        /// <summary>
        /// Gets an existing individual in the Graph
        /// </summary>
        /// <param name="resource">Individual Resource</param>
        /// <returns></returns>
        public virtual Individual CreateIndividual(Uri resource)
        {
            return this.CreateIndividual(this.CreateUriNode(resource));
        }

        /// <summary>
        /// Gets/Creates an individual in the Graph of the given class
        /// </summary>
        /// <param name="resource">Individual Resource</param>
        /// <param name="class">Class</param>
        /// <returns></returns>
        public virtual Individual CreateIndividual(Uri resource, Uri @class)
        {
            return this.CreateIndividual(this.CreateUriNode(resource), this.CreateUriNode(@class));
        }
    }
}
