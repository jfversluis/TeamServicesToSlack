using Newtonsoft.Json;

namespace TeamServicesToSlack.Models
{
	public class StrideMessageModel
	{
		[JsonProperty("version")]
		public long Version { get; set; }

		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("content")]
		public Content[] Content { get; set; }
	}

	public class Content
	{
		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("attrs")]
		public Attrs Attrs { get; set; }
	}

	public class Attrs
	{
		[JsonProperty("actions", NullValueHandling = NullValueHandling.Ignore)]
		public Action[] Actions { get; set; }

		[JsonProperty("text")]
		public string Text { get; set; }

		[JsonProperty("link")]
		public Link Link { get; set; }

		[JsonProperty("collapsible")]
		public bool Collapsible { get; set; }

		[JsonProperty("title")]
		public Title Title { get; set; }

		[JsonProperty("description")]
		public Description Description { get; set; }

		[JsonProperty("context")]
		public Context Context { get; set; }

		[JsonProperty("preview")]
		public Link Preview { get; set; }
	}

	public class Action
	{
		[JsonProperty("key")]
		public string Key { get; set; }

		[JsonProperty("title")]
		public string Title { get; set; }

		[JsonProperty("target")]
		public Target Target { get; set; }
	}

	public class Target
	{
		[JsonProperty("key")]
		public string Key { get; set; }
	}

	public class Context
	{
		[JsonProperty("text")]
		public string Text { get; set; }

		[JsonProperty("icon")]
		public Icon Icon { get; set; }
	}

	public class Icon
	{
		[JsonProperty("url")]
		public string Url { get; set; }

		[JsonProperty("label")]
		public string Label { get; set; }
	}

	public class Description
	{
		[JsonProperty("text")]
		public string Text { get; set; }
	}

	public class Link
	{
		[JsonProperty("url")]
		public string Url { get; set; }
	}

	public class Title
	{
		[JsonProperty("text")]
		public string Text { get; set; }

		[JsonProperty("user")]
		public User User { get; set; }
	}

	public class User
	{
		[JsonProperty("icon")]
		public Icon Icon { get; set; }
	}
}