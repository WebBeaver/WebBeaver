using System.Security.Cryptography.X509Certificates;
using WebBeaver.Routing;
using WebBeaver.Net;
using WebBeaver.Example.Controller;
using Newtonsoft.Json.Linq;

// Create an http server
Http server = new Http(80);

// Create an https server
//
/*Https server = new Https(443);
server.Certificate(
	// Add cert for https
	new X509Certificate2(Http.RootDirectory + "/localhost.pfx", "admin")
);
*/
// Create a new router
Router router = new Router(server);

// Handler the log messages the router sends
router.onLogMessage += (sender, log) =>
{
	switch (log.Type)
	{
		case WebBeaver.LogType.Fatal:
		case WebBeaver.LogType.Error:
			Console.ForegroundColor = ConsoleColor.Red;
			break;
		case WebBeaver.LogType.Warning:
			Console.ForegroundColor = ConsoleColor.Yellow;
			break;
		case WebBeaver.LogType.Info:
			Console.ForegroundColor = ConsoleColor.Blue;
			break;
		default:
			Console.ForegroundColor = ConsoleColor.Magenta;
			break;
	}
	Console.WriteLine("[{0}] [{1}]\t{2}", log.Timestamp, log.Type, log.Message);
};

// All static file requests will go to the 'public' folder
router.Static("public");

// Set the template engine handler.
// This template engine will run when response.Render is called.
//
router.SetTemplateEngine((file, args) =>
{
	// Parse the file contents using scriban
	//
	Scriban.Template template = Scriban.Template.Parse(file);

	// Return our rendered result.
	return template.Render(args);
});

// Add middleware that will parse our body
router.middleware += (Request req, Response res) =>
{
	if (req.Headers.ContainsKey("Content-Type"))
	{
		string contentType = req.Headers["Content-Type"];

		switch (contentType)
		{
			case "text/json":
				req.body = JObject.Parse(req.body.ToString() ?? "{}");
				break;
		}
	}
	return true; // Continue
};

// Import our routes
router.Import<HomeController>();
router.Import<ApiController>();

// An example on how to use regex in a route
//
[Route("/id_example/:id(^[0-9]+$)")]
void ExampleNumberID(Request req, Response res)
{
	res.Send("The given id matches regex pattern '^[0-9]+$'!");
}
router.Import(ExampleNumberID);

// Start the http server
server.Start();