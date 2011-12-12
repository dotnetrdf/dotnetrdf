/*

Copyright Robert Vesse 2009-11
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

If this license is not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.Common;

namespace VDS.RDF
{
    /// <summary>
    /// A static helper class for interning URIs to reduce memory usage
    /// </summary>
    public static class UriFactory
    {
        private static StringTrie<Uri> _uris = new StringTrie<Uri>();

        /// <summary>
        /// Creates a URI interning it if interning is enabled via the <see cref="Options.InternUris">Options.InternUris</see>
        /// </summary>
        /// <param name="uri">String URI</param>
        /// <returns></returns>
        /// <remarks>
        /// When URI interning is disabled this is equivalent to just invoking the constructor of the <see cref="Uri">Uri</see> class
        /// </remarks>
        public static Uri Create(String uri)
        {
            if (Options.InternUris)
            {
                TrieNode<char, Uri> node = _uris.MoveToNode(uri);
                if (node.HasValue)
                {
                    return node.Value;
                }
                else
                {
                    Uri u = new Uri(uri);
                    node.Value = u;
                    return node.Value;
                }
            }
            else
            {
                return new Uri(uri);
            }
        }
    }
}
