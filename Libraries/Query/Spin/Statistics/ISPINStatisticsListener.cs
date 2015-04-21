namespace VDS.RDF.Query.Spin.Statistics
{
    /**
     * An interface for objects interested in updates to the SPINStatisticsManager.
     * This can be used to track the execution of SPIN with real-time updates.
     *
     * @author Holger Knublauch
     */

    public interface ISpinStatisticsListener
    {
        /**
         * Called whenever the statistics have been updated.
         */

        void statisticsUpdated();
    }
}