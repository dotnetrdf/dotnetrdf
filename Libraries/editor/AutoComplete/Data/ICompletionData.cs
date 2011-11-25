using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Editor.AutoComplete.Data
{
    /// <summary>
    /// Interface for completion data
    /// </summary>
    public interface ICompletionData 
        : IComparable, IComparable<ICompletionData>, IEquatable<ICompletionData>
    {
        /// <summary>
        /// Gets the description that should be displayed as a tool tip (if available)
        /// </summary>
        String Description
        {
            get;
        }

        /// <summary>
        /// Gets the text to display in the auto-complete list
        /// </summary>
        String DisplayText
        {
            get;
        }

        /// <summary>
        /// Gets the text that should actually be inserted
        /// </summary>
        String InsertionText
        {
            get;
        }

        /// <summary>
        /// Gets the priority
        /// </summary>
        double Priority
        {
            get;
        }
    }
}
