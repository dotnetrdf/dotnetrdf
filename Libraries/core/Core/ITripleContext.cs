/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

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
            this._properties = new Dictionary<string, object>();
        }

        /// <summary>
        /// Checks whether a given property is defined in this Context object
        /// </summary>
        /// <param name="name">Name of the Property</param>
        /// <returns></returns>
        public bool HasProperty(string name)
        {
            return this._properties.ContainsKey(name);
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
                //Check if the Property exists
                if (this._properties.ContainsKey(name))
                {
                    //Return the Property
                    return this._properties[name];
                }
                else
                {
                    //Return a Null when the Property doesn't exist
                    return null;
                }
            }
            set
            {
                //Check if the Property exists
                if (this._properties.ContainsKey(name))
                {
                    //Update the Property
                    this._properties[name] = value;
                }
                else
                {
                    //Define a new Property
                    this._properties.Add(name, value);
                }
            }
        }
    }
}
