/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

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

using System.Collections.Generic;

namespace VDS.RDF.Nodes
{
    /// <summary>
    /// A Node Comparer which does faster comparisons since it only does lexical comparisons for literals rather than value comparisons
    /// </summary>
    public class FastNodeComparer
        : IComparer<INode>
    {
        /// <summary>
        /// Compares two Nodes
        /// </summary>
        /// <param name="x">Node</param>
        /// <param name="y">Node</param>
        /// <returns></returns>
        public int Compare(INode x, INode y)
        {
            if (ReferenceEquals(x, y)) return 0;

            if (x == null)
            {
                if (y == null)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            else if (y == null)
            {
                return 1;
            }

            if (x.NodeType == NodeType.Literal && y.NodeType == NodeType.Literal)
            {
                //Use faster comparison for literals - standard comparison is valued based 
                //and so gets slower as amount of nodes to compare gets larger

                //Sort order for literals is as follows
                //plain literals < language spec'd literals < typed literals
                //Within a category ordering is lexical on modifier, then lexical on lexical value
                if (x.HasLanguage)
                {
                    if (y.HasLanguage)
                    {
                        //Compare language specifiers
                        int c = x.Language.CompareTo(y.Language);
                        if (c == 0)
                        {
                            //Same language so compare lexical values
                            return x.Value.CompareTo(y.Value);
                        }
                        //Different language specifiers
                        return c;
                    }
                    else
                    {
                        //y in plain literal so x is greater then y
                        return 1;
                    }
                }
                else if (x.HasDataType)
                {
                    if (y.HasDataType)
                    {
                        //Compare datatypes
                        int c = ComparisonHelper.CompareUris(x.DataType, y.DataType);
                        if (c == 0)
                        {
                            //Same datatype so compare lexical values
                            return x.Value.CompareTo(y.Value);
                        }
                        //Different datatypes
                        return c;
                    }
                    else
                    {
                        //y is untyped literal so x is greater than y
                        return 1;
                    }
                }

                else
                {
                    //Plain literals so just compare lexical value
                    return x.Value.CompareTo(y.Value);
                }
            }
            else
            {
                //Non-literal nodes use their normal IComparable implementations
                return x.CompareTo(y);
            }
        }
    }
}