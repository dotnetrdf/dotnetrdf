/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2021 dotNetRDF Project (http://dotnetrdf.org/)
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
using VDS.RDF.Query.Construct;

namespace VDS.RDF.Query.Patterns
{
    public class QuotedTriplePattern : PatternItem
    {
        public TriplePattern QuotedTriple { get; }
        public QuotedTriplePattern(TriplePattern qtPattern)
        {
            QuotedTriple = qtPattern;
        }

        public override bool Accepts(IPatternEvaluationContext context, INode obj)
        {
            throw new NotImplementedException();
        }

        public override INode Construct(ConstructContext context)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }

        // TODO: Change signature of PatternItem to allow for patterns with multiple variables
        public override string VariableName => QuotedTriple.Variables.FirstOrDefault();

        public bool HasNoExplicitVariables => QuotedTriple.HasNoExplicitVariables;
        
        public bool HasNoBlankVariables => QuotedTriple.HasNoBlankVariables;
    }
}
