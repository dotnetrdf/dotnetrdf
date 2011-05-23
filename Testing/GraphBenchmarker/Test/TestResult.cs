using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.GraphBenchmarker.Test
{
    public class TestResult
    {
        private TimeSpan _elapsed;
        private int _actions;
        private String _unit;

        public TestResult(TimeSpan elapsed, int actions, String unit)
        {
            this._elapsed = elapsed;
            this._actions = actions;
            this._unit = unit;
        }

        public TimeSpan Elapsed
        {
            get
            {
                return this._elapsed;
            }
        }

        public int Actions
        {
            get
            {
                return this._actions;
            }
        }

        public String Unit
        {
            get
            {
                return this._unit + "/Second";
            }
        }

        public double Speed
        {
            get
            {
                double seconds = ((double)this._elapsed.TotalMilliseconds) / 1000d;
                return ((double)this._actions) / seconds;
            }
        }

        public override string ToString()
        {
            return this.Speed + " " + this.Unit;
        }
    }
}
