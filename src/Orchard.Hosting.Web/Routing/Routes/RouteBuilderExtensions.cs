﻿using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Routing;

namespace Orchard.Hosting.Web.Routing.Routes {
    public static class RouteBuilderExtensions {
        public static IRouteBuilder AddTenantRoute(this IRouteBuilder routeBuilder,
                                                   string urlHost,
                                                   IRouter handler,
                                                   RequestDelegate pipeline) {
            routeBuilder.Routes.Add(new TenantRoute(handler, urlHost, pipeline));
            return routeBuilder;
        }
    }
}