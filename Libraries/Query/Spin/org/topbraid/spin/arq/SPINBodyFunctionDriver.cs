
using org.topbraid.spin.model;
using org.topbraid.spin.arq;

namespace org.topbraid.spin.arq
{

    /**
     * A (default) SPINFunctionDriver using spin:body to find an executable
     * body for a SPIN function.
     * 
     * @author Holger Knublauch
     */
    public class SPINBodyFunctionDriver : SPINFunctionDriver
    {

        override public SPINFunctionFactory create(Function spinFunction)
        {
            return doCreate(spinFunction);
        }


        public static SPINFunctionFactory doCreate(Function spinFunction)
        {
            return new SPINARQFunction(spinFunction);
        }
    }
}