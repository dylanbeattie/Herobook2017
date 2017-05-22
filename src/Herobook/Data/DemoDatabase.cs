using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using Herobook.Data.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Herobook.Data {
    public class DemoDatabase : IDatabase {
        private static readonly IList<Profile> profiles;
        private static readonly IList<Friendship> friendships;
        private static readonly IList<Status> statuses;
        private static readonly IList<Photo> photos;

        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Formatting.Indented
        };

        static DemoDatabase() {
            profiles = ReadData<IList<Profile>>("profiles") ?? new List<Profile>();
            friendships = ReadData<IList<Friendship>>("friendships") ?? new List<Friendship>();
            statuses = ReadData<IList<Status>>("statuses") ?? new List<Status>();
            photos = ReadData<IList<Photo>>("photos") ?? new List<Photo>();
        }


        private void Save() {
            WriteData("profiles", profiles);
            WriteData("friendships", friendships);
            WriteData("statuses", statuses);
            WriteData("photos", photos);
        }

        public void CreateFriendship(string username1, string username2) {
            if (friendships.Any(f => f.Names.Contains(username1) && f.Names.Contains(username1))) return;
            var friendship = new Friendship(username1, username2);
            friendships.Add(friendship);
            Save();
        }

        public void DeleteProfile(string username) {
            var target = FindProfile(username);
            profiles.Remove(target);
            Save();
        }

        public Profile UpdateProfile(string username, Profile profile) {
            var record = FindProfile(username);
            if (record == null) profiles.Add(record = new Profile());
            record.Username = profile.Username;
            record.Name = profile.Name;
            Save();
            return record;
        }

        public int CountProfiles() {
            return profiles.Count;
        }

        public IEnumerable<Profile> ListProfiles() {
            return profiles;
        }

        public void CreateProfile(Profile profile) {
            if (FindProfile(profile.Username) != null) throw new ArgumentException("That username is not available");
            profiles.Add(profile);
            Save();
        }

        public Profile FindProfile(string username) {
            return profiles.FirstOrDefault(p => p.Username == username);
        }

        public IEnumerable<Profile> LoadFriends(string username) {
            var friends = friendships.Where(f => f.Names.Contains(username))
                .SelectMany(f => f.Names)
                .Distinct()
                .Where(g => g != username);
            return friends.Select(FindProfile);
        }

        private static string Qualify(string filePath) {
            var appDataAbsolutePath = HostingEnvironment.MapPath("~/App_Data/");
            return Path.Combine(appDataAbsolutePath, filePath);
        }

        private static T ReadData<T>(string filename) {
            try {
                return JsonConvert.DeserializeObject<T>(File.ReadAllText(Qualify(filename + ".json")));
            } catch (FileNotFoundException) {
                return default(T);
            }
        }


        private void WriteData(string filename, object data) {
            File.WriteAllText(Qualify(filename + ".json"), JsonConvert.SerializeObject(data, settings));
        }

        public IEnumerable<Status> LoadStatuses(string username) {
            return statuses.Where(s => s.Username == username);
        }

        public Status LoadStatus(Guid statusId) {
            return statuses.FirstOrDefault(s => s.StatusId == statusId);
        }

        public Status CreateStatus(Status status) {
            status.PostedAt = DateTimeOffset.Now;
            return UpdateStatus(Guid.NewGuid(), status);
        }

        public Status UpdateStatus(Guid statusId, Status status) {
            var target = LoadStatus(statusId);
            if (target == default(Status)) {
                target = new Status() {
                    StatusId = statusId
                };
                statuses.Add(target);
            }
            target.Comment = status.Comment;
            target.PostedAt = status.PostedAt;
            target.Username = status.Username;
            Save();
            return target;
        }

        public void DeleteStatus(Guid statusId) {
            var target = LoadStatus(statusId);
            if (target == default(Status)) return;
            statuses.Remove(target);
            Save();
        }

        public IEnumerable<Photo> LoadPhotos(string username) {
            return photos.Where(p => p.Username == username);
        }

        public Photo CreatePhoto(Photo photo) {
            return UpdatePhoto(Guid.NewGuid(), photo);
        }

        public Photo LoadPhoto(Guid photoGuid) {
            return photos.FirstOrDefault(photo => photo.PhotoId == photoGuid);
        }

        public Photo UpdatePhoto(Guid photoId, Photo photo) {
            var target = LoadPhoto(photoId);
            if (target == default(Photo)) {
                target = new Photo() {
                    PhotoId = photoId
                };
                photos.Add(target);
            }
            target.Caption = photo.Caption;
            target.PostedAt = photo.PostedAt;
            target.Username = photo.Username;
            Save();
            return target;
        }

        public void DeletePhoto(Guid photoId) {
            var target = LoadPhoto(photoId);
            if (target == default(Photo)) return;
            photos.Remove(target);
            Save();
        }
    }
}
