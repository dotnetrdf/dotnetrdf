using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using ICSharpCode.AvalonEdit.Rendering;

namespace rdfEditor.Syntax
{
    public class ValidationErrorLineText : VisualLineText
    {
        public ValidationErrorLineText(VisualLine parentLine, int length)
            : base(parentLine, length)
        {

        }

        public override TextRun CreateTextRun(int startVisualColumn, ITextRunConstructionContext context)
        {
            this.TextRunProperties.SetTextDecorations(TextDecorations.Underline);
            this.TextRunProperties.SetBackgroundBrush(Brushes.DarkRed);
            this.TextRunProperties.SetForegroundBrush(Brushes.White);
            return base.CreateTextRun(startVisualColumn, context);
        }

        protected override VisualLineText CreateInstance(int length)
        {
            return new ValidationErrorLineText(this.ParentVisualLine, length);
        }

        protected override void OnQueryCursor(System.Windows.Input.QueryCursorEventArgs e)
        {
            e.Handled = true;
            
        }
    }
}
