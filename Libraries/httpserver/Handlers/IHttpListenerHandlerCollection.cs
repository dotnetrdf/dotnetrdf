using System;
using VDS.Web;

namespace VDS.Web.Handlers
{
    public interface IHttpListenerHandlerCollection
    {
        void AddMapping(VDS.Web.HttpRequestMapping mapping);

        int Count 
        {
            get;
        }

        IHttpListenerHandler GetHandler(HttpServerContext context);

        IHttpListenerHandler GetHandler(Type handlerType);

        void InsertMapping(HttpRequestMapping mapping, int insertAt);
    }
}
