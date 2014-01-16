/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
namespace org.topbraid.spin.system
{


    /**
     * A singleton that provides access to the current SPIN rendering settings.
     * The singleton can be replaced to install different default settings.
     * For example, TopBraid Composer stores these settings in the Eclipse
     * preferences.
     * 
     * @author Holger Knublauch
     */
    public class SPINPreferences
    {

        private static SPINPreferences singleton = new SPINPreferences();


        /**
         * Gets the singleton instance of this class.
         * @return the singleton
         */
        public static SPINPreferences get()
        {
            return singleton;
        }


        /**
         * Changes the singleton to some subclass.
         * @param value  the new singleton (not null)
         */
        public static void set(SPINPreferences value)
        {
            SPINPreferences.singleton = value;
        }


        /**
         * Indicates whether the SPIN generator shall convert variables into
         * URI nodes, so that they can be shared between multiple queries.
         * @return true  to create shared URI variables (default: false)
         */
        public bool isCreateURIVariables()
        {
            return false;
        }


        /**
         * Indicates whether the SPIN generator shall reuse the same blank node
         * for a variable multiple times within the same query.
         * This is off by default to make bnode structures more self-contained.
         * @return true  to reuse blank nodes
         */
        public bool isReuseLocalVariables()
        {
            return false;
        }
    }
}