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
    public class ComplexInputGesture : KeyGesture
    {
        private List<KeyGesture> _gestures = new List<KeyGesture>();
        private int _index = 0;
        private DateTime _lastKeyPress;
        private TimeSpan _keyPressInterval = new TimeSpan(0, 0, 1);

        public ComplexInputGesture(IEnumerable<KeyGesture> gestureSequence, String displayString)
            : base(Key.None, ModifierKeys.None, displayString)
        {
            this._gestures.AddRange(gestureSequence);
        }

        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            if (this._index >= this._gestures.Count) this._index = 0;

            KeyEventArgs e = inputEventArgs as KeyEventArgs;
            if (e == null || IsIgnorableKey(e.Key, e.KeyboardDevice.Modifiers))
            {
                return false;
            }

            if (this._index > 0 && (DateTime.Now - this._lastKeyPress) > _keyPressInterval)
            {
                this._index = 0;
            }

            if (this._gestures[this._index].Matches(targetElement, inputEventArgs))
            {
                this._lastKeyPress = DateTime.Now;
                this._index++;
                inputEventArgs.Handled = true;
                return (this._index == this._gestures.Count);
            }
            else
            {
                this._index = 0;
                return false;
            }
        }

        private static bool IsIgnorableKey(Key key, ModifierKeys modifiers)
        {
            if ((key == Key.LeftCtrl || key == Key.RightCtrl) && modifiers == ModifierKeys.Control)
            {
                return true;
            }
            else if ((key == Key.LeftAlt || key == Key.RightAlt) && modifiers == ModifierKeys.Alt)
            {
                return true;
            }
            else if ((key == Key.LeftShift || key == Key.RightShift) && modifiers == ModifierKeys.Shift)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            for (int i = 0; i < this._gestures.Count; i++)
            {
                output.Append(this._gestures[i].ToString());
                if (i < this._gestures.Count - 1)
                {
                    output.Append(",");
                }
            }
            return output.ToString();
        }

    }

    public class UnmodifiedKeyGesture : KeyGesture
    {
        private Key _key;

        public UnmodifiedKeyGesture(Key k)
            : base(Key.None)
        {
            this._key = k;
        }

        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            if (inputEventArgs is KeyEventArgs)
            {
                KeyEventArgs e = (KeyEventArgs)inputEventArgs;
                return (e.Key == this._key && e.KeyboardDevice.Modifiers == ModifierKeys.None);
            }
            else
            {
                return false;
            }
        }

        public override string ToString()
        {
            return this._key.ToString();
        }
    }

    public class ShiftKeyGesture : KeyGesture
    {
        private Key _key;

        public ShiftKeyGesture(Key k)
            : base(Key.None)
        {
            this._key = k;
        }

        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            if (inputEventArgs is KeyEventArgs)
            {
                KeyEventArgs e = (KeyEventArgs)inputEventArgs;
                return (e.Key == this._key && e.KeyboardDevice.Modifiers == ModifierKeys.Shift);
            }
            else
            {
                return false;
            }
        }

        public override string ToString()
        {
            return "Shift+" + this._key.ToString();
        }

    }
}
