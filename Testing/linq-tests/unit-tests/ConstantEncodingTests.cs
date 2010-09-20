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
using System.Linq.Expressions;
using System.Text;
using VDS.RDF.Linq.Sparql;
using NUnit.Framework;
using RdfMusic;
using VDS.RDF.Linq;

namespace UnitTests
{
	[TestFixture]
	public class ConstantEncodingTests
	{
		[Test]
		public void ConstantTest_string()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();
			ConstantExpression ce = Expression.Constant("hello world"); // string
			StringBuilder sb = new StringBuilder();
			target.StringBuilder = sb;
			target.Constant(ce);
			string actualResult = sb.ToString();
			string expectedResult = "\"hello world\"";
			Assert.AreEqual(expectedResult, actualResult);
		}

		[Test]
		public void ConstantTest_char()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();
			ConstantExpression ce = Expression.Constant('a'); // string
			StringBuilder sb = new StringBuilder();
			target.StringBuilder = sb;
			target.Constant(ce);
			string actualResult = sb.ToString();
			string expectedResult = "\"a\"";
			Assert.AreEqual(expectedResult, actualResult);
		}

		[Test]
		public void ConstantTest_short()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();
			ConstantExpression ce = Expression.Constant((short)4); // string
			StringBuilder sb = new StringBuilder();
			target.StringBuilder = sb;
			target.Constant(ce);
			string actualResult = sb.ToString();
			string expectedResult = "4^^xsd:short";
			Assert.AreEqual(expectedResult, actualResult);
		}

		[Test]
		public void ConstantTest_int()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();
			ConstantExpression ce = Expression.Constant(4); // string
			StringBuilder sb = new StringBuilder();
			target.StringBuilder = sb;
			target.Constant(ce);
			string actualResult = sb.ToString();
			string expectedResult = "4";
			Assert.AreEqual(expectedResult, actualResult);
		}

		[Test]
		public void ConstantTest_long()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();
			ConstantExpression ce = Expression.Constant((long)4); // string
			StringBuilder sb = new StringBuilder();
			target.StringBuilder = sb;
			target.Constant(ce);
			string actualResult = sb.ToString();
			string expectedResult = "4^^xsd:long";
			Assert.AreEqual(expectedResult, actualResult);
		}

		[Test]
		public void ConstantTest_float()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();
			ConstantExpression ce = Expression.Constant((float)3.14); // string
			StringBuilder sb = new StringBuilder();
			target.StringBuilder = sb;
			target.Constant(ce);
			string actualResult = sb.ToString();
			string expectedResult = "3.14^^xsd:float";
			Assert.AreEqual(expectedResult, actualResult);
		}

		[Test]
		public void ConstantTest_double()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();
			ConstantExpression ce = Expression.Constant(3.14); // string
			StringBuilder sb = new StringBuilder();
			target.StringBuilder = sb;
			target.Constant(ce);
			string actualResult = sb.ToString();
			string expectedResult = "3.14^^xsd:double";
			Assert.AreEqual(expectedResult, actualResult);
		}

		[Test]
		public void ConstantTest_decimal()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();
			ConstantExpression ce = Expression.Constant((decimal)3.14); // string
			StringBuilder sb = new StringBuilder();
			target.StringBuilder = sb;
			target.Constant(ce);
			string actualResult = sb.ToString();
			string expectedResult = "3.14^^xsd:decimal";
			Assert.AreEqual(expectedResult, actualResult);
		}

		/// <summary>
		///  warning this ontology very fragle test that only works from Australia during daylight savings time :-(
		/// </summary>
		[Test]
		public void ConstantTest_DateTime()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();
			ConstantExpression ce = Expression.Constant(new DateTime(2002, 03, 04, 05, 06, 07));
			StringBuilder sb = new StringBuilder();
			target.StringBuilder = sb;
			target.Constant(ce);
			string actualResult = sb.ToString();
			string expectedResult = "\"2002-03-04T05:06:07+00:00\"^^xsd:dateTime";
			Assert.AreEqual(expectedResult, actualResult);
		}

	}
}
