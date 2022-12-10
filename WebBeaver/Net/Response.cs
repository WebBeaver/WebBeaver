using MimeTypes;
using System.Net;
using System.Text;
using WebBeaver.Collections;

namespace WebBeaver.Net
{
	/// <summary>
	/// An object for creating a http response that will be send back.
	/// </summary>
	public class Response
	{
		/// <summary>
		/// The http status code.
		/// </summary>
		public int status = 200;
		/// <summary>
		/// A collection of http headers.
		/// </summary>
		public WebCollection<string, string> Headers { get; }

		private Stream _stream;
		private string _httpVersion;

		internal Func<string, object, string>? templateEngine;

		public Response(Stream stream, Request req)
		{
			// Create a Dictionary for headers
			Headers = new WebCollection<string, string>();
			_stream = stream;
			_httpVersion = req.HttpVersion;

			// Add default headers
			Headers.Add("Access-Control-Allow-Origin", "*");
			Headers.Add("Connection", "Keep-Alive");
		}

		/// <summary>
		/// Renders a file with the set template engine.
		/// <para>You can set a template engine using the Router.SetTemplateEngine method.</para>
		/// </summary>
		/// <param name="path">Path to the file to render</param>
		/// <param name="args">Arguments to give to the file</param>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="FileNotFoundException"></exception>
		/// <exception cref="NullReferenceException"></exception>
		public void Render(string path, object? args = null)
		{
			// Check if we recived a path argument
			//
			if (path == null)
			{
				throw new ArgumentException("path");
			}

			// Check if the requested file exists
			//
			if (!File.Exists(Http.RootDirectory + path))
			{
				throw new FileNotFoundException(Http.RootDirectory + path);
			}

			// Check if a template engine is defined
			//
			if (templateEngine == null)
			{
				throw new NullReferenceException("No template engine set");
			}

			// Read data from our file
			//
			string fileData;
			using (StreamReader streamReader = new StreamReader(Http.RootDirectory + path, Encoding.UTF8))
			{
				fileData = streamReader.ReadToEnd();
			}

			// Invoke our template engine and respond to our request with the given html
			//
			Send(templateEngine.Invoke(fileData, args ?? new { }));
		}

		/// <summary>
		/// Sends string as html to the client.
		/// </summary>
		public void Send(string rawHtml) => Send("text/html", rawHtml);

		/// <summary>
		/// Send data to the client.
		/// </summary>
		/// <param name="memeType">Content meme type</param>
		/// <param name="content">Content data</param>
		public void Send(string memeType, string content)
		{
			// Check if our parameters exist
			if (memeType == null)
			{
				throw new ArgumentNullException("memeType");
			}
			if (content == null)
			{
				throw new ArgumentNullException("content");
			}

			// Send a response with the content we want to send
			string headers = string.Join('\n', Headers.Select(header => header.Key + ": " + header.Value).ToArray());

			//  Build our http request
			//
			StringBuilder req = new StringBuilder();
			req.AppendLine($"{_httpVersion} {status} {GetStatusMessage(status)}"); // Add our request line

			// Check if the user added headers to the response
			//
			if (headers != string.Empty)
			{
				// Add our user given headers and our content
				req.AppendLine(headers);
			}

			req.Append($"Content-Type: {memeType}\nContent-Length: {content.Length}\n\n{content}"); // Add our default headers

			// Write our request to the client
			WriteResponse(req.ToString());
		}

		/// <summary>
		/// Sends file data to the client.
		/// </summary>
		/// <param name="path">File path starting with Http.RootDirectory (project/dll directory) to send.</param>
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

			string mime = MimeTypeMap.GetMimeType(Path.GetExtension(path).Substring(1));
			//string mime = "text/html";

			// Send data to the client
			Send(mime, result);
		}

		/// <summary>
		/// Send a status message.
		/// </summary>
		public void SendStatus(int status)
		{
			this.status = status;
			Send("text/html", GetStatusMessage(status));
		}

		/// <summary>
		/// Sets the http status code for our response.
		/// </summary>
		/// <param name="status"></param>
		/// <returns></returns>
		public Response Status(int status)
		{
			this.status = status;
			return this;
		}
		/// <summary>
		/// Sets the http status code for our response.
		/// </summary>
		/// <param name="status"></param>
		/// <returns></returns>
		public Response Status(HttpStatusCode status)
		{
			this.status = (int)status;
			return this;
		}

		/// <summary>
		/// Redirect to an other url.
		/// </summary>
		/// <param name="path">url to redirect to</param>
		public void Redirect(string path)
		{
			// Check if our parameters exist
			if (path == null)
			{
				throw new ArgumentNullException("memeType");
			}

			// Send a response with the location the client should redirect to
			//
			WriteResponse($"{_httpVersion} 302 {GetStatusMessage(302)}\nLocation: {path}");
		}

		/// <summary>
		/// Adds a set cookie header with the given cookie data.
		/// </summary>
		/// <param name="cookie">Cookie to set</param>
		public void SetCookie(Cookie cookie) => Headers.Add("Set-Cookie", cookie.ToString());

		/// <summary>
		// Writes the http response to the client
		/// </summary>
		/// <param name="httpResponse"></param>
		protected void WriteResponse(string httpResponse)
		{
			// Check if we already responded to the client
			//
			/*if (_stream.CanWrite)
			{
				throw new EndOfStreamException("Can not send more than one responses");
			}*/

			// Create a buffer from our http string
			byte[] buffer = Encoding.UTF8.GetBytes(httpResponse);

			// Write the buffer to the client
			try
			{
				_stream.Write(buffer, 0, buffer.Length);

				// Close the stream after we have send our response
				_stream.Close();
			}
			catch
			{

			}
		}

		/// <summary>
		/// Gets the string message for a http status code.
		/// </summary>
		/// <param name="status"></param>
		/// <returns></returns>
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