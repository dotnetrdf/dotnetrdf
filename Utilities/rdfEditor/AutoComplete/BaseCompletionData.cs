using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace rdfEditor.AutoComplete
{
    public abstract class BaseCompletionData : ICompletionData, IComparable<ICompletionData>, IComparable, IEquatable<ICompletionData>
    {
        public virtual void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, this.Text);
        }

        public virtual object Content
        {
            get 
            { 
                return this.Text;
            }
        }

        public abstract object Description
        {
            get;
        }

        public virtual System.Windows.Media.ImageSource Image
        {
            get 
            { 
                return null; 
            }
        }

        public abstract double Priority
        {
            get;
            set;
        }

        public abstract string Text
        {
            get;
        }

        public virtual int CompareTo(ICompletionData other)
        {
            int c = this.Priority.CompareTo(other.Priority);
            if (c == 0)
            {
                return this.Text.CompareTo(other.Text);
            }
            else
            {
                return c;
            }
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;
            if (obj is ICompletionData)
            {
                return this.CompareTo((ICompletionData)obj);
            }
            else
            {
                return -1;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is ICompletionData)
            {
                return this.Equals((ICompletionData)obj);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public override string ToString()
        {
            return this.GetType().Name + ": " + this.Text;
        }

        public bool Equals(ICompletionData other)
        {
            return this.GetHashCode().Equals(other.GetHashCode());
        }
    }
}
