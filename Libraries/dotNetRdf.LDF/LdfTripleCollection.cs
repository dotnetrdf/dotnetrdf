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
using System.Linq;
using VDS.RDF.LDF.Hydra;

namespace VDS.RDF.LDF;

internal class LdfTripleCollection : BaseTripleCollection
{
    private readonly IriTemplate template;

    internal LdfTripleCollection(IriTemplate template) => this.template = template;

    public override int Count // TODO: 0 if missing
    {
        get
        {
            using var loader = new LdfLoader(new Parameters(template));
            return (int)loader.Metadata.TripleCount;
        }
    }

    public override IEnumerable<INode> ObjectNodes => this.Select(t => t.Object).Distinct();

    public override IEnumerable<INode> PredicateNodes => this.Select(t => t.Predicate).Distinct();

    public override IEnumerable<INode> SubjectNodes => this.Select(t => t.Subject).Distinct();

    public override Triple this[Triple t] => Contains(t) ? t : throw new KeyNotFoundException();

    public override bool Contains(Triple t) => new LdfEnumerable(new Parameters(template, t.Subject, t.Predicate, t.Object)).Any();

    public override void Dispose() { }

    public override IEnumerator<Triple> GetEnumerator() => new LdfEnumerator(new Parameters(template));

    public override IEnumerable<Triple> WithObject(INode obj) => new LdfEnumerable(new Parameters(template, @object: obj));

    public override IEnumerable<Triple> WithPredicate(INode pred) => new LdfEnumerable(new Parameters(template, predicate: pred));

    public override IEnumerable<Triple> WithPredicateObject(INode pred, INode obj) => new LdfEnumerable(new Parameters(template, predicate: pred, @object: obj));

    public override IEnumerable<Triple> WithSubject(INode subj) => new LdfEnumerable(new Parameters(template, subj));

    public override IEnumerable<Triple> WithSubjectObject(INode subj, INode obj) => new LdfEnumerable(new Parameters(template, subj, @object: obj));

    public override IEnumerable<Triple> WithSubjectPredicate(INode subj, INode pred) => new LdfEnumerable(new Parameters(template, subj, pred));

    public override IEnumerable<Triple> Asserted => this;

    #region Mutation methods throw because this triple collection is read-only

    protected override bool Add(Triple t) => throw new NotSupportedException("This triple collection is read-only.");

    protected override bool Delete(Triple t) => throw new NotSupportedException("This triple collection is read-only.");

    #endregion

    #region Some methods and properties short-circuit to empty due to unsupported features in LDF

    public override bool ContainsQuoted(Triple t) => false;

    public override IEnumerable<Triple> Quoted => Enumerable.Empty<Triple>();

    public override int QuotedCount => 0;

    public override IEnumerable<INode> QuotedObjectNodes => Enumerable.Empty<INode>();

    public override IEnumerable<INode> QuotedPredicateNodes => Enumerable.Empty<INode>();

    public override IEnumerable<INode> QuotedSubjectNodes => Enumerable.Empty<INode>();

    public override IEnumerable<Triple> QuotedWithObject(INode obj) => Enumerable.Empty<Triple>();

    public override IEnumerable<Triple> QuotedWithPredicate(INode pred) => Enumerable.Empty<Triple>();

    public override IEnumerable<Triple> QuotedWithPredicateObject(INode pred, INode obj) => Enumerable.Empty<Triple>();

    public override IEnumerable<Triple> QuotedWithSubject(INode subj) => Enumerable.Empty<Triple>();

    public override IEnumerable<Triple> QuotedWithSubjectObject(INode subj, INode obj) => Enumerable.Empty<Triple>();

    public override IEnumerable<Triple> QuotedWithSubjectPredicate(INode subj, INode pred) => Enumerable.Empty<Triple>();

    #endregion
}
