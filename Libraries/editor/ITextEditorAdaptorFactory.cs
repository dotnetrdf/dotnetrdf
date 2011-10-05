using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Editor
{
    public interface ITextEditorAdaptorFactory<T>
    {
        ITextEditorAdaptor<T> CreateAdaptor();
    }

    public interface IVisualTextEditorAdaptorFactory<TControl, TFont, TColor>
        : ITextEditorAdaptorFactory<TControl>
          where TFont : class
          where TColor : struct
    {
        VisualOptions<TFont, TColor> GetDefaultVisualOptions();
    }
}
