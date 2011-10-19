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
using System.Drawing;

namespace VDS.RDF.GUI
{
    /// <summary>
    /// Constants for use in GUI Applications
    /// </summary>
    public static class Constants
    {
        private static Icon _windowIcon = null;

        /// <summary>
        /// Filename Filter for RDF Graphs for Open/Save Dialogs
        /// </summary>
        [Obsolete("Use MimeTypesHelper.GetFilenameFilter(true,false,false,false,false,true) instead", true)]
        public const String RdfFilter = "All Supported RDF Formats|*.nt;*.ttl;*.n3;*.rdf;*.json;*.html;*.xhtml;*.htm|NTriples Files (*.nt)|*.nt|Turtle Files (*.ttl)|*.ttl|Notation 3 Files (*.n3)|*.n3|RDF/XML Files (*.rdf)|*.rdf|RDF/JSON Files (*.json)|*.json|RDFa Files|*.html,*.xhtml,*.htm|All Files|*.*";

        /// <summary>
        /// Filename Filter for RDF Datasets for Open/Save Dialogs
        /// </summary>
        [Obsolete("Use MimeTypesHelper.GetFilenameFilter(false,true,false,false,false,true) instead", true)]
        public const String RdfDatasetFilter = "All Supported RDF Dataset Formats|*.nq;*.trig;*.xml|NQuads Files (*.nq)|*.nq|TriG Files (*.trig)|*.trig|TriX Files (*.xml)|*.xml|All Files|*.*";

        /// <summary>
        /// Filname Filter for SPARQL Results for Open/Save Dialogs
        /// </summary>
        [Obsolete("Use MimeTypesHelper.GetFilenameFilter(false,false,true,false,false,true) instead", true)]
        public const String SparqlResultsFilter = "All Supported SPARQL Result Formats|*.srx;*.srj;*.json|SPARQL Results XML Files (*.srx)|*.srx|SPARQL Results JSON Files (*.srj,*.json)|*.srj,*.json|HTML Files (*.html,*.htm,*.xhtml)|*.html,*.xhtml,*.htm|All Files|*.*";

        /// <summary>
        /// Filename Filter for SPARQL Queries
        /// </summary>
        [Obsolete("Use MimeTypesHelper.GetFilenameFilter(false,false,false,true,false,true) instead", true)]
        public const String SparqlQueryFilter = "SPARQL Query Files|*.rq|All Files|*.*";

        /// <summary>
        /// Filename Filter for SPARQL Updates
        /// </summary>
        [Obsolete("Use MimeTypesHelper.GetFilenameFilter(false,false,false,false,true,true) instead", true)]
        public const String SparqlUpdateFilter = "SPARQL Update Files|*.ru|All Files|*.*";

        /// <summary>
        /// Filename Filter for Formats which can be used to serialise RDF and SPARQL Results but which are not standardised
        /// </summary>
        [Obsolete("Use MimeTypesHelper.GetFilenameFilter(true,true,false,false,false,true) instead", true)]
        public const String NonStandardFilter = "Comma Separated Values Files (*.csv)|*.csv|Tab Separated Values Files (*.tsv)|*.tsv";

        /// <summary>
        /// Filename Filter for RDF Graphs/Datasets for Open/Save Dialogs
        /// </summary>
        [Obsolete("Use MimeTypesHelper.GetFilenameFilter(true,true,false,false,false,true) instead", true)]
        public static String RdfOrDatasetFilter
        {
            get
            {
                return RdfFilter + "|" + RdfDatasetFilter;
            }
        }

        [Obsolete("Use MimeTypesHelper.GetFilenameFilter() instead", true)]
        public static String AllFilter
        {
            get
            {
                String temp = RdfFilter + "|" + RdfDatasetFilter + "|" + SparqlResultsFilter + "|" + SparqlQueryFilter + "|" + SparqlUpdateFilter;
                temp = temp.Replace("All Files|*.*|", "");
                return temp;
            }
        }

        public static Icon WindowIcon
        {
            get
            {
                return _windowIcon;
            }
            set
            {
                _windowIcon = value;
            }
        }
    }

    /// <summary>
    /// Comparer which compares objects based on the value of their ToString() method
    /// </summary>
    /// <typeparam name="T">Type to compare</typeparam>
    class ToStringComparer<T> : IComparer<T>
    {

        public int Compare(T x, T y)
        {
            if (x == null && y == null)
            {
                return 0;
            }
            else if (x == null)
            {
                return -1;
            }
            else if (y == null)
            {
                return 1;
            }
            else
            {
                return x.ToString().CompareTo(y.ToString());
            }
        }
    }

}
