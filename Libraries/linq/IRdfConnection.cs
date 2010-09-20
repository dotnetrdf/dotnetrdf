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
using VDS.RDF.Linq.Sparql;

namespace VDS.RDF.Linq
{
	public interface IRdfConnection<T>
	{
		IRdfCommand<T> CreateCommand();

		LinqTripleStore Store
		{
			get;
		}
	}
}