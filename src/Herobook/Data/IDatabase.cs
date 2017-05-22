using System;
using System.Collections.Generic;
using Herobook.Data.Entities;

namespace Herobook.Data {
    public interface IDatabase {
        IEnumerable<Profile> ListProfiles();
        int CountProfiles();
        void CreateProfile(Profile profile);
        Profile FindProfile(string username);
        IEnumerable<Profile> LoadFriends(string username);
        void CreateFriendship(string username1, string username2);
        void DeleteProfile(string username);
        Profile UpdateProfile(string username, Profile profile);

        IEnumerable<Status> LoadStatuses(string username);
        Status LoadStatus(Guid statusId);
        Status UpdateStatus(Guid statusId, Status status);
        void DeleteStatus(Guid statusId);
        Status CreateStatus(Status status);

        IEnumerable<Photo> LoadPhotos(string username);
        Photo CreatePhoto(Photo photo);
        Photo LoadPhoto(Guid photoGuid);
        Photo UpdatePhoto(Guid photoId, Photo photo);
        void DeletePhoto(Guid photoId);
    }
}
