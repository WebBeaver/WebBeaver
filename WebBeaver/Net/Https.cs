using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using WebBeaver.Interfaces;

namespace WebBeaver.Net
{
	public class Https : IHttpServer
	{
		public int Port { get; }
		public X509Certificate2 Cert { get; private set; }

		public static string? RootDirectory { get; set; }

		public event RequestEventHandler onRequest;

		private TcpListener _tcp;

		public Https(int port) : this(IPAddress.Any, port) { }
		public Https(IPAddress address, int port)
		{
			if (RootDirectory == null)
			{
				// When Debugger IsAttached we are running in VisualStudio
				if (Debugger.IsAttached)
				{
					// Set the Root as the project folder
					Http.RootDirectory = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory + "../../../");
				}
				// Else as a exe/dll
				else
				{
					// Set the Root as the dll folder
					Http.RootDirectory = AppDomain.CurrentDomain.BaseDirectory;
				}

			}
			Port = port;
			_tcp = new TcpListener(address, port);
		}

		/// <summary>
		/// Sets the Certificate
		/// </summary>
		/// <param name="cert"></param>
		public void Certificate(X509Certificate2 cert) => Cert = cert;

		public void Start()
		{
			if (Cert == null)
			{
				throw new ArgumentNullException("No certificate given");
			}
			_tcp.Start();
			while (true)
			{
				// Wait for a request
				TcpClient client = _tcp.AcceptTcpClient();

				// Run the request in a Thread
				//
				Thread thread = new Thread(new ParameterizedThreadStart(HandleRequest));
				thread.Start(client);

				// Sleep 1ms
				Thread.Sleep(1);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="client"></param>
		private void HandleRequest(object? client)
		{
			try
			{
				if (client is TcpClient)
				{
					TcpClient tcpClient = (TcpClient)client;

					// Get the request from a SslStream
					using (SslStream stream = new SslStream(tcpClient.GetStream(), false, ValidateCertificate))
					{
						// Authenticate the server certificate
						stream.AuthenticateAsServer(Cert, false, SslProtocols.Tls12, false);

						// Get the request
						Request request = GetRequest(stream);

						// Check if we realy got a request
						if (request == null) return;

						request.IP = tcpClient.Client.RemoteEndPoint as IPEndPoint;

						onRequest.Invoke(request, new Response(stream, request));
					}
				}
			}
			catch
			{
				// Connection stopped while handling request
			}
		}
		private static bool ValidateCertificate(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;

		private Request GetRequest(SslStream stream)
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
			} while (size == 0);

			return Request.ParseHttp(Encoding.UTF8.GetString(memoryStream.ToArray()));
		}
	}
}
