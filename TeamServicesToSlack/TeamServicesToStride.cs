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
		private static string[] _successImages = new[]
		{
			"https://hips.hearstapps.com/cosmouk.cdnds.net/15/31/1438173668-cute-success-kid.jpg",
			"https://www.mememaker.net/api/bucket?path=static/img/memes/full/2015/May/1/23/cheers-to-your-success.jpg",
			"https://memegenerator.net/img/instances/43124381.jpg",
			"https://i.pinimg.com/originals/89/6b/ea/896beade2bfe8861cb41f645bb189ab5.jpg",
			"https://memegenerator.net/img/instances/74853181.jpg"
		};

		private static string[] _failedImages = new[]
		{
			"https://vignette.wikia.nocookie.net/cardfight/images/2/2c/Sad_panda.jpg/revision/latest?cb=20140720193511",
			"https://i.ytimg.com/vi/lMCpdJ0Q-pk/maxresdefault.jpg",
			"https://memegenerator.net/img/instances/47036108.jpg",
			"https://i.imgflip.com/117mlj.jpg"
		};

		private static Random _randomizer = new Random();

		[FunctionName("TeamServicesToStride")]
		public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
		{
			log.Info("C# HTTP trigger function processed a request.");

			string channelUrl = req.Headers?.FirstOrDefault(h => h.Key.ToLowerInvariant() == "channelurl").Value?.ToString();
			string channelKey = req.Headers?.FirstOrDefault(h => h.Key.ToLowerInvariant() == "channelkey").Value?.ToString();

			if (string.IsNullOrWhiteSpace(channelUrl) || string.IsNullOrWhiteSpace(channelKey))
			{
				channelUrl = KeyManager.GetSecret("StrideWebhookUrl");
				channelKey = KeyManager.GetSecret("StrideAuthToken");

				if (string.IsNullOrWhiteSpace(channelUrl) || string.IsNullOrWhiteSpace(channelKey))
					return new BadRequestResult();
			}

			string requestBody = new StreamReader(await req.Content.ReadAsStreamAsync()).ReadToEnd();
			var request = JsonConvert.DeserializeObject<VstsMessageModel>(requestBody);

			if (request.EventType.ToLowerInvariant() != "build.complete")
				return new OkObjectResult("");
			
			var buildSucceeded = request.Resource.Status.ToLowerInvariant() == "succeeded";

			var triggeredByUser = request.Resource.Requests.FirstOrDefault() == null ? "Someone"
				: request.Resource.Requests.FirstOrDefault()?.RequestedFor?.DisplayName;

			var triggeredByUserImage = "https://avatarfiles.alphacoders.com/643/thumb-64385.png";

			var buildDuration = Math.Round(request.Resource.FinishTime.Subtract(request.Resource.StartTime).TotalMinutes, 1);

			var strideService = new StrideService(log, channelUrl, channelKey);
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
							Details = new[]
							{
								new Detail
								{
									Lozenge = new Lozenge
									{
										Appearance = "default",
										Text = $"{buildDuration} minute(s)"
									}
								}
							},
								Text = request.DetailedMessage.Text,
								Link = new Link
								{
									Url = $"https://dotcontrol.visualstudio.com/HaviConnect%20Native%20App/_build/index?buildId={request.Resource.Requests.FirstOrDefault()?.Id}&_a=summary"
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
									Url = buildSucceeded ? _successImages[_randomizer.Next(_successImages.Length)]
										: _failedImages[_randomizer.Next(_failedImages.Length)]
								},
								Context = new Context
								{
									Icon = new Icon
									{
										Url = "https://a3bf67a2345da5d6ee8b-6d37b1ee447a16ff81e1420be19ec8c3.ssl.cf5.rackcdn.com/vsts/vsts.png",
										Label = "VSTS"
									},
									Text = $"{request.Resource.Definition.Name} {request.Resource.Status}"
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