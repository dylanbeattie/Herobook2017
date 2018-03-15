using System;
using System.Linq;
using Newtonsoft.Json;

namespace Herobook.Data.Entities
{
	public class Profile
	{
		public Profile() { }

		public Profile(string username, string name)
		{
			Username = username;
			Name = name;
		}

		[JsonIgnore]
		public Name NewName {
			get {
				var tokens = this.Name.Split(' ');

				return new Entities.Name
				{
					FirstName = tokens.Length > 1 ? tokens.First() : null,
					LastName = tokens.LastOrDefault()
				};
			}
		}

		public string Name { get; set; }
		public string Username { get; set; }
		public DateTime? Birthday { get; set; }


	}

	public class Name
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
	}
}
