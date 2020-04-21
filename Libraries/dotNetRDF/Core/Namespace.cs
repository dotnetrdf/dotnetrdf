/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2020 dotNetRDF Project (http://dotnetrdf.org/)
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
using System.Dynamic;
using System.Reflection;
using System.Text;

namespace VDS.RDF.Core
{
    public class Namespace : DynamicObject
    {
        private string BaseUri { get; }

        public Namespace(string baseUri)
        {
            if (baseUri == null) throw new ArgumentNullException(nameof(baseUri), "Base URI must not be null");
            if (string.Empty.Equals(baseUri)) throw new ArgumentException("Base Uri must be a non-empty string", nameof(baseUri));
            BaseUri = baseUri;
        }

        public Expansion this[string suffix] => new Expansion(BaseUri + suffix);

        /// <inheritdoc />
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (binder.ReturnType.IsAssignableFrom(typeof(Expansion)))
            {
                result = new Expansion(BaseUri + binder.Name);
                return true;
            }
            result = null;
            return false;
        }


        public static readonly dynamic Xsd = new Namespace("http://www.w3.org/2001/XMLSchema#");
        public static readonly dynamic Rdf = new Namespace("http://www.w3.org/1999/02/22-rdf-syntax-ns#");
        public static readonly dynamic Rdfs = new Namespace("http://www.w3.org/2000/01/rdf-schema#");
    }

    public class Expansion
    {
        private readonly string _value;

        public Expansion(string value)
        {
            _value = value;
        }

        public static implicit operator Uri(Expansion e)
        {
            return e.AsUri();
        }

        public static implicit operator string(Expansion e)
        {
            return e.AsString();
        }

        public Uri AsUri()
        {
            return UriFactory.Create(_value);
        }

        public string AsString()
        {
            return _value;
        }
    }

}
