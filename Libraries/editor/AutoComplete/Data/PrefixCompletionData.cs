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

namespace VDS.RDF.Utilities.Editor.AutoComplete.Data
{
    /// <summary>
    /// Completion data for prefix declarations
    /// </summary>
    public abstract class BasePrefixDeclarationData
        : BaseCompletionData
    {
        /// <summary>
        /// Creates new completion data
        /// </summary>
        /// <param name="nsPrefix">Namespace Prefix</param>
        /// <param name="nsUri">Namespace URI</param>
        /// <param name="prefix">Prefix Text</param>
        /// <param name="postfix">Postfix Text</param>
        public BasePrefixDeclarationData(String nsPrefix, String nsUri, String prefix, String postfix)
            : base(prefix + nsPrefix + ": <" + nsUri + ">" + postfix, prefix + nsPrefix + ": <" + nsUri + ">" + postfix, "Inserts a prefix declaration for the " + nsPrefix + " prefix which has a namespace URI of " + nsUri) { }
    }

    /// <summary>
    /// Completion data for Turtle style prefix declarations
    /// </summary>
    public class TurtleStylePrefixDeclarationData
        : BasePrefixDeclarationData
    {
        /// <summary>
        /// Creates new completion data
        /// </summary>
        /// <param name="nsPrefix">Namespace Prefix</param>
        /// <param name="nsUri">Namespace URI</param>
        public TurtleStylePrefixDeclarationData(String nsPrefix, String nsUri)
            : base(nsPrefix, nsUri, "@prefix ", ".") { }
    }

    /// <summary>
    /// Completion data for SPARQL style prefix declarations
    /// </summary>
    public class SparqlStylePrefixDeclarationData 
        : BasePrefixDeclarationData
    {
        /// <summary>
        /// Creates new completion data
        /// </summary>
        /// <param name="nsPrefix">Namespace Prefix</param>
        /// <param name="nsUri">Namespace URI</param>
        public SparqlStylePrefixDeclarationData(String nsPrefix, String nsUri)
            : base(nsPrefix, nsUri, "PREFIX ", String.Empty) { }
    }
}
