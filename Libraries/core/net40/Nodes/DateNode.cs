using System;
using VDS.RDF.Specifications;

namespace VDS.RDF.Nodes
{
    /// <summary>
    /// Valued Node representing a Date value
    /// </summary>
    public class DateNode
        : DateTimeNode
    {
        /// <summary>
        /// Creates a new Date valued node
        /// </summary>
        /// <param name="value">Date Time value</param>
        public DateNode(DateTimeOffset value)
            : base(value, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDate)) { }

        /// <summary>
        /// Creates a new Date valued node
        /// </summary>
        /// <param name="value">Date Time value</param>
        /// <param name="lexicalValue">Lexical Value</param>
        public DateNode(DateTimeOffset value, String lexicalValue)
            : base(value, lexicalValue, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDate)) { }

        /// <summary>
        /// Creates a new Date valued node
        /// </summary>
        /// <param name="value">Date Time value</param>
        public DateNode(DateTime value)
            : base(value, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDate)) { }

        /// <summary>
        /// Creates a new Date valued node
        /// </summary>
        /// <param name="value">Date Time value</param>
        /// <param name="lexicalValue">Lexical Value</param>
        public DateNode(DateTime value, String lexicalValue)
            : base(value, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDate)) { }
    }
}