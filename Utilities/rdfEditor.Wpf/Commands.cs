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
