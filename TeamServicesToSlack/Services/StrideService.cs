﻿using System.Net.Http;
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

				var content = new StringContent(payloadJson, Encoding.UTF8, "application/json");
				var response = await client.PostAsync(strideWebhook, content);
				response.EnsureSuccessStatusCode();
			}
		}
	}
}