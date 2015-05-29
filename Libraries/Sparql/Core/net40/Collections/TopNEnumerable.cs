/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

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

namespace VDS.RDF.Collections
{
    public class TopNEnumerable<T>
        : WrapperEnumerable<T>
    {
        public TopNEnumerable(IEnumerable<T> enumerable, IComparer<T> comparer, long n)
            : base(enumerable)
        {
            if (comparer == null) throw new ArgumentNullException("comparer");
            this.Comparer = comparer;
            if (n < 1) throw new ArgumentException("N must be >= 1", "n");
            this.N = n;
        }

        public IComparer<T> Comparer { get; private set; }

        public long N { get; private set; }

        public override IEnumerator<T> GetEnumerator()
        {
            return new TopNEnumerator<T>(this.InnerEnumerable.GetEnumerator(), this.Comparer, this.N);
        }
    }

    public class TopNEnumerator<T>
        : WrapperEnumerator<T>
    {
        public TopNEnumerator(IEnumerator<T> enumerator, IComparer<T> comparer, long n)
            : base(enumerator)
        {
            if (comparer == null) throw new ArgumentNullException("comparer");
            if (n < 1) throw new ArgumentException("N must be >= 1", "n");
            this.N = n;
            this.TopItems = new SortedList<T, byte>(comparer);
        }

        public long N { get; private set; }

        private SortedList<T, byte> TopItems { get; set; }

        private IEnumerator<KeyValuePair<T, byte>> TopItemsEnumerator { get; set; } 

        protected override bool TryMoveNext(out T item)
        {
            // First time this is accessed need to populate the Top N items list
            if (this.TopItemsEnumerator == null)
            {
                while (this.InnerEnumerator.MoveNext())
                {
                    this.TopItems.Add(this.InnerEnumerator.Current, 0);
                    if (this.TopItems.Count > this.N) this.TopItems.RemoveAt(this.TopItems.Count - 1);
                }
                this.TopItemsEnumerator = this.TopItems.GetEnumerator();
            }

            // Afterwards we just pull items from that list
            item = default(T);
            if (!this.TopItemsEnumerator.MoveNext()) return false;
            item = this.TopItemsEnumerator.Current.Key;
            return true;
        }

        protected override void ResetInternal()
        {
            this.TopItems.Clear();
            this.TopItemsEnumerator.Dispose();
            this.TopItemsEnumerator = null;
        }
    }
}
