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
using System.ComponentModel;
using System.Linq;
using System.Text;
using VDS.RDF.GUI.WinForms.Controls;

namespace VDS.RDF.GUI.WinForms
{
    /// <summary>
    /// Event that occurs when a URI is clicked
    /// </summary>
    /// <param name="sender">Originator of the event</param>
    /// <param name="u">URI that was clicked</param>
    public delegate void UriClickedEventHandler(Object sender, Uri u);

    /// <summary>
    /// Event that occurs when the formatter is changed
    /// </summary>
    /// <param name="sender">Originator of the event</param>
    /// <param name="formatter">Formatter that is now selected</param>
    public delegate void FormatterChanged(Object sender, Formatter formatter);

    /// <summary>
    /// Event that occurs when result are requested to be closed
    /// </summary>
    /// <param name="sender">Originator of the event</param>
    public delegate void ResultCloseRequested(Object sender);

    /// <summary>
    /// Event that occurs when results are requested to be detached
    /// </summary>
    /// <param name="sender">Originator of the event</param>
    public delegate void ResultDetachRequested(Object sender);
}
