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

namespace VDS.RDF.Query
{
    /// <summary>
    /// A Selector which finds all Triples containing the given Blank Node
    /// </summary>
    public class CommonBlankNodeSelector : ISelector<Triple>
    {

        private BlankNode _blank;

        /// <summary>
        /// Creates a new Common Blank Node Selector based on the given Blank Node
        /// </summary>
        /// <param name="blank">Blank Node to Select upon</param>
        public CommonBlankNodeSelector(BlankNode blank)
        {
            this._blank = blank;
        }

        /// <summary>
        /// Accepts all Triples which have the Blank Node as their Subject or Object
        /// </summary>
        /// <param name="obj">Triple to test</param>
        /// <returns></returns>
        public bool Accepts(Triple obj)
        {
            if (obj.HasSubject(this._blank) || obj.HasObject(this._blank))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// A Selector which finds all Triples not involving a Blank Node
    /// </summary>
    public class NonBlankSelector : ISelector<Triple>
    {

        /// <summary>
        /// Accepts all Triples which don't involve a Blank Node
        /// </summary>
        /// <param name="obj">Triple to Test</param>
        /// <returns></returns>
        public bool Accepts(Triple obj)
        {
            if (obj.Subject.NodeType != NodeType.Blank && obj.Predicate.NodeType != NodeType.Blank && obj.Object.NodeType != NodeType.Blank)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    /// <summary>
    /// A Selector which finds all Nodes which aren't Blank
    /// </summary>
    public class NonBlankNodeSelector : ISelector<INode>
    {

        /// <summary>
        /// Accepts all Nodes which aren't a Blank Node
        /// </summary>
        /// <param name="obj">Node to Test</param>
        /// <returns></returns>
        public bool Accepts(INode obj)
        {
            return (obj.NodeType != NodeType.Blank);
        }
    }

    /// <summary>
    /// A Selector which finds all Triples involving a Blank Node
    /// </summary>
    public class HasBlankSelector : ISelector<Triple>
    {
        /// <summary>
        /// Accepts Triples which have at least one Blank Node in them
        /// </summary>
        /// <param name="obj">Triple to test</param>
        /// <returns></returns>
        public bool Accepts(Triple obj)
        {
            return (obj.Subject.NodeType == NodeType.Blank || obj.Predicate.NodeType == NodeType.Blank || obj.Object.NodeType == NodeType.Blank);
        }
    }
}
