using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Herobook.Data;
using Herobook.Data.Entities;

using Herobook.Hypermedia;
using Newtonsoft.Json;

namespace Herobook.Controllers.Api
{

	public class ProfilesController : ApiController
	{
		private readonly IDatabase db;

		public ProfilesController()
		{
			db = new DemoDatabase();
		}

		[Route("api/profiles/")]
		[HttpGet]
		public object GetProfiles(int index = 0, int count = 10)
		{
			var _links = Hal.Paginate(Request.RequestUri.AbsolutePath, index, count, db.CountProfiles());
			var items = db.ListProfiles().Skip(index).Take(count);
			var result = new
			{
				_links,
				items
			};
			return result;
		}


		[Route("api/profiles/{username}")]
		[HttpDelete]
		public object DeleteProfile(string username)
		{
			var record = db.FindProfile(username);

			db.DeleteProfile(username);
			return Request.CreateResponse(HttpStatusCode.OK);
		}

		[Route("api/profiles/{username}")]
		[HttpGet]
		public object GetProfile(string username, string expand = "")
		{
			var expansions = expand.Split(',');
			var resource = db.FindProfile(username);
			if (resource == default(Profile)) return NotFound();
			if (resource.IsDeleted) return Request.CreateResponse(HttpStatusCode.Gone);
			var result = resource.ToDynamic();
			result._links = new
			{
				self = Hal.Href($"/api/profiles/{username}"),
				friends = Hal.Href($"/api/profiles/{username}/friends")
			};
			dynamic embedded = new ExpandoObject();
			if (expansions.Contains("friends"))
			{
				embedded.friends = GetProfileFriends(username);
			}

			if (expansions.Contains("photos"))
			{
				var photoController = new PhotosController();

				embedded.photos = photoController.GetProfilePhotos(username);

			}
			result._embedded = embedded;

			dynamic actions = new ExpandoObject();
			actions.delete = new
			{
				name = "Delete this profile",
				href = $"/api/profiles/{username}",
				method = "DELETE"
			};
			actions.update = new
			{
				name = "Update this profile",
				href = $"/api/profiles/{username}",
				method = "PATCH",
				type = "application/json"
			};
			result._actions = actions;
			return result;
		}

		[Route("api/profiles/{username}/friends")]
		[HttpGet]
		public object GetProfileFriends(string username)
		{
			return db.LoadFriends(username);
		}


		[Route("api/profiles/")]
		[HttpPost]
		public object Post([FromBody] Profile profile)
		{
			var existing = db.FindProfile(profile.Username);
			if (existing != null) return Request.CreateResponse(HttpStatusCode.Conflict, "That username is not available");
			db.CreateProfile(profile);
			return Created(Url.Content($"~/api/profiles/{profile.Username}"), profile);
		}

		[Route("api/profiles/{username}")]
		[HttpPut]
		public object Put(string username, [FromBody] Profile profile)
		{
			var result = db.UpdateProfile(username, profile);
			return result;
		}

		[Route("api/profiles/{username}")]
		[HttpPatch]
		public object Patch(string username, [FromBody]Dictionary<string, object> changes)
		{

			var record = db.FindProfile(username);
			ApplyPatch(record, changes);
			var response = Request.CreateResponse(HttpStatusCode.OK);
			response.ReasonPhrase = "It worked! Yaaaay!";
			return (response);
		}

		//public void ApplyPatch(Profile profile, IDictionary<string, object> changes)
		//{
		//	var props = TypeDescriptor.GetProperties(profile.GetType());
		//	foreach (PropertyDescriptor prop in props)
		//	{
		//		var change = changes.FirstOrDefault(c => c.Key.Equals(prop.Name, StringComparison.InvariantCultureIgnoreCase));
		//		if (change.Equals(default(KeyValuePair<string, object>))) continue;

		//		var jsonValue = change.Value.ToString();
		//		var t = prop.PropertyType;
		//		try
		//		{
		//			prop.SetValue(profile, JsonConvert.DeserializeObject(jsonValue, t));
		//		}
		//		catch (Exception ex)
		//		{
		//			var foo = ex.ToString();
		//		}
		//	}
		//}
	}

}

