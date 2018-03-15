using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
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
			var items = db.ListProfiles().Skip(index).Take(count).Select(profile => profile.ToResource());
			var _actions = new
			{
				create = new
				{
					name = "Create a new profile",
					href = Request.RequestUri.AbsolutePath,
					method = "POST",
					type = "application/json",
					schema = new { href = "/schemas/profile.json" }
				}
			};
			var result = new
			{
				_links,
				_actions,
				items
			};
			return result;
		}

		[Route("api/profiles/{username}")]
		[HttpGet]
		public object GetProfile(string username)
		{
			var profile = db.FindProfile(username);
			if (profile == default(Profile)) return (NotFound());
			if (profile?.LastModified < Request.Headers.IfModifiedSince)
			{
				return Request.CreateResponse(HttpStatusCode.NotModified);
			}

			var profileEtag = CalculateEtag(profile);
			foreach (var etag in Request.Headers.IfNoneMatch)
			{
				if (etag.Tag.Equals(profileEtag)) return Request.CreateResponse(HttpStatusCode.NotModified);

			}

			var response = Request.CreateResponse(HttpStatusCode.OK, (object)profile.ToResource());

			response.Headers.ETag = new EntityTagHeaderValue(profileEtag);
			return response;
		}

		private string Quote(string s)
		{
			return $"\"{s}\"";
		}

		private string CalculateEtag(Profile profile)
		{
			var json = JsonConvert.SerializeObject(profile);
			var md5 = MD5.Create();
			var inputBytes = Encoding.ASCII.GetBytes(json);
			var hashBytes = md5.ComputeHash(inputBytes);
			var etag = String.Join("", hashBytes.Select(b => b.ToString("X2")));
			return Quote(etag);
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
			return Created(Url.Content($"~/api/profiles/{profile.Username}"), profile.ToResource());
		}

		[Route("api/profiles/{username}")]
		[HttpPut]
		public object Put(string username, [FromBody] Profile profile)
		{
			var result = db.UpdateProfile(username, profile);
			return result.ToResource();
		}


		[Route("api/profiles/{username}")]
		[HttpDelete]
		public object Delete(string username)
		{
			db.DeleteProfile(username);
			return Request.CreateResponse(HttpStatusCode.OK);
		}
	}
}
