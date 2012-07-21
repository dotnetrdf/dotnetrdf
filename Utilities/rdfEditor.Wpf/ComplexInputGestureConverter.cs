/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

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
