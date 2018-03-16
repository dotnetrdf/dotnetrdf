namespace Grom.Tests
{
    using Grom;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using VDS.RDF;
    using VDS.RDF.Nodes;
    using VDS.RDF.Parsing;

    public static class Data
    {
        public static IEnumerable<object[]> IndexParameters
        {
            get
            {
                return new[] {
                    new object[] { "predicate1" },
                    new object[] { ":predicate1" },
                    new object[] { "/predicate1" },
                    new object[] { "http://example.com/predicate1" },
                    new object[] { new Uri("predicate1", UriKind.Relative) },
                    new object[] { new Uri("http://example.com/predicate1") },
                    new object[] { new NodeFactory().CreateUriNode(new Uri("http://example.com/predicate1")) },
                };
            }
        }

        public static IEnumerable<object[]> TypedLiteralConversions
        {
            get
            {
                return new[] {
                    new object[] { "string", new Uri(XmlSpecsHelper.XmlSchemaDataTypeString), typeof(string) },
                    new object[] { "0", new Uri(XmlSpecsHelper.XmlSchemaDataTypeString), typeof(string) },
                    new object[] { "0", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDouble), typeof(double) },
                    new object[] { "0", new Uri(XmlSpecsHelper.XmlSchemaDataTypeFloat), typeof(float) },
                    new object[] { "0", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDecimal), typeof(decimal) },
                    new object[] { "true", new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean), typeof(bool) },
                    new object[] { "2000-01-01", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDate), typeof(DateTimeOffset) },
                    new object[] { "2000-01-01", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDateTime), typeof(DateTimeOffset) },
                    new object[] { "2000-01-01T00:00:00", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDateTime), typeof(DateTimeOffset) },
                    new object[] { "P0Y0M0DT0H0M0S", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDuration), typeof(TimeSpan) },
                    new object[] { "string", new Uri(XmlSpecsHelper.XmlSchemaDataTypeInt), typeof(string) },
                    new object[] { "0", new Uri(XmlSpecsHelper.XmlSchemaDataTypeInt), typeof(long) },
                    new object[] { "xxx", new Uri("http://example.com/"), typeof(string) },
                };
            }
        }

        public static IEnumerable<object[]> TypedValueConversions
        {
            get
            {
                return new[] {
                    new object[] { true, typeof(BooleanNode) },
                    new object[] { byte.MaxValue, typeof(ByteNode) },
                    new object[] { DateTime.MaxValue, typeof(DateTimeNode) },
                    new object[] { DateTimeOffset.MaxValue, typeof(DateTimeNode) },
                    new object[] { decimal.MaxValue, typeof(DecimalNode) },
                    new object[] { double.MaxValue, typeof(DoubleNode) },
                    new object[] { float.MaxValue, typeof(FloatNode) },
                    new object[] { long.MaxValue, typeof(LongNode) },
                    new object[] { int.MaxValue, typeof(LongNode) },
                    new object[] { "string", typeof(StringNode) },
                    new object[] { 'c', typeof(StringNode) },
                    new object[] { TimeSpan.MaxValue, typeof(TimeSpanNode) }
                };
            }
        }

        public static IEnumerable<object[]> MemberNames
        {
            get
            {
                return new[] {
                    new object[] { "http://example.com/predicate1", null, "http://example.com/predicate1" },
                    new object[] { "http://example.com/predicate1", new Uri("http://example.com/"), "predicate1" },
                    new object[] { "http://example.com/#predicate1", new Uri("http://example.com/#"), "predicate1" },
                };
            }
        }

        public static IEnumerable<object[]> ComparisomParameters
        {
            get
            {
                var uriNode1 = new NodeFactory().CreateUriNode(new Uri("http://example.com/1"));
                var uriNode2 = new NodeFactory().CreateUriNode(new Uri("http://example.com/2"));

                var node1 = new NodeWrapper(uriNode1);
                var node2 = new NodeWrapper(uriNode2);

                var isTrue = nameof(Assert.IsTrue);
                var isFalse = nameof(Assert.IsFalse);

                var gt = ExpressionType.GreaterThan;
                var gte = ExpressionType.GreaterThanOrEqual;
                var lt = ExpressionType.LessThan;
                var lte = ExpressionType.LessThanOrEqual;
                var eq = ExpressionType.Equal;
                var neq = ExpressionType.NotEqual;

                return new[] {
                    new object[] { node1, gt,  null,  isTrue  },
                    new object[] { node1, gt,  node1, isFalse },
                    new object[] { node1, lte, node2, isTrue  },
                    new object[] { node1, lte, node1, isTrue  },
                    new object[] { node1, gte, node2, isFalse },
                    new object[] { node1, gte, node1, isTrue  },
                    new object[] { node1, neq, node1, isFalse },
                    new object[] { node1, neq, null,  isTrue  },
                    new object[] { null,  eq,  node1, isFalse },
                    new object[] { null,  lt,  node1, isTrue  },
                    new object[] { null,  gt,  node1, isFalse },
                };
            }
        }
    }
}
