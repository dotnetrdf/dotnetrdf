using System;
using VDS.RDF.Specifications;

namespace VDS.RDF.Nodes
{
    /// <summary>
    /// Value node representing a signed byte (8-bit signed integer)
    /// </summary>
    public class SignedByteNode
        : NumericNode
    {
        private sbyte _value;

        /// <summary>
        /// Creates a new signed byte node
        /// </summary>
        /// <param name="value">Signed Byte value</param>
        /// <param name="lexicalValue">Lexical value</param>
        public SignedByteNode(sbyte value, String lexicalValue)
            : base(lexicalValue, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeByte), EffectiveNumericType.Integer)
        {
            this._value = value;
        }

        /// <summary>
        /// Creates a new signed byte node
        /// </summary>
        /// <param name="value">Signed Byte value</param>
        public SignedByteNode(sbyte value)
            : this(value, value.ToString()) { }

        /// <summary>
        /// Gets the integer value of the signed byte
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
                throw new NodeValueException("Unable to upcast byte to integer");
            }
        }

        /// <summary>
        /// Gets the decimal value of the signed byte
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
                throw new NodeValueException("Unable to upcast byte to decimal");
            }
        }

        /// <summary>
        /// Gets the float value of the signed byte
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
                throw new NodeValueException("Unable to upcast byte to float");
            }
        }

        /// <summary>
        /// Gets the double value of the signed byte
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
                throw new NodeValueException("Unable to upcast byte to double");
            }
        }
    }
}