using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using ICSharpCode.AvalonEdit.Rendering;

namespace VDS.RDF.Utilities.Editor.Syntax
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
            if (Properties.Settings.Default.ErrorHighlightDecoration != null && !Properties.Settings.Default.ErrorHighlightDecoration.Equals(String.Empty))
            {
                switch (Properties.Settings.Default.ErrorHighlightDecoration)
                {
                    case "Baseline":
                        this.TextRunProperties.SetTextDecorations(TextDecorations.Baseline);
                        break;
                    case "OverLine":
                        this.TextRunProperties.SetTextDecorations(TextDecorations.OverLine);
                        break;
                    case "Strikethrough":
                        this.TextRunProperties.SetTextDecorations(TextDecorations.Strikethrough);
                        break;
                    case "Underline":
                        this.TextRunProperties.SetTextDecorations(TextDecorations.Underline);
                        break;
                }
                
            }
            this.TextRunProperties.SetBackgroundBrush(new SolidColorBrush(Properties.Settings.Default.ErrorHighlightBackground));
            this.TextRunProperties.SetForegroundBrush(new SolidColorBrush(Properties.Settings.Default.ErrorHighlightForeground));
            if (Properties.Settings.Default.ErrorHighlightFontFamily != null)
            {
                this.TextRunProperties.SetTypeface(new Typeface(Properties.Settings.Default.ErrorHighlightFontFamily, new FontStyle(), new FontWeight(), new FontStretch()));
            }
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
