/* 
 * Copyright (C) 2007, Andrew Matthews http://aabs.wordpress.com/
 *
 * This file is Free Software and part of LinqToRdf http://code.google.com/fromName/linqtordf/
 *
 * It is licensed under the following license:
 *   - Berkeley License, V2.0 or any newer version
 *
 * You may not use this file except in compliance with the above license.
 *
 * See http://code.google.com/fromName/linqtordf/ for the complete text of the license agreement.
 *
 */
using System;
using System.Diagnostics;
using System.Text;

namespace VDS.RDF.Linq.Sparql
{
	/// <summary>
	/// A supertype for all expression translators that target RDF in one form or another.
	/// </summary>
	public class RdfExpressionTranslator<T>
	{
		private ITypeTranslator typeTranslator = null;
		protected StringBuilder stringBuilder = new StringBuilder();

		public StringBuilder StringBuilder
		{
			get { return stringBuilder; }
			set { stringBuilder = value; }
		}

		public ITypeTranslator TypeTranslator
		{
			get { return typeTranslator; }
			set { typeTranslator = value; }
		}

		public string InstancePlaceholderName
		{
			get
			{
				return "?" + Sanitise(typeof(T).Name);
			}
		}

		protected void QueryAppend(string fmt, params object[] args)
		{
			stringBuilder.AppendFormat(fmt, args);
		}

		private string Sanitise(string s)
		{
			return s.Replace("<", "").Replace(">", "").Replace("'", "");
		}

		protected void Log(string msg, params object[] args)
		{
			Console.WriteLine(string.Format(msg, args));
		}
	}
}