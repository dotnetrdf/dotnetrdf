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
    /// Static Helper class for the Ontology API
    /// </summary>
    /// <remarks>
    /// <para>
    /// See <a href="http://www.dotnetrdf.org/content.asp?pageID=Ontology%20API">Using the Ontology API</a> for some informal documentation on the use of the Ontology namespace
    /// </para>
    /// </remarks>
    public static class OntologyHelper
    {
        /// <summary>
        /// Constant URIs for properties exposed by <see cref="OntologyResource">OntologyResource</see> and its derived classes
        /// </summary>
        public const String PropertyVersionInfo = NamespaceMapper.OWL + "versionInfo",
                            PropertySameAs = NamespaceMapper.OWL + "sameAs",
                            PropertyDifferentFrom = NamespaceMapper.OWL + "differentFrom",
                            PropertyEquivalentClass = NamespaceMapper.OWL + "equivalentClass",
                            PropertyDisjointWith = NamespaceMapper.OWL + "disjointWith",
                            PropertyEquivalentProperty = NamespaceMapper.OWL + "equivalentProperty",
                            PropertyInverseOf = NamespaceMapper.OWL + "inverseOf",
                            PropertyBackwardCompatibleWith = NamespaceMapper.OWL + "backwardCompatibleWith",
                            PropertyIncompatibleWith = NamespaceMapper.OWL + "incompatibleWith",
                            PropertyPriorVersion = NamespaceMapper.OWL + "priorVersion",
                            PropertyImports = NamespaceMapper.OWL + "imports",

                            PropertyComment = NamespaceMapper.RDFS + "comment",
                            PropertyLabel = NamespaceMapper.RDFS + "label",
                            PropertySeeAlso = NamespaceMapper.RDFS + "seeAlso",
                            PropertyIsDefinedBy = NamespaceMapper.RDFS + "isDefinedBy",
                            PropertySubClassOf = NamespaceMapper.RDFS + "subClassOf",
                            PropertySubPropertyOf = NamespaceMapper.RDFS + "subPropertyOf",
                            PropertyRange = NamespaceMapper.RDFS + "range",
                            PropertyDomain = NamespaceMapper.RDFS + "domain",

                            PropertyType = NamespaceMapper.RDF + "type";

        /// <summary>
        /// Constants for URIs for classes in Ontologies
        /// </summary>
        public const String RdfsClass = NamespaceMapper.RDFS + "Class",
                            OwlClass = NamespaceMapper.OWL + "Class",
                            RdfsProperty = NamespaceMapper.RDF + "Property",
                            OwlObjectProperty = NamespaceMapper.OWL + "ObjectProperty",
                            OwlDataProperty = NamespaceMapper.OWL + "DataProperty",
                            OwlAnnotationProperty = NamespaceMapper.OWL + "AnnotationProperty",
                            OwlOntology = NamespaceMapper.OWL + "Ontology";

    }
}
