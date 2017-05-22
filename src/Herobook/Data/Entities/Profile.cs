using System;

namespace Herobook.Data.Entities {
    public class Profile {
        public Profile() { }

        public Profile(string username, string name) {
            Username = username;
            Name = name;
        }

        public string Name { get; set; }
        public string Username { get; set; }
        public DateTime? Birthday { get; set; }
    }
}
