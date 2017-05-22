using System.Web.Http;
using System.Web.Http.Controllers;

namespace Herobook {
    public static class WebApiConfig {
        public static void Register(HttpConfiguration config) {

            config.UseCamelCaseJson();

            config.EnableBrowserJsonSupport();

            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                "DefaultApi",
                "api/{controller}/{id}",
                new { id = RouteParameter.Optional }
            );
        }
    }
}
