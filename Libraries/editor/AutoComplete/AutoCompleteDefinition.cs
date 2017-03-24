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
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Editor.AutoComplete
{
    /// <summary>
    /// Definition of an auto-completer
    /// </summary>
    public class AutoCompleteDefinition
    {
        private String _name;
        private Type _type;

        /// <summary>
        /// Creates a new auto-complete definition
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="autoCompleteType">Type of the auto-complete implementation</param>
        public AutoCompleteDefinition(String name, Type autoCompleteType)
        {
            this._name = name;
            this._type = autoCompleteType;
        }

        /// <summary>
        /// Name of the Syntax for which auto-completion is provided
        /// </summary>
        public String Name
        {
            get
            {
                return this._name;
            }
        }

        /// <summary>
        /// Type of the auto-completer
        /// </summary>
        public Type Type
        {
            get
            {
                return this._type;
            }
        }
    }
}
