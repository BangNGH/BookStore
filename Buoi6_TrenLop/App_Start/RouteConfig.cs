using System.Web.Mvc;
using System.Web.Routing;

namespace Buoi6_TrenLop
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Book", action = "Index", id = UrlParameter.Optional },
                 namespaces: new[] { "Buoi6_TrenLop.Controllers" }
            );
        }


    }
}
