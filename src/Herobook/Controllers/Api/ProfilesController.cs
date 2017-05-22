using System;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Herobook.Data;
using Herobook.Data.Entities;

using Herobook.Hypermedia;

namespace Herobook.Controllers.Api {

    public class ProfilesController : ApiController {
        private readonly IDatabase db;

        public ProfilesController() {
            db = new DemoDatabase();
        }

        [Route("api/profiles/")]
        [HttpGet]
        public object GetProfiles() {
            return db.ListProfiles();
        }

        [Route("api/profiles/{username}")]
        [HttpGet]
        public object GetProfile(string username, string expand = null) {
            var resource = db.FindProfile(username);
            return (object)resource ?? NotFound();
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
            return Created(Url.Content($"~/api/profiles/{profile.Username}"), profile);
        }

        [Route("api/profiles/{username}")]
        [HttpPut]
        public object Put(string username, [FromBody] Profile profile) {
            var result = db.UpdateProfile(username, profile);
            return result;
        }


        [Route("api/profiles/{username}")]
        [HttpDelete]
        public object Delete(string username) {
            db.DeleteProfile(username);
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
