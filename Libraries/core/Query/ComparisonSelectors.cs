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
using System.Linq;
using System.Text;

namespace VDS.RDF.Query
{
    /// <summary>
    /// A Selector which finds all Triples where the Predicate is a given Property and the Value is less than a given Value
    /// </summary>
    public class HasPropertyLessThanSelector : ISelector<Triple>
    {
        private INode _pred, _value;

        /// <summary>
        /// Creates a new HasPropertyLessThanSelector for the given Property and Value
        /// </summary>
        /// <param name="property">Property</param>
        /// <param name="value">Value that the Property should be Less Than</param>
        public HasPropertyLessThanSelector(INode property, INode value)
        {
            this._pred = property;
            this._value = value;
        }

        /// <summary>
        /// Accepts Triples where the Predicate is the desired Property and the Object has a value Less Than the value specified when the Selector was instantiated with
        /// </summary>
        /// <param name="obj">Triple to Test</param>
        /// <returns></returns>
        public bool Accepts(Triple obj)
        {
            //Firstly Predicates must match
            if (this._pred.Equals(obj.Predicate))
            {
                //Then Value must be less than or equal to set Value
                int c = obj.Object.CompareTo(this._value);
                return (c < 0);
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// A Selector which finds all Triples where the Predicate is a given Property and the Value is less than or equal to a given Value
    /// </summary>
    public class HasPropertyLessThanOrEqualToSelector : ISelector<Triple>
    {
        private INode _pred, _value;

        /// <summary>
        /// Creates a new HasPropertyLessThanOrEqualSelector for the given Property and Value
        /// </summary>
        /// <param name="property">Property</param>
        /// <param name="value">Value that the Property should be Less Than or Equal To</param>
        public HasPropertyLessThanOrEqualToSelector(INode property, INode value)
        {
            this._pred = property;
            this._value = value;
        }

        /// <summary>
        /// Accepts Triples where the Predicate is the desired Property and the Object has a value Less Than or Equal To the value specified when the Selector was instantiated with
        /// </summary>
        /// <param name="obj">Triple to Test</param>
        /// <returns></returns>
        public bool Accepts(Triple obj)
        {
            //Firstly Predicates must match
            if (this._pred.Equals(obj.Predicate))
            {
                //Then Value must be less than or equal to set Value
                int c = obj.Object.CompareTo(this._value);
                return (c <= 0);
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// A Selector which finds all Triples where the Predicate is a given Property and the Value is greater than a given Value
    /// </summary>
    public class HasPropertyGreaterThanSelector : ISelector<Triple>
    {
        private INode _pred, _value;

        /// <summary>
        /// Creates a new HasPropertyGreaterThanSelector for the given Property and Value
        /// </summary>
        /// <param name="property">Property</param>
        /// <param name="value">Value that the Property should be Greater Than</param>
        public HasPropertyGreaterThanSelector(INode property, INode value)
        {
            this._pred = property;
            this._value = value;
        }

        /// <summary>
        /// Accepts Triples where the Predicate is the desired Property and the Object has a value Greater Than the value specified when the Selector was instantiated with
        /// </summary>
        /// <param name="obj">Triple to Test</param>
        /// <returns></returns>
        public bool Accepts(Triple obj)
        {
            //Firstly Predicates must match
            if (this._pred.Equals(obj.Predicate))
            {
                //Then Value must be less than or equal to set Value
                int c = obj.Object.CompareTo(this._value);
                return (c > 0);
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// A Selector which finds all Triples where the Predicate is a given Property and the Value is greater than or equal to a given Value
    /// </summary>
    public class HasPropertyGreaterThanOrEqualToSelector : ISelector<Triple>
    {
        private INode _pred, _value;

        /// <summary>
        /// Creates a new HasPropertyGreaterThanOrEqualToSelector for the given Property and Value
        /// </summary>
        /// <param name="property">Property</param>
        /// <param name="value">Value that the Property should be Greater Than or Equal To</param>
        public HasPropertyGreaterThanOrEqualToSelector(INode property, INode value)
        {
            this._pred = property;
            this._value = value;
        }

        /// <summary>
        /// Accepts Triples where the Predicate is the desired Property and the Object has a value Greater Than or Equal To the value specified when the Selector was instantiated with
        /// </summary>
        /// <param name="obj">Triple to Test</param>
        /// <returns></returns>
        public bool Accepts(Triple obj)
        {
            //Firstly Predicates must match
            if (this._pred.Equals(obj.Predicate))
            {
                //Then Value must be greater than or equal to set Value
                int c = obj.Object.CompareTo(this._value);
                return (c >= 0);
            }
            else
            {
                return false;
            }
        }
    }
}