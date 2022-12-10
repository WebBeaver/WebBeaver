using WebBeaver.Net;
using WebBeaver.Example.Controller;
using System.Net;

namespace WebBeaver.Example.Handlers
{
	internal class ApiAuthHandler : HandlerAttribute
	{
		public override bool Run(Request req, Response res)
		{
			// Check if the required header is given
			//
			if (!req.Headers.ContainsKey(ApiController.API_KEY_HEADER))
			{
				res.Status(HttpStatusCode.NotFound).SendJson(new { error = "No apiKey given" });
				return false; // Don't run the method we are attached to
			}

			// Check if the given apiKey matches the required apiKey
			//
			if (req.Headers[ApiController.API_KEY_HEADER] != ApiController.API_KEY)
			{
				res.Status(HttpStatusCode.Unauthorized).SendJson(new { error = "Invalid apiKey given" });
				return false; // Don't run the method we are attached to
			}

			return true; // Run the method we are attached to. (aka: continue)
		}
	}
}
