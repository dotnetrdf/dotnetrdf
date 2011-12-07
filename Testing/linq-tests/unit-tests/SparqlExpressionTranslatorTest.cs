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
using System.Reflection;
using System.Text;
using VDS.RDF.Linq;
using VDS.RDF.Linq.Sparql;
using NUnit.Framework;
using RdfMusic;
using EH = UnitTests.ExpressionHelper;

namespace UnitTests
{
	public static class ExpressionHelper
	{
		public static Expression CreateBinaryExpression(ExpressionType expressionType, object lh, object rh)
		{
			Expression left = (Expression) ((lh is Expression) ? lh : Expression.Constant(lh));
			Expression right = (Expression) ((rh is Expression) ? rh : Expression.Constant(rh));

			switch (expressionType)
			{
				case ExpressionType.Add:
					return Expression.Add(left, right);
				case ExpressionType.Subtract:
					return Expression.Subtract(left, right);
				case ExpressionType.Multiply:
					return Expression.Multiply(left, right);
				case ExpressionType.Divide:
					return Expression.Divide(left, right);
				case ExpressionType.And:
					return Expression.And(left, right);
				case ExpressionType.AndAlso:
					return Expression.AndAlso(left, right);
				case ExpressionType.Or:
					return Expression.Or(left, right);
				case ExpressionType.OrElse:
					return Expression.Or(left, right);
			}
			throw new ApplicationException("huh?");
		}

		public static Expression CreateUnaryExpression(ExpressionType expressionType, Expression exp)
		{
			throw new NotImplementedException();
		}

		public static Expression Member(MemberInfo mi)
		{
			if (mi is FieldInfo)
			{
				return Expression.Field(Expression.Parameter(mi.DeclaringType, "mi"), mi as FieldInfo);
			}
			else if (mi is PropertyInfo)
			{
				return Expression.Property(Expression.Parameter(mi.DeclaringType, "mi"), mi as PropertyInfo);
			}
			else if (mi is MethodInfo)
			{
				return Expression.Property(Expression.Parameter(mi.DeclaringType, "mi"), mi as MethodInfo);
			}
			else throw new NotImplementedException("Member type not supported");
		}
	}

	/// <summary>
	///This is ontology test class for RdfSerialisation.SparqlExpressionTranslator&lt;T&gt; and is intended
	///to contain all RdfSerialisation.SparqlExpressionTranslator&lt;T&gt; Unit Tests
	///</summary>
	[TestFixture]
	public class SparqlExpressionTranslatorTest
	{
		public bool BooleanTestProperty
		{
			get { return (DateTime.Now.Second%2) == 0; }
		}

		public int IntTestProperty
		{
			get { return DateTime.Now.Second; }
			set { }
		}

		public int IntTest(int arg)
		{
			return ++arg;
		}

		public int[] Ia
		{
			get { return ia; }
		}

		public int[] ia = new int[] {1, 2, 3, 4, 5, 6};

		private PropertyInfo GetProperty(string arg)
		{
			return GetType().GetProperty(arg);
		}

		#region Unit Tests

		/// <summary>
		///A test for Add (Expression)
		///</summary>
		[Test]
		public void AddTest()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();

			Expression e = Expression.Add(
				EH.Member(GetType().GetProperty("IntTestProperty")),
				Expression.Constant(5));
			StringBuilder sb = new StringBuilder();
			target.StringBuilder = sb;
			target.Add(e);
			string actualResult = sb.ToString();
			string expectedResult = "(?IntTestProperty)+(5)";
			Assert.AreEqual(expectedResult, actualResult);
		}

		/// <summary>
		///A test for AddChecked (Expression)
		///</summary>
		[Test]
		public void AddCheckedTest()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();

			Expression e = Expression.AddChecked(
				EH.Member(GetType().GetProperty("IntTestProperty")),
				Expression.Constant(5));
			StringBuilder sb = new StringBuilder();
			target.StringBuilder = sb;
			target.AddChecked(e);
			string actualResult = sb.ToString();
			string expectedResult = "(?IntTestProperty)+(5)";
			Assert.AreEqual(expectedResult, actualResult);
		}

		/// <summary>
		///A test for And (Expression)
		///</summary>
		[Test]
		public void AndTest()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();

			Expression e = Expression.And(
				EH.Member(GetType().GetProperty("BooleanTestProperty")),
				Expression.Constant(true));
			StringBuilder sb = new StringBuilder();
			target.StringBuilder = sb;
			target.And(e);
			string actualResult = sb.ToString();
			string expectedResult = "(?BooleanTestProperty)&&(True^^xsd:boolean)";
			Assert.AreEqual(expectedResult, actualResult);
		}

		/// <summary>
		///A test for AndAlso (Expression)
		///</summary>
		[Test]
		public void AndAlsoTest()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();

			Expression e = Expression.AndAlso(
				EH.Member(GetType().GetProperty("BooleanTestProperty")),
				Expression.Constant(true));
			StringBuilder sb = new StringBuilder();
			target.StringBuilder = sb;
			target.AndAlso(e);
			string actualResult = sb.ToString();
			string expectedResult = "(?BooleanTestProperty)&&(True^^xsd:boolean)";
			Assert.AreEqual(expectedResult, actualResult);
		}
		class A { }
		class B : A { }
		/// <summary>
		///A test for TypeAs (Expression)
		///</summary>
		[Test]
		[ExpectedException(typeof (NotSupportedException))]
		public void AsTest()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();

			Expression e = Expression.TypeAs(Expression.Constant(new B()), typeof (A));
			target.StringBuilder = new StringBuilder();
			target.TypeAs(e);
		}

		/// <summary>
		///A test for BitwiseXor (Expression)
		///</summary>
		[Test]
		[ExpectedException(typeof (NotImplementedException))]
		public void XorTest()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();

			Expression e = Expression.ExclusiveOr(Expression.Constant(10), Expression.Constant(8));
			target.StringBuilder = new StringBuilder();
			target.ExclusiveOr(e);
		}

		/// <summary>
		///A test for Cast (Expression)
		///</summary>
		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void TypeAsTest()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();

			Expression e = Expression.TypeAs(Expression.Constant("3.14"), typeof (double));
			target.StringBuilder = new StringBuilder();
			target.TypeAs(e);
		}

		/// <summary>
		///A test for Coalesce (Expression)
		///</summary>
		[Test]
		[ExpectedException(typeof (NotSupportedException))]
		public void CoalesceTest()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();
			string testVal = "hello";
			string defaultVal = "world";
			Expression e = Expression.Coalesce(Expression.Constant(testVal), Expression.Constant(defaultVal));
			target.StringBuilder = new StringBuilder();
			target.Coalesce(e);
		}

		/// <summary>
		///A test for Conditional (Expression)
		///</summary>
		[Test]
		[ExpectedException(typeof (NotSupportedException))]
		public void ConditionalTest()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();

			Expression e = Expression.Condition(
				Expression.Equal(EH.Member(GetType().GetProperty("BooleanTestProperty")),
				              Expression.Constant(true)),
				Expression.Constant(10), Expression.Constant(15));
			target.StringBuilder = new StringBuilder();
			target.Conditional(e);
		}

		/// <summary>
		///A test for Convert (Expression)
		///</summary>
		[Test]
		[ExpectedException(typeof (NotSupportedException))]
		public void ConvertTest()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();

			Expression e = Expression.Convert(Expression.Constant(10), typeof (double));
			target.StringBuilder = new StringBuilder();
			target.Convert(e);
		}

		/// <summary>
		///A test for ConvertChecked (Expression)
		///</summary>
		[Test]
		[ExpectedException(typeof (NotSupportedException))]
		public void ConvertCheckedTest()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();

			Expression e = Expression.ConvertChecked(Expression.Constant(10), typeof (double));
			target.StringBuilder = new StringBuilder();
			target.ConvertChecked(e);
		}

		/// <summary>
		///A test for Divide (Expression)
		///</summary>
		[Test]
		public void DivideTest()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();

			Expression e = Expression.Divide(Expression.Constant(10), Expression.Constant(15));
			StringBuilder sb = new StringBuilder();
			target.StringBuilder = sb;
			target.Divide(e);
			string actualResult = sb.ToString();
			string expectedResult = "(10)/(15)";
			Assert.AreEqual(expectedResult, actualResult);
		}

		/// <summary>
		///A test for Equal (Expression)
		///</summary>
		[Test]
		public void EQTest()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();

			Expression e = Expression.Equal(Expression.Constant(10), Expression.Constant(15));
			StringBuilder sb = new StringBuilder();
			target.StringBuilder = sb;
			target.Equal(e);
			string actualResult = sb.ToString();
			string expectedResult = "10 = 15";
			Assert.AreEqual(expectedResult, actualResult);
		}

		/// <summary>
		///A test for Funclet (Expression)
		///</summary>
		[Test, Ignore, ExpectedException(typeof (NotSupportedException))]
		public void FuncletTest()
		{
			// SparqlExpressionTranslator<T> target = new SparqlExpressionTranslator<T>();
			// 
			// Expression e = null; // TODO: Initialize to an appropriate value
			// 
			// target.Funclet(e);
			// 
			// Assert.Inconclusive("A method that does not return ontology value cannot be verified.");
			Assert.Fail("Generics testing must be manually provided.");
		}

		/// <summary>
		///A test for GreaterThanOrEqual (Expression)
		///</summary>
		[Test]
		public void GETest()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();
			Expression e = Expression.GreaterThanOrEqual(Expression.Constant(10), Expression.Constant(15));
			StringBuilder sb = new StringBuilder();
			target.StringBuilder = sb;
			target.GreaterThanOrEqual(e);
			string actualResult = sb.ToString();
			string expectedResult = "(10)>=(15)";
			Assert.AreEqual(expectedResult, actualResult);
		}

		/// <summary>
		///A test for GreaterThan (Expression)
		///</summary>
		[Test]
		public void GTTest()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();
			Expression e = Expression.GreaterThan(Expression.Constant(10), Expression.Constant(15));
			StringBuilder sb = new StringBuilder();
			target.StringBuilder = sb;
			target.GreaterThan(e);
			string actualResult = sb.ToString();
			string expectedResult = "(10)>(15)";
			Assert.AreEqual(expectedResult, actualResult);
		}

		/// <summary>
		///A test for Index (Expression)
		///</summary>
		[Test]
		public void IndexTest()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();
			Expression e = Expression.ArrayIndex(EH.Member(GetType().GetProperty("Ia")), Expression.Constant(1));
			StringBuilder sb = new StringBuilder();
			target.StringBuilder = sb;
			target.ArrayIndex(e);
			string actualResult = sb.ToString();
			string expectedResult = "?Ia[1]";
			Assert.AreEqual(expectedResult, actualResult);
		}

		/// <summary>
		///A test for Invoke (Expression)
		///</summary>
		[Test]
		[Ignore, ExpectedException(typeof (NotSupportedException))]
		public void InvokeTest()
		{
			// not sure how to construct ontology lambda expression
		}

		/// <summary>
		///A test for TypeIs (Expression)
		///</summary>
		[Test]
		[ExpectedException(typeof (NotSupportedException))]
		public void IsTest()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();
			Expression e = Expression.TypeIs(Expression.Constant(10), typeof(int));
			StringBuilder sb = new StringBuilder();
			target.StringBuilder = sb;
			target.TypeIs(e);
			string actualResult = sb.ToString();
			string expectedResult = "(10)>(15)";
			Assert.AreEqual(expectedResult, actualResult);
		}

		/// <summary>
		///A test for Lambda (Expression)
		///</summary>
		[Test]
		[Ignore, ExpectedException(typeof (NotSupportedException))]
		public void LambdaTest()
		{
			// not sure how to construct ontology lambda expression
		}

		/// <summary>
		///A test for LessThanOrEqual (Expression)
		///</summary>
		[Test]
		public void LETest()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();
			Expression e = Expression.LessThanOrEqual(Expression.Constant(10), Expression.Constant(15));
			StringBuilder sb = new StringBuilder();
			target.StringBuilder = sb;
			target.LessThanOrEqual(e);
			string actualResult = sb.ToString();
			string expectedResult = "(10)<=(15)";
			Assert.AreEqual(expectedResult, actualResult);
		}

		/// <summary>
		///A test for ArrayLength (Expression)
		///</summary>
		[Test]
		[ExpectedException(typeof (NotSupportedException))]
		public void LenTest()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();
			Expression e = Expression.ArrayLength(EH.Member(GetType().GetProperty("Ia")));
			StringBuilder sb = new StringBuilder();
			target.StringBuilder = sb;
			target.ArrayLength(e);
			string actualResult = sb.ToString();
			string expectedResult = "(10)<=(15)";
			Assert.AreEqual(expectedResult, actualResult);
		}

		/// <summary>
		///A test for ListInit (Expression)
		///</summary>
		[Test]
		[Ignore, ExpectedException(typeof (NotSupportedException))]
		public void ListInitTest()
		{
			// not sure how to build this
		}

		/// <summary>
		///A test for LShift (Expression)
		///</summary>
		[Test]
        [ExpectedException(typeof(NotSupportedException))]
		public void LShiftTest()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();
			Expression e = Expression.LeftShift(Expression.Constant(10), Expression.Constant(15));
			StringBuilder sb = new StringBuilder();
			target.StringBuilder = sb;
			target.LeftShift(e);
			string actualResult = sb.ToString();
			string expectedResult = "(10)<=(15)";
			Assert.AreEqual(expectedResult, actualResult);
		}

		/// <summary>
		///A test for LessThan (Expression)
		///</summary>
		[Test]
		public void LTTest()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();
			Expression e = Expression.LessThan(Expression.Constant(10), Expression.Constant(15));
			StringBuilder sb = new StringBuilder();
			target.StringBuilder = sb;
			target.LessThan(e);
			string actualResult = sb.ToString();
			string expectedResult = "(10)<(15)";
			Assert.AreEqual(expectedResult, actualResult);
		}

		/// <summary>
		///A test for MemberAccess (Expression)
		///</summary>
		[Test]
		public void MemberAccessTest()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();
			Expression e = EH.Member(GetType().GetProperty("Ia"));
			StringBuilder sb = new StringBuilder();
			target.StringBuilder = sb;
			target.MemberAccess(e);
			string actualResult = sb.ToString();
			string expectedResult = "?Ia";
			Assert.AreEqual(expectedResult, actualResult);
		}

		/// <summary>
		///A test for MemberInit (Expression)
		///</summary>
		[Test]
		[Ignore, ExpectedException(typeof (NotSupportedException))]
		public void MemberInitTest()
		{
			// not sure how to test this
		}

		/// <summary>
		///A test for MethodCall (Expression)
		///</summary>
		[Ignore, Test]
		public void MethodCallTest()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();
			Expression[] ea = new Expression[] { Expression.Constant(15) };
			Expression e = Expression.Call(GetType().GetMethod("IntTest"), ea);
			StringBuilder sb = new StringBuilder();
			target.StringBuilder = sb;
			target.Call(e);
			string actualResult = sb.ToString();
			string expectedResult = "(mi.IntTest(15)";
			Assert.AreEqual(expectedResult, actualResult);
		}

		/// <summary>
		///A test for Modulo (Expression)
		///</summary>
		[Test]
		[ExpectedException(typeof (NotSupportedException))]
		public void ModuloTest()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();
			Expression e = Expression.Modulo(Expression.Constant(10), Expression.Constant(3));
			StringBuilder sb = new StringBuilder();
			target.StringBuilder = sb;
			target.Modulo(e);
			string actualResult = sb.ToString();
			string expectedResult = "ignore";
			Assert.AreEqual(expectedResult, actualResult);
		}

		/// <summary>
		///A test for Multiply (Expression)
		///</summary>
		[Test]
		public void MultiplyTest()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();
			Expression e = EH.CreateBinaryExpression(ExpressionType.Multiply, 10, 15);
			StringBuilder sb = new StringBuilder();
			target.StringBuilder = sb;
			target.Multiply(e);
			string actualResult = sb.ToString();
			string expectedResult = "(10)*(15)";
			Assert.AreEqual(expectedResult, actualResult);
		}

		/// <summary>
		///A test for MultiplyChecked (Expression)
		///</summary>
		[Test]
		public void MultiplyCheckedTest()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();
			Expression e = Expression.MultiplyChecked(Expression.Constant(10), Expression.Constant(15));
			StringBuilder sb = new StringBuilder();
			target.StringBuilder = sb;
			target.MultiplyChecked(e);
			string actualResult = sb.ToString();
			string expectedResult = "(10)*(15)";
			Assert.AreEqual(expectedResult, actualResult);
		}

		/// <summary>
		///A test for NotEqual (Expression)
		///</summary>
		[Test]
		public void NETest()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();
			Expression e = Expression.NotEqual(Expression.Constant(10), Expression.Constant(15));
			StringBuilder sb = new StringBuilder();
			target.StringBuilder = sb;
			target.NotEqual(e);
			string actualResult = sb.ToString();
			string expectedResult = "(10)!=(15)";
			Assert.AreEqual(expectedResult, actualResult);
		}

		/// <summary>
		///A test for Negate (Expression)
		///</summary>
		[Test]
		[ExpectedException(typeof (NotSupportedException))]
		public void NegateTest()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();
			Expression e = Expression.Negate(EH.Member(GetProperty("IntTestProperty")));
			StringBuilder sb = new StringBuilder();
			target.StringBuilder = sb;
			target.Negate(e);
			string actualResult = sb.ToString();
			string expectedResult = "!(?BooleanTestProperty)";
			Assert.AreEqual(expectedResult, actualResult);
		}

		/// <summary>
		///A test for New (Expression)
		///</summary>
		[Test]
		[ExpectedException(typeof (NotSupportedException))]
		public void NewTest()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();
			Expression e = Expression.New(GetType());
			StringBuilder sb = new StringBuilder();
			target.StringBuilder = sb;
			target.New(e);
			string actualResult = sb.ToString();
			string expectedResult = "(?BooleanTestProperty)";
			Assert.AreEqual(expectedResult, actualResult);
		}

		/// <summary>
		///A test for NewArrayBounds (Expression)
		///</summary>
		[Test]
		[ExpectedException(typeof (NotSupportedException))]
		public void NewArrayBoundsTest()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();
			Expression e = Expression.NewArrayBounds(GetType(), new Expression[] { Expression.Constant(10) }); //  array of ten
			StringBuilder sb = new StringBuilder();
			target.StringBuilder = sb;
			target.NewArrayBounds(e);
			string actualResult = sb.ToString();
			string expectedResult = "(?BooleanTestProperty)";
			Assert.AreEqual(expectedResult, actualResult);
		}

		/// <summary>
		///A test for NewArrayInit (Expression)
		///</summary>
		[Test]
		[Ignore, ExpectedException(typeof (NotSupportedException))]
		public void NewArrayInitTest()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();
			Expression e = Expression.NewArrayInit(GetType(), new Expression[] { Expression.Constant(10) }); //  array of ten
			StringBuilder sb = new StringBuilder();
			target.StringBuilder = sb;
			target.NewArrayInit(e);
			string actualResult = sb.ToString();
			string expectedResult = "(?BooleanTestProperty)";
			Assert.AreEqual(expectedResult, actualResult);
		}

		/// <summary>
		///A test for Not (Expression)
		///</summary>
		[Test]
		public void NotTest()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();
			Expression e = Expression.Not(EH.Member(GetType().GetProperty("BooleanTestProperty")));
			StringBuilder sb = new StringBuilder();
			target.StringBuilder = sb;
			target.Not(e);
			string actualResult = sb.ToString();
			string expectedResult = "!(?BooleanTestProperty)";
			Assert.AreEqual(expectedResult, actualResult);
		}

		/// <summary>
		///A test for Or (Expression)
		///</summary>
		[Test]
		public void OrTest()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();
			Expression e =
				Expression.Or(EH.Member(GetProperty("BooleanTestProperty")),
				              Expression.Not(EH.Member(GetProperty("BooleanTestProperty"))));
			StringBuilder sb = new StringBuilder();
			target.StringBuilder = sb;
			target.Or(e);
			string actualResult = sb.ToString();
			string expectedResult = "(?BooleanTestProperty)||(!(?BooleanTestProperty))";
			Assert.AreEqual(expectedResult, actualResult);
		}

		/// <summary>
		///A test for OrElse (Expression)
		///</summary>
		[Test]
		public void OrElseTest()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();
			Expression e =
				Expression.OrElse(EH.Member(GetProperty("BooleanTestProperty")),
				                  Expression.Not(EH.Member(GetProperty("BooleanTestProperty"))));
			StringBuilder sb = new StringBuilder();
			target.StringBuilder = sb;
			target.OrElse(e);
			string actualResult = sb.ToString();
			string expectedResult = "(?BooleanTestProperty)||(!(?BooleanTestProperty))";
			Assert.AreEqual(expectedResult, actualResult);
		}

		/// <summary>
		///A test for Parameter (Expression)
		///</summary>
		[Test]
		public void ParameterTest()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();
			Expression e = Expression.Parameter(GetType(), "mi");
			StringBuilder sb = new StringBuilder();
			target.StringBuilder = sb;
			target.Parameter(e);
			string actualResult = sb.ToString();
			string expectedResult = "mi"; //  not sure that any output should be expected
			Assert.AreEqual(expectedResult, actualResult);
		}

		/// <summary>
		///A test for Quote (Expression)
		///</summary>
		[Test]
		public void QuoteTest()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();
			Expression e = Expression.Quote(Expression.Constant(5));
			StringBuilder sb = new StringBuilder();
			target.StringBuilder = sb;
			target.Quote(e);
			string actualResult = sb.ToString();
			string expectedResult = "\"5\""; //  assumption - it should retain its XSDT type?
			Assert.AreEqual(expectedResult, actualResult);
		}

		/// <summary>
		///A test for RShift (Expression)
		///</summary>
		[Test]
		[ExpectedException(typeof (NotSupportedException))]
		public void RShiftTest()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();
			Expression e = Expression.RightShift(Expression.Constant(1), Expression.Constant(5));
			StringBuilder sb = new StringBuilder();
			target.StringBuilder = sb;
			target.RightShift(e);
			string actualResult = sb.ToString();
			string expectedResult = ""; //  shouldn'mi make it to here
			Assert.AreEqual(expectedResult, actualResult);
		}

		/// <summary>
		///A test for Sanitise (string)
		///</summary>
		[Test, Ignore, ExpectedException(typeof (NotSupportedException))]
		public void SanitiseTest()
		{
			// Unit Test Generation Error: A private accessor could not be created for RdfSerialisation.SparqlExpressionTranslator<T>.Sanitise: Private accessors cannot be created for generic types
			Assert.Fail("Unit Test Generation Error: A private accessor could not be created for RdfSerial" +
			            "isation.SparqlExpressionTranslator<T>.Sanitise: Private accessors cannot be crea" +
			            "ted for generic types");
		}

		/// <summary>
		///A test for SparqlExpressionTranslator ()
		///</summary>
		[Test, Ignore, ExpectedException(typeof (NotSupportedException))]
		public void ConstructorTest()
		{
			// SparqlExpressionTranslator<T> target = new SparqlExpressionTranslator<T>();
			// 
			// // TODO: Implement code to verify target
			// Assert.Inconclusive("TODO: Implement code to verify target");
			Assert.Fail("Generics testing must be manually provided.");
		}

		/// <summary>
		///A test for SparqlExpressionTranslator (StringBuilder)
		///</summary>
		[Test, Ignore, ExpectedException(typeof (NotSupportedException))]
		public void ConstructorTest1()
		{
			// StringBuilder stringBuilder = null; // TODO: Initialize to an appropriate value
			// 
			// SparqlExpressionTranslator<T> target = new SparqlExpressionTranslator<T>(stringBuilder);
			// 
			// // TODO: Implement code to verify target
			// Assert.Inconclusive("TODO: Implement code to verify target");
			Assert.Fail("Generics testing must be manually provided.");
		}

		/// <summary>
		///A test for StringBuilder
		///</summary>
		[Test, Ignore, ExpectedException(typeof (NotSupportedException))]
		public void StringBuilderTest()
		{
			// SparqlExpressionTranslator<T> target = new SparqlExpressionTranslator<T>();
			// 
			// StringBuilder val = null; // TODO: Assign to an appropriate value for the property
			// 
			// target.StringBuilder = val;
			// 
			// 
			// Assert.AreEqual(val, target.StringBuilder, "RdfSerialisation.SparqlExpressionTranslator<T>.StringBuilder was not set correctl" +
			//        "y.");
			// Assert.Inconclusive("Verify the correctness of this test method.");
			Assert.Fail("Generics testing must be manually provided.");
		}

		/// <summary>
		///A test for Subtract (Expression)
		///</summary>
		[Test]
		public void SubtractTest()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();
			Expression e = Expression.Subtract(EH.Member(GetType().GetProperty("IntTestProperty")), Expression.Constant(1));
			StringBuilder sb = new StringBuilder();
			target.StringBuilder = sb;
			target.Subtract(e);
			string actualResult = sb.ToString();
			string expectedResult = "(?IntTestProperty)-(1)";
			Assert.AreEqual(expectedResult, actualResult);
		}

		/// <summary>
		///A test for SubtractChecked (Expression)
		///</summary>
		[Test]
		public void SubtractCheckedTest()
		{
			LinqToSparqlExpTranslator<Track> target = new LinqToSparqlExpTranslator<Track>();
			target.TypeTranslator = new XsdtTypeConverter();
			Expression e =
				Expression.SubtractChecked(EH.Member(GetType().GetProperty("IntTestProperty")), Expression.Constant(1));
			StringBuilder sb = new StringBuilder();
			target.StringBuilder = sb;
			target.SubtractChecked(e);
			string actualResult = sb.ToString();
			string expectedResult = "(?IntTestProperty)-(1)";
			Assert.AreEqual(expectedResult, actualResult);
		}

		#endregion
	}
}
