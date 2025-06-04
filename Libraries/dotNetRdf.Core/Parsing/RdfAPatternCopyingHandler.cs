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
using System.Linq;

namespace VDS.RDF.Parsing;

/// <summary>
/// A handler which captures RDFa Pattern resources and rdfa:copy properties during processing and emits pattern copies at the end of processing.
/// </summary>
/// <remarks>This streaming implementation will only capture pattern triples that are emitted after declaration of the rdfa:Pattern type for the pattern resource is encountered *or* after an rdfa:copy triple with the pattern as its object is encountered.</remarks>
public class RdfAPatternCopyingHandler : IWrappingRdfHandler
{
    private readonly IRdfHandler _innerHandler;
    private readonly Dictionary<INode, List<Triple>> _patternTriples;
    private readonly Dictionary<INode, List<INode>> _patternRefs;
    private readonly INode _rdfType, _rdfaPattern, _rdfaCopy;

    /// <summary>
    /// Create a new RDFa Pattern Copying handler that wraps the specified inner handler.
    /// </summary>
    /// <param name="inner">The inner handler to receive all events emitted by this handler.</param>
    public RdfAPatternCopyingHandler(IRdfHandler inner)
    {
        _innerHandler = inner;
        _patternTriples = new Dictionary<INode, List<Triple>>();
        _patternRefs = new Dictionary<INode, List<INode>>();
        _rdfType = _innerHandler.CreateUriNode(_innerHandler.UriFactory.Create(RdfSpecsHelper.RdfType));
        _rdfaPattern = _innerHandler.CreateUriNode(_innerHandler.UriFactory.Create(RdfASpecsHelper.RdfAPattern));
        _rdfaCopy = _innerHandler.CreateUriNode(_innerHandler.UriFactory.Create(RdfASpecsHelper.RdfACopy));
    }


    /// <inheritdoc />
    public Uri BaseUri
    {
        get => _innerHandler.BaseUri;
        set => _innerHandler.BaseUri = value;
    }

    /// <inheritdoc />
    public INamespaceMapper NamespaceMap => _innerHandler.NamespaceMap;

    /// <inheritdoc />
    public IUriFactory UriFactory
    {
        get => _innerHandler.UriFactory;
        set => _innerHandler.UriFactory = value;
    }

    /// <inheritdoc />
    public IBlankNode CreateBlankNode()
    {
        return _innerHandler.CreateBlankNode();
    }

    /// <inheritdoc />
    public IBlankNode CreateBlankNode(string nodeId)
    {
        return _innerHandler.CreateBlankNode(nodeId);
    }

    /// <inheritdoc />
    public IGraphLiteralNode CreateGraphLiteralNode()
    {
        return _innerHandler.CreateGraphLiteralNode();
    }

    /// <inheritdoc />
    public IGraphLiteralNode CreateGraphLiteralNode(IGraph subgraph)
    {
        return _innerHandler.CreateGraphLiteralNode(subgraph);
    }

    /// <inheritdoc />
    public ILiteralNode CreateLiteralNode(string literal, Uri datatype)
    {
        return _innerHandler.CreateLiteralNode(literal, datatype);
    }

    /// <inheritdoc />
    public ILiteralNode CreateLiteralNode(string literal)
    {
        return _innerHandler.CreateLiteralNode(literal);
    }

    /// <inheritdoc />
    public ILiteralNode CreateLiteralNode(string literal, string langSpec)
    {
        return _innerHandler.CreateLiteralNode(literal, langSpec);
    }

    /// <inheritdoc />
    public IUriNode CreateUriNode(Uri uri)
    {
        return _innerHandler.CreateUriNode(uri);
    }

    /// <inheritdoc />
    public IUriNode CreateUriNode(string qName)
    {
        return _innerHandler.CreateUriNode(qName);
    }

    /// <inheritdoc />
    public IUriNode CreateUriNode()
    {
        return _innerHandler.CreateUriNode();
    }

    /// <inheritdoc />
    public IVariableNode CreateVariableNode(string varName)
    {
        return _innerHandler.CreateVariableNode(varName);
    }

    /// <inheritdoc />
    public ITripleNode CreateTripleNode(Triple triple)
    {
        return _innerHandler.CreateTripleNode(triple);
    }

    /// <inheritdoc />
    public string GetNextBlankNodeID()
    {
        return _innerHandler.GetNextBlankNodeID();
    }

    /// <inheritdoc />
    public bool NormalizeLiteralValues
    {
        get => _innerHandler.NormalizeLiteralValues;
        set => _innerHandler.NormalizeLiteralValues = value;
    }

    /// <inheritdoc />
    public LanguageTagValidationMode LanguageTagValidation
    {
        get => _innerHandler.LanguageTagValidation;
        set => _innerHandler.LanguageTagValidation = value;
    }
    
    /// <inheritdoc />
    public Uri ResolveQName(string qName)
    {
        return _innerHandler.ResolveQName(qName);
    }

    /// <inheritdoc />
    public void StartRdf()
    {
        _innerHandler.StartRdf();
    }

    /// <inheritdoc />
    public void EndRdf(bool ok)
    {
        if (ok)
        {
            ok = EmitPatternCopy();
        }
        _innerHandler.EndRdf(ok);
    }

    /// <inheritdoc />
    public bool HandleNamespace(string prefix, Uri namespaceUri)
    {
        return _innerHandler.HandleNamespace(prefix, namespaceUri);
    }

    /// <inheritdoc />
    public bool HandleBaseUri(Uri baseUri)
    {
        return _innerHandler.HandleBaseUri(baseUri);
    }

    /// <inheritdoc />
    public bool HandleTriple(Triple t)
    {
        if (_patternTriples.TryGetValue(t.Subject, out List<Triple> triples))
        {
            // Add this triple to the list of triples defined by the pattern t.Subject
            // unless it is a (re)declaration of the RDFa Pattern type
            if (!(t.HasPredicate(_rdfType) && t.HasObject(_rdfaPattern)))
            {
                triples.Add(t);
            }
            // Return without passing to the inner handler
            return true;
        }

        if (t.HasPredicate(_rdfType) && t.HasObject(_rdfaPattern))
        {
            // Record a new (initially empty) list of pattern triples
            if (!_patternTriples.ContainsKey(t.Subject))
            {
                _patternTriples[t.Subject] = new List<Triple>();
            }
            // Return without passing to the inner handler
            return true;
        }

        if (t.HasPredicate(_rdfaCopy))
        {
            // Create a new empty pattern if we haven't encountered this pattern before
            if (!_patternTriples.ContainsKey(t.Object))
            {
                _patternTriples[t.Object] = new List<Triple>();
            }

            // Record a reference from t.Subject to the pattern defined by t.Object
            if (_patternRefs.TryGetValue(t.Subject, out List<INode> refList))
            {
                refList.Add(t.Object);
            }
            else
            {
                _patternRefs[t.Subject] = new List<INode> { t.Object };
            }
            // Return without passing to the inner handler
            return true;
        }

        return _innerHandler.HandleTriple(t);
    }

    /// <inheritdoc />
    public bool HandleQuad(Triple t, IRefNode graph)
    {
        return _innerHandler.HandleQuad(t, graph);
    }

    /// <inheritdoc />
    public bool AcceptsAll => _innerHandler.AcceptsAll;

    private bool EmitPatternCopy()
    {
        var referencedPatterns = new HashSet<INode>();
        foreach (INode copySubject in _patternRefs.Keys)
        {
            var patternsToCopy = new List<INode>(_patternRefs[copySubject]);
            for (var i = 0; i < patternsToCopy.Count; i++)
            {
                INode patternNode = patternsToCopy[i];
                referencedPatterns.Add(patternNode);
                if (_patternTriples.TryGetValue(patternNode, out List<Triple> patternTriples))
                {
                    foreach (Triple patternTriple in patternTriples)
                    {
                        if (patternTriple.HasPredicate(_rdfaCopy))
                        {
                            // Push the nested pattern reference to the end of the list of patterns to process if it isn't already in the list
                            if (!patternsToCopy.Contains(patternTriple.Object))
                            {
                                patternsToCopy.Add(patternTriple.Object);
                            }
                        }
                        else
                        {
                            // Copy other pattern triples
                            if (!_innerHandler.HandleTriple(new Triple(copySubject, patternTriple.Predicate,
                                    patternTriple.Object)))
                            {
                                return false;
                            }
                        }
                    }
                }
            }
        }
        // Emit any unreferenced patterns
        foreach (INode unreferencedPattern in _patternTriples.Keys.Where(node => !referencedPatterns.Contains(node)))
        {
            if (!_innerHandler.HandleTriple(new Triple(unreferencedPattern, _rdfType, _rdfaPattern)))
            {
                return false;
            }

            foreach (Triple t in _patternTriples[unreferencedPattern])
            {
                if (!_innerHandler.HandleTriple(t)) return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public IEnumerable<IRdfHandler> InnerHandlers => new[]{_innerHandler};
}
