// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 1965 $</version>
// </file>

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace VDS.RDF.Utilities.Editor.WinForms.Syntax
{
	public class RdfSyntaxModeProvider : ISyntaxModeFileProvider
	{
		List<SyntaxMode> syntaxModes = null;
		
		public ICollection<SyntaxMode> SyntaxModes 
        {
			get
            {
				return syntaxModes;
			}
		}
		
		public RdfSyntaxModeProvider()
		{
            Assembly assembly = Assembly.GetExecutingAssembly();
			Stream syntaxModeStream = assembly.GetManifestResourceStream("VDS.RDF.Utilities.Editor.WinForms.Syntax.SyntaxModes.xml");
			if (syntaxModeStream != null)
            {
				syntaxModes = SyntaxMode.GetSyntaxModes(syntaxModeStream);
			} 
            else
            {
				syntaxModes = new List<SyntaxMode>();
			}
		}
		
		public XmlTextReader GetSyntaxModeFile(SyntaxMode syntaxMode)
		{
            Assembly assembly = Assembly.GetExecutingAssembly();
			return new XmlTextReader(assembly.GetManifestResourceStream("VDS.RDF.Utilities.Editor.WinForms.Syntax." + syntaxMode.FileName));
		}
		
		public void UpdateSyntaxModeList()
		{
			// resources don't change during runtime
		}
	}
}
