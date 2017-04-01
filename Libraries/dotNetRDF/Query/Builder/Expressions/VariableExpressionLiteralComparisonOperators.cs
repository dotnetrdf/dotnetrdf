/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;

namespace VDS.RDF.Query.Builder.Expressions
{
#pragma warning disable 660,661
    public partial class VariableExpression 
#pragma warning restore 660,661
    {
#pragma warning disable 1591
        public static BooleanExpression operator >(int left, VariableExpression right)
        {
            return new LiteralExpression(left.ToConstantTerm()) > right;
        }

        public static BooleanExpression operator <(int left, VariableExpression right)
        {
            return new LiteralExpression(left.ToConstantTerm()) < right;
        }

        public static BooleanExpression operator >(VariableExpression left, int right)
        {
            return left > new LiteralExpression(right.ToConstantTerm());
        }

        public static BooleanExpression operator <(VariableExpression left, int right)
        {
            return left < new LiteralExpression(right.ToConstantTerm());
        }

        public static BooleanExpression operator >(decimal left, VariableExpression right)
        {
            return new LiteralExpression(left.ToConstantTerm()) > right;
        }

        public static BooleanExpression operator <(decimal left, VariableExpression right)
        {
            return new LiteralExpression(left.ToConstantTerm()) < right;
        }

        public static BooleanExpression operator >(VariableExpression left, decimal right)
        {
            return left > new LiteralExpression(right.ToConstantTerm());
        }

        public static BooleanExpression operator <(VariableExpression left, decimal right)
        {
            return left < new LiteralExpression(right.ToConstantTerm());
        }

        public static BooleanExpression operator >(float left, VariableExpression right)
        {
            return new LiteralExpression(left.ToConstantTerm()) > right;
        }

        public static BooleanExpression operator <(float left, VariableExpression right)
        {
            return new LiteralExpression(left.ToConstantTerm()) < right;
        }

        public static BooleanExpression operator >(VariableExpression left, float right)
        {
            return left > new LiteralExpression(right.ToConstantTerm());
        }

        public static BooleanExpression operator <(VariableExpression left, float right)
        {
            return left < new LiteralExpression(right.ToConstantTerm());
        }

        public static BooleanExpression operator >(double left, VariableExpression right)
        {
            return new LiteralExpression(left.ToConstantTerm()) > right;
        }

        public static BooleanExpression operator <(double left, VariableExpression right)
        {
            return new LiteralExpression(left.ToConstantTerm()) < right;
        }

        public static BooleanExpression operator >(VariableExpression left, double right)
        {
            return left > new LiteralExpression(right.ToConstantTerm());
        }

        public static BooleanExpression operator <(VariableExpression left, double right)
        {
            return left < new LiteralExpression(right.ToConstantTerm());
        }

        public static BooleanExpression operator >(string left, VariableExpression right)
        {
            return new LiteralExpression(left.ToConstantTerm()) > right;
        }

        public static BooleanExpression operator <(string left, VariableExpression right)
        {
            return new LiteralExpression(left.ToConstantTerm()) < right;
        }

        public static BooleanExpression operator >(VariableExpression left, string right)
        {
            return left > new LiteralExpression(right.ToConstantTerm());
        }

        public static BooleanExpression operator <(VariableExpression left, string right)
        {
            return left < new LiteralExpression(right.ToConstantTerm());
        }

        public static BooleanExpression operator >(bool left, VariableExpression right)
        {
            return new LiteralExpression(left.ToConstantTerm()) > right;
        }

        public static BooleanExpression operator <(bool left, VariableExpression right)
        {
            return new LiteralExpression(left.ToConstantTerm()) < right;
        }

        public static BooleanExpression operator >(VariableExpression left, bool right)
        {
            return left > new LiteralExpression(right.ToConstantTerm());
        }

        public static BooleanExpression operator <(VariableExpression left, bool right)
        {
            return left < new LiteralExpression(right.ToConstantTerm());
        }

        public static BooleanExpression operator >(DateTime left, VariableExpression right)
        {
            return new LiteralExpression(left.ToConstantTerm()) > right;
        }

        public static BooleanExpression operator <(DateTime left, VariableExpression right)
        {
            return new LiteralExpression(left.ToConstantTerm()) < right;
        }

        public static BooleanExpression operator >(VariableExpression left, DateTime right)
        {
            return left > new LiteralExpression(right.ToConstantTerm());
        }

        public static BooleanExpression operator <(VariableExpression left, DateTime right)
        {
            return left < new LiteralExpression(right.ToConstantTerm());
        }

        public static BooleanExpression operator >=(int left, VariableExpression right)
        {
            return new LiteralExpression(left.ToConstantTerm()) >= right;
        }

        public static BooleanExpression operator <=(int left, VariableExpression right)
        {
            return new LiteralExpression(left.ToConstantTerm()) <= right;
        }

        public static BooleanExpression operator >=(VariableExpression left, int right)
        {
            return left >= new LiteralExpression(right.ToConstantTerm());
        }

        public static BooleanExpression operator <=(VariableExpression left, int right)
        {
            return left <= new LiteralExpression(right.ToConstantTerm());
        }

        public static BooleanExpression operator >=(decimal left, VariableExpression right)
        {
            return new LiteralExpression(left.ToConstantTerm()) >= right;
        }

        public static BooleanExpression operator <=(decimal left, VariableExpression right)
        {
            return new LiteralExpression(left.ToConstantTerm()) <= right;
        }

        public static BooleanExpression operator >=(VariableExpression left, decimal right)
        {
            return left >= new LiteralExpression(right.ToConstantTerm());
        }

        public static BooleanExpression operator <=(VariableExpression left, decimal right)
        {
            return left <= new LiteralExpression(right.ToConstantTerm());
        }

        public static BooleanExpression operator >=(float left, VariableExpression right)
        {
            return new LiteralExpression(left.ToConstantTerm()) >= right;
        }

        public static BooleanExpression operator <=(float left, VariableExpression right)
        {
            return new LiteralExpression(left.ToConstantTerm()) <= right;
        }

        public static BooleanExpression operator >=(VariableExpression left, float right)
        {
            return left >= new LiteralExpression(right.ToConstantTerm());
        }

        public static BooleanExpression operator <=(VariableExpression left, float right)
        {
            return left <= new LiteralExpression(right.ToConstantTerm());
        }

        public static BooleanExpression operator >=(double left, VariableExpression right)
        {
            return new LiteralExpression(left.ToConstantTerm()) >= right;
        }

        public static BooleanExpression operator <=(double left, VariableExpression right)
        {
            return new LiteralExpression(left.ToConstantTerm()) <= right;
        }

        public static BooleanExpression operator >=(VariableExpression left, double right)
        {
            return left >= new LiteralExpression(right.ToConstantTerm());
        }

        public static BooleanExpression operator <=(VariableExpression left, double right)
        {
            return left <= new LiteralExpression(right.ToConstantTerm());
        }

        public static BooleanExpression operator >=(string left, VariableExpression right)
        {
            return new LiteralExpression(left.ToConstantTerm()) >= right;
        }

        public static BooleanExpression operator <=(string left, VariableExpression right)
        {
            return new LiteralExpression(left.ToConstantTerm()) <= right;
        }

        public static BooleanExpression operator >=(VariableExpression left, string right)
        {
            return left >= new LiteralExpression(right.ToConstantTerm());
        }

        public static BooleanExpression operator <=(VariableExpression left, string right)
        {
            return left <= new LiteralExpression(right.ToConstantTerm());
        }

        public static BooleanExpression operator >=(bool left, VariableExpression right)
        {
            return new LiteralExpression(left.ToConstantTerm()) >= right;
        }

        public static BooleanExpression operator <=(bool left, VariableExpression right)
        {
            return new LiteralExpression(left.ToConstantTerm()) <= right;
        }

        public static BooleanExpression operator >=(VariableExpression left, bool right)
        {
            return left >= new LiteralExpression(right.ToConstantTerm());
        }

        public static BooleanExpression operator <=(VariableExpression left, bool right)
        {
            return left <= new LiteralExpression(right.ToConstantTerm());
        }

        public static BooleanExpression operator >=(DateTime left, VariableExpression right)
        {
            return new LiteralExpression(left.ToConstantTerm()) >= right;
        }

        public static BooleanExpression operator <=(DateTime left, VariableExpression right)
        {
            return new LiteralExpression(left.ToConstantTerm()) <= right;
        }

        public static BooleanExpression operator >=(VariableExpression left, DateTime right)
        {
            return left >= new LiteralExpression(right.ToConstantTerm());
        }

        public static BooleanExpression operator <=(VariableExpression left, DateTime right)
        {
            return left <= new LiteralExpression(right.ToConstantTerm());
        }

        public static BooleanExpression operator !=(int left, VariableExpression right)
        {
            return new LiteralExpression(left.ToConstantTerm()) != right;
        }

        public static BooleanExpression operator ==(int left, VariableExpression right)
        {
            return new LiteralExpression(left.ToConstantTerm()) == right;
        }

        public static BooleanExpression operator !=(VariableExpression left, int right)
        {
            return left != new LiteralExpression(right.ToConstantTerm());
        }

        public static BooleanExpression operator ==(VariableExpression left, int right)
        {
            return left == new LiteralExpression(right.ToConstantTerm());
        }

        public static BooleanExpression operator !=(decimal left, VariableExpression right)
        {
            return new LiteralExpression(left.ToConstantTerm()) != right;
        }

        public static BooleanExpression operator ==(decimal left, VariableExpression right)
        {
            return new LiteralExpression(left.ToConstantTerm()) == right;
        }

        public static BooleanExpression operator !=(VariableExpression left, decimal right)
        {
            return left != new LiteralExpression(right.ToConstantTerm());
        }

        public static BooleanExpression operator ==(VariableExpression left, decimal right)
        {
            return left == new LiteralExpression(right.ToConstantTerm());
        }

        public static BooleanExpression operator !=(float left, VariableExpression right)
        {
            return new LiteralExpression(left.ToConstantTerm()) != right;
        }

        public static BooleanExpression operator ==(float left, VariableExpression right)
        {
            return new LiteralExpression(left.ToConstantTerm()) == right;
        }

        public static BooleanExpression operator !=(VariableExpression left, float right)
        {
            return left != new LiteralExpression(right.ToConstantTerm());
        }

        public static BooleanExpression operator ==(VariableExpression left, float right)
        {
            return left == new LiteralExpression(right.ToConstantTerm());
        }

        public static BooleanExpression operator !=(double left, VariableExpression right)
        {
            return new LiteralExpression(left.ToConstantTerm()) != right;
        }

        public static BooleanExpression operator ==(double left, VariableExpression right)
        {
            return new LiteralExpression(left.ToConstantTerm()) == right;
        }

        public static BooleanExpression operator !=(VariableExpression left, double right)
        {
            return left != new LiteralExpression(right.ToConstantTerm());
        }

        public static BooleanExpression operator ==(VariableExpression left, double right)
        {
            return left == new LiteralExpression(right.ToConstantTerm());
        }

        public static BooleanExpression operator !=(string left, VariableExpression right)
        {
            return new LiteralExpression(left.ToConstantTerm()) != right;
        }

        public static BooleanExpression operator ==(string left, VariableExpression right)
        {
            return new LiteralExpression(left.ToConstantTerm()) == right;
        }

        public static BooleanExpression operator !=(VariableExpression left, string right)
        {
            return left != new LiteralExpression(right.ToConstantTerm());
        }

        public static BooleanExpression operator ==(VariableExpression left, string right)
        {
            return left == new LiteralExpression(right.ToConstantTerm());
        }

        public static BooleanExpression operator !=(bool left, VariableExpression right)
        {
            return new LiteralExpression(left.ToConstantTerm()) != right;
        }

        public static BooleanExpression operator ==(bool left, VariableExpression right)
        {
            return new LiteralExpression(left.ToConstantTerm()) == right;
        }

        public static BooleanExpression operator !=(VariableExpression left, bool right)
        {
            return left != new LiteralExpression(right.ToConstantTerm());
        }

        public static BooleanExpression operator ==(VariableExpression left, bool right)
        {
            return left == new LiteralExpression(right.ToConstantTerm());
        }

        public static BooleanExpression operator !=(DateTime left, VariableExpression right)
        {
            return new LiteralExpression(left.ToConstantTerm()) != right;
        }

        public static BooleanExpression operator ==(DateTime left, VariableExpression right)
        {
            return new LiteralExpression(left.ToConstantTerm()) == right;
        }

        public static BooleanExpression operator !=(VariableExpression left, DateTime right)
        {
            return left != new LiteralExpression(right.ToConstantTerm());
        }

        public static BooleanExpression operator ==(VariableExpression left, DateTime right)
        {
            return left == new LiteralExpression(right.ToConstantTerm());
        }
#pragma warning restore 1591
    }
}