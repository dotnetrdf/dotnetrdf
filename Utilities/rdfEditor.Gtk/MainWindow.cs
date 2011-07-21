using System;
using Gtk;
using Mono.TextEditor;
using VDS.RDF.Utilities.Editor;
using VDS.RDF.Utilities.Editor.Gtk;

public partial class MainWindow : Gtk.Window
{
	private Editor<TextEditor> _editor;
	
	public MainWindow () : base(Gtk.WindowType.Toplevel)
	{
		Build ();
		
		//Create the Editor
		this._editor = new Editor<TextEditor>(new GtkTextEditorAdaptorFactory());
		
		//Create an initial document and display on screen
		Document<TextEditor> doc = this._editor.DocumentManager.New();
		this.Add(doc.TextEditor.Control);
	}

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}
}

