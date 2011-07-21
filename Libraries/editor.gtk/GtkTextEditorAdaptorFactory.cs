using System;
using Mono.TextEditor;

namespace VDS.RDF.Utilities.Editor.Gtk
{
	public class GtkTextEditorAdaptorFactory : ITextEditorAdaptorFactory<TextEditor>
	{

		public ITextEditorAdaptor<TextEditor> CreateAdaptor()
		{
			return new GtkTextEditorAdaptor();
		}

	}
}

