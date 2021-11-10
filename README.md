# WebBeaver
WebBeaver is a web framework for c#.

## Installation
To install and use WebBeaver you will need to install the dll file from the [latest release](https://github.com/RickLugtigheid/Web-Beaver/releases/latest).
After that you will need to import the dll file into your project:
- Open your project and right click on Dependencies
- Click add Project Reference
- Click on browse and navigate to the dll file and double click it

## Example
```cs
using WebBeaver;
using WebBeaver.Framework;
class Program
{
	static void Main(string[] args)
	{
		// Create a webserver on port 80
		Http server = new Http(80);

		// Create a new router that uses our webserver
		Router router = new Router(server);

		// Import routes
		router.Import(Home);
		
		// Start the webserver
		server.Start();
	}
	[Route("/")]
	static void Home(Request req, Response res)
	{
		res.Send("text/html", "<b>My homepage</b>");
	}
}
```
