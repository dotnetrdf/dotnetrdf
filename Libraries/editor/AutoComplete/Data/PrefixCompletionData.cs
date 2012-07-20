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
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Editor.AutoComplete.Data
{
    public abstract class BasePrefixDeclarationData : BaseCompletionData
    {
        public BasePrefixDeclarationData(String nsPrefix, String nsUri, String prefix, String postfix)
            : base(prefix + nsPrefix + ": <" + nsUri + ">" + postfix, prefix + nsPrefix + ": <" + nsUri + ">" + postfix, "Inserts a prefix declaration for the " + nsPrefix + " prefix which has a namespace URI of " + nsUri) { }
    }

    public class TurtleStylePrefixDeclarationData : BasePrefixDeclarationData
    {
        public TurtleStylePrefixDeclarationData(String nsPrefix, String nsUri)
            : base(nsPrefix, nsUri, "@prefix ", ".") { }
    }

    public class SparqlStylePrefixDeclarationData : BasePrefixDeclarationData
    {
        public SparqlStylePrefixDeclarationData(String nsPrefix, String nsUri)
            : base(nsPrefix, nsUri, "PREFIX ", String.Empty) { }
    }
}
