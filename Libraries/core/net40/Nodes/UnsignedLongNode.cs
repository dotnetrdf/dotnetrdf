using System;
using VDS.RDF.Specifications;

namespace VDS.RDF.Nodes
{
    /// <summary>
    /// A Valued Node with a unsigned long value
    /// </summary>
    public class UnsignedLongNode
        : NumericNode
    {
        private ulong _value;

        /// <summary>
        /// Creates a new unsigned long valued node
        /// </summary>
        /// <param name="value">Unsigned Long value</param>
        /// <param name="lexicalValue">Lexical Value</param>
        public UnsignedLongNode(ulong value, String lexicalValue)
            : this(value, lexicalValue, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeUnsignedInt)) { }

        /// <summary>
        /// Creates a new unsigned long valued node
        /// </summary>
        /// <param name="value">Unsigned Long value</param>
        /// <param name="lexicalValue">Lexical Value</param>
        /// <param name="datatype">Datatype URI</param>
        public UnsignedLongNode(ulong value, String lexicalValue, Uri datatype)
            : base(lexicalValue, datatype, EffectiveNumericType.Integer)
        {
            this._value = value;
        }

        /// <summary>
        /// Creates a new usigned long valued node
        /// </summary>
        /// <param name="g">Graph the node belongs to</param>
        /// <param name="value">Unsigned Long value</param>
        public UnsignedLongNode(ulong value)
            : this(value, value.ToString()) { }

        /// <summary>
        /// Gets the long value of the ulong
        /// </summary>
        /// <returns></returns>
        public override long AsInteger()
        {
            try
            {
                return Convert.ToInt64(this._value);
            }
            catch
            {
                throw new NodeValueException("Unable to downcast unsigned Long to long");
            }
        }

        /// <summary>
        /// Gets the decimal value of the ulong
        /// </summary>
        /// <returns></returns>
        public override decimal AsDecimal()
        {
            try
            {
                return Convert.ToDecimal(this._value);
            }
            catch
            {
                throw new NodeValueException("Unable to upcast Long to Double");
            }
        }

        /// <summary>
        /// Gets the float value of the ulong
        /// </summary>
        /// <returns></returns>
        public override float AsFloat()
        {
            try
            {
                return Convert.ToSingle(this._value);
            }
            catch
            {
                throw new NodeValueException("Unable to upcast Long to Float");
            }
        }

        /// <summary>
        /// Gets the double value of the ulong
        /// </summary>
        /// <returns></returns>
        public override double AsDouble()
        {
            try
            {
                return Convert.ToDouble(this._value);
            }
            catch
            {
                throw new NodeValueException("Unable to upcast Long to Double");
            }
        }
    }
}