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

namespace VDS.RDF.Query.Inference.Pellet
{
    /// <summary>
    /// Helper class provided constants and helper methods for use with Pellet Server
    /// </summary>
    public static class PelletHelper
    {
        /// <summary>
        /// Constants for Service Names for Services that may be provided by a Pellet Server
        /// </summary>
        public const String ServiceServerDescription = "ps-discovery",
                            ServiceKBDescription = "kb-discovery",
                            ServiceRealize = "realize",
                            ServiceNamespaces = "ns-prefix",
                            ServiceQuery = "query",
                            ServiceConsistency = "consistency",
                            ServiceExplainUnsat = "explain-unsat",
                            ServiceExplainInstance = "explain-instance",
                            ServiceClassify = "classify",
                            ServiceSearch = "search",
                            ServiceExplainSubclass = "explain-subclass",
                            ServiceExplainInconsistent = "explain-inconsistent",
                            ServiceExplain = "explain",
                            ServiceExplainProperty = "explain-property",
                            ServiceIntegrityConstraintValidation = "icv",
                            ServicePredict = "predict",
                            ServiceCluster = "cluster",
                            ServiceSimilarity = "similarity";
    }
}
