using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace rdfEditor
{
    public static class Commands
    {
        public static readonly ICommand SaveWithNTriples = new RoutedCommand();
        public static readonly ICommand SaveWithTurtle = new RoutedCommand();
        public static readonly ICommand SaveWithN3 = new RoutedCommand();
        public static readonly ICommand SaveWithRdfXml = new RoutedCommand();
        public static readonly ICommand SaveWithRdfJson = new RoutedCommand();
        public static readonly ICommand SaveWithXHtmlRdfA = new RoutedCommand();

        public static readonly ICommand ToggleLineNumbers = new RoutedCommand();
        public static readonly ICommand ToggleWordWrap = new RoutedCommand();
        public static readonly ICommand ToggleHighlighting = new RoutedCommand();
        public static readonly ICommand ToggleAutoCompletion = new RoutedCommand();
        public static readonly ICommand ToggleValidateAsYouType = new RoutedCommand();
    }
}
