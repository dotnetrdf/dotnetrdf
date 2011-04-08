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
using VDS.RDF.Parsing;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query
{

    /// <summary>
    /// Comparer class for implementing the SPARQL semantics for the relational operators
    /// </summary>
    public class SparqlNodeComparer : IComparer<INode>
    {
        /// <summary>
        /// Compares two Nodes
        /// </summary>
        /// <param name="x">Node</param>
        /// <param name="y">Node</param>
        /// <returns></returns>
        public virtual int Compare(INode x, INode y)
        {
            //Nulls are less than everything
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            //If the Node Types are different use Node ordering
            if (x.NodeType != y.NodeType) return x.CompareTo(y);

            if (x.NodeType == NodeType.Literal)
            {
                //Do they have supported Data Types?
                String xtype, ytype;
                try
                {
                    xtype = XmlSpecsHelper.GetSupportedDataType(x);
                    ytype = XmlSpecsHelper.GetSupportedDataType(y);
                }
                catch (RdfException)
                {
                    //Can't determine a Data Type for one/both of the Nodes so use Node ordering
                    return x.CompareTo(y);
                }

                if (xtype.Equals(String.Empty) || ytype.Equals(String.Empty))
                {
                    //One/both has an unknown type
                    if (x.Equals(y))
                    {
                        //If RDF Term equality returns true then we return that they are equal
                        return 0;
                    }
                    else
                    {
                        //If RDF Term equality returns false then we error
                        //UNLESS they have the same Datatype
                        throw new RdfQueryException("Unable to determine ordering since one/both arguments has an Unknown Type");
                    }
                }
                else
                {
                    //Both have known types
                    SparqlNumericType xnumtype = SparqlSpecsHelper.GetNumericTypeFromDataTypeUri(xtype);
                    SparqlNumericType ynumtype = SparqlSpecsHelper.GetNumericTypeFromDataTypeUri(ytype);
                    SparqlNumericType numtype = (SparqlNumericType)Math.Max((int)xnumtype, (int)ynumtype);
                    if (numtype != SparqlNumericType.NaN)
                    {
                        if (xnumtype == SparqlNumericType.NaN || ynumtype == SparqlNumericType.NaN)
                        {
                            //If one is non-numeric then we can't assume non-equality
                            throw new RdfQueryException("Unable to determine ordering since one/both arguments does not return a Number");
                        }

                        //Both are Numeric so use Numeric ordering
                        try
                        {
                            return this.NumericCompare(x, y, numtype);
                        }
                        catch (FormatException)
                        {
                            if (x.Equals(y)) return 0;
                            throw new RdfQueryException("Unable to determine ordering since one/both arguments does not contain a valid value for it's type");
                        }
                        catch (RdfQueryException)
                        {
                            //If this errors try RDF Term equality since 
                            if (x.Equals(y)) return 0;
                            throw new RdfQueryException("Unable to determine ordering since one/both arguments was not a valid numeric");
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
                                //Both Strings so use Lexical string ordering
                                return ((ILiteralNode)x).Value.CompareTo(((ILiteralNode)y).Value);
                            default:
                                //Use node ordering
                                return x.CompareTo(y);
                        }
                    }
                    else
                    {
                        String commontype = XmlSpecsHelper.GetCompatibleSupportedDataType(xtype, ytype, true);
                        if (commontype.Equals(String.Empty))
                        {
                            //Use Node ordering
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
                                    //Use Node ordering
                                    return x.CompareTo(y);
                            }
                        }
                    }
                }
            }
            else
            {
                //If not Literals use Node ordering
                return x.CompareTo(y);
            }
        }

        /// <summary>
        /// Compares two Nodes for Numeric Ordering
        /// </summary>
        /// <param name="x">Node</param>
        /// <param name="y">Node</param>
        /// <param name="type">Numeric Type</param>
        /// <returns></returns>
        protected int NumericCompare(INode x, INode y, SparqlNumericType type)
        {
            if (x == null || y == null) throw new RdfQueryException("Cannot evaluate numeric ordering when one or both arguments are Null");
            if (type == SparqlNumericType.NaN) throw new RdfQueryException("Cannot evaluate numeric ordering when the Numeric Type is NaN");

            try
            {
                ILiteralNode a = (ILiteralNode)x;
                ILiteralNode b = (ILiteralNode)y;

                switch (type)
                {
                    case SparqlNumericType.Decimal:
                        return SparqlSpecsHelper.ToDecimal(a).CompareTo(SparqlSpecsHelper.ToDecimal(b));
                    case SparqlNumericType.Double:
                        return SparqlSpecsHelper.ToDouble(a).CompareTo(SparqlSpecsHelper.ToDouble(b));
                    case SparqlNumericType.Float:
                        return SparqlSpecsHelper.ToFloat(a).CompareTo(SparqlSpecsHelper.ToFloat(b));
                    case SparqlNumericType.Integer:
                        return SparqlSpecsHelper.ToInteger(a).CompareTo(SparqlSpecsHelper.ToInteger(b));
                    default:
                        throw new RdfQueryException("Cannot evaluate numeric equality since of the arguments is not numeric");
                }
            }
            catch (FormatException)
            {
                throw;// new RdfQueryException("Cannot evaluate numeric equality since one of the arguments does not have a valid lexical value for the given type");
            }
        }

        /// <summary>
        /// Compares two Date Times for Date Time ordering
        /// </summary>
        /// <param name="x">Node</param>
        /// <param name="y">Node</param>
        /// <returns></returns>
        protected int DateTimeCompare(INode x, INode y)
        {
            if (x == null || y == null) throw new RdfQueryException("Cannot evaluate date time equality when one or both arguments are Null");
            try
            {
                ILiteralNode a = (ILiteralNode)x;
                ILiteralNode b = (ILiteralNode)y;

                DateTimeOffset c = SparqlSpecsHelper.ToDateTimeOffset(a);
                DateTimeOffset d = SparqlSpecsHelper.ToDateTimeOffset(b);

                if (!c.Offset.Equals(d.Offset)) throw new RdfQueryException("Cannot order Dates which are from different time zones");

                return c.CompareTo(d);
            }
            catch (FormatException)
            {
                throw new RdfQueryException("Cannot evaluate date time equality since one of the arguments does not have a valid lexical value for a Date Time");
            }
        }

        /// <summary>
        /// Compares two Dates for Date ordering
        /// </summary>
        /// <param name="x">Node</param>
        /// <param name="y">Node</param>
        /// <returns></returns>
        protected int DateCompare(INode x, INode y)
        {
            if (x == null || y == null) throw new RdfQueryException("Cannot evaluate date equality when one or both arguments are Null");
            try
            {
                ILiteralNode a = (ILiteralNode)x;
                ILiteralNode b = (ILiteralNode)y;

                DateTimeOffset c = SparqlSpecsHelper.ToDateTimeOffset(a);
                DateTimeOffset d = SparqlSpecsHelper.ToDateTimeOffset(b);

                if (!c.Offset.Equals(d.Offset)) throw new RdfQueryException("Cannot order Dates which are from different time zones");

                int res = c.Year.CompareTo(d.Year);
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
                throw new RdfQueryException("Cannot evaluate date equality since one of the arguments does not have a valid lexical value for a Date");
            }
        }
    }

    /// <summary>
    /// Comparer class for use in SPARQL ORDER BY - implements the Semantics broadly similar to the relational operator but instead of erroring using Node/Lexical ordering where an error would occur
    /// </summary>
    public class SparqlOrderingComparer : SparqlNodeComparer
    {
        /// <summary>
        /// Compares two Nodes
        /// </summary>
        /// <param name="x">Node</param>
        /// <param name="y">Node</param>
        /// <returns></returns>
        public override int Compare(INode x, INode y)
        {
            //Nulls are less than everything
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            //If the Node Types are different use Node ordering
            if (x.NodeType != y.NodeType) return x.CompareTo(y);

            if (x.NodeType == NodeType.Literal)
            {
                try
                {
                    //Do they have supported Data Types?
                    String xtype, ytype;
                    try
                    {
                        xtype = XmlSpecsHelper.GetSupportedDataType(x);
                        ytype = XmlSpecsHelper.GetSupportedDataType(y);
                    }
                    catch (RdfException)
                    {
                        //Can't determine a Data Type for one/both of the Nodes so use Node ordering
                        return x.CompareTo(y);
                    }

                    if (xtype.Equals(String.Empty) || ytype.Equals(String.Empty))
                    {
                        //One/both has an unknown type
                        if (x.Equals(y))
                        {
                            //If RDF Term equality returns true then we return that they are equal
                            return 0;
                        }
                        else
                        {
                            //If RDF Term equality returns false then we fall back to Node Ordering
                            return x.CompareTo(y);
                        }
                    }
                    else
                    {
                        //Both have known types
                        SparqlNumericType xnumtype = SparqlSpecsHelper.GetNumericTypeFromDataTypeUri(xtype);
                        SparqlNumericType ynumtype = SparqlSpecsHelper.GetNumericTypeFromDataTypeUri(ytype);
                        SparqlNumericType numtype = (SparqlNumericType)Math.Max((int)xnumtype, (int)ynumtype);
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

                            //Both are Numeric so use Numeric ordering
                            try
                            {
                                return this.NumericCompare(x, y, numtype);
                            }
                            catch (FormatException)
                            {
                                if (x.Equals(y)) return 0;
                                //Otherwise fall back to Node Ordering
                                return x.CompareTo(y);
                            }
                            catch (RdfQueryException)
                            {
                                //If this errors try RDF Term equality since that might still give equality
                                if (x.Equals(y)) return 0;
                                //Otherwise fall back to Node Ordering
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
                                    //Both Strings so use Lexical string ordering
                                    return ((ILiteralNode)x).Value.CompareTo(((ILiteralNode)y).Value);
                                default:
                                    //Use node ordering
                                    return x.CompareTo(y);
                            }
                        }
                        else
                        {
                            String commontype = XmlSpecsHelper.GetCompatibleSupportedDataType(xtype, ytype, true);
                            if (commontype.Equals(String.Empty))
                            {
                                //Use Node ordering
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
                                        //Use Node ordering
                                        return x.CompareTo(y);
                                }
                            }
                        }
                    }
                }
                catch
                {
                    //If error then can't determine ordering so fall back to node ordering
                    return x.CompareTo(y);
                }
            }
            else
            {
                //If not Literals use Node ordering
                return x.CompareTo(y);
            }
        }
    }
}
