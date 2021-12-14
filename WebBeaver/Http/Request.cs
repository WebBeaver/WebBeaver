using System;
using System.Net;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WebBeaver
{
	public class Request
	{
		public object user;
		public string Method { get; }
		public string Url { get; }
		public string HttpVersion { get; }
		public IPEndPoint IP { get; set; }
		public IDictionary<string, string> Headers { get; }
		public IDictionary<string, string> Params { get; set; }
		public IDictionary<string, string> Body { get; }
		public IDictionary<string, string> Query { get; }
		public Request(string requestData)
		{
			string[] headerAndBody = requestData.Split("\r\n\r\n");
			Query = new Dictionary<string, string>();
			Body = new Dictionary<string, string>();
			// Parse the request line
			Match requestLine = Regex.Match(
				headerAndBody[0].Substring(0, headerAndBody[0].IndexOf(Environment.NewLine)), // Get the request line
				"(.*) (.*) (.*)");
			Method = requestLine.Groups[1].Value;
			HttpVersion = requestLine.Groups[3].Value;

			// Get url query parameters
			string[] uri = requestLine.Groups[2].Value.Split('?');
			if (uri.Length > 1)
			{
				MatchCollection matches = Regex.Matches('?' + uri[1], @"(\?|\&)([^=]+)\=([^&]+)");
				for (int i = 0; i < matches.Count; i++)
					Query.Add(matches[i].Groups[2].Value, matches[i].Groups[3].Value);
			}
			Url = uri[0];

			// Get all request headers
			Headers = new Dictionary<string, string>();
			MatchCollection headers = Regex.Matches(requestData, "(.*): (.*)");
			foreach (Match match in headers)
			{
				Headers.Add(match.Groups[1].Value, match.Groups[2].Value);
			}

			// Try to get the body
			if (headerAndBody.Length == 2 && headerAndBody[1] != string.Empty)
			{
				foreach (string part in headerAndBody[1].Split('&'))
				{
					string[] keyVal = part.Split('=');
					Body.Add(keyVal[0], keyVal[1]);
				}
			}
		}
	}
}
