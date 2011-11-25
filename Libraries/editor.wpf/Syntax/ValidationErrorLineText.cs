using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using ICSharpCode.AvalonEdit.Rendering;

namespace VDS.RDF.Utilities.Editor.Wpf.Syntax
{
    public class ValidationErrorLineText
        : VisualLineText
    {
        private VisualOptions<FontFamily, Color> _options;

        public ValidationErrorLineText(VisualOptions<FontFamily, Color> options, VisualLine parentLine, int length)
            : base(parentLine, length)
        {
            this._options = options;
        }

        public override TextRun CreateTextRun(int startVisualColumn, ITextRunConstructionContext context)
        {
            this.TextRunProperties.SetTextDecorations(TextDecorations.Underline);
            if (this._options.ErrorDecoration != null && !this._options.ErrorDecoration.Equals(String.Empty))
            {
                switch (this._options.ErrorDecoration)
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
            this.TextRunProperties.SetBackgroundBrush(new SolidColorBrush(this._options.ErrorBackground));
            this.TextRunProperties.SetForegroundBrush(new SolidColorBrush(this._options.ErrorForeground));
            if (this._options.ErrorFontFace != null)
            {
                this.TextRunProperties.SetTypeface(new Typeface(this._options.ErrorFontFace, new FontStyle(), new FontWeight(), new FontStretch()));
            }
            return base.CreateTextRun(startVisualColumn, context);
        }

        protected override VisualLineText CreateInstance(int length)
        {
            return new ValidationErrorLineText(this._options, this.ParentVisualLine, length);
        }

        protected override void OnQueryCursor(System.Windows.Input.QueryCursorEventArgs e)
        {
            e.Handled = true;
            
        }
    }
}
