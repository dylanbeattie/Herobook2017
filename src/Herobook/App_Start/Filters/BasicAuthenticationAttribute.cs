using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace Herobook.Filters {
    // Code from https://docs.microsoft.com/en-us/aspnet/web-api/overview/security/authentication-filters
    public class WorkshopBasicAuthenticationAttribute : Attribute, IAuthenticationFilter {

        public string Password { get; set; }
        public string Realm { get; set; }

        public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken) {
            var request = context.Request;
            var authorization = request.Headers.Authorization;

            // No authentication was attempted (for this authentication method).
            // Do not set either Principal (which would indicate success) or ErrorResult (indicating an error).
            if (authorization == null) return;


            // No authentication was attempted (for this authentication method).
            // Do not set either Principal (which would indicate success) or ErrorResult (indicating an error).
            if (authorization.Scheme != "Basic") return;

            if (string.IsNullOrEmpty(authorization.Parameter)) {
                // Authentication was attempted but failed. Set ErrorResult to indicate an error.
                context.ErrorResult = new AuthenticationFailureResult("Missing credentials", request);
                return;
            }

            var credential = ExtractUserNameAndPassword(authorization.Parameter);
            if (credential == null) {
                // Authentication was attempted but failed. Set ErrorResult to indicate an error.
                context.ErrorResult = new AuthenticationFailureResult("Invalid credentials", request);
                return;
            }

            var principal = await AuthenticateAsync(credential, cancellationToken);

            if (principal == null) {
                // Authentication was attempted but failed. Set ErrorResult to indicate an error.   
                context.ErrorResult = new AuthenticationFailureResult("Invalid username or password", request);
            } else {
                // Authentication was attempted and succeeded. Set Principal to the authenticated user.
                context.Principal = principal;
            }
        }

        private Task<IPrincipal> AuthenticateAsync(NetworkCredential credential, CancellationToken cancellationToken) {
            IPrincipal principal = null;
            if (credential.Password == Password) {
                principal = new GenericPrincipal(new GenericIdentity(credential.UserName), null);
            }
            return Task.FromResult(principal);
        }

        private static NetworkCredential ExtractUserNameAndPassword(string authorizationParameter) {
            try {
                var credentialBytes = Convert.FromBase64String(authorizationParameter);
                var decodedCredentials = Encoding.ASCII.GetString(credentialBytes);
                var tokens = decodedCredentials.Split(':');
                return tokens.Length == 2 ? new NetworkCredential { UserName = tokens[0], Password = tokens[1] } : (null);
            } catch (FormatException) {
                return null;
            } catch (DecoderFallbackException) {
                return null;
            }
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken) {
            Challenge(context);
            return Task.FromResult(0);
        }

        private void Challenge(HttpAuthenticationChallengeContext context) {
            var parameter = string.IsNullOrEmpty(Realm) ? null : "realm=\"" + Realm + "\"";
            context.ChallengeWith("Basic", parameter);
        }

        public virtual bool AllowMultiple => false;
    }

}