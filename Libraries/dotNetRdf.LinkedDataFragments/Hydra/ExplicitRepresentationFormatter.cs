/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2023 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.Globalization;
using System.Text;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.LDF.Hydra
{
    internal class ExplicitRepresentationFormatter : INodeFormatter
    {
        string INodeFormatter.Format(INode n)
        {
            switch (n)
            {
                case IUriNode uriNode:
                    return uriNode.Uri.AbsoluteUri;

                case ILiteralNode literalNode:
                    var builder = new StringBuilder();
                    builder.AppendFormat(CultureInfo.InvariantCulture, "\"{0}\"", literalNode.Value);

                    if (literalNode.DataType is not null)
                    {
                        builder.AppendFormat(CultureInfo.InvariantCulture, "^^{0}", literalNode.DataType.AbsoluteUri);
                    }

                    if (!string.IsNullOrEmpty(literalNode.Language))
                    {
                        builder.AppendFormat(CultureInfo.InvariantCulture, "@{0}", literalNode.Language);
                    }

                    return builder.ToString();

                default:
                    throw new NotSupportedException("Only IRI and literal nodes are supported.");
            }
        }

        string INodeFormatter.Format(INode n, TripleSegment? segment)
        {
            return ((INodeFormatter)this).Format(n);
        }
    }
}
