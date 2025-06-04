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

using System.Text.RegularExpressions;

namespace VDS.RDF.Parsing;

/// <summary>
/// Helper class with useful constants relating to the RDF Specification.
/// </summary>
public class RdfSpecsHelper
{
    /// <summary>
    /// URI for rdf:List.
    /// </summary>
    public const string RdfList = "http://www.w3.org/1999/02/22-rdf-syntax-ns#List";
    /// <summary>
    /// URI for rdf:first.
    /// </summary>
    public const string RdfListFirst = "http://www.w3.org/1999/02/22-rdf-syntax-ns#first";
    /// <summary>
    /// URI for rdf:rest.
    /// </summary>
    public const string RdfListRest = "http://www.w3.org/1999/02/22-rdf-syntax-ns#rest";
    /// <summary>
    /// URI for rdf:nil.
    /// </summary>
    public const string RdfListNil = "http://www.w3.org/1999/02/22-rdf-syntax-ns#nil";
    /// <summary>
    /// URI for rdf:type.
    /// </summary>
    public const string RdfType = "http://www.w3.org/1999/02/22-rdf-syntax-ns#type";
    /// <summary>
    /// URI for rdf:XMLLiteral.
    /// </summary>
    public const string RdfXmlLiteral = "http://www.w3.org/1999/02/22-rdf-syntax-ns#XMLLiteral";
    /// <summary>
    /// URI for rdf:subject.
    /// </summary>
    public const string RdfSubject = "http://www.w3.org/1999/02/22-rdf-syntax-ns#subject";
    /// <summary>
    /// URI for rdf:predicate.
    /// </summary>
    public const string RdfPredicate = "http://www.w3.org/1999/02/22-rdf-syntax-ns#predicate";
    /// <summary>
    /// URI for rdf:object.
    /// </summary>
    public const string RdfObject = "http://www.w3.org/1999/02/22-rdf-syntax-ns#object";
    /// <summary>
    /// URI for rdf:Statement.
    /// </summary>
    public const string RdfStatement = "http://www.w3.org/1999/02/22-rdf-syntax-ns#Statement";

    /// <summary>
    /// URI for rdf:langString the implicit type of language specified literals.
    /// </summary>
    public const string RdfLangString = "http://www.w3.org/1999/02/22-rdf-syntax-ns#langString";

    /// <summary>
    /// Pattern for Valid Language Specifiers.
    /// </summary>
    public const string ValidLangSpecifiersPattern = "^@?[A-Za-z]+(\\-[A-Za-z0-9]+)*$";

    /// <summary>
    /// Regular Expression for Valid Language Specifiers.
    /// </summary>
    private static readonly Regex ValidLangSpecifier = new Regex(ValidLangSpecifiersPattern);

    /// <summary>
    /// Determines whether a given String is a valid Language Specifier.
    /// </summary>
    /// <param name="value">String to test.</param>
    /// <returns></returns>
    public static bool IsValidLangSpecifier(string value)
    {
        return ValidLangSpecifier.IsMatch(value);
    }

    /// <summary>
    /// URI for rdf:value.
    /// </summary>
    public const string RdfValue = "http://www.w3.org/1999/02/22-rdf-syntax-ns#value";

    /// <summary>
    /// URI for rdf:language.
    /// </summary>
    public const string RdfLanguage = "http://www.w3.org/1999/02/22-rdf-syntax-ns#language";

    /// <summary>
    /// URI for rdf:direction.
    /// </summary>
    public const string RdfDirection = "http://www.w3.org/1999/02/22-rdf-syntax-ns#direction";

    /// <summary>
    /// URI for JSON literal datatype.
    /// </summary>
    public const string RdfJson = "http://www.w3.org/1999/02/22-rdf-syntax-ns#JSON";
}
