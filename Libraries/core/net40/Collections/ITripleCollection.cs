/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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

using System.Collections.Generic;
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;

namespace VDS.RDF.Collections
{
    /// <summary>
    /// Interface for Triple Collections, a triple collection is a set of triples thus duplicates are ignored.
    /// </summary>
    public interface ITripleCollection
        : IRdfCollection<Triple>
    {
        IEnumerable<INode> ObjectNodes { get; }

        IEnumerable<INode> PredicateNodes { get; }

        IEnumerable<INode> SubjectNodes { get; }

        IEnumerable<Triple> WithObject(INode obj);

        IEnumerable<Triple> WithPredicate(INode pred);

        IEnumerable<Triple> WithPredicateObject(INode pred, INode obj);

        IEnumerable<Triple> WithSubject(INode subj);

        IEnumerable<Triple> WithSubjectObject(INode subj, INode obj);

        IEnumerable<Triple> WithSubjectPredicate(INode subj, INode pred);
    }
}
