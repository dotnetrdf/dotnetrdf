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

namespace VDS.Common
{
    /// <summary>
    /// Represents a reference whose value may change based on nesting level
    /// </summary>
    /// <typeparam name="T">Reference Type</typeparam>
    internal class NestedReference<T> 
        where T : class
    {
        private Dictionary<int, T> _values = new Dictionary<int, T>();
        private int _currLevel = 0;
        private T _currRef;

        /// <summary>
        /// Creates a new Nested Reference with an initial null value
        /// </summary>
        public NestedReference()
        {
            this._currLevel++;
        }

        /// <summary>
        /// Creates a new nested reference with an initial value
        /// </summary>
        /// <param name="initValue">Initial Value</param>
        public NestedReference(T initValue)
        {
            this._values.Add(this._currLevel, initValue);
            this._currRef = initValue;
            this._currLevel++;
        }

        /// <summary>
        /// Gets/Sets the value based on the current nesting level
        /// </summary>
        public T Value
        {
            get
            {
                return this._currRef;
            }
            set
            {
                if (this._values.ContainsKey(this._currLevel))
                {
                    this._values[this._currLevel] = value;
                }
                else
                {
                    this._values.Add(this._currLevel, value);
                }
                this._currRef = value;
            }
        }

        /// <summary>
        /// Increments the nesting level
        /// </summary>
        public void IncrementNesting()
        {
            this._currLevel++;
        }

        /// <summary>
        /// Decrements the nesting level
        /// </summary>
        public void DecrementNesting()
        {
            if (this._currLevel == 0) throw new InvalidOperationException("Cannot decrement nesting when current nesting level is 0");

            //Revert to the most recent reference
            if (this._values.ContainsKey(this._currLevel))
            {
                this._values.Remove(this._currLevel);
                int i = this._currLevel;
                while (i > 1)
                {
                    i--;
                    if (this._values.ContainsKey(i))
                    {
                        this._currRef = this._values[i];
                        break;
                    }
                    else
                    {
                        this._currRef = null;
                    }
                }
            }
            //Finally decrement the level
            this._currLevel--;
        }
    }
}
