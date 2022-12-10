namespace WebBeaver.Interfaces
{
	/// <summary>
	/// An interface for implementing a webserver.
	/// </summary>
	public interface IHttpServer
	{
		/// <summary>
		/// Port the server should listen to.
		/// </summary>
		public int Port { get; }
		/// <summary>
		/// An event that is fired for every incoming request.
		/// </summary>
		public event RequestEventHandler onRequest;
		/// <summary>
		/// Start the http server.
		/// </summary>
		public void Start();
	}
}
