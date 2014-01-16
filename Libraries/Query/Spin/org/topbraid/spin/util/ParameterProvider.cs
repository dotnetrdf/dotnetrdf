/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved.
 *******************************************************************************/
using System;
using System.Collections.Generic;
namespace org.topbraid.spin.util
{
    /**
     * A platform independent wrapper of HttpServletRequest, factoring out the <CODE>getParameter</CODE>
     * method.
     *
     * @author Holger Knublauch
     */
    public interface ParameterProvider
    {

        /**
         * Gets the value of a given parameter.
         * The value "" is a real value, and does not indicate 'not defined'.
         * i.e. if the parameter is missing, this must return null.
         * @param key  the parameter
         * @return the value Is null if the parameter is not defined.
         */
        String getParameter(String key);


        /**
         * Gets an Iterator over all known parameter names.
         * @return the names
         */
        IEnumerator<String> listParameterNames();
    }
}