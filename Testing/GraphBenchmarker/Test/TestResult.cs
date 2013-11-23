using System;

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
        private readonly TestMetricType _metric;

        public TestResult(TimeSpan elapsed, long actions, String unit, TestMetricType metric)
        {
            Memory = 0d;
            this.Elapsed = elapsed;
            this.Actions = actions;
            this.Unit = unit;
            this._metric = metric;
        }

        public TestResult(TimeSpan elapsed, long memory)
            : this(elapsed, 0, "Bytes", TestMetricType.MemoryUsage)
        {
            this.Memory = (double)memory;

            //Convert up to KB if possible
            if (this.Memory > 1024d)
            {
                this.Memory /= 1024d;
                this.Unit = "Kilobytes";

                //Convert up to MB if possible
                if (this.Memory > 1024d)
                {
                    this.Memory /= 1024d;
                    this.Unit = "Megabytes";

                    //Convert up to GB if possible
                    if (this.Memory > 1024d)
                    {
                        this.Memory /= 1024d;
                        this.Unit = "Gigabytes";
                    }
                }
            }
        }

        public TimeSpan Elapsed { get; private set; }

        public long Actions { get; private set; }

        public string Unit { get; private set; }

        public double Speed
        {
            get
            {
                if (this.Actions > 0)
                {
                    double seconds = ((double)this.Elapsed.TotalMilliseconds) / 1000d;
                    return ((double)this.Actions) / seconds;
                }
                else
                {
                    return Double.NaN;
                }
            }
        }

        public double Memory { get; private set; }

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
