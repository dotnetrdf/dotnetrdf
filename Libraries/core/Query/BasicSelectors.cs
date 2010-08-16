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
    /// A Selector which finds Triples which have a given value for a given Property
    /// </summary>
    public class HasPropertyValueSelector : ISelector<Triple>
    {
        private INode _prop, _value;

        /// <summary>
        /// Creates a new HasPropertyValueSelector for the given Property and Value
        /// </summary>
        /// <param name="property">Property that Triples must have as their Predicates</param>
        /// <param name="value">Value that Triples must have as their Objects</param>
        public HasPropertyValueSelector(INode property, INode value)
        {
            this._prop = property;
            this._value = value;
        }

        /// <summary>
        /// Accepts Triples which have the given Property as their Predicate and the given Value as their Object
        /// </summary>
        /// <param name="obj">Triple to test</param>
        /// <returns></returns>
        public bool Accepts(Triple obj)
        {
            //Matching Triples need to match the Property we're interested in
            if (this._prop.Equals(obj.Predicate))
            {
                //Test whether the Triple matches the required value
                return this._value.Equals(obj.Object);
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// A Selector which finds Triples which have a given Property which does not match the given Value
    /// </summary>
    public class HasNonMatchingPropertyValueSelector : ISelector<Triple>
    {
        private INode _prop, _value;

        /// <summary>
        /// Creates a new HasNonMatchingPropertyValueSelector for the given Property and Value
        /// </summary>
        /// <param name="property">Property that Triples must have as their Predicate</param>
        /// <param name="value">Value that Triples must not have as their Object</param>
        public HasNonMatchingPropertyValueSelector(INode property, INode value)
        {
            this._prop = property;
            this._value = value;
        }

        /// <summary>
        /// Accepts Triples which have the given Property as their Predicate and not the given Value as their Object
        /// </summary>
        /// <param name="obj">Triple to test</param>
        /// <returns></returns>
        public bool Accepts(Triple obj)
        {
            //Matching Triples need to match the Property we're interested in
            if (this._prop.Equals(obj.Predicate))
            {
                //Test whether the Triple doesn't match the required value
                return !this._value.Equals(obj.Object);
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// A Selector which finds Triples where a given Subject has a given Property
    /// </summary>
    public class SubjectHasPropertySelector : ISelector<Triple>
    {
        private INode _subj, _prop;

        /// <summary>
        /// Creates a new SubjectHasPropertySelector which selects Triples which have a given Subject and Predicate
        /// </summary>
        /// <param name="subject">Subject that Triples must have</param>
        /// <param name="property">Property that Triples must have as their Predicate</param>
        public SubjectHasPropertySelector(INode subject, INode property)
        {
            this._subj = subject;
            this._prop = property;
        }

        /// <summary>
        /// Accepts Triples which have the Subject and Predicate specified when the Selector was instantiated
        /// </summary>
        /// <param name="obj">Triple to test</param>
        /// <returns></returns>
        public bool Accepts(Triple obj)
        {
            if (this._subj.Equals(obj.Subject))
            {
                return this._prop.Equals(obj.Predicate);
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// A Selector which finds Triples which have a given Subject
    /// </summary>
    [Obsolete("Deprecated since using the GetTriplesWithSubject method should be far faster as it is typically indexed")]
    public class SubjectIsSelector : ISelector<Triple>
    {
        private INode _subj;

        /// <summary>
        /// Creates a new SubjectIsSelector using the given Subject
        /// </summary>
        /// <param name="subject">Subject to Select</param>
        public SubjectIsSelector(INode subject)
        {
            this._subj = subject;
        }

        /// <summary>
        /// Accepts Triples whose Subject matches the given Subject this Selector was initialised with
        /// </summary>
        /// <param name="obj">Triple to test</param>
        /// <returns></returns>
        public bool Accepts(Triple obj)
        {
            return this._subj.Equals(obj.Subject);
        }
    }

    /// <summary>
    /// A Selector which finds Triples which have a given Predicate
    /// </summary>
    [Obsolete("Deprecated since using the GetTriplesWithPredicate method should be far faster as it is typically indexed")]
    public class PredicateIsSelector : ISelector<Triple>
    {
        private INode _pred;

        /// <summary>
        /// Creates a new PredicateIsSelector using the given Predicate
        /// </summary>
        /// <param name="predicate">Predicate to Select</param>
        public PredicateIsSelector(INode predicate)
        {
            this._pred = predicate;
        }

        /// <summary>
        /// Accepts Triples whose Predicate matches the given Predicate this Selector was initialised with
        /// </summary>
        /// <param name="obj">Triple to test</param>
        /// <returns></returns>
        public bool Accepts(Triple obj)
        {
            return this._pred.Equals(obj.Predicate);
        }
    }

    /// <summary>
    /// A Selector which finds Triples which have a given Object
    /// </summary>
    [Obsolete("Deprecated since using the GetTriplesWithObject method should be far faster as it is typically indexed")]
    public class ObjectIsSelector : ISelector<Triple>
    {
        private INode _obj;

        /// <summary>
        /// Creates a new ObjectIsSelector using the given Object
        /// </summary>
        /// <param name="obj">Object to Select</param>
        public ObjectIsSelector(INode obj)
        {
            this._obj = obj;
        }

        /// <summary>
        /// Accepts Triples whose Object matches the given Object this Selector was initialised with
        /// </summary>
        /// <param name="obj">Triple to test</param>
        /// <returns></returns>
        public bool Accepts(Triple obj)
        {
            return this._obj.Equals(obj.Object);
        }
    }

    /// <summary>
    /// A Dependent Selector Wrapper which accepts Triples accepted by the underlying Selector if their Subject is the Subject of one of the Triples the Dependent Wrapper was initialised with
    /// </summary>
    public class SubjectDependentWrapperSelector : IDependentSelector<Triple>
    {

        private List<INode> _subjects = new List<INode>();
        private ISelector<Triple> _innerSelector;

        /// <summary>
        /// Creates a new SubjectDependentWrapperSelector using the given Selector
        /// </summary>
        /// <param name="selector"></param>
        public SubjectDependentWrapperSelector(ISelector<Triple> selector)
        {
            this._innerSelector = selector;
        }

        /// <summary>
        /// Intialises this Selector
        /// </summary>
        /// <param name="input">List of Triples to initialise this Selector</param>
        public void Initialise(IEnumerable<Triple> input)
        {
            foreach (Triple t in input) {
                if (!this._subjects.Contains(t.Subject))
                {
                    this._subjects.Add(t.Subject);
                }
            }
        }

        /// <summary>
        /// Accepts Triples whose Subjects were the Subject of any of the Triples this Selector was initialised with and which are accepted by the underlying selector that this Selector was instantiated with
        /// </summary>
        /// <param name="obj">Triple to test</param>
        /// <returns></returns>
        public bool Accepts(Triple obj)
        {
            //First must have a Subject in our Subject List
            if (this._subjects.Contains(obj.Subject))
            {
                //Then acceptance is based on the Inner Selector
                return this._innerSelector.Accepts(obj);
            }
            else
            {
                return false;
            }
        }
    }
}