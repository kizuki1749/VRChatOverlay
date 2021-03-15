using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using VRChatApi.Classes;
using VRChatApi.Logging;

namespace VRChatApi.Endpoints
{
    public class NotificationApi
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        public async Task<NotificationResponse> SendInvite(string userId, string location, string message = "Test")
        {
            Logger.Debug(() => $"Sending friend request to {userId}");

            JObject json = new JObject()
            {
                { "type", "invite" },
                { "details", new JObject()
                {
                    { "worldId", location }
                }
                },
                { "message", message }
            };

            Logger.Debug(() => $"Prepared JSON to post: {json}");

            string jsonText = json.ToString();
            StringContent content = new StringContent(jsonText, Encoding.UTF8);

            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpResponseMessage response = await Global.HttpClient.PostAsync($"user/{userId}/notification?apiKey={Global.ApiKey}", content);

            return await Utils.ParseResponse<NotificationResponse>(response);
        }
    }
}
