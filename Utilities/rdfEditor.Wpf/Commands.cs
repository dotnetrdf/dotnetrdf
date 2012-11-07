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
using System.Windows.Input;

namespace VDS.RDF.Utilities.Editor.Wpf
{
    /// <summary>
    /// Editor Commands
    /// </summary>
    public static class Commands
    {
        public static readonly ICommand NewFromActive = new RoutedCommand();
        public static readonly ICommand SaveAll = new RoutedCommand();
        public static readonly ICommand SaveWithNTriples = new RoutedCommand();
        public static readonly ICommand SaveWithTurtle = new RoutedCommand();
        public static readonly ICommand SaveWithN3 = new RoutedCommand();
        public static readonly ICommand SaveWithRdfXml = new RoutedCommand();
        public static readonly ICommand SaveWithRdfJson = new RoutedCommand();
        public static readonly ICommand SaveWithXHtmlRdfA = new RoutedCommand();
        public static readonly ICommand CloseAll = new RoutedCommand();

        public static readonly ICommand ConvertToNTriples = new RoutedCommand();
        public static readonly ICommand ConvertToTurtle = new RoutedCommand();
        public static readonly ICommand ConvertToN3 = new RoutedCommand();
        public static readonly ICommand ConvertToRdfXml = new RoutedCommand();
        public static readonly ICommand ConvertToRdfJson = new RoutedCommand();
        public static readonly ICommand ConvertToXHtmlRdfa = new RoutedCommand();

        public static readonly ICommand Find = new RoutedCommand();
        public static readonly ICommand FindNext = new RoutedCommand();
        public static readonly ICommand Replace = new RoutedCommand();
        public static readonly ICommand GoToLine = new RoutedCommand();

        public static readonly ICommand IncreaseTextSize = new RoutedCommand();
        public static readonly ICommand DecreaseTextSize = new RoutedCommand();
        public static readonly ICommand ResetTextSize = new RoutedCommand();

        public static readonly ICommand CommentSelection = new RoutedCommand();
        public static readonly ICommand UncommentSelection = new RoutedCommand();

        public static readonly ICommand ToggleLineNumbers = new RoutedCommand();
        public static readonly ICommand ToggleWordWrap = new RoutedCommand();
        public static readonly ICommand ToggleClickableUris = new RoutedCommand();

        public static readonly ICommand ToggleSymbolSelection = new RoutedCommand();

        public static readonly ICommand ToggleHighlighting = new RoutedCommand();
        public static readonly ICommand ToggleAutoCompletion = new RoutedCommand();
        public static readonly ICommand ToggleValidateAsYouType = new RoutedCommand();
        public static readonly ICommand ToggleValidationErrorHighlighting = new RoutedCommand();

        public static readonly ICommand ValidateSyntax = new RoutedCommand();
    }
}
