using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace WebBeaver
{
	public class Http
	{
		public delegate void RequestEventHandler(Request req, Response res);
		public int Port { get; }
		public event RequestEventHandler onRequest;

		private TcpListener _tcp;

		public Http(int port)
		{
			Port = port;
			_tcp = new TcpListener(port);
		}

		public void Start()
		{
			_tcp.Start();
			while (true)
			{
				// Wait for a request
				TcpClient client = _tcp.AcceptTcpClient();

				// Get the request from a NetworkStream
				using (NetworkStream stream = client.GetStream())
				{
					// Get the request
					Request request = GetRequest(stream);

					onRequest.Invoke(request, new Response(stream, request));
				}
			}
		}

		private Request GetRequest(NetworkStream stream)
		{
			MemoryStream memoryStream = new MemoryStream();
			byte[] data = new byte[256];
			int size;
			do
			{
				size = stream.Read(data, 0, data.Length);
				if (size == 0)
					return null; // The client has disconected
				memoryStream.Write(data, 0, size);
			} while (stream.DataAvailable);
			return new Request(Encoding.UTF8.GetString(memoryStream.ToArray()));
		}
	}

	public class Request
	{
		public string Method { get; }
		public string Url { get; }
		public string HttpVersion { get; }
		public IDictionary<string, string> Headers { get; }
		public IDictionary<string, string> Params { get; set; }
		public IDictionary<string, string> Body { get; }
		public IDictionary<string, string> Query { get; }
		public Request(string requestData)
		{
			string[] headerAndBody = requestData.Split("\r\n\r\n");
			Query = new Dictionary<string, string>();
			Body  = new Dictionary<string, string>();
			// Parse the request line
			Match requestLine = Regex.Match(
				headerAndBody[0].Substring(0, headerAndBody[0].IndexOf(Environment.NewLine)), // Get the request line
				"(.*) (.*) (.*)");
			Method		= requestLine.Groups[1].Value;
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
	public class Response
	{
		public int status = 200;
		public IDictionary<string, string> Headers { get; }
		private NetworkStream _stream;
		private string _httpVersion;
		public Response(NetworkStream stream, Request req)
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
			string headers = String.Join('\n', Headers.Select(header => header.Key + ": " + header.Value).ToArray());
			byte[] buffer = Encoding.UTF8.GetBytes($"{_httpVersion} {status} {GetStatusMessage(status)}\n{headers}Connection: keep-alive\nContent-Type: {memeType}\nContent-Length: {content.Length}\n\n{content}");
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