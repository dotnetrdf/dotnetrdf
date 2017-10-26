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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF
{
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
            this._dim = dimension;
            this._g = g;
            this._rdfValue = this._g.CreateUriNode(new Uri(NamespaceMapper.RDF + "value"));
            this._corners = new INode[1 << dimension];

            for (int i = 0; i < this._corners.Length; i++)
            {
                this._corners[i] = this._g.CreateBlankNode();
            }
            for (int i = 0; i < this._corners.Length; i++)
            {
                this.AddTriple(i, this._corners[i]);
            }
        }

        private void AddTriple(int corner, INode n)
        {
            for (int j = 0; j < this._dim; j++)
            {
                int bit = 1 << j;
                this._g.Assert(n, this._rdfValue, this._corners[corner ^ bit]);
            }
        }

        public Hypercube Duplicate(int corner)
        {
            INode n = this._g.CreateBlankNode();
            this.AddTriple(corner, n);
            return this;
        }

        public Hypercube Toggle(int from, int to)
        {
            INode f = this._corners[from];
            INode t = this._corners[to];

            Triple triple = new Triple(f, this._rdfValue, t);
            if (this._g.ContainsTriple(triple))
            {
                this._g.Retract(triple);
            }
            else
            {
                this._g.Assert(triple);
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
            this._dim = dimension;
            this._g = g;
            this._rdfValue = this._g.CreateUriNode(new Uri(NamespaceMapper.RDF + "value"));
            this._corners = new INode[1 << dimension];

            for (int i = 0; i < this._corners.Length; i++)
            {
                this._corners[i] = this._g.CreateBlankNode();
            }
            for (int i = 0; i < this._corners.Length; i++)
            {
                this.AddTriple(i, this._corners[i]);
            }
        }

        private void AddTriple(int corner, INode n)
        {
            for (int j = 0; j < this._dim; j++)
            {
                int bit = 1 << j;
                if ((corner & bit) != 0)
                {
                    this._g.Assert(n, this._rdfValue, this._corners[corner ^ bit]);
                }
            }
        }

        public DiHypercube Duplicate(int corner)
        {
            INode n = this._g.CreateBlankNode();
            for (int j = 0; j < this._dim; j++)
            {
                int bit = 1 << j;
                if ((corner & bit) != 0)
                {
                    this._g.Assert(n, this._rdfValue, this._corners[corner ^ bit]);
                }
                else
                {
                    this._g.Assert(this._corners[corner ^ bit], this._rdfValue, n);
                }
            }
            return this;
        }
    }
}
