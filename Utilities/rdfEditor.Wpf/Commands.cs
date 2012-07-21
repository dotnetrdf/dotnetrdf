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
