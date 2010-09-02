using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public class ComplexInputBinding : InputBinding
    {
        [TypeConverter(typeof(ComplexInputGestureConverter))]
        public override InputGesture Gesture
        {
            get 
            { 
                return base.Gesture as ComplexInputGesture; 
            }
            set
            {
                if (!(value is ComplexInputGesture))
                {
                    throw new ArgumentException();
                }

                base.Gesture = value;
            }
        }
    }
}
