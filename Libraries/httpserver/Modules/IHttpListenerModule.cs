using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.Web.Modules
{
    /// <summary>
    /// Interface for Modules which can be used to modify the request/response as desired
    /// </summary>
    /// <remarks>
    /// <para>
    /// Modules are either applied Pre-Request (i.e. before a <see cref="IHttpListenerHandler">IHttpListenerHandler</see> is selected to provide the actual response) or Pre-Response (i.e. before the response object is closed and sent to the client)
    /// </para>
    /// </remarks>
    public interface IHttpListenerModule
    {
        /// <summary>
        /// Processes a Request and returns false if no further handling of the request should take place
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// If used as a Pre-Request Module returning false will cause the actual process request step where a <see cref="IHttpListenerHandler">IHttpListenerHandler</see> is applied to be skipped as well as any further pre-request modules
        /// </para>
        /// <para>
        /// If used as a Pre-Response Module returning false will cause any further Pre-Response modules to be skipped
        /// </para>
        /// </remarks>
        bool ProcessRequest(HttpServerContext context);
    }
}
