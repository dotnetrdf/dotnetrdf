using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.GraphBenchmarker.Test
{
    public enum TestMetricType
    {
        MemoryUsage,
        Speed,
        Count
    }

    public class TestResult
    {
        private TimeSpan _elapsed;
        private int _actions;
        private String _unit;
        private double _memory = 0d;
        private TestMetricType _metric;

        public TestResult(TimeSpan elapsed, int actions, String unit, TestMetricType metric)
        {
            this._elapsed = elapsed;
            this._actions = actions;
            this._unit = unit;
            this._metric = metric;
        }

        public TestResult(TimeSpan elapsed, long memory)
            : this(elapsed, 0, "Bytes", TestMetricType.MemoryUsage)
        {
            this._memory = (double)memory;

            //Convert up to KB if possible
            if (this._memory > 1024d)
            {
                this._memory /= 1024d;
                this._unit = "Kilobytes";

                //Convert up to MB if possible
                if (this._memory > 1024d)
                {
                    this._memory /= 1024d;
                    this._unit = "Megabytes";

                    //Convert up to GB if possible
                    if (this._memory > 1024d)
                    {
                        this._memory /= 1024d;
                        this._unit = "Gigabytes";
                    }
                }
            }
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
                return this._unit;
            }
        }

        public double Speed
        {
            get
            {
                if (this._actions > 0)
                {
                    double seconds = ((double)this._elapsed.TotalMilliseconds) / 1000d;
                    return ((double)this._actions) / seconds;
                }
                else
                {
                    return Double.NaN;
                }
            }
        }

        public double Memory
        {
            get
            {
                return this._memory;
            }
        }

        public override string ToString()
        {
            switch (this._metric)
            {
                case TestMetricType.Speed:
                    return this.Speed.ToString("N3") + " " + this.Unit;
                case TestMetricType.MemoryUsage:
                    return this.Memory.ToString("F3") + " " + this.Unit + "";
                case TestMetricType.Count:
                    return this.Actions.ToString("F3") + " " + this.Unit + "";
                default:
                    return "Unknown";
            }
        }
    }
}
