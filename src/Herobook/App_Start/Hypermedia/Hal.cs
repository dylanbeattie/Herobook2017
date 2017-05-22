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
