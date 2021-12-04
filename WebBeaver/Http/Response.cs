using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WebBeaver
{
	public class Response
	{
		public int status = 200;
		public IDictionary<string, string> Headers { get; }
		private Stream _stream;
		private string _httpVersion;
		public Response(Stream stream, Request req)
		{
			// Create a Dictionary for headers
			Headers = new Dictionary<string, string>();
			_stream = stream;
			_httpVersion = req.HttpVersion;
		}
		/// <summary>
		/// Send data to the client
		/// </summary>
		/// <param name="memeType">Content meme type</param>
		/// <param name="content">Content data</param>
		public void Send(string memeType, string content)
		{
			// Check if our parameters exist
			if (memeType == null)
				throw new ArgumentNullException("memeType");
			if (content == null)
				throw new ArgumentNullException("content");

			// Send a response with the content we ant to send
			string headers = String.Join('\n', Headers.Select(header => header.Key + ": " + header.Value).ToArray());
			byte[] buffer = Encoding.UTF8.GetBytes($"{_httpVersion} {status} {GetStatusMessage(status)}\n{headers}Connection: keep-alive\nContent-Type: {memeType}\nContent-Length: {content.Length}\n\n{content}");
			_stream.Write(buffer, 0, buffer.Length);
		}

		/// <summary>
		/// Sends file data to the client
		/// </summary>
		/// <param name="path">File path in project/dll directory to send</param>
		public void SendFile(string path)
		{
			// Check if parameter Path exists
			if (path == null)
				throw new ArgumentNullException("path");
			if (!File.Exists(Http.RootDirectory + path))
				throw new FileNotFoundException(Http.RootDirectory + path);

			// Read data from file
			string result;
			using (StreamReader streamReader = new StreamReader(Http.RootDirectory + path, Encoding.UTF8))
			{
				result = streamReader.ReadToEnd();
			}

			// Send data to the client
			Send(Http.GetMimeType(Path.GetExtension(path)),
				result);
		}
		/// <summary>
		/// Redirect to an other url
		/// </summary>
		/// <param name="path">url to redirect to</param>
		public void Redirect(string path)
		{
			// Send a response with the location the client should redirect to
			byte[] buffer = Encoding.UTF8.GetBytes($"{_httpVersion} 302 {GetStatusMessage(302)}\nLocation: {path}");
			_stream.Write(buffer, 0, buffer.Length);
		}

		public static string GetStatusMessage(int status)
		{
			#region [Status]
			switch (status)
			{
				// informational response
				case 100:
					return "Continue";
				case 101:
					return "Switching Protocols";
				case 102:
					return "Processing";
				case 103:
					return "Early Hints";
				// success
				case 200:
					return "OK";
				case 201:
					return "Created";
				case 202:
					return "Accepted";
				// client errors
				case 400:
					return "Bad Request";
				case 401:
					return "Unauthorized";
				case 403:
					return "Forbidden";
				case 404:
					return "Not Found";
				case 405:
					return "Method Not Allowed";
				case 406:
					return "Not Acceptable";
				case 408:
					return "Request Timeout";
				case 409:
					return "Conflict";
				case 410:
					return "Gone";
				case 411:
					return "Length Required";
				case 429:
					return "Too Many Requests";
				// server errors
				case 500:
					return "Internal Server Error";
				case 501:
					return "Not Implemented";
				case 502:
					return "Bad Gateway";
				case 503:
					return "Service Unavailable";
				case 504:
					return "Gateway Timeout";
				case 505:
					return "HTTP Version Not Supported";
				default:
					return String.Empty;
			}
			#endregion
		}
	}
}
