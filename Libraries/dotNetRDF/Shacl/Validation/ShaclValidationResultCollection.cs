/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
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

namespace VDS.RDF.Shacl
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    internal class ShaclValidationResultCollection : ICollection<ShaclValidationResult>
    {
        private readonly ShaclValidationReport report;

        [DebuggerStepThrough]
        internal ShaclValidationResultCollection(ShaclValidationReport shaclValidationReport)
        {
            report = shaclValidationReport;
        }

        int ICollection<ShaclValidationResult>.Count => Results.Count();

        bool ICollection<ShaclValidationResult>.IsReadOnly => false;

        private IEnumerable<ShaclValidationResult> Results =>
            from result in Shacl.Result.ObjectsOf(report)
            select ShaclValidationResult.Parse(result);

        void ICollection<ShaclValidationResult>.Add(ShaclValidationResult item) => report.Graph.Assert(report, Shacl.Result, item);

        void ICollection<ShaclValidationResult>.Clear()
        {
            foreach (var result in Results.ToList())
            {
                ((ICollection<ShaclValidationResult>)this).Remove(result);
            }
        }

        bool ICollection<ShaclValidationResult>.Contains(ShaclValidationResult item) => Results.Contains(item);

        void ICollection<ShaclValidationResult>.CopyTo(ShaclValidationResult[] array, int arrayIndex) => Results.ToList().CopyTo(array, arrayIndex);

        IEnumerator<ShaclValidationResult> IEnumerable<ShaclValidationResult>.GetEnumerator() => Results.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<ShaclValidationResult>)this).GetEnumerator();

        bool ICollection<ShaclValidationResult>.Remove(ShaclValidationResult result)
        {
            var contains = ((ICollection<ShaclValidationResult>)this).Contains(result);
            report.Graph.Retract(report, Shacl.Result, result);
            return contains;
        }
    }
}