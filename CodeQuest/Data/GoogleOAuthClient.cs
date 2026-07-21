using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace CodeQuest.Data
{
    /// <summary>
    /// Small server-side OpenID Connect client for Google sign-in.
    /// Only the Google subject ID is stored in dbo.User; access tokens are not persisted.
    /// </summary>
    public sealed class GoogleOAuthClient
    {
        private const string AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
        private const string TokenEndpoint = "https://oauth2.googleapis.com/token";
        private const string UserInfoEndpoint = "https://openidconnect.googleapis.com/v1/userinfo";

        public static bool IsConfigured
        {
            get
            {
                return !string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["CodeQuestGoogleClientId"])
                    && !string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["CodeQuestGoogleClientSecret"]);
            }
        }

        public static string CreateState()
        {
            byte[] bytes = new byte[32];
            using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(bytes);
            }

            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        public static string GetRedirectUri(HttpRequest request)
        {
            string configured = ConfigurationManager.AppSettings["CodeQuestGoogleRedirectUri"];
            if (!string.IsNullOrWhiteSpace(configured))
            {
                return configured.Trim();
            }

            return request.Url.GetLeftPart(UriPartial.Authority).TrimEnd('/')
                + VirtualPathUtility.ToAbsolute("~/GoogleCallback.aspx");
        }

        public static string BuildAuthorizationUrl(string state, string redirectUri)
        {
            string clientID = ConfigurationManager.AppSettings["CodeQuestGoogleClientId"];
            return AuthorizationEndpoint
                + "?client_id=" + Uri.EscapeDataString(clientID)
                + "&redirect_uri=" + Uri.EscapeDataString(redirectUri)
                + "&response_type=code"
                + "&scope=" + Uri.EscapeDataString("openid email profile")
                + "&state=" + Uri.EscapeDataString(state)
                + "&include_granted_scopes=true"
                + "&prompt=select_account";
        }

        public GoogleProfile ExchangeCode(string code, string redirectUri)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new InvalidOperationException("Google did not return an authorization code.");
            }

            string tokenBody = "code=" + Uri.EscapeDataString(code)
                + "&client_id=" + Uri.EscapeDataString(ConfigurationManager.AppSettings["CodeQuestGoogleClientId"])
                + "&client_secret=" + Uri.EscapeDataString(ConfigurationManager.AppSettings["CodeQuestGoogleClientSecret"])
                + "&redirect_uri=" + Uri.EscapeDataString(redirectUri)
                + "&grant_type=authorization_code";

            IDictionary<string, object> token = ReadObject(SendPost(TokenEndpoint, tokenBody));
            object accessTokenValue;
            string accessToken = token != null && token.TryGetValue("access_token", out accessTokenValue)
                ? Convert.ToString(accessTokenValue)
                : null;
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new InvalidOperationException("Google did not return an access token.");
            }

            IDictionary<string, object> profile = ReadObject(SendGet(UserInfoEndpoint, accessToken));
            object subjectValue;
            object emailValue;
            object verifiedValue;
            object nameValue;
            string subject = profile != null && profile.TryGetValue("sub", out subjectValue)
                ? Convert.ToString(subjectValue)
                : null;
            string email = profile != null && profile.TryGetValue("email", out emailValue)
                ? Convert.ToString(emailValue)
                : null;
            bool emailVerified = profile != null && profile.TryGetValue("email_verified", out verifiedValue)
                && Convert.ToBoolean(verifiedValue);
            string name = profile != null && profile.TryGetValue("name", out nameValue)
                ? Convert.ToString(nameValue)
                : null;

            if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(email) || !emailVerified)
            {
                throw new InvalidOperationException("Google returned an unverified or incomplete account profile.");
            }

            return new GoogleProfile
            {
                Subject = subject,
                Email = email.Trim(),
                Name = string.IsNullOrWhiteSpace(name) ? email.Split('@')[0] : name.Trim(),
                EmailVerified = emailVerified
            };
        }

        private static string SendPost(string endpoint, string body)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(endpoint);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Accept = "application/json";
            request.Timeout = 30000;
            request.ReadWriteTimeout = 30000;

            byte[] bytes = Encoding.UTF8.GetBytes(body);
            request.ContentLength = bytes.Length;
            try
            {
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (WebException exception)
            {
                throw new InvalidOperationException("Google sign-in could not exchange the authorization code. "
                    + ReadError(exception), exception);
            }
        }

        private static string SendGet(string endpoint, string accessToken)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(endpoint);
            request.Method = "GET";
            request.Accept = "application/json";
            request.Headers[HttpRequestHeader.Authorization] = "Bearer " + accessToken;
            request.Timeout = 30000;

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (WebException exception)
            {
                throw new InvalidOperationException("Google sign-in could not load the account profile. "
                    + ReadError(exception), exception);
            }
        }

        private static IDictionary<string, object> ReadObject(string json)
        {
            return new JavaScriptSerializer().DeserializeObject(json) as IDictionary<string, object>;
        }

        private static string ReadError(WebException exception)
        {
            HttpWebResponse response = exception.Response as HttpWebResponse;
            if (response == null)
            {
                return "Check your network connection and OAuth configuration.";
            }

            try
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    IDictionary<string, object> error = ReadObject(reader.ReadToEnd());
                    object description;
                    if (error != null && error.TryGetValue("error_description", out description)
                        && !string.IsNullOrWhiteSpace(Convert.ToString(description)))
                    {
                        return Convert.ToString(description);
                    }

                    object errorValue;
                    if (error != null && error.TryGetValue("error", out errorValue))
                    {
                        return Convert.ToString(errorValue);
                    }
                }
            }
            catch (Exception)
            {
                // Keep the user-facing error safe if Google's response is not JSON.
            }

            return "Google returned HTTP " + (int)response.StatusCode + ".";
        }
    }

    public sealed class GoogleProfile
    {
        public string Subject { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public bool EmailVerified { get; set; }
    }
}
