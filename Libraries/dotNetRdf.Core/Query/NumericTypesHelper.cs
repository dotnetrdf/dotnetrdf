/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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
using System.Linq;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query;

/// <summary>
/// Class containing helper functions related to SPARQL numeric types.
/// </summary>
public static class NumericTypesHelper
{
    /// <summary>
    /// Set of XML Schema Data Types which are derived from Integer and can be treated as Integers by SPARQL.
    /// </summary>
    public static string[] IntegerDataTypes = {
        XmlSpecsHelper.XmlSchemaDataTypeByte,
        XmlSpecsHelper.XmlSchemaDataTypeInt,
        XmlSpecsHelper.XmlSchemaDataTypeInteger,
        XmlSpecsHelper.XmlSchemaDataTypeLong,
        XmlSpecsHelper.XmlSchemaDataTypeNegativeInteger,
        XmlSpecsHelper.XmlSchemaDataTypeNonNegativeInteger,
        XmlSpecsHelper.XmlSchemaDataTypeNonPositiveInteger,
        XmlSpecsHelper.XmlSchemaDataTypePositiveInteger,
        XmlSpecsHelper.XmlSchemaDataTypeShort,
        XmlSpecsHelper.XmlSchemaDataTypeUnsignedByte,
        XmlSpecsHelper.XmlSchemaDataTypeUnsignedInt,
        XmlSpecsHelper.XmlSchemaDataTypeUnsignedLong,
        XmlSpecsHelper.XmlSchemaDataTypeUnsignedShort,
    };

    #region Numeric Type determination

    /// <summary>
    /// Determines the Sparql Numeric Type for a Literal based on its Data Type Uri.
    /// </summary>
    /// <param name="dtUri">Data Type Uri.</param>
    /// <returns></returns>
    public static SparqlNumericType GetNumericTypeFromDataTypeUri(Uri dtUri)
    {
        return GetNumericTypeFromDataTypeUri(dtUri.AbsoluteUri);
    }

    /// <summary>
    /// Determines the Sparql Numeric Type for a Literal based on its Data Type Uri.
    /// </summary>
    /// <param name="dtUri">Data Type Uri as a String.</param>
    /// <returns></returns>
    public static SparqlNumericType GetNumericTypeFromDataTypeUri(string dtUri)
    {
        if (dtUri.Equals(XmlSpecsHelper.XmlSchemaDataTypeDouble))
        {
            return SparqlNumericType.Double;
        }
        else if (dtUri.Equals(XmlSpecsHelper.XmlSchemaDataTypeFloat))
        {
            return SparqlNumericType.Float;
        }
        else if (dtUri.Equals(XmlSpecsHelper.XmlSchemaDataTypeDecimal))
        {
            return SparqlNumericType.Decimal;
        }
        else if (IntegerDataTypes.Contains(dtUri))
        {
            return SparqlNumericType.Integer;
        }
        else
        {
            return SparqlNumericType.NaN;
        }
    }

    #endregion
}
