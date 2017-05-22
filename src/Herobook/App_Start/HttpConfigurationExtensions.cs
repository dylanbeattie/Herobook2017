using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Herobook {
    public static class HttpConfigurationExtensions {
        /// <summary>
        ///     Override the default JSON serialization to return pretty-printed camel-cased JSON.
        /// </summary>
        /// <param name="config">The instance of <see cref="HttpConfiguration" /> that configures this server instance.</param>
        public static void UseCamelCaseJson(this HttpConfiguration config) {
            var formatters = GlobalConfiguration.Configuration.Formatters;
            var jsonFormatter = formatters.JsonFormatter;
            var settings = jsonFormatter.SerializerSettings;
            settings.Formatting = Formatting.Indented;
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }

        /// <summary>
        ///     Configures WebAPI to send formatted JSON responses (instead of XML) to browsers that request text/html.
        /// </summary>
        /// <param name="config">The instance of <see cref="HttpConfiguration" /> that configures this server instance.</param>
        public static void EnableBrowserJsonSupport(this HttpConfiguration config) {
            config.Formatters.Add(new BrowserJsonFormatter());
        }
    }
}