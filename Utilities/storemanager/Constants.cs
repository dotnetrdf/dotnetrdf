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

namespace dotNetRDFStore
{
    public static class Constants
    {
        /// <summary>
        /// Filename Filter for RDF Graphs for Open/Save Dialogs
        /// </summary>
        public const String RdfFilter = "NTriples Files (*.nt)|*.nt|Turtle Files (*.ttl)|*.ttl|Notation 3 Files (*.n3)|*.n3|RDF/XML Files (*.rdf)|*.rdf|RDF/JSON Files (*.json)|*.json|RDFa Files|*.html,*.xhtml,*.htm";
        /// <summary>
        /// Filename Filter for RDF Datasets for Open/Save Dialogs
        /// </summary>
        public const String RdfDatasetFilter = "NQuads Files (*.nq)|*.nq|TriG Files (*.trig)|*.trig|TriX Files (*.xml)|*.xml";

        public const String SparqlQueryFilter = "SPARQL Query Files|*.rq|All Files|*.*";

        public const String NonStandardFilter = "Comma Separated Values Files (*.csv)|*.csv|Tab Separated Values Files (*.tsv)|*.tsv";

        /// <summary>
        /// Filename Filter for RDF Graphs/Datasets for Open/Save Dialogs
        /// </summary>
        public static String RdfOrDatasetFilter
        {
            get
            {
                return RdfFilter + "|" + RdfDatasetFilter + "|All Files|*.*";
            }
        }
    }
}
