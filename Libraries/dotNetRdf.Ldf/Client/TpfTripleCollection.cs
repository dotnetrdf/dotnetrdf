/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2026 dotNetRDF Project (http://dotnetrdf.org/)
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
using VDS.RDF.Parsing;

namespace VDS.RDF.LDF.Client;

internal class TpfTripleCollection : BaseTripleCollection
{
    private readonly IriTemplate _template;
    private readonly IRdfReader _reader;
    private readonly Loader _loader;

    internal TpfTripleCollection(IriTemplate template, IRdfReader reader = null, Loader loader = null)
    {
        this._template = template ?? throw new ArgumentNullException(nameof(template));
        this._reader = reader;
        this._loader = loader;
    }

    /// <remarks>Caution: When the LDF response has no triple count statement in the metadata section, then every invocation of this property enumerates the collection which potentially involves numerous network requests.</remarks>
    public override int Count
    {
        get
        {
            using var fragment = new TpfLoader(new TpfParameters(_template), _reader, _loader);

            return fragment.Metadata.TripleCount switch
            {
                null => this.Count(),
                < 0 => default,
                > int.MaxValue => int.MaxValue,
                var castable => (int)castable
            };
        }
    }

    public override IEnumerable<INode> ObjectNodes => (from t in this select t.Object).Distinct();

    public override IEnumerable<INode> PredicateNodes => (from t in this select t.Predicate).Distinct();

    public override IEnumerable<INode> SubjectNodes => (from t in this select t.Subject).Distinct();

    public override Triple this[Triple t] => Contains(t) ? t : throw new KeyNotFoundException();

    public override bool Contains(Triple t) => TpfEnumerable(t.Subject, t.Predicate, t.Object).Any();

    public override IEnumerator<Triple> GetEnumerator() => new TpfEnumerator(new TpfParameters(_template), _reader, _loader);

    public override IEnumerable<Triple> WithObject(INode o) => TpfEnumerable(o: o);

    public override IEnumerable<Triple> WithPredicate(INode p) => TpfEnumerable(p: p);

    public override IEnumerable<Triple> WithPredicateObject(INode p, INode o) => TpfEnumerable(p: p, o: o);

    public override IEnumerable<Triple> WithSubject(INode s) => TpfEnumerable(s);

    public override IEnumerable<Triple> WithSubjectObject(INode s, INode o) => TpfEnumerable(s, o: o);

    public override IEnumerable<Triple> WithSubjectPredicate(INode s, INode p) => TpfEnumerable(s, p);

    public override IEnumerable<Triple> Asserted => this;

    private IEnumerable<Triple> TpfEnumerable(INode s = null, INode p = null, INode o = null) => new TpfEnumerable(new TpfParameters(_template, s, p, o), _reader, _loader);

    #region Mutation methods throw because this triple collection is read-only

    protected override bool Add(Triple t) => throw new NotSupportedException("This triple collection is read-only.");

    protected override bool Delete(Triple t) => throw new NotSupportedException("This triple collection is read-only.");

    #endregion

    #region Some methods and properties short-circuit to empty due to unsupported features in LDF

    public override bool ContainsQuoted(Triple t) => default;

    public override IEnumerable<Triple> Quoted => [];

    public override int QuotedCount => default;

    public override IEnumerable<INode> QuotedObjectNodes => [];

    public override IEnumerable<INode> QuotedPredicateNodes => [];

    public override IEnumerable<INode> QuotedSubjectNodes => [];

    public override IEnumerable<Triple> QuotedWithObject(INode obj) => [];

    public override IEnumerable<Triple> QuotedWithPredicate(INode pred) => [];

    public override IEnumerable<Triple> QuotedWithPredicateObject(INode pred, INode obj) => [];

    public override IEnumerable<Triple> QuotedWithSubject(INode subj) => [];

    public override IEnumerable<Triple> QuotedWithSubjectObject(INode subj, INode obj) => [];

    public override IEnumerable<Triple> QuotedWithSubjectPredicate(INode subj, INode pred) => [];

    #endregion
}
