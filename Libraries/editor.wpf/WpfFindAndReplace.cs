using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using ICSharpCode.AvalonEdit;

namespace VDS.RDF.Utilities.Editor.Wpf
{
    public class WpfFindAndReplace
        : FindAndReplace<TextEditor>
    {

        protected override bool ShouldRestartSearchFromStart()
        {
            return (MessageBox.Show("No further instances of the Find Text were found.  Would you like to restart the search from beginning of document?", "Text Not Found", MessageBoxButton.YesNo) == MessageBoxResult.Yes);
        }

        protected override void ShowMessage(string message)
        {
            MessageBox.Show(message, "Find and Replace");
        }
    }
}
