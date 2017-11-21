/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
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

using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query.Spin.Model
{
    internal class AttributeImpl : AbstractAttributeImpl, IAttribute
    {

        public AttributeImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {
        }


        public new bool IsOptional()
        {
            return getMinCount() == 0;
        }


        public INode getDefaultValue()
        {
            return getObject(SPL.PropertyDefaultValue);
        }


        public int? getMaxCount()
        {
            int? value = (int?)getLong(SPL.PropertyMaxCount);
            if (value != null)
            {
                return value;
            }
            else
            {
                return null;
            }
        }


        public int getMinCount()
        {
            int? value = (int?)getLong(SPL.PropertyMaxCount);
            if (value != null)
            {
                return (int)value;
            }
            else
            {
                return 0;
            }
        }
    }
}