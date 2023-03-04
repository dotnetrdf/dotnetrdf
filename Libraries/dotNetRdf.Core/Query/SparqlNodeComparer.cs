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
using System.Collections.Generic;
using System.Globalization;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query
{
    /// <summary>
    /// Interface for a comparer that can compare INode and IValuedNode instances.
    /// </summary>
    public interface ISparqlNodeComparer
    {
        /// <summary>
        /// Get the culture to use for string literal comparison.
        /// </summary>
        CultureInfo Culture { get; }
        /// <summary>
        /// Get the options to apply to string literal comparison.
        /// </summary>
        CompareOptions Options { get; }

        
        public bool TryCompare(INode x, INode y, out int result);
        public bool TryCompare(IValuedNode x, IValuedNode y, out int result);
    }

    /// <summary>
    /// Comparer class for implementing the SPARQL semantics for the relational operators.
    /// </summary>
    public class SparqlNodeComparer 
        : ISparqlNodeComparer
    {
        /// <summary>
        /// Get the culture to use for string literal comparison.
        /// </summary>
        public CultureInfo Culture { get; }

        /// <summary>
        /// Get the options to apply to string literal comparison.
        /// </summary>
        public CompareOptions Options { get; }

        public bool TryCompare(INode x, INode y, out int result)
        {
            result = 0;
            // If either node is null, there is no valid sort order
            if (x == null || y == null)
            {
                return false;
            }

            // If the Node Types are different, there is no valid sort order
            if (x.NodeType != y.NodeType)
            {
                return false;
            }

            switch (x.NodeType)
            {
                case NodeType.Blank or NodeType.Uri or NodeType.Variable:
                    // No comparison for blank nodes, URI nodes or variable nodes
                    return TryCheckEquality(x, y, out result);
                case NodeType.Literal:
                    {
                        // Do they have supported Data Types?
                        string xtype, ytype;
                        try
                        {
                            xtype = XmlSpecsHelper.GetSupportedDataType(x);
                            ytype = XmlSpecsHelper.GetSupportedDataType(y);
                        }
                        catch (RdfException)
                        {
                            // Can't determine a Data Type for one/both of the Nodes
                            return false;
                        }

                        if (xtype.Equals(string.Empty) || ytype.Equals(string.Empty))
                        {
                            // One/both has an unknown type
                            return TryCheckEquality(x, y, out result);
                        }

                        // Both have known types
                        if (SparqlSpecsHelper.IsStringDatatype(xtype) && SparqlSpecsHelper.IsStringDatatype(ytype))
                        {
                            // Compare on literal value
                            result = Culture.CompareInfo.Compare(((ILiteralNode)x).Value, ((ILiteralNode)y).Value,
                                Options);
                            return true;
                        }

                        if (SparqlSpecsHelper.IsStringDatatype(ytype) || SparqlSpecsHelper.IsStringDatatype(ytype))
                        {
                            // Only one of the two nodes is a string literal
                            return false;
                        }

                        SparqlNumericType xnumtype = NumericTypesHelper.GetNumericTypeFromDataTypeUri(xtype);
                        SparqlNumericType ynumtype = NumericTypesHelper.GetNumericTypeFromDataTypeUri(ytype);
                        var numtype = (SparqlNumericType)Math.Max((int)xnumtype, (int)ynumtype);
                        if (numtype != SparqlNumericType.NaN)
                        {
                            if (xnumtype == SparqlNumericType.NaN || ynumtype == SparqlNumericType.NaN)
                            {
                                // If one is non-numeric then we can't assume non-equality
                                return false;
                            }

                            // Both are Numeric so use Numeric ordering
                            try
                            {
                                result = NumericCompare(x, y, numtype);
                                return true;
                            }
                            catch (FormatException)
                            {
                                return TryCheckEquality(x, y, out result);
                            }
                            catch (RdfQueryException)
                            {
                                return TryCheckEquality(x, y, out result);
                            }
                        }

                        if (xtype.Equals(ytype))
                        {
                            switch (xtype)
                            {
                                case XmlSpecsHelper.XmlSchemaDataTypeDate:
                                    result = DateCompare(x, y);
                                    return true;

                                case XmlSpecsHelper.XmlSchemaDataTypeDateTime:
                                    result = DateTimeCompare(x, y);
                                    return true;

                                case XmlSpecsHelper.XmlSchemaDataTypeBoolean:
                                    result = BooleanCompare(x, y);
                                    return true;

                                case XmlSpecsHelper.XmlSchemaDataTypeString:
                                    // Both Strings so use Lexical string ordering
                                    result = string.Compare(((ILiteralNode)x).Value, ((ILiteralNode)y).Value, Culture,
                                        Options);
                                    return true;
                                default:
                                    // No ordering defined for other types
                                    return false;
                            }
                        }

                        var commontype = XmlSpecsHelper.GetCompatibleSupportedDataType(xtype, ytype, true);
                        if (commontype.Equals(string.Empty))
                        {
                            // No common type
                            return false;
                        }

                        switch (commontype)
                        {
                            case XmlSpecsHelper.XmlSchemaDataTypeDate:
                                result = DateCompare(x, y);
                                return true;
                            case XmlSpecsHelper.XmlSchemaDataTypeDateTime:
                                result = DateTimeCompare(x, y);
                                return true;
                            default:
                                // No ordering defined
                                return false;

                        }
                    }

                case NodeType.Triple:
                    // Comparing two triple nodes by comparing pairwise subject, predicate then object.
                    if (x is ITripleNode xt && y is ITripleNode yt)
                    {
                        return TryCompareTripleNodes(xt, yt, out result);
                    }
                    return false;

                default:
                    return false;
            }

        }

        private static bool TryCheckEquality(INode x, INode y, out int result)
        {
            result = 0;
            return x.Equals(y);
        }

        private bool TryCompareTripleNodes(ITripleNode x, ITripleNode y, out int result)
        {
            if (!TryCompare(x.Triple.Subject, y.Triple.Subject, out result)) return false;
            if (result != 0) return true;

            if (!TryCompare(x.Triple.Predicate, y.Triple.Predicate, out result)) return false;
            return result != 0 || TryCompare(x.Triple.Object, y.Triple.Object, out result);
        }

        public bool TryCompare(IValuedNode x, IValuedNode y, out int result)
        {
            result = 0;

            // If either node is null, no ordering is defined
            if (x == null || y == null) return false;

            // If the Node Types are different no ordering is defined
            if (x.NodeType != y.NodeType) return false;
            switch (x.NodeType)
            {
                case NodeType.Literal:


                    // Do they have supported Data Types?
                    string xtype, ytype;
                    try
                    {
                        xtype = XmlSpecsHelper.GetSupportedDataType(x);
                        ytype = XmlSpecsHelper.GetSupportedDataType(y);
                    }
                    catch (RdfException)
                    {
                        // Can't determine a Data Type for one/both of the Nodes
                        return TryCheckEquality(x, y, out result);
                    }

                    if (xtype.Equals(string.Empty) || ytype.Equals(string.Empty))
                    {
                        // One/both has an unknown type
                        return TryCheckEquality(x, y, out result);
                    }
                    else
                    {
                        // Both have known types
                        SparqlNumericType xnumtype = x.NumericType;
                        SparqlNumericType ynumtype = y.NumericType;
                        var numtype = (SparqlNumericType)Math.Max((int)xnumtype, (int)ynumtype);
                        if (numtype != SparqlNumericType.NaN)
                        {
                            if (xnumtype == SparqlNumericType.NaN || ynumtype == SparqlNumericType.NaN)
                            {
                                // If one is non-numeric then we can't assume non-equality
                                return false;
                            }

                            // Both are Numeric so use Numeric ordering
                            try
                            {
                                result = NumericCompare(x, y, numtype);
                                return true;
                            }
                            catch (FormatException)
                            {
                                return TryCheckEquality(x, y, out result);
                            }
                            catch (RdfQueryException)
                            {
                                return TryCheckEquality(x, y, out result);
                            }
                        }

                        if (xtype.Equals(ytype))
                        {
                            switch (xtype)
                            {
                                case XmlSpecsHelper.XmlSchemaDataTypeDate:
                                    result = DateCompare(x, y);
                                    return true;
                                case XmlSpecsHelper.XmlSchemaDataTypeDateTime:
                                    result = DateTimeCompare(x, y);
                                    return true;
                                case XmlSpecsHelper.XmlSchemaDataTypeBoolean:
                                    result = BooleanCompare(x, y);
                                    return true;
                                case XmlSpecsHelper.XmlSchemaDataTypeString:
                                    // Both Strings so use Lexical string ordering
                                    result = string.Compare(((ILiteralNode)x).Value, ((ILiteralNode)y).Value, Culture,
                                        Options);
                                    return true;
                                default:
                                    return false;
                            }
                        }

                        var commontype = XmlSpecsHelper.GetCompatibleSupportedDataType(xtype, ytype, true);
                        if (commontype.Equals(string.Empty))
                        {
                            return false;
                        }

                        switch (commontype)
                        {
                            case XmlSpecsHelper.XmlSchemaDataTypeDate:
                                result = DateCompare(x, y);
                                return true;
                            case XmlSpecsHelper.XmlSchemaDataTypeDateTime:
                                result = DateTimeCompare(x, y);
                                return true;
                            default:
                                return false;
                        }
                    }


                case NodeType.Triple:
                    if (x is ITripleNode xt && y is ITripleNode yt)
                    {
                        return TryCompareTripleNodes(xt, yt, out result);
                    }

                    return false;

                default:
                    // For all other node types, no comparison ordering is defined.
                    return false;
            }
        }

        /// <summary>
        /// Create a new comparer that will use the specified culture and options when comparing string literals.
        /// </summary>
        /// <param name="culture">The culture to use for string literal comparison.</param>
        /// <param name="options">The options to apply to string literal comparison.</param>
        public SparqlNodeComparer(CultureInfo culture, CompareOptions options)
        {
            Culture = culture;
            Options = options;
        }


        /// <summary>
        /// Compares two Nodes.
        /// </summary>
        /// <param name="x">Node.</param>
        /// <param name="y">Node.</param>
        /// <returns></returns>
        [Obsolete("Replaced with bool TryCompare(INode, INode, out int)")]
        public virtual int Compare(INode x, INode y)
        {
            if (TryCompare(x, y, out var result)) return result;
            throw new RdfQueryComparisonException(x, y);
        }

        /// <summary>
        /// Compares two valued Nodes.
        /// </summary>
        /// <param name="x">Node.</param>
        /// <param name="y">Node.</param>
        /// <returns></returns>
        [Obsolete("Replaced with bool TryCompare(IValuedNode, IValuedNode, out int)")]
        public virtual int Compare(IValuedNode x, IValuedNode y)
        {
            if (TryCompare(x, y, out var compare)) return compare;
            throw new RdfQueryComparisonException(x, y);
        }

        /// <summary>
        /// Compares two Nodes for Numeric Ordering.
        /// </summary>
        /// <param name="x">Node.</param>
        /// <param name="y">Node.</param>
        /// <param name="type">Numeric Type.</param>
        /// <returns></returns>
        protected virtual int NumericCompare(INode x, INode y, SparqlNumericType type)
        {
            return NumericCompare(x.AsValuedNode(), y.AsValuedNode(), type);
        }

        /// <summary>
        /// Compares two Nodes for Numeric Ordering.
        /// </summary>
        /// <param name="x">Node.</param>
        /// <param name="y">Node.</param>
        /// <param name="type">Numeric Type.</param>
        /// <returns></returns>
        protected virtual int NumericCompare(IValuedNode x, IValuedNode y, SparqlNumericType type)
        {
            if (x == null || y == null) throw new RdfQueryException("Cannot evaluate numeric ordering when one or both arguments are Null");
            if (type == SparqlNumericType.NaN) throw new RdfQueryException("Cannot evaluate numeric ordering when the Numeric Type is NaN");

            switch (type)
            {
                case SparqlNumericType.Decimal:
                    return x.AsDecimal().CompareTo(y.AsDecimal());
                case SparqlNumericType.Double:
                    return x.AsDouble().CompareTo(y.AsDouble());
                case SparqlNumericType.Float:
                    return x.AsFloat().CompareTo(y.AsFloat());
                case SparqlNumericType.Integer:
                    return x.AsInteger().CompareTo(y.AsInteger());
                default:
                    throw new RdfQueryException("Cannot evaluate numeric equality since of the arguments is not numeric");
            }
        }

        /// <summary>
        /// Compares two Date Times for Date Time ordering.
        /// </summary>
        /// <param name="x">Node.</param>
        /// <param name="y">Node.</param>
        /// <returns></returns>
        protected virtual int DateTimeCompare(INode x, INode y)
        {
            return DateTimeCompare(x.AsValuedNode(), y.AsValuedNode());
        }

        /// <summary>
        /// Compares two Date Times for Date Time ordering.
        /// </summary>
        /// <param name="x">Node.</param>
        /// <param name="y">Node.</param>
        /// <returns></returns>
        protected virtual int DateTimeCompare(IValuedNode x, IValuedNode y)
        {
            if (x == null || y == null) throw new RdfQueryException("Cannot evaluate date time comparison when one or both arguments are Null");
            try
            {
                DateTime c = x.AsDateTime();
                DateTime d = y.AsDateTime();

                switch (c.Kind)
                {
                    case DateTimeKind.Unspecified:
                        if (d.Kind != DateTimeKind.Unspecified) throw new RdfQueryException(
                                "Dates are incomparable, one specifies time zone information while the other does not");
                        break;
                    case DateTimeKind.Local:
                        if (d.Kind == DateTimeKind.Unspecified) throw new RdfQueryException(
                                "Dates are incomparable, one specifies time zone information while the other does not");
                        c = c.ToUniversalTime();
                        if (d.Kind == DateTimeKind.Local) d = d.ToUniversalTime();
                        break;
                    default:
                        if (d.Kind == DateTimeKind.Unspecified) throw new RdfQueryException(
                                "Dates are incomparable, one specifies time zone information while the other does not");
                        if (d.Kind == DateTimeKind.Local) d = d.ToUniversalTime();
                        break;
                }

                // Compare on unspecified/UTC form as appropriate
                return c.CompareTo(d);
            }
            catch (FormatException)
            {
                throw new RdfQueryException("Cannot evaluate date time comparison since one of the arguments does not have a valid lexical value for a Date");
            }
        }

        /// <summary>
        /// Compares two Dates for Date ordering.
        /// </summary>
        /// <param name="x">Node.</param>
        /// <param name="y">Node.</param>
        /// <returns></returns>
        protected virtual int DateCompare(INode x, INode y)
        {
            return DateCompare(x.AsValuedNode(), y.AsValuedNode());
        }

        /// <summary>
        /// Compares two Dates for Date ordering.
        /// </summary>
        /// <param name="x">Node.</param>
        /// <param name="y">Node.</param>
        /// <returns></returns>
        protected virtual int DateCompare(IValuedNode x, IValuedNode y)
        {
            if (x == null || y == null) throw new RdfQueryException("Cannot evaluate date comparison when one or both arguments are Null");
            try
            {
                DateTime c = x.AsDateTime();
                DateTime d = y.AsDateTime();

                switch (c.Kind)
                {
                    case DateTimeKind.Unspecified:
                        break;
                    case DateTimeKind.Local:
                        c = c.ToUniversalTime();
                        if (d.Kind == DateTimeKind.Local) d = d.ToUniversalTime();
                        break;
                    default:
                        if (d.Kind == DateTimeKind.Local) d = d.ToUniversalTime();
                        break;
                }

                // Timezone irrelevant for date comparisons since we don't have any time to normalize to
                // Thus Open World Assumption means we can compare
                // For Local times we normalize to UTC

                // Compare on the Unspecified/UTC form as appropriate
                var res = c.Year.CompareTo(d.Year);
                if (res == 0)
                {
                    res = c.Month.CompareTo(d.Month);
                    if (res == 0)
                    {
                        res = c.Day.CompareTo(d.Day);
                    }
                }
                return res;
            }
            catch (FormatException)
            {
                throw new RdfQueryException("Cannot evaluate date comparison since one of the arguments does not have a valid lexical value for a Date");
            }
        }

        protected virtual int BooleanCompare(INode x, INode y)
        {
            return BooleanCompare(x.AsValuedNode(), y.AsValuedNode());
        }

        protected virtual int BooleanCompare(IValuedNode x, IValuedNode y)
        {
            if (x == null || y == null) throw new RdfQueryException("Cannot evaluate boolean comparison when one or both arguments are Null");
            try
            {
                var c = x.AsBoolean();
                var d = y.AsBoolean();
                if (c == d) return 0;
                if (c) return 1;
                return -1;
            }
            catch (FormatException)
            {
                throw new RdfQueryException("Cannot evaluate boolean comparison since one of the arguments does not have a valid lexical value for a boolean");
            }
        }
    }

    // TODO: This class should take a SparqlNodeComparer instance as a constructor parameter and only implement handling the error states.
    /// <summary>
    /// Comparer class for use in SPARQL ORDER BY - implements the Semantics broadly similar to the relational operator but instead of erroring using Node/Lexical ordering where an error would occur it makes an appropriate decision.
    /// </summary>
    public class SparqlOrderingComparer 
        : SparqlNodeComparer, IComparer<INode>, IComparer<IValuedNode>
    {
        /// <summary>
        /// Create a new comparer that uses the same culture and comparison options as the specified node comparer.
        /// </summary>
        /// <param name="nodeComparer">The node comparer whose culture and comparison options are to be used.</param>
        public SparqlOrderingComparer(ISparqlNodeComparer nodeComparer):base(nodeComparer?.Culture ?? CultureInfo.InvariantCulture, nodeComparer?.Options ?? CompareOptions.Ordinal) {}

        /// <summary>
        /// Create a  new comparer that uses the specified culture and options when comparing string literals.
        /// </summary>
        /// <param name="culture">The culture to use for string literal comparison.</param>
        /// <param name="options">The string comparison options to apply to string literal comparison.</param>
        public SparqlOrderingComparer(CultureInfo culture, CompareOptions options) : base(culture, options)
        {

        }

        /// <summary>
        /// Compares two Nodes.
        /// </summary>
        /// <param name="x">Node.</param>
        /// <param name="y">Node.</param>
        /// <returns></returns>
        public override int Compare(INode x, INode y)
        {
            // Nulls are less than everything
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            // If the Node Types are different use Node ordering
            if (x.NodeType != y.NodeType) return x.CompareTo(y);

            if (x.NodeType == NodeType.Literal)
            {
                try
                {
                    // Do they have supported Data Types?
                    string xtype, ytype;
                    try
                    {
                        xtype = XmlSpecsHelper.GetSupportedDataType(x);
                        ytype = XmlSpecsHelper.GetSupportedDataType(y);
                    }
                    catch (RdfException)
                    {
                        // Can't determine a Data Type for one/both of the Nodes so use Node ordering
                        return x.CompareTo(y);
                    }

                    if (xtype.Equals(string.Empty) || ytype.Equals(string.Empty))
                    {
                        // One/both has an unknown type
                        if (x.Equals(y))
                        {
                            // If RDF Term equality returns true then we return that they are equal
                            return 0;
                        }

                        // If RDF Term equality returns false then we fall back to Node Ordering
                        return x.CompareTo(y);
                    }

                    // Both have known types

                    if (SparqlSpecsHelper.IsStringDatatype(xtype))

                    {
                        if (SparqlSpecsHelper.IsStringDatatype(ytype))
                        {
                            // Compare literal values using the current culture ordering and string comparison options
                            return Culture.CompareInfo.Compare(((ILiteralNode) x).Value, ((ILiteralNode) y).Value,
                                Options);
                        }

                        return -1;
                    }

                    if (SparqlSpecsHelper.IsStringDatatype(ytype))
                    {
                        return 1;
                    }

                    SparqlNumericType xnumtype = NumericTypesHelper.GetNumericTypeFromDataTypeUri(xtype);
                    SparqlNumericType ynumtype = NumericTypesHelper.GetNumericTypeFromDataTypeUri(ytype);
                    var numtype = (SparqlNumericType) Math.Max((int) xnumtype, (int) ynumtype);
                    if (numtype != SparqlNumericType.NaN)
                    {
                        if (xnumtype == SparqlNumericType.NaN)
                        {
                            return 1;
                        }

                        if (ynumtype == SparqlNumericType.NaN)
                        {
                            return -1;
                        }

                        // Both are Numeric so use Numeric ordering
                        try
                        {
                            return NumericCompare(x, y, numtype);
                        }
                        catch (FormatException)
                        {
                            // If this errors try RDF Term equality since that might still give equality, otherwise fall back to Node Ordering
                            return x.Equals(y) ? 0 : x.CompareTo(y);
                        }
                        catch (RdfQueryException)
                        {
                            // If this errors try RDF Term equality since that might still give equality, otherwise fall back to Node Ordering
                            return x.Equals(y) ? 0 : x.CompareTo(y);
                        }
                    }

                    if (xtype.Equals(ytype))
                    {
                        switch (xtype)
                        {
                            case XmlSpecsHelper.XmlSchemaDataTypeDate:
                                return DateCompare(x, y);
                            case XmlSpecsHelper.XmlSchemaDataTypeDateTime:
                                return DateTimeCompare(x, y);
                            default:
                                // Use node ordering
                                return x.CompareTo(y);
                        }
                    }

                    var commonType = XmlSpecsHelper.GetCompatibleSupportedDataType(xtype, ytype, true);
                    if (commonType.Equals(string.Empty))
                    {
                        // Use Node ordering
                        return x.CompareTo(y);
                    }

                    switch (commonType)
                    {
                        case XmlSpecsHelper.XmlSchemaDataTypeDate:
                            return DateCompare(x, y);
                        case XmlSpecsHelper.XmlSchemaDataTypeDateTime:
                            return DateTimeCompare(x, y);
                        default:
                            // Use Node ordering
                            return x.CompareTo(y);
                    }
                }
                catch
                {
                    // If error then can't determine ordering so fall back to node ordering
                    return x.CompareTo(y);
                }
            }

            if (x.NodeType == NodeType.Triple)
            {
                Triple xt = (x as ITripleNode)?.Triple, yt = (y as ITripleNode)?.Triple;
                if (xt == null || yt == null) 
                {
                    return x.CompareTo(y);
                }
                // Comparing two triple nodes
                var cmp = Compare(xt.Subject, yt.Subject);
                if (cmp == 0)
                {
                    cmp = Compare(xt.Predicate, yt.Predicate);
                    if (cmp == 0)
                    {
                        cmp = Compare(xt.Object, yt.Object);
                    }
                }

                return cmp;
            }

            // If not Literals use Node ordering
            return x.CompareTo(y);
        }

        /// <summary>
        /// Compares two Nodes.
        /// </summary>
        /// <param name="x">Node.</param>
        /// <param name="y">Node.</param>
        /// <returns></returns>
        public override int Compare(IValuedNode x, IValuedNode y)
        {
            // Nulls are less than everything
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            // If the Node Types are different use Node ordering
            if (x.NodeType != y.NodeType) return x.CompareTo(y);

            if (x.NodeType == NodeType.Literal)
            {
                try
                {
                    // Do they have supported Data Types?
                    string xtype, ytype;
                    try
                    {
                        xtype = XmlSpecsHelper.GetSupportedDataType(x);
                        ytype = XmlSpecsHelper.GetSupportedDataType(y);
                    }
                    catch (RdfException)
                    {
                        // Can't determine a Data Type for one/both of the Nodes so use Node ordering
                        return x.CompareTo(y);
                    }

                    if (xtype.Equals(string.Empty) || ytype.Equals(string.Empty))
                    {
                        // One/both has an unknown type
                        if (x.Equals(y))
                        {
                            // If RDF Term equality returns true then we return that they are equal
                            return 0;
                        }
                        else
                        {
                            // If RDF Term equality returns false then we fall back to Node Ordering
                            return x.CompareTo(y);
                        }
                    }
                    else
                    {
                        // Both have known types
                        SparqlNumericType xnumtype = x.NumericType;
                        SparqlNumericType ynumtype = y.NumericType;
                        var numtype = (SparqlNumericType)Math.Max((int)xnumtype, (int)ynumtype);
                        if (numtype != SparqlNumericType.NaN)
                        {
                            if (xnumtype == SparqlNumericType.NaN)
                            {
                                return 1;
                            }
                            else if (ynumtype == SparqlNumericType.NaN)
                            {
                                return -1;
                            }

                            // Both are Numeric so use Numeric ordering
                            try
                            {
                                return NumericCompare(x, y, numtype);
                            }
                            catch (FormatException)
                            {
                                if (x.Equals(y)) return 0;
                                // Otherwise fall back to Node Ordering
                                return x.CompareTo(y);
                            }
                            catch (RdfQueryException)
                            {
                                // If this errors try RDF Term equality since that might still give equality
                                if (x.Equals(y)) return 0;
                                // Otherwise fall back to Node Ordering
                                return x.CompareTo(y);
                            }
                        }
                        else if (xtype.Equals(ytype))
                        {
                            switch (xtype)
                            {
                                case XmlSpecsHelper.XmlSchemaDataTypeDate:
                                    return DateCompare(x, y);
                                case XmlSpecsHelper.XmlSchemaDataTypeDateTime:
                                    return DateTimeCompare(x, y);
                                case XmlSpecsHelper.XmlSchemaDataTypeString:
                                    // Both Strings so use Lexical string ordering
                                    return ((ILiteralNode)x).Value.CompareTo(((ILiteralNode)y).Value);
                                default:
                                    // Use node ordering
                                    return x.CompareTo(y);
                            }
                        }
                        else
                        {
                            var commontype = XmlSpecsHelper.GetCompatibleSupportedDataType(xtype, ytype, true);
                            if (commontype.Equals(string.Empty))
                            {
                                // Use Node ordering
                                return x.CompareTo(y);
                            }
                            else
                            {
                                switch (commontype)
                                {
                                    case XmlSpecsHelper.XmlSchemaDataTypeDate:
                                        return DateCompare(x, y);
                                    case XmlSpecsHelper.XmlSchemaDataTypeDateTime:
                                        return DateTimeCompare(x, y);
                                    default:
                                        // Use Node ordering
                                        return x.CompareTo(y);
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // If error then can't determine ordering so fall back to node ordering
                    return x.CompareTo(y);
                }
            }

            if (x.NodeType == NodeType.Triple)
            {
                Triple xt = (x as ITripleNode)?.Triple, yt = (y as ITripleNode)?.Triple;
                if (xt == null || yt == null)
                {
                    return x.CompareTo(y);
                }
                // Comparing two triple nodes
                var cmp = Compare(xt.Subject, yt.Subject);
                if (cmp == 0)
                {
                    cmp = Compare(xt.Predicate, yt.Predicate);
                    if (cmp == 0)
                    {
                        cmp = Compare(xt.Object, yt.Object);
                    }
                }

                return cmp;
            }

            // If not Literals use Node ordering
            return x.CompareTo(y);
        }

        /// <summary>
        /// Compares two Date Times for Date Time ordering.
        /// </summary>
        /// <param name="x">Node.</param>
        /// <param name="y">Node.</param>
        /// <returns></returns>
        protected override int DateTimeCompare(IValuedNode x, IValuedNode y)
        {
            if (x == null || y == null) throw new RdfQueryException("Cannot evaluate date time comparison when one or both arguments are Null");
            try
            {
                DateTime c = x.AsDateTime();
                DateTime d = y.AsDateTime();

                switch (c.Kind)
                {
                    case DateTimeKind.Unspecified:
                        // Sort unspecified lower than Local/UTC date
                        if (d.Kind != DateTimeKind.Unspecified) return -1;
                        break;
                    case DateTimeKind.Local:
                        // Sort Local higher than Unspecified
                        if (d.Kind == DateTimeKind.Unspecified) return 1;
                        c = c.ToUniversalTime();
                        if (d.Kind == DateTimeKind.Local) d = d.ToUniversalTime();
                        break;
                    default:
                        // Sort UTC higher than Unspecified
                        if (d.Kind == DateTimeKind.Unspecified) return 1;
                        if (d.Kind == DateTimeKind.Local) d = d.ToUniversalTime();
                        break;
                }

                // Compare on unspecified/UTC form as appropriate
                return c.CompareTo(d);
            }
            catch (FormatException)
            {
                throw new RdfQueryException("Cannot evaluate date time comparison since one of the arguments does not have a valid lexical value for a Date");
            }
        }
    }
}
