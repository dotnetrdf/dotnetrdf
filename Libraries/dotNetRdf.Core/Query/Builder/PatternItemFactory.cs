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
using VDS.RDF.Parsing;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder;

/// <summary>
/// Class responsible for creating <see cref="PatternItem"/>s.
/// </summary>
internal class PatternItemFactory : IPatternItemFactory
{
    private readonly NodeFactory _nodeFactory = new(new NodeFactoryOptions());

    public PatternItem CreateVariablePattern(string variableName)
    {
        return new VariablePattern(variableName);
    }

    public PatternItem CreateNodeMatchPattern(string qName, INamespaceMapper namespaceMapper)
    {
        var qNameResolved = Tools.ResolveQName(qName, namespaceMapper, null);
        return CreateNodeMatchPattern(new Uri(qNameResolved));
    }

    public PatternItem CreateNodeMatchPattern(Uri uri)
    {
        return CreateNodeMatchPattern(_nodeFactory.CreateUriNode(uri));
    }

    public PatternItem CreateNodeMatchPattern(INode node)
    {
        return new NodeMatchPattern(node);
    }

    public PatternItem CreateBlankNodeMatchPattern(string blankNodeIdentifier)
    {
        return new BlankNodePattern(blankNodeIdentifier);
    }

    public PatternItem CreatePatternItem(Type nodeType, string patternString, INamespaceMapper namespaceMapper)
    {
        if (nodeType == typeof(IUriNode))
        {
            return CreateNodeMatchPattern(patternString, namespaceMapper);
        }
        if (nodeType == typeof(IBlankNode))
        {
            return CreateBlankNodeMatchPattern(patternString);
        }
        if (nodeType == typeof(ILiteralNode))
        {
            return CreateLiteralNodeMatchPattern(patternString);
        }
        if(nodeType == typeof(IVariableNode))
        {
            return CreateVariablePattern(patternString);
        }

        throw new ArgumentException(string.Format("Invalid node type {0}", nodeType));
    }

    public PatternItem CreateLiteralNodeMatchPattern(object literal)
    {
        var literalString = GetLiteralString(literal);

        return new NodeMatchPattern(_nodeFactory.CreateLiteralNode(literalString));
    }

    public PatternItem CreateLiteralNodeMatchPattern(object literal, Uri datatype)
    {
        var literalString = GetLiteralString(literal);

        return new NodeMatchPattern(_nodeFactory.CreateLiteralNode(literalString, datatype));
    }

    public PatternItem CreateLiteralNodeMatchPattern(object literal, string langSpec)
    {
        var literalString = GetLiteralString(literal);

        return new NodeMatchPattern(_nodeFactory.CreateLiteralNode(literalString, langSpec));
    }

    private static string GetLiteralString(object literal)
    {
        var literalString = literal.ToString();
        if (literal is DateTimeOffset)
        {
            literalString = GetDatetimeString((DateTimeOffset) literal);
        }
        else if (literal is DateTime)
        {
            literalString = GetDatetimeString((DateTime) literal);
        }
        return literalString;
    }

    private static string GetDatetimeString(DateTimeOffset literal)
    {
        var datetimeString = literal.ToString(XmlSpecsHelper.XmlSchemaDateTimeFormat);
        return datetimeString;
    }

    private static string GetDatetimeString(DateTime literal)
    {
        var datetimeString = literal.ToString(XmlSpecsHelper.XmlSchemaDateTimeFormat);
        return datetimeString;
    }
}