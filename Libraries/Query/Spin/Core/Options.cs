/*

dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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

namespace VDS.RDF.Query.Spin
{
    public static class Options
    {
        private static bool _useCaching = false;
        private static bool _emulateSpinProcessing = true;
        private static bool _autoCommitTransactions = true;
        private static bool _recordStatistics = true;

        public static bool AutoCommitTransactions
        {
            get
            {
                return _autoCommitTransactions;
            }
            set
            {
                _autoCommitTransactions = value;
            }
        }

        public static bool UseCaching
        {
            get
            {
                return _useCaching;
            }
            set
            {
                _useCaching = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether to emulate SPIN features locally.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Should be set to true if the underlying storage already provides a SPIN implementation.
        /// </para>
        /// </remarks>
        public static bool EmulateSpinProcessing
        {
            get
            {
                return _emulateSpinProcessing;
            }
            set
            {
                _emulateSpinProcessing = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether to record SPIN statistics.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Statistics can be recorded only if EmulateSpinProcessing is set to true
        /// </para>
        /// </remarks>
        public static bool RecordStatistics
        {
            get
            {
                return _emulateSpinProcessing && _recordStatistics;
            }
            set
            {
                _recordStatistics = value;
            }
        }
    }
}