using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Dynamic;
using System.Linq;
using Herobook.Data.Entities;

namespace Herobook.Hypermedia {
    public class Hal {
        public static dynamic Href(string url) {
            return new { href = url };
        }

        /// <summary>Generate a hypermedia links object containing first/final/prev/next links for paging through datasets.</summary>
        /// <param name="path">The absolute URL path to be decorated with paging querystring parameters</param>
        /// <param name="index">The index of the first record on the current page</param>
        /// <param name="count">The count of items on each page</param>
        /// <param name="total">The total number of items in the collection</param>
        /// <returns></returns>
        public static dynamic Paginate(string path, int index, int count, int total) {
            dynamic _links = new ExpandoObject();
            var maxIndex = total - 1;
            _links.first = Href($"{path}?index=0");
            _links.final = Href($"{path}?index={maxIndex - maxIndex % count}");
            if (index > 0) _links.previous = Href($"{path}?index={index - count}");
            if (index + count < maxIndex) _links.next = Href($"{path}?index={index + count}");
            return _links;
        }
    }

    public static class EntityExtensions {
        public static dynamic ToDynamic(this object value) {
            IDictionary<string, object> expando = new ExpandoObject();
            foreach (PropertyDescriptor property
                in TypeDescriptor.GetProperties(value.GetType())) {
                expando.Add(property.Name, property.GetValue(value));
            }
            return (ExpandoObject)expando;
        }


        public static IDictionary<string, object> ToDictionary(this object d) {
            return d.GetType().GetProperties().ToDictionary(x => x.Name, x => x.GetValue(d, null));
        }

        public static dynamic ToResource(this Status status) {
            dynamic resource = status.ToDynamic();
            var href = $"/api/profiles/{status.Username}/statuses/{status.StatusId}";
            resource._actions = new {
                update = new {
                    name = "Update this status",
                    href,
                    method = "PUT",
                    type = "application/json"
                },
                delete = new {
                    name = "Delete this status",
                    href,
                    method = "DELETE"
                }
            };
            return (resource);
        }

        public static dynamic ToResource(this Photo photo) {
            dynamic resource = photo.ToDynamic();
            var href = $"/api/profiles/{photo.Username}/photos/{photo.PhotoId}";
            resource._links = new {
                self = Hal.Href(href)
            };
            resource._actions = new {
                update = new {
                    name = "Update this photo",
                    href = href,
                    method = "PUT",
                    type = "application/json"
                },
                delete = new {
                    name = "Delete this photo",
                    href,
                    method = "DELETE"
                }
            };
            return (resource);
        }

        public static dynamic ToResource(this Profile profile) {
            dynamic resource = profile.ToDynamic();
            resource._links = new {
                self = Hal.Href($"/api/profiles/{profile.Username}"),
                friends = Hal.Href($"/api/profiles/{profile.Username}/friends"),
                statuses = Hal.Href($"/api/profiles/{profile.Username}/statuses"),
                photos = Hal.Href($"/api/profiles/{profile.Username}/photos")
            };
            resource._actions = new {
                update = new {
                    name = "Update this profile",
                    href = $"/api/profiles/{profile.Username}",
                    method = "PUT",
                    type = "application/json"
                },
                delete = new {
                    name = "Delete this profile",
                    href = $"/api/profiles/{profile.Username}",
                    method = "DELETE"
                }
            };
            return resource;
        }
    }

    /// <summary>
    ///     Turn the ImageCodecInfo Extension property, which looks like *.JPG;*.JPE*,*.JPEG, into a writable file
    ///     extension
    /// </summary>
    public static class ImageCodecInfoExtensions {
        public static string GetWritableFileExtension(this ImageCodecInfo codec) {
            var extension = codec.FilenameExtension.ToLowerInvariant().Split(';').First();
            return extension.Substring(2);
        }
    }
}
