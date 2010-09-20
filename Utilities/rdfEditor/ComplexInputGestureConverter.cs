using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace rdfEditor
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
