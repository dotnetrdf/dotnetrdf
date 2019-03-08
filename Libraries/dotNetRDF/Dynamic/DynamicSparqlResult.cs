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

namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;
    using VDS.RDF.Query;

    /// <summary>
    /// Provides read/write dictionary and dynamic functionality for <see cref="SparqlResult">SPARQL results</see>.
    /// </summary>
    public class DynamicSparqlResult : IDictionary<string, object>, IDynamicMetaObjectProvider
    {
        private readonly SparqlResult original;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicSparqlResult"/> class.
        /// </summary>
        /// <param name="original">The SPARQL result to wrap.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="original"/> is null.</exception>
        public DynamicSparqlResult(SparqlResult original)
        {
            if (original is null)
            {
                throw new ArgumentNullException(nameof(original));
            }

            this.original = original;
        }

        /// <summary>
        /// Gets or sets values equivalent to bindings in the result.
        /// </summary>
        /// <param name="variable"></param>
        /// <returns>The binding converted to a native object.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="variable"/> is null.</exception>
        public object this[string variable]
        {
            get
            {
                if (variable is null)
                {
                    throw new ArgumentNullException(nameof(variable));
                }

                return this.original[variable].AsObject();
            }

            set
            {
                if (variable is null)
                {
                    throw new ArgumentNullException(nameof(variable));
                }

                this.original.SetValue(variable, value.AsNode());
            }
        }

        /// <summary>
        /// Gets the variable names in the SPARQL result.
        /// </summary>
        public ICollection<string> Keys
        {
            get
            {
                return (ICollection<string>)this.original.Variables;
            }
        }

        /// <summary>
        /// Gets native values equivalent to bindings in the result.
        /// </summary>
        public ICollection<object> Values
        {
            get
            {
                return this.original.Select(result => result.Value.AsObject()).ToList();
            }
        }

        /// <summary>
        /// Gets the number of variables in the result.
        /// </summary>
        public int Count
        {
            get
            {
                return this.original.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this SPARQL result dictionary is read only (always false).
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Binds a variable to a node equivalent to <paramref name="value"/>.
        /// </summary>
        /// <param name="variable">The name of the variable to bind.</param>
        /// <param name="value">An object that is converted to an equivalent node and bound to the variable.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="variable"/> is null.</exception>
        public void Add(string variable, object value)
        {
            if (variable is null)
            {
                throw new ArgumentNullException(nameof(variable));
            }

            this[variable] = value.AsNode();
        }

        /// <inheritdoc/>
        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
        {
            this.Add(item.Key, item.Value.AsNode());
        }

        /// <summary>
        /// Removes all variables in the result.
        /// </summary>
        public void Clear()
        {
            foreach (var variable in this.Keys)
            {
                this.Remove(variable);
            }

            this.original.Trim();
        }

        /// <inheritdoc/>
        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            if (!this.original.TryGetValue(item.Key, out var value))
            {
                return false;
            }

            return value.Equals(item.Value.AsNode());
        }

        /// <summary>
        /// Checks whether a variable exists in the result.
        /// </summary>
        /// <param name="variable">The name of the variable to check.</param>
        /// <returns>Whether a variable exists in the result.</returns>
        public bool ContainsKey(string variable)
        {
            if (variable is null)
            {
                return false;
            }

            return this.original.HasValue(variable);
        }

        /// <inheritdoc/>
        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            this.original.Select(pair => new KeyValuePair<string, object>(pair.Key, pair.Value.AsObject())).ToArray().CopyTo(array, arrayIndex);
        }

        /// <inheritdoc/>
        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return this.original.Select(pair => new KeyValuePair<string, object>(pair.Key, pair.Value.AsObject())).GetEnumerator();
        }

        /// <summary>
        /// Unbinds a variable from the result.
        /// </summary>
        /// <param name="variable">The variable to unbind.</param>
        /// <returns>Whether a variable was removed.</returns>
        public bool Remove(string variable)
        {
            if (variable is null)
            {
                return false;
            }

            if (!this.original.HasValue(variable))
            {
                return false;
            }

            this[variable] = null;
            this.original.Trim();
            return true;
        }

        /// <inheritdoc/>
        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            if (!this.original.TryGetValue(item.Key, out var value))
            {
                return false;
            }

            if (!value.Equals(item.Value.AsNode()))
            {
                return false;
            }

            this[item.Key] = null;
            this.original.Trim();
            return true;
        }

        /// <summary>
        /// Tries to get a native value equivalent to a binding from the result.
        /// </summary>
        /// <param name="variable">The name of the variable to try.</param>
        /// <param name="value">A native value equivalent to the binding.</param>
        /// <returns>Whether <paramref name="value"/> was set.</returns>
        public bool TryGetValue(string variable, out object value)
        {
            value = null;

            if (variable is null)
            {
                return false;
            }

            if (!this.original.TryGetValue(variable, out var originalValue))
            {
                return false;
            }

            value = originalValue.AsObject();
            return true;
        }

        /// <summary>
        /// Returns an enumerator that iterates through pairs of variable names and native values equivalent to bindings in the result.
        /// </summary>
        /// <returns>An enumerator that iterates through pairs of variable names and native values equivalent to bindings in the result.</returns>
        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, object>>)this).GetEnumerator();
        }

        /// <inheritdoc/>
        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
        {
            return new DictionaryMetaObject(parameter, this);
        }
    }
}