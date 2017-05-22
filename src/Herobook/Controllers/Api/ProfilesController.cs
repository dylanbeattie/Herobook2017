using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Herobook.Data;
using Herobook.Data.Entities;
using Herobook.Filters;
using Herobook.Hypermedia;

namespace Herobook.Controllers.Api {
    [WorkshopBasicAuthentication(Password = "workshop")]
    public class ProfilesController : ApiController {
        private readonly IDatabase db;

        public ProfilesController() {
            db = new DemoDatabase();
        }

        [Route("api/profiles/")]
        [HttpGet]
        public object GetProfiles(int index = 0, int count = 10) {
            var _links = Hal.Paginate(Request.RequestUri.AbsolutePath, index, count, db.CountProfiles());
            var items = db.ListProfiles().Skip(index).Take(count).Select(profile => profile.ToResource());
            var _actions = new {
                create = new {
                    name = "Create a new profile",
                    href = Request.RequestUri.AbsolutePath,
                    method = "POST",
                    type = "application/json",
                    schema = new { href = "/schemas/profile.json" }
                }
            };
            var result = new {
                _links,
                _actions,
                items
            };
            return result;
        }

        [Route("api/profiles/{username}")]
        [HttpGet]
        public object GetProfile(string username) {
            return (object)db.FindProfile(username)?.ToResource() ?? NotFound();
        }

        [Route("api/profiles/{username}/friends")]
        [HttpGet]
        public object GetProfileFriends(string username) {
            return db.LoadFriends(username);
        }


        [Route("api/profiles/")]
        [HttpPost]
        public object Post([FromBody] Profile profile) {
            var existing = db.FindProfile(profile.Username);
            if (existing != null) return Request.CreateResponse(HttpStatusCode.Conflict, "That username is not available");
            db.CreateProfile(profile);
            return Created(Url.Content($"~/api/profiles/{profile.Username}"), profile.ToResource());
        }

        [Authorize]
        [Route("api/profiles/{username}")]
        [HttpPut]
        public object Put(string username, [FromBody] Profile profile) {
            if (User.Identity.Name != username) return Request.CreateErrorResponse(HttpStatusCode.Forbidden, $"User '{User.Identity.Name}' does not have permission to update user '{username}'");
            var result = db.UpdateProfile(username, profile);
            return result.ToResource();
        }

        [Authorize]
        [Route("api/profiles/{username}")]
        [HttpDelete]
        public object Delete(string username) {
            if (User.Identity.Name != username) return Request.CreateErrorResponse(HttpStatusCode.Forbidden, $"User '{User.Identity.Name}' does not have permission to delete user '{username}'");
            db.DeleteProfile(username);
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
