/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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

namespace VDS.Common
{
    /// <summary>
    /// Represents the classic use case of a Trie data structure, keys are strings with a character stored at each Node
    /// </summary>
    /// <typeparam name="T">Type of values to be stored</typeparam>
    public class StringTrie<T>
        : Trie<String, char, T>
        where T : class
    {
        /// <summary>
        /// Creates a new String Trie
        /// </summary>
        public StringTrie()
            : base(StringTrie<T>.KeyMapper) { }

        /// <summary>
        /// Key Mapper function for String Trie
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Array of characters</returns>
        public static IEnumerable<char> KeyMapper(String key)
        {
            return key.ToCharArray();
        }
    }
}
