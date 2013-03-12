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
using System.Text;
using VDS.RDF.Parsing;

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
        public OntologyGraph() 
        {
            this.NamespaceMap.AddNamespace("owl", UriFactory.Create(NamespaceMapper.OWL));
        }

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

        /// <summary>
        /// Get all OWL classes defined in the graph
        /// </summary>
        public IEnumerable<OntologyClass> OwlClasses
        {
            get
            {
                return this.GetClasses(this.CreateUriNode(UriFactory.Create(OntologyHelper.OwlClass)));
            }
        }

        /// <summary>
        /// Get all the RDFS classes defined in the graph
        /// </summary>
        public IEnumerable<OntologyClass> RdfClasses
        {
            get
            {
                return this.GetClasses(this.CreateUriNode(UriFactory.Create(OntologyHelper.RdfsClass)));
            }
        }

        /// <summary>
        /// Gets all classes defined in the graph using the standard rdfs:Class and owl:Class types
        /// </summary>
        public IEnumerable<OntologyClass> AllClasses
        {
            get
            {
                return this.RdfClasses.Concat(this.OwlClasses);
            }
        }

        /// <summary>
        /// Get all classes defined in the graph where anything of a specific type is considered a class
        /// </summary>
        /// <param name="classType">Type which represents classes</param>
        /// <returns>Enumeration of classes</returns>
        public IEnumerable<OntologyClass> GetClasses(INode classType)
        {
            INode rdfType = this.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
            return (from t in this.GetTriplesWithPredicateObject(rdfType, classType)
                    select this.CreateOntologyClass(t.Subject));
        }

        /// <summary>
        /// Gets all RDF properties defined in the graph
        /// </summary>
        public IEnumerable<OntologyProperty> RdfProperties
        {
            get
            {
                return this.GetProperties(this.CreateUriNode(UriFactory.Create(OntologyHelper.RdfsProperty)));
            }
        }

        /// <summary>
        /// Gets all OWL Object properties defined in the graph
        /// </summary>
        public IEnumerable<OntologyProperty> OwlObjectProperties
        {
            get
            {
                return this.GetProperties(this.CreateUriNode(UriFactory.Create(OntologyHelper.OwlObjectProperty)));
            }
        }

        /// <summary>
        /// Gets all OWL Data properties defined in the graph
        /// </summary>
        public IEnumerable<OntologyProperty> OwlDatatypeProperties
        {
            get
            {
                return this.GetProperties(this.CreateUriNode(UriFactory.Create(OntologyHelper.OwlDataProperty)));
            }
        }

        /// <summary>
        /// Gets all OWL Annotation properties defined in the graph
        /// </summary>
        public IEnumerable<OntologyProperty> OwlAnnotationProperties
        {
            get
            {
                return this.GetProperties(this.CreateUriNode(UriFactory.Create(OntologyHelper.OwlAnnotationProperty)));
            }
        }

        /// <summary>
        /// Gets all properties defined in the graph using any of the standard OWL property types (owl:AnnotationProperty, owl:DataProperty, owl:ObjectProperty)
        /// </summary>
        public IEnumerable<OntologyProperty> OwlProperties
        {
            get
            {
                return this.OwlAnnotationProperties.Concat(this.OwlDatatypeProperties).Concat(this.OwlObjectProperties);
            }
        }

        /// <summary>
        /// Gets all properties defined in the graph using any of the standard property types (rdf:Property, owl:AnnotationProperty, owl:DataProperty, owl:ObjectProperty)
        /// </summary>
        public IEnumerable<OntologyProperty> AllProperties
        {
            get
            {
                return this.RdfProperties.Concat(this.OwlAnnotationProperties).Concat(this.OwlDatatypeProperties).Concat(this.OwlObjectProperties);
            }
        }

        /// <summary>
        /// Get all properties defined in the graph where anything of a specific type is considered a property
        /// </summary>
        /// <param name="propertyType">Type which represents properties</param>
        /// <returns>Enumeration of properties</returns>
        public IEnumerable<OntologyProperty> GetProperties(INode propertyType)
        {
            INode rdfType = this.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
            return (from t in this.GetTriplesWithPredicateObject(rdfType, propertyType)
                    select this.CreateOntologyProperty(t.Subject));
        }
    }
}
