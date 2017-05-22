using System;

namespace Herobook.Data.Entities {
    public class Status {
        public Guid StatusId { get; set; }
        public string Username { get; set; }
        public string Comment { get; set; }
        public DateTimeOffset PostedAt { get; set; }
    }
}
