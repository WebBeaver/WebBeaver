using WebBeaver.Routing;
using WebBeaver.Net;

namespace WebBeaver.Example.Controller
{
	internal class HomeController
	{
		[Route("/")]
		static void Index(Request req, Response res)
		{
			res.SendFile("view/index.html");
		}
	}
}
