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

namespace VDS.RDF;

/// <summary>
/// Based on code from the Jena project - <a href="http://jena.cvs.sourceforge.net/viewvc/jena/jena2/src/test/java/com/hp/hpl/jena/regression/HyperCube.java?revision=1.1&view=markup">View original Java source</a>
/// </summary>
class Hypercube
{
    private INode[] _corners;
    private int _dim;
    private IGraph _g;
    private INode _rdfValue;

    public Hypercube(int dimension, IGraph g)
    {
        _dim = dimension;
        _g = g;
        _rdfValue = _g.CreateUriNode(new Uri(NamespaceMapper.RDF + "value"));
        _corners = new INode[1 << dimension];

        for (var i = 0; i < _corners.Length; i++)
        {
            _corners[i] = _g.CreateBlankNode();
        }
        for (var i = 0; i < _corners.Length; i++)
        {
            AddTriple(i, _corners[i]);
        }
    }

    private void AddTriple(int corner, INode n)
    {
        for (var j = 0; j < _dim; j++)
        {
            var bit = 1 << j;
            _g.Assert(n, _rdfValue, _corners[corner ^ bit]);
        }
    }

    public Hypercube Duplicate(int corner)
    {
        INode n = _g.CreateBlankNode();
        AddTriple(corner, n);
        return this;
    }

    public Hypercube Toggle(int from, int to)
    {
        INode f = _corners[from];
        INode t = _corners[to];

        var triple = new Triple(f, _rdfValue, t);
        if (_g.ContainsTriple(triple))
        {
            _g.Retract(triple);
        }
        else
        {
            _g.Assert(triple);
        }
        return this;
    }
}

/// <summary>
/// Based on code from the Jena project - <a href="http://jena.cvs.sourceforge.net/viewvc/jena/jena2/src/test/java/com/hp/hpl/jena/regression/DiHyperCube.java?revision=1.1&view=markup">View original Java source</a>
/// </summary>
class DiHypercube
{
    private INode[] _corners;
    private int _dim;
    private IGraph _g;
    private INode _rdfValue;

    public DiHypercube(int dimension, IGraph g)
    {
        _dim = dimension;
        _g = g;
        _rdfValue = _g.CreateUriNode(new Uri(NamespaceMapper.RDF + "value"));
        _corners = new INode[1 << dimension];

        for (var i = 0; i < _corners.Length; i++)
        {
            _corners[i] = _g.CreateBlankNode();
        }
        for (var i = 0; i < _corners.Length; i++)
        {
            AddTriple(i, _corners[i]);
        }
    }

    private void AddTriple(int corner, INode n)
    {
        for (var j = 0; j < _dim; j++)
        {
            var bit = 1 << j;
            if ((corner & bit) != 0)
            {
                _g.Assert(n, _rdfValue, _corners[corner ^ bit]);
            }
        }
    }

    public DiHypercube Duplicate(int corner)
    {
        INode n = _g.CreateBlankNode();
        for (var j = 0; j < _dim; j++)
        {
            var bit = 1 << j;
            if ((corner & bit) != 0)
            {
                _g.Assert(n, _rdfValue, _corners[corner ^ bit]);
            }
            else
            {
                _g.Assert(_corners[corner ^ bit], _rdfValue, n);
            }
        }
        return this;
    }
}
