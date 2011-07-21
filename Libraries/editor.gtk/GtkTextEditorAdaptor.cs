using System;
using System.Collections.Generic;
using System.Linq;
using Mono.TextEditor;

namespace VDS.RDF.Utilities.Editor.Gtk
{
	public class GtkTextEditorAdaptor : BaseTextEditorAdaptor<TextEditor>
	{
		public GtkTextEditorAdaptor()
			: base(new TextEditor())
		{
			this.Control.Document.TextReplacing += delegate(object sender, ReplaceEventArgs e)
			{
				this.RaiseTextChanged(sender);	
			};
		}
		
		public override String Text
		{
			get
			{
				return this.Control.Document.Text;	
			}
			set 
			{
				this.Control.Document.Text = value;
			}
		}
		
		public override int TextLength
		{
			get 
			{
				return this.Control.Document.Length;	
			}
		}
		
		public override int CaretOffset
		{
			get
			{
				return this.Control.Caret.Offset;	
			}
		}
		
		public override int SelectionStart
		{
			get 
			{
				if (this.Control.IsSomethingSelected)
				{
					return this.Control.SelectionRange.Offset;	
				}
				else
				{
					return -1;	
				}
			}
			set
			{
				if (this.Control.IsSomethingSelected)
				{
					this.Control.ClearSelection();	
				}
				this.Control.SelectionRange = new Segment(value, 1);
			}
		}
		
		public override int SelectionLength
		{
			get
			{
				if (this.Control.IsSomethingSelected)
				{
					return this.Control.SelectionRange.Length;	
				}
				else 
				{
					return 0;	
				}
			}
			set 
			{
				if (this.Control.IsSomethingSelected)
				{
					this.Control.SelectionRange.Length = value;	
				}
			}
		}

		public override void ScrollToLine (int line)
		{
			throw new System.NotImplementedException();
		}
			
		public override int GetLineByOffset (int offset)
		{
			throw new System.NotImplementedException();
		}
		
		public override void Cut ()
		{
			throw new System.NotImplementedException();
		}
			
		public override void Copy ()
		{
			throw new System.NotImplementedException();
		}
			
		public override void Paste ()
		{
			throw new System.NotImplementedException();
		}
		
		public override void Undo ()
		{
			throw new System.NotImplementedException();
		}
		
		public override void Redo ()
		{
			throw new System.NotImplementedException();
		}
	
		public override bool WordWrap {
			get {
				throw new System.NotImplementedException();
			}
			set {
				throw new System.NotImplementedException();
			}
		}
		
		public override bool ShowLineNumbers {
			get {
				throw new System.NotImplementedException();
			}
			set {
				throw new System.NotImplementedException();
			}
		}
		
		public override bool ShowEndOfLine {
			get {
				throw new System.NotImplementedException();
			}
			set {
				throw new System.NotImplementedException();
			}
		}
		
		public override bool ShowSpaces {
			get {
				throw new System.NotImplementedException();
			}
			set {
				throw new System.NotImplementedException();
			}
		}
		
		public override bool ShowTabs {
			get {
				throw new System.NotImplementedException();
			}
			set {
				throw new System.NotImplementedException();
			}
		}

	}
}

