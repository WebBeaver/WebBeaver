using System.Net;
using System.Text.RegularExpressions;
using WebBeaver.Collections;

namespace WebBeaver.Net
{
	/// <summary>
	/// An object that holds http request data.
	/// </summary>
	public class Request
	{
		//public WebCollection<string, object> user = new WebCollection<string, object>();
		/// <summary>
		/// The http request method. (GET, POST, PATCH, PUT, etc)
		/// </summary>
		public string Method { get; private set; }
		/// <summary>
		/// The request url.
		/// </summary>
		public string Url { get; private set; }
		/// <summary>
		/// The http version used.
		/// </summary>
		public string HttpVersion { get; private set; }
		/// <summary>
		/// The IP of the sender.
		/// </summary>
		public IPEndPoint IP { get; internal set; }
		/// <summary>
		/// Cookies found in the request.
		/// </summary>
		public CookieArray Cookies { get; private set; }
		/// <summary>
		/// A collection of Http headers.
		/// </summary>
		public WebCollection<string, string> Headers { get; private set; }
		/// <summary>
		/// A collection of url parameters. (/:id -> id=value)
		/// </summary>
		public WebCollection<string, string> Params { get; internal set; }
		/// <summary>
		/// The body of the request. If not parsed the object contains a string.
		/// </summary>
		public object body;
		/// <summary>
		/// A collection of url query parameters. (?param1=value&2=value)
		/// </summary>
		public WebCollection<string, string> Query { get; private set; }

		public Request()
		{
			Headers = new WebCollection<string, string>();
			Params = new WebCollection<string, string>();
			Query = new WebCollection<string, string>();
		}
		
		/// <summary>
		/// Parses an http request to an Request object.
		/// </summary>
		/// <param name="httpRequestRaw">The raw http request string</param>
		/// <returns></returns>
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
