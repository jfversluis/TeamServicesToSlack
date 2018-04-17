using System;
using System.Net.Http;
using System.Net.Http.Headers;
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
		private readonly string _strideApiKey;

		public StrideService(TraceWriter log) : this(log, "", "")
		{
		}

		public StrideService(TraceWriter log, string strideWebhook, string strideApiKey)
		{
			_log = log;
			_strideWebhook = strideWebhook;
			_strideApiKey = strideApiKey;
		}

		public void PostToStride(StrideMessageModel payload)
		{
			PostToStride(_strideWebhook, _strideApiKey, payload);
		}

		public async void PostToStride(string strideWebhook, string strideApiKey, StrideMessageModel payload)
		{
			try
			{
				var payloadJson = JsonConvert.SerializeObject(payload);
				_log.Info($"Payload JSON {payloadJson}");
				_log.Info($"Slack Webhook URL {strideWebhook}");

				using (var client = new HttpClient())
				{
					client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", strideApiKey);

					var content = new StringContent(payloadJson, Encoding.UTF8, "application/json");
					var response = await client.PostAsync(strideWebhook, content);
					response.EnsureSuccessStatusCode();
				}
			}
			catch (Exception ex)
			{
				_log.Error(ex.Message, ex);
			}
		}
	}
}