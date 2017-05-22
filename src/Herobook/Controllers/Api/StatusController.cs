using System.Web.Http;
using Herobook.Data;
using Herobook.Filters;

namespace Herobook.Controllers.Api {
    [Authorize]
    [WorkshopBasicAuthentication(Password = "workshop")]
    public class StatusController : ApiController {
        private readonly IDatabase db;

        public StatusController() {
            db = new DemoDatabase();
        }

        [Route("api/status")]
        public object GetStatus() {
            var result = new {
                assembly = this.GetType().Assembly.GetName().Name,
                version = this.GetType().Assembly.GetName().Version,
                username = this.User.Identity.Name,
                profiles = db.CountProfiles()
            };
            return (result);
        }
    }
}