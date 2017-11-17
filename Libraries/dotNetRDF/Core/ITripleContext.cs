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

using System;
using System.Collections.Generic;

namespace VDS.RDF
{
    /// <summary>
    /// An Interface for classes which provide Context Information for Triples thus allowing you to create Quads with arbitrary extra information attached to Triples via your Context Objects
    /// </summary>
    /// <remarks>
    /// A Triple Context is simply a name-value pair collection of arbitrary data that can be attached to a Triple.  Internal representation of this is left to the implementor.
    /// </remarks>
    public interface ITripleContext
    {
        /// <summary>
        /// A Method which will indicate whether the Context contains some arbitrary property
        /// </summary>
        bool HasProperty(String name);

        /// <summary>
        /// A Property which exposes the arbitrary properties of the Context as an Key Based Index
        /// </summary>
        /// <param name="name">Name of the Property</param>
        /// <returns></returns>
        Object this[String name]
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Class which implements a very basic Triple Context
    /// </summary>
    /// <remarks>
    /// The Name Value collection is represented internally as a Dictionary
    /// </remarks>
    public class BasicTripleContext : ITripleContext
    {
        private Dictionary<String, Object> _properties;

        /// <summary>
        /// Creates a new Basic Triple Context without a Source
        /// </summary>
        public BasicTripleContext()
        {
            _properties = new Dictionary<string, object>();
        }

        /// <summary>
        /// Checks whether a given property is defined in this Context object
        /// </summary>
        /// <param name="name">Name of the Property</param>
        /// <returns></returns>
        public bool HasProperty(string name)
        {
            return _properties.ContainsKey(name);
        }

        /// <summary>
        /// Gets/Sets the value of a Property
        /// </summary>
        /// <param name="name">Name of the Property</param>
        /// <returns></returns>
        public object this[string name]
        {
            get
            {
                // Check if the Property exists
                if (_properties.ContainsKey(name))
                {
                    // Return the Property
                    return _properties[name];
                }
                else
                {
                    // Return a Null when the Property doesn't exist
                    return null;
                }
            }
            set
            {
                // Check if the Property exists
                if (_properties.ContainsKey(name))
                {
                    // Update the Property
                    _properties[name] = value;
                }
                else
                {
                    // Define a new Property
                    _properties.Add(name, value);
                }
            }
        }
    }
}
