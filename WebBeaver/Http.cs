using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace WebBeaver.Http
{
	public class Http
	{
		public int Port { get; }
		public delegate void RequestEventHandler(Request req, Response res);
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

					// Check if we realy got a request
					if (request == null) continue;

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
					return null;
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
		public IDictionary<string, string> Query { get; }
		public Request(string requestData)
		{
			Query = new Dictionary<string, string>();
			// Parse the request line
			Match requestLine = Regex.Match(
				requestData.Substring(0, requestData.IndexOf(Environment.NewLine)), // Get the request line
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
		}
	}
	public class Response
	{
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
			byte[] buffer = Encoding.UTF8.GetBytes($"{_httpVersion} 200 OK\n{String.Join('\n', Headers.Select(header => header.Key + ": " + header.Value).ToArray())}\nConnection: keep-alive\nContent-Type: {memeType}\nContent-Length: {content.Length}\n\n{content}");
			_stream.Write(buffer, 0, buffer.Length);
		}
	}
}