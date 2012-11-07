/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace VDS.RDF.Utilities.Editor.Wpf
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Based on code by Kent Boogaart from <a href="http://kentb.blogspot.com/2009/03/multikeygesture.html">this blog entry</a>
    /// </remarks>
    public class ComplexInputGestureConverter : TypeConverter
    {
        private readonly KeyConverter _keyConverter;
		private readonly ModifierKeysConverter _modifierKeysConverter;

		public ComplexInputGestureConverter()
		{
			_keyConverter = new KeyConverter();
			_modifierKeysConverter = new ModifierKeysConverter();
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof(string);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var keyStrokes = (value as string).Split(',');

            List<KeyGesture> gestures = new List<KeyGesture>();

			for (var i = 0; i < keyStrokes.Length; i++)
			{
                String keyStroke = keyStrokes[i];
                if (keyStroke.Contains('+'))
                {
                    Key k = this.ConvertKey(keyStroke.Substring(keyStroke.IndexOf('+') + 1));
                    ModifierKeys modifier = (ModifierKeys)_modifierKeysConverter.ConvertFrom(keyStroke.Substring(0, keyStroke.IndexOf('+')));
                    if (modifier != ModifierKeys.Shift)
                    {
                        gestures.Add(new KeyGesture(k, modifier));
                    }
                    else
                    {
                        gestures.Add(new ShiftKeyGesture(k));
                    }
                }
                else
                {
                    gestures.Add(new UnmodifiedKeyGesture(this.ConvertKey(keyStroke)));
                }
			}

            return new ComplexInputGesture(gestures, value.ToString());
		}

        private Key ConvertKey(String keyDef)
        {
            switch (keyDef)
            {
                case "+":
                    return Key.OemPlus;
                case "-":
                    return Key.OemMinus;
                default:
                    return (Key)_keyConverter.ConvertFrom(keyDef);
            }
        }
    }
}
