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
            var _actions = new {
                create = new {
                    name = "Add a new photo to this profile",
                    href = Request.RequestUri.AbsolutePath,
                    method = "POST",
                    type = "application/json"
                }
            };
            var result = new {
                _actions,
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

        public static string GetMimeType(ImageFormat imageFormat) {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            return codecs.First(codec => codec.FormatID == imageFormat.Guid).MimeType;
        }

        private object File(string filePath, string mimeType) {
            var result = new HttpResponseMessage(HttpStatusCode.OK);
            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            result.Content = new StreamContent(stream);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
            return result;
        }

        private object RenderImage(Photo photo, HttpHeaderValueCollection<MediaTypeWithQualityHeaderValue> types) {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            var type = types.FirstOrDefault(t => codecs.Any(c => c.MimeType == t.MediaType));
            if (type == null) return Request.CreateResponse(HttpStatusCode.NotAcceptable);
            var path = HostingEnvironment.MapPath($"~/App_Data/photos/{photo.Username}/");
            var files = Directory.GetFiles(path, $"{photo.PhotoId}.*");
            if (!files.Any()) return NotFound();

            var codec = codecs.FirstOrDefault(c => c.MimeType == type.MediaType);
            var format = new ImageFormat(codec.FormatID);


            foreach (var file in files) {
                var pathToReturn = file;
                using (var bitmap = Image.FromFile(file)) {
                    var requestedType = type.MediaType;
                    var availableType = GetMimeType(bitmap.RawFormat);
                    if (type.MediaType != availableType) {
                        pathToReturn = Path.Combine(path, photo.PhotoId + "." + codec.GetWritableFileExtension());
                        bitmap.Save(pathToReturn, format);
                    }
                    return (File(pathToReturn, type.MediaType));
                }
            }
            return Request.CreateResponse(HttpStatusCode.NotAcceptable);
        }


        [Route("api/profiles/{username}/photos/{photoId}")]
        [HttpGet]
        public object GetProfilePhoto(string username, Guid photoId) {
            var photo = db.LoadPhoto(photoId);
            var accept = Request.Headers.Accept;
            if (accept.Any(t => t.MediaType.StartsWith("image/"))) return RenderImage(photo, accept);
            return photo.ToResource();
        }

        [ContentTypeRoute("api/profiles/{username}/photos/{photoId}", "application/json")]
        [HttpPut]
        public object UpdateProfilePhoto(string username, Guid photoId, [FromBody] Photo photo) {
            return db.UpdatePhoto(photoId, photo);
        }

        private object UploadFile(string username, Guid photoId, string fileExtension) {
            var stream = this.Request.Content.ReadAsStreamAsync().Result;
            var path = HostingEnvironment.MapPath($"~/App_Data/photos/{username}/{photoId}.{fileExtension}");
            var directoryPath = Path.GetDirectoryName(path);
            var directory = new DirectoryInfo(directoryPath);
            if (!directory.Exists) directory.Create();
            using (var file = new FileStream(path, FileMode.Create)) stream.CopyTo(file);
            return this.Request.CreateResponse(HttpStatusCode.OK);
        }

        [ContentTypeRoute("api/profiles/{username}/photos/{photoId}", "image/jpeg")]
        [HttpPut]
        public object UploadProfilePhotoFromJpgPut(string username, Guid photoId) {
            return UploadFile(username, photoId, "jpg");
        }


        [ContentTypeRoute("api/profiles/{username}/photos/{photoId}", "image/png")]
        [HttpPut]
        public object UploadProfilePhotoFromPngPut(string username, Guid photoId) {
            return UploadFile(username, photoId, "png");
        }

        [Route("api/profiles/{username}/photos/{photoId}")]
        [HttpDelete]
        public void DeleteProfilePhoto(string username, Guid photoId) {
            db.DeletePhoto(photoId);
        }
    }
}