using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace TimeSheet
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
             name: "subscribe",
             url: "timesheet-subscribe",
             defaults: new { controller = "Home", action = "subscribe" }
         );


            routes.MapRoute(
             name: "worklog",
             url: "worklogdetails",
             defaults: new { controller = "Home", action = "GetReportBasedonQuery" }
         );
            
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
