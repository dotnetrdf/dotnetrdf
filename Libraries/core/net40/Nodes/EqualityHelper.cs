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

namespace VDS.RDF.Nodes
{
    /// <summary>
    /// Static Helper class containing standard implementations of Equality between various Node types
    /// </summary>
    public static class EqualityHelper
    {
        /// <summary>
        /// Determines whether two URIs are equal
        /// </summary>
        /// <param name="a">First URI</param>
        /// <param name="b">Second URI</param>
        /// <returns></returns>
        /// <remarks>
        /// Unlike the Equals method provided by the <see cref="Uri">Uri</see> class by default this takes into account Fragment IDs which are essential for checking URI equality in RDF
        /// </remarks>
        public static bool AreUrisEqual(Uri a, Uri b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null)
            {
                return b == null;
            }
            if (b == null)
            {
                return false;
            }

            if (a.IsAbsoluteUri && b.IsAbsoluteUri)
            {
                // Absolute URIs are equal if the Scheme, Host and Post are equivalent (case insensitive)
                // and the User Info (if any) is equivalent (case sensitive)
                // and the Path, Query and Fragment are equivalent (case sensitive)
                return a.Scheme.Equals(b.Scheme, StringComparison.OrdinalIgnoreCase)
                       && a.Host.Equals(b.Host, StringComparison.OrdinalIgnoreCase)
                       && a.Port.Equals(b.Port)
                       && a.UserInfo.Equals(b.UserInfo, StringComparison.Ordinal)
#if !SILVERLIGHT
                       && a.PathAndQuery.Equals(b.PathAndQuery, StringComparison.Ordinal)
#else
                   && a.PathAndQuery().Equals(b.PathAndQuery(), StringComparison.Ordinal)
#endif
                       && a.Fragment.Equals(b.Fragment, StringComparison.Ordinal);
            }
            if (!a.IsAbsoluteUri && !b.IsAbsoluteUri)
            {
                // Relative URIs are equal if the Path, Query and Fragment are equivalent (case sensitive)
#if !SILVERLIGHT
                return a.PathAndQuery.Equals(b.PathAndQuery, StringComparison.Ordinal)
#else
                return a.PathAndQuery().Equals(b.PathAndQuery(), StringComparison.Ordinal)
#endif
                       && a.Fragment.Equals(b.Fragment, StringComparison.Ordinal);
            }
            // An absolute URI can't be equal to a relative URI
            return false;
        }

        /// <summary>
        /// Determines whether two nodes are equal
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool AreNodesEqual(INode a, INode b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null))
            {
                return ReferenceEquals(b, null);
            }
            return !ReferenceEquals(b, null) && a.Equals(b);
        }

        /// <summary>
        /// Determines whether two URI nodes are equal
        /// </summary>
        /// <param name="a">First URI Node</param>
        /// <param name="b">Second URI Node</param>
        /// <returns></returns>
        public static bool AreUrisEqual(INode a, INode b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null)
            {
                return b == null;
            }
            return b != null && AreUrisEqual(a.Uri, b.Uri);
        }

        /// <summary>
        /// Determines whether two Literals are equal
        /// </summary>
        /// <param name="a">First Literal</param>
        /// <param name="b">Second Literal</param>
        /// <returns></returns>
        public static bool AreLiteralsEqual(INode a, INode b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null)
            {
                return b == null;
            }
            if (b == null)
            {
                return false;
            }

            // Language Tags must be equal (if present)
            if (a.HasLanguage || b.HasLanguage)
            {
                // If both don't have a language tag then they are not equal
                if (!(a.HasLanguage && b.HasLanguage)) return false;

                // Check Language Tags are equal, language tags are case insensitive
                return a.Language.Equals(b.Language, StringComparison.OrdinalIgnoreCase) && a.Value.Equals(b.Value, StringComparison.Ordinal);
            }

            // For plain literals the lexical value must be equal
            if (!a.HasDataType && !b.HasDataType) return a.Value.Equals(b.Value, StringComparison.Ordinal);

            // If both don't have same data type then they are not equal
            if (!AreUrisEqual(a.DataType, b.DataType)) return false;

            //We have equal DataTypes so use check if lexical values are equivalent
            if (Options.LiteralEqualityMode == LiteralEqualityMode.Strict)
            {
                //Strict Equality Mode uses Ordinal Lexical Comparison for Equality as per W3C RDF Spec
                return a.Value.Equals(b.Value, StringComparison.Ordinal);
            }
            //Loose Equality Mode uses Value Based Comparison for Equality of Typed Nodes
            return (a.CompareTo(b) == 0);
        }

        /// <summary>
        /// Determines whether two Blank Nodes are equal
        /// </summary>
        /// <param name="a">First Blank Node</param>
        /// <param name="b">Second Blank Node</param>
        /// <returns></returns>
        public static bool AreBlankNodesEqual(INode a, INode b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null)
            {
                return b == null;
            }
            return b != null && a.AnonID.Equals(b.AnonID);
        }

        /// <summary>
        /// Determines whether two Graph Literals are equal
        /// </summary>
        /// <param name="a">First Blank Node</param>
        /// <param name="b">Second Blank Node</param>
        /// <returns></returns>
        public static bool AreGraphLiteralsEqual(INode a, INode b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null)
            {
                return b == null;
            }
            return b != null && a.SubGraph.Equals(b.SubGraph);
        }

        /// <summary>
        /// Determines whether two Variable Nodes are equal
        /// </summary>
        /// <param name="a">First Variable Node</param>
        /// <param name="b">Second Variable Node</param>
        /// <returns></returns>
        public static bool AreVariablesEqual(INode a, INode b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null)
            {
                return b == null;
            }
            return b != null && a.VariableName.Equals(b.VariableName, StringComparison.Ordinal);
        }
    }
}