/*

dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using VDS.RDF.Query.Spin.Utility;

namespace VDS.RDF.Query.Spin.Core.Runtime
{
    public static class RuntimeHelper
    {
        /// <summary>
        /// The base namespace prefix for temporary resources and vocabulary
        /// </summary>
        public const String BASE_URI = "tmp:dotnetrdf.org";

        /// <summary>
        /// The Uri prefix for temporary resources
        /// </summary>
        public const String NS_URI = BASE_URI + ":";

        public static readonly IUriNode
                ClassTemporaryGraph = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "TemporaryGraph")),
                PropertyRequiredBy = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "requiredBy")),
                PropertyLastAccess = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "lastAccess")),
                PropertyStartedAt = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "startedAt"));

        public static readonly IUriNode BLACKHOLE = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "NULL"));
    }
}