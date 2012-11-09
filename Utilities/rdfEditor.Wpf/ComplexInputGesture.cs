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
