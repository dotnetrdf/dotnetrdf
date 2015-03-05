using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
