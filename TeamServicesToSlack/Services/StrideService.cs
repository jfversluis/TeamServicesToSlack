using System.Net.Http;
using System.Text;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using TeamServicesToSlack.Models;

namespace TeamServicesToSlack.Services
{
	public class StrideService
	{
		private readonly TraceWriter _log;
		private readonly string _strideWebhook;

		public StrideService(TraceWriter log) : this(log, "")
		{
		}

		public StrideService(TraceWriter log, string strideWebhook)
		{
			_log = log;
			_strideWebhook = strideWebhook;
		}

		public void PostToStride(StrideMessageModel payload)
		{
			PostToStride(_strideWebhook, payload);
		}

		public async void PostToStride(string strideWebhook, StrideMessageModel payload)
		{
			var payloadJson = JsonConvert.SerializeObject(payload);
			_log.Info($"Payload JSON {payloadJson}");
			_log.Info($"Slack Webhook URL {strideWebhook}");

			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", KeyManager.GetSecret("StrideAuthToken"));

				//var payl = "{\r\n  \"version\": 1,\r\n  \"type\": \"doc\",\r\n  \"content\": [\r\n    {\r\n      \"type\": \"applicationCard\",\r\n      \"attrs\": {\r\n        \"actions\": [{\r\n          \"key\": \"unique-app-card-action-key\",\r\n          \"title\": \"Show VSTS\",\r\n          \"target\": {\r\n            \"key\": \"app-dialog\"\r\n          }\r\n        }],\r\n        \"text\": \"some text\",\r\n        \"link\": {\r\n          \"url\": \"https://atlassian.com\"\r\n        },\r\n        \"collapsible\": true,\r\n        \"title\": {\r\n          \"text\": \"Gerald broke the build\",\r\n          \"user\": {\r\n            \"icon\": {\r\n              \"url\": \"https://www.gravatar.com/avatar/c3c9338e575a73892b0f1257eb9ee997\",\r\n              \"label\": \"Donovan Kolbly\"\r\n            }\r\n          }\r\n        },\r\n        \"description\": {\r\n          \"text\": \"bla\"\r\n        },\r\n        \"context\": {\r\n          \"text\": \"Build failed\",\r\n          \"icon\": {\r\n            \"url\": \"https://www.delta-n.nl/actueel/blogs/development-blog/PublishingImages/VSTSlogobijTips.png\",\r\n            \"label\": \"VSTS\"\r\n          }\r\n        },\r\n        \"preview\": {\r\n          \"url\": \"https://vignette.wikia.nocookie.net/cardfight/images/2/2c/Sad_panda.jpg/revision/latest?cb=20140720193511\"\r\n        }\r\n      }\r\n    }\r\n  ]\r\n}";

				//client.DefaultRequestHeaders.Add("Content-Type", "application/json");
				var content = new StringContent(payloadJson, Encoding.UTF8, "application/json");
				var response = await client.PostAsync(strideWebhook, content);
				response.EnsureSuccessStatusCode();
			}
		}
	}
}