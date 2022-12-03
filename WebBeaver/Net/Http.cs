using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using WebBeaver.Interfaces;

namespace WebBeaver.Net
{
	public class Http : IHttpServer
	{
		public int Port { get; }
		public static string? RootDirectory { get; set; }

		public event RequestEventHandler onRequest;

		private TcpListener _tcp;

		public Http(int port) : this(IPAddress.Any, port) { }
		public Http(IPAddress address, int port)
		{
			if (RootDirectory == null)
			{
				// When Debugger IsAttached we are running in VisualStudio
				if (Debugger.IsAttached)
				{
					// Set the Root as the project folder
					RootDirectory = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory + "../../../");
				}
				// Else as a exe/dll
				else
				{
					// Set the Root as the dll folder
					RootDirectory = AppDomain.CurrentDomain.BaseDirectory;
				}

			}
			Port = port;
			_tcp = new TcpListener(address, port);
		}

		public void Start()
		{
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

					// Get the request from a NetworkStream
					using (NetworkStream stream = tcpClient.GetStream())
					{
						// Get the request
						Request request = GetRequest(stream);

						// Check if we realy got a request
						//
						if (request == null)
						{
							return;
						}

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

			return Request.ParseHttp(Encoding.UTF8.GetString(memoryStream.ToArray()));
		}
	}
}
