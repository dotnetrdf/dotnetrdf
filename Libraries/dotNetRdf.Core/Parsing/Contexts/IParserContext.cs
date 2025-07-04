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
using System.Collections.Generic;
using VDS.RDF.Parsing.Tokens;

namespace VDS.RDF.Parsing.Contexts;

/// <summary>
/// Interface for Parser Contexts.
/// </summary>
public interface IParserContext
{
    /// <summary>
    /// Gets the RDF Handler which is used to instantiate Nodes and to handle the generated RDF.
    /// </summary>
    IRdfHandler Handler
    {
        get;
    }

    /// <summary>
    /// Gets/Sets whether Parser Tracing should be used (if the Parser supports it).
    /// </summary>
    bool TraceParsing
    {
        get;
        set;
    }

    /// <summary>
    /// Gets the Namespace Map for the Handler.
    /// </summary>
    INestedNamespaceMapper Namespaces
    {
        get;
    }

    /// <summary>
    /// Gets the Base URI for the Handler.
    /// </summary>
    Uri BaseUri
    {
        get;
        set;
    }

    /// <summary>
    /// Gets the URI factory for the handler.
    /// </summary>
    IUriFactory UriFactory { get; }
}

/// <summary>
/// Interface for Parser Contexts which use Tokeniser based parsing.
/// </summary>
public interface ITokenisingParserContext
{
    /// <summary>
    /// Gets/Sets whether Tokenisation is Traced.
    /// </summary>
    bool TraceTokeniser
    {
        get;
        set;
    }

    /// <summary>
    /// Gets the Local Tokens Stack.
    /// </summary>
    Stack<IToken> LocalTokens
    {
        get;
    }

    /// <summary>
    /// Gets the Token Queue.
    /// </summary>
    ITokenQueue Tokens
    {
        get;
    }
}
