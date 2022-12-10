using WebBeaver.Net;
using WebBeaver.Routing;

namespace WebBeaver.Example.Controller
{
	[Route("/api/v1")] // Makes all routes in this class start with '/api/v1'
	internal class ApiController
	{
		public const string API_KEY = "0ffd292e3289cc31d106bdf35d302cc7";
		public const string API_KEY_HEADER = "X-API-Key";

		/// <summary>
		/// A Simple ping endpoint
		/// </summary>
		/// <param name="req"></param>
		/// <param name="res"></param>
		[Route("/ping")]
		static void Ping(Request req, Response res)
		{
			// We use an extension method to send our json response
			res.SendJson(new { status = 200, data = "pong" });
		}

		/// <summary>
		/// A Endpoint for stopping this application.
		/// <para>
		/// Usage: 
		/// POST {http|https}://{hostname}/api/v1/server/stop
		/// </para>
		/// </summary>
		/// <param name="req"></param>
		/// <param name="res"></param>
		[Route("POST", "/server/stop")]
		[Handlers.ApiAuthHandler]
		static void ServerStop(Request req, Response res)
		{
			res.SendJson(new { status = 200, data = "Server closing" });
			Environment.Exit(0);
		}
	}
}
