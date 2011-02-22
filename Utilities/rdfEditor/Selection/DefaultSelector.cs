using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rdfEditor.Selection
{
    public class DefaultSelector : BaseSelector
    {
        /// <summary>
        /// Gets whether a Character is a Starting Deliminator
        /// </summary>
        /// <param name="c">Character</param>
        /// <returns></returns>
        protected override bool IsStartingDeliminator(char c)
        {
            switch (c)
            {
                case '"':
                case '<':
                case '\n':
                case '\r':
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Gets whether a Character is an Ending Deliminator
        /// </summary>
        /// <param name="c">Character</param>
        /// <returns></returns>
        protected override bool IsEndingDeliminator(char c)
        {
            switch (c)
            {
                case '"':
                case '>':
                case '\n':
                case '\r':
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Gets whether a specific Starting Deliminator should be matched with a specific ending deliminator
        /// </summary>
        /// <param name="c">Starting Deliminator</param>
        /// <returns></returns>
        protected override char? RequireMatchingDeliminator(char c)
        {
            switch (c)
            {
                case '"':
                    return c;
                case '<':
                    return '>';
                default:
                    return null;
            }
        }
    }
}
