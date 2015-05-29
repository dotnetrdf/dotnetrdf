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
    public class LongSkipEnumerable<T>
        : WrapperEnumerable<T>
    {
        public LongSkipEnumerable(IEnumerable<T> enumerable, long toSkip)
            : base(enumerable)
        {
            if (toSkip <= 0) throw new ArgumentException("toSkip must be > 0", "toSkip");
            this.ToSkip = toSkip;
        }

        private long ToSkip { get; set; }

        public override IEnumerator<T> GetEnumerator()
        {
            return new LongSkipEnumerator<T>(this.InnerEnumerable.GetEnumerator(), this.ToSkip);
        }
    }

    public class LongSkipEnumerator<T>
        : WrapperEnumerator<T>
    {
        public LongSkipEnumerator(IEnumerator<T> enumerator, long toSkip)
            : base(enumerator)
        {
            if (toSkip <= 0) throw new ArgumentException("toSkip must be > 0", "toSkip");
            this.ToSkip = toSkip;
            this.Skipped = 0;
        }

        private long ToSkip { get; set; }

        private long Skipped { get; set; }

        private bool TrySkip()
        {
            while (this.Skipped < this.ToSkip)
            {
                if (!this.InnerEnumerator.MoveNext()) return false;
                this.Skipped++;
            }
            return true;
        }

        protected override bool TryMoveNext(out T item)
        {
            item = default(T);

            // If we've previously done the skipping so can just defer to inner enumerator
            if (this.Skipped == this.ToSkip)
            {
                if (!this.InnerEnumerator.MoveNext()) return false;
                item = this.InnerEnumerator.Current;
                return true;
            }

            // First time being accessed so attempt to skip if possible
            if (!this.TrySkip()) return false;
            item = this.InnerEnumerator.Current;
            return true;
        }

        protected override void ResetInternal()
        {
            this.Skipped = 0;
        }
    }
}
