using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace WebBeaver
{
    public class Https : IHttpServer
    {
        public int Port { get; }
        public X509Certificate2 Cert { get; private set; }
        public event RequestEventHandler onRequest;

        private TcpListener _tcp;

        public Https(int port) : this(IPAddress.Any, port) { }
        public Https(IPAddress address, int port)
        {
            if (Http.RootDirectory == null)
            {
                // When Debugger IsAttached we are running in VisualStudio
                if (Debugger.IsAttached)
                    Http.RootDirectory = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory + "../../../"); // Project folder
                // Else as a exe/dll
                else Http.RootDirectory = AppDomain.CurrentDomain.BaseDirectory; // dll folder
            }

            Port = port;
            _tcp = new TcpListener(address, port);
        }
        public void Certificate(X509Certificate2 cert) => Cert = cert;

        /// <summary>
        /// Start the secure webserver
        /// </summary>
        public void Start()
        {
            if (Cert == null)
                throw new ArgumentNullException("No certificate given");
            _tcp.Start();
            while (true)
            {
                // Wait for a request
                TcpClient client = _tcp.AcceptTcpClient();

                // Run the request in a task
                new Task(() =>
                {
                    // Get the request from a SslStream
                    using (SslStream stream = new SslStream(client.GetStream(), false, ValidateCertificate))
                    {
                        // Authenticate the server certificate
                        stream.AuthenticateAsServer(Cert, false, SslProtocols.Tls12, false);

                        // Get the request
                        Request request = GetRequest(stream);

                        // Check if we realy got a request
                        if (request == null) return;

                        request.IP = client.Client.RemoteEndPoint as IPEndPoint;

                        onRequest.Invoke(request, new Response(stream, request));
                    }
                }).Start();
            }
        }

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
            return new Request(Encoding.UTF8.GetString(memoryStream.ToArray()));
        }

        private static bool ValidateCertificate(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;
    }
}
