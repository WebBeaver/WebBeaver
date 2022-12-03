using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using WebBeaver.Collections;

namespace WebBeaver.Net
{
	public class Request
	{
		//public WebCollection<string, object> user = new WebCollection<string, object>();
		public string Method { get; private set; }
		public string Url { get; private set; }
		public string HttpVersion { get; private set; }
		public IPEndPoint IP { get; internal set; }
		public CookieArray Cookies { get; private set; }
		public WebCollection<string, string> Headers { get; private set; }
		public WebCollection<string, string> Params { get; internal set; }
		public object body;
		public WebCollection<string, string> Query { get; private set; }

		public Request()
		{
			Headers = new WebCollection<string, string>();
			Params = new WebCollection<string, string>();
			Query = new WebCollection<string, string>();
		}
		
		public static Request ParseHttp(string httpRequestRaw)
		{
			Request result = new Request();

			// Open a string reader with to read or raw request data
			//
			using (StringReader reader = new StringReader(httpRequestRaw))
			{
				// The first line is our requestLine
				string requestLine = reader.ReadLine();

				// The requestline contains the following values: {method} {url} HTTP/{version}
				// Parse the requestline and store these values in our result.
				//
				string[] requestLineValues = requestLine.Split(' ');
				result.Method		= requestLineValues[0];
				result.Url			= requestLineValues[1];
				result.HttpVersion	= requestLineValues[2];

				// Read our headers. When we find and Empty line (space) we stop.
				//
				string? nextLine = string.Empty;
				while ((nextLine = reader.ReadLine()) != string.Empty && nextLine != null)
				{
					// Get our header key and value
					string[] keyValue = nextLine.Split(": ");

					// Add our header
					//
					if (keyValue.Length > 1)
					{
						result.Headers.Add(keyValue[0], keyValue[1]);
					}
					else if (keyValue.Length == 1)
					{
						// Add an empty header
						result.Headers.Add(keyValue[0], string.Empty);
					}
				}

				// The rest of the request contains the body.
				// Read and store the body.
				//
				result.body = reader.ReadToEnd();
			}

			// Check if a query is given in the url (example: /user?name=Bob&email=bob@mail.com)
			//
			if (result.Url.Contains('?'))
			{
				// Use regex to get all keys & values from the url query.
				//
				foreach (Match match in Regex.Matches(result.Url, @"(\?|\&)([^=]+)\=([^&]+)"))
				{
					result.Query.Add(match.Groups[2].Value, match.Groups[3].Value);
				}

				// Remove the query from the url
				result.Url = result.Url.Split('?')[0];
			}

			// Check if we have a Cookie header.
			//
			if (result.Headers.ContainsKey("Cookie"))
			{
				// Parse our cookies
				result.Cookies = Cookie.Parse(result.Headers["Cookie"]);
			}
			return result;
		}
	}
}
