using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using TeamServicesToSlack.Models;
using TeamServicesToSlack.Services;
using TeamServicesToStride.Models;

namespace TeamServicesToSlack
{
	public static partial class TeamServicesToStride
	{
		[FunctionName("TeamServicesToStride")]
		public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
		{
			log.Info("C# HTTP trigger function processed a request.");
			bool simple = !(req.Headers.FirstOrDefault(h => h.Key.ToLowerInvariant() == "format").Value?.ToString().ToLowerInvariant() == "extended");

			string requestBody = new StreamReader(await req.Content.ReadAsStreamAsync()).ReadToEnd();
			//var request = JsonConvert.DeserializeObject<WebhookRequest>(requestBody);
			var request = JsonConvert.DeserializeObject<VstsMessageModel>(requestBody);
			//var message = data?.message?.text;
			//var detailedMessage = request.DetailedMessage.text;
			// important - get this data
			//var resourceLink = request.Resource.url;
			//var changesData = await GetVstsResource($"{resourceLink}/changes");
			//var changes = JsonConvert.DeserializeObject<Changes>(changesData);
			//var changesCount = changes.count;
			//var changesText = "";
			//foreach (var change in changes.value)
			//{
			//    changesText +=
			//        $"<{change.displayUri}|{change.message}> by *{change.Author.displayName}*\n";
			//    if (!simple)
			//        changesText += $"{change.message}\n\n";
			//}
			//var resource = await GetVstsResource(resourceLink);
			//var resourceData = JsonConvert.DeserializeObject<ResourceDetails>(resource);
			//var projectName = resourceData.Project.Name;
			//var buildLink = resourceData._Links.Web.Href;
			//var sourceBranch = resourceData.SourceBranch;
			//var sourceVersion = resourceData.SourceVersion;
			//var repository = resourceData.Repository.Id;
			//var definitionName = request.Resource.definition.name;
			//var gitUrl = $"{repository.Replace(".git", "")}/commit/{sourceVersion}";

			//var timeline = await GetVstsResource(resourceData._Links.Timeline.Href);
			//var timeLineData = JsonConvert.DeserializeObject<Timeline>(timeline);
			//var failingTask = timeLineData.Records.FirstOrDefault(x => x.Result == "failed");
			if (request.EventType.ToLowerInvariant() != "build.complete")
				return new OkObjectResult("");
			
			var buildSucceeded = request.Resource.Status.ToLowerInvariant() == "succeeded";

			var triggeredByUser = request.Resource.Requests.FirstOrDefault() == null ? "Someone"
				: request.Resource.Requests.FirstOrDefault()?.RequestedFor?.DisplayName;

			var triggeredByUserImage = "https://avatarfiles.alphacoders.com/643/thumb-64385.png";

			var strideService = new StrideService(log, KeyManager.GetSecret("StrideWebhookUrl"));
			var model = new StrideMessageModel
			{
				Version = 1,
				Type = "doc",
				Content = new[] {
						new Content
						{
							Type = "applicationCard",
							Attrs = new Attrs
							{
								//Actions = new[]
								//{
								//	new Models.Action
								//	{
								//		Key = "unique-app-card-action-key",
								//		Target = new Target
								//		{
								//			Key = "app-dialog"
								//		},
								//		Title = "Show VSTS"
								//	}
								//},
								Text = request.DetailedMessage.Text,
								Link = new Link
								{
									Url = request.Resource.Url
								},
								Collapsible = true,
								Title = new Title
								{
									Text = buildSucceeded
										? $"{triggeredByUser} successfully triggered a build, hooray!"
										: $"{triggeredByUser} broke the build!",
									User = new User
									{
										Icon = new Icon
										{
											Url = triggeredByUserImage,
											Label = triggeredByUser
										}
									}
								},
								Description = new Description
								{
									Text = request.DetailedMessage.Text
								},
								Preview = new Link
								{
								Url = buildSucceeded ? "https://hips.hearstapps.com/cosmouk.cdnds.net/15/31/1438173668-cute-success-kid.jpg"
									: "https://vignette.wikia.nocookie.net/cardfight/images/2/2c/Sad_panda.jpg/revision/latest?cb=20140720193511"
								},
								Context = new Context
								{
									Icon = new Icon
									{
										Url = "https://a3bf67a2345da5d6ee8b-6d37b1ee447a16ff81e1420be19ec8c3.ssl.cf5.rackcdn.com/vsts/vsts.png",
										Label = "VSTS"
									},
									Text = request.Message.Text
								}
							}
						}
				}
			};

			strideService.PostToStride(model);
			return new OkObjectResult("");
		}

		public static async Task<string> GetVstsResource(string url)
		{
			try
			{
				var personalaccesstoken = KeyManager.GetSecret("VSTSPersonalAccessToken");

				using (HttpClient client = new HttpClient())
				{
					client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

					client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
						Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", "", personalaccesstoken))));

					using (HttpResponseMessage response = client.GetAsync(
						url).Result)
					{
						response.EnsureSuccessStatusCode();
						string responseBody = await response.Content.ReadAsStringAsync();
						return responseBody;
					}
				}
			}
			catch (Exception ex)
			{
				return ex.ToString();
			}
		}
	}
}