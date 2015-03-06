using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using VDS.Common.References;

namespace VDS.RDF.Query.Spin.Statistics
{

    /**
     * A singleton managing statistics for SPIN execution.
     * In TopBraid, this singleton is used as a single entry point for various
     * statistics producing engines such as TopSPIN.
     * The results are displayed in the SPIN Statistics view of TBC.
     * 
     * The SPINStatisticsManager is off by default, and needs to be activated
     * with <code>setRecording(true);</code>.
     * 
     * @author Holger Knublauch
     */
    // TODO change IsXXX into get/set properties
    public class SpinStatisticsManager
    {

        private static ThreadIsolatedReference<SpinStatisticsManager> _singleton = new ThreadIsolatedReference<SpinStatisticsManager>();

        /**
         * Gets the singleton instance of this class.
         * @return the SPINStatisticsManager (never null)
         */
        public static SpinStatisticsManager Get()
        {
            if (_singleton.Value == null) _singleton.Value = new SpinStatisticsManager();
            _singleton.Value.SetRecording(Options.RecordStatistics);
            return _singleton.Value;
        }


        private HashSet<ISpinStatisticsListener> _listeners = new HashSet<ISpinStatisticsListener>();

        private bool _recording;

        private bool _recordingNativeFunctions;

        private bool _recordingSPINFunctions;

        //TODO: check for thread safety and synchronization. If order is not an issue, use System.Collections.Concurrent.ConcurrentBag instead
        private List<SpinStatistics> stats = new List<SpinStatistics>();


        public void AddListener(ISpinStatisticsListener listener)
        {
            _listeners.Add(listener);
        }


        /**
         * Adds new statistics and notifies any registered listeners.
         * This should only be called if <code>isRecording()</code> is true
         * to prevent the unnecessary creation of SPINStatistics objects.
         * @param values  the statistics to add
         */
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Add(SpinStatistics value)
        {
            if (_recording)
            {
                AddSilently(value);
                NotifyListeners();
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Add(IEnumerable<SpinStatistics> values)
        {
            if (_recording)
            {
                AddSilently(values);
                NotifyListeners();
            }
        }

        /**
         * Adds new statistics without notifying listeners.
         * This should only be called if <code>isRecording()</code> is true
         * to prevent the unnecessary creation of SPINStatistics objects.
         * @param values  the statistics to add
         */
        public void AddSilently(IEnumerable<SpinStatistics> values)
        {
            if (_recording)
            {
                foreach (SpinStatistics s in values)
                {
                    AddSilently(s);
                }
            }
        }

        public void AddSilently(SpinStatistics value)
        {
            if (_recording)
            {
                stats.Add(value);
            }
        }

        /**
         * Gets all previously added statistics.
         * @return the statistics
         */
        [MethodImpl(MethodImplOptions.Synchronized)]
        public List<SpinStatistics> GetStatistics()
        {
            return stats;
        }

        public TimeSpan TotalDuration {
            get {
                long ticks = 0;
                foreach (SpinStatistics stat in stats.Where(s => s.Label == "Query Execution"))
                {
                    ticks += stat.Duration.Ticks;
                }
                return new TimeSpan(ticks);
            }
        }

        public bool IsRecording()
        {
            return _recording;
        }


        public bool IsRecordingNativeFunctions()
        {
            return _recordingNativeFunctions;
        }


        public bool IsRecordingSPINFunctions()
        {
            return _recordingSPINFunctions;
        }


        public void RemoveListener(ISpinStatisticsListener listener)
        {
            _listeners.Remove(listener);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Reset()
        {
            stats.Clear();
            NotifyListeners();
        }


        /**
         * Notifies all registered SPINStatisticsListeners so that they can refresh themselves.
         */
        public void NotifyListeners()
        {
            foreach (ISpinStatisticsListener listener in new List<ISpinStatisticsListener>(_listeners))
            {
                listener.statisticsUpdated();
            }
        }


        public void SetRecording(bool value)
        {
            this._recording = value;
        }


        public void SetRecordingNativeFunctions(bool value)
        {
            this._recordingNativeFunctions = value;
        }


        public void SetRecordingSPINFunctions(bool value)
        {
            this._recordingSPINFunctions = value;
        }
    }
}