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
using System.Collections;
using System.Collections.Generic;

namespace VDS.RDF.LDF
{
    internal class LdfEnumerator : IEnumerator<Triple>
    {
        private IEnumerator<Triple> e;
        private Uri uri;

        internal LdfEnumerator(Uri uri)
        {
            this.uri = uri;
        }

        Triple IEnumerator<Triple>.Current => this.e.Current;

        object IEnumerator.Current => ((IEnumerator<Triple>)this).Current;

        void IDisposable.Dispose()
        {
            this.e?.Dispose();
        }

        bool IEnumerator.MoveNext()
        {
            if (this.e is null)
            {
                using var ts = new LdfTripleStore(this.uri);

                this.e = ts.Data.GetEnumerator();
                this.uri = ts.Metadata.NextPageUri;
            }

            if (this.e.MoveNext())
            {
                return true;
            }

            if (this.uri is null)
            {
                return false;
            }

            this.e.Dispose();
            this.e = null;

            return ((IEnumerator)this).MoveNext();
        }

        void IEnumerator.Reset()
        {
            throw new NotSupportedException("This enumerator cannot be reset.");
        }
    }
}
