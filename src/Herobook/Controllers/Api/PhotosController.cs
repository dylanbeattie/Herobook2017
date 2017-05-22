using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Hosting;
using System.Web.Http;
using Herobook.Data;
using Herobook.Data.Entities;
using Herobook.Hypermedia;
using Herobook.Routing;

namespace Herobook.Controllers.Api {
    public class PhotosController : ApiController {
        private readonly IDatabase db;

        public PhotosController() {
            this.db = new DemoDatabase();
        }

        [Route("api/profiles/{username}/photos")]
        [HttpGet]
        public object GetProfilePhotos(string username) {
            var items = db.LoadPhotos(username).Select(s => s.ToResource());
            var result = new {
                items
            };
            return result;
        }


        [Route("api/profiles/{username}/photos")]
        [HttpPost]
        public object PostProfilePhoto(string username, [FromBody] Photo photo) {
            photo.Username = username;
            photo.PostedAt = DateTimeOffset.Now;
            return db.CreatePhoto(photo).ToResource();
        }

        [Route("api/profiles/{username}/photos/{photoId}")]
        [HttpGet]
        public object GetProfilePhoto(string username, Guid photoId) {
            var photo = db.LoadPhoto(photoId);
            return photo.ToResource();
        }

        [ContentTypeRoute("api/profiles/{username}/photos/{photoId}", "application/json")]
        [HttpPut]
        public object UpdateProfilePhoto(string username, Guid photoId, [FromBody] Photo photo) {
            return db.UpdatePhoto(photoId, photo);
        }

        [Route("api/profiles/{username}/photos/{photoId}")]
        [HttpDelete]
        public void DeleteProfilePhoto(string username, Guid photoId) {
            db.DeletePhoto(photoId);
        }
    }
}