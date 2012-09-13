/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

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
