﻿using System.Web;
using System.Web.Routing;

namespace Subtext.Framework.Routing
{
    public class HttpRouteHandler<THandler> : IRouteHandler where THandler : IHttpHandler, new()
    {
        public HttpRouteHandler(THandler handler) {
            HttpHandler = handler;
        }

        public HttpRouteHandler()
        {
            HttpHandler = new THandler();
        }

        public IHttpHandler HttpHandler { 
            get; 
            private set; 
        }

        public IHttpHandler GetHttpHandler(RequestContext requestContext) {
            IRouteableHandler routableHandler = HttpHandler as IRouteableHandler;
            if (routableHandler != null) {
                routableHandler.RequestContext = requestContext;
            }

            return HttpHandler;
        }
    }
}
