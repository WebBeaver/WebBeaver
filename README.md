# WebBeaver
[![Version](https://img.shields.io/nuget/v/WebBeaver)]
[![Downloads](https://img.shields.io/nuget/dt/WebBeaver)]
WebBeaver is a flexible web framework for c#.

## Installation

You can install WebBeaver with [NuGet](https://www.nuget.org/packages/WebBeaver),
or by importing the dll found in the [release](https://github.com/RickLugtigheid/Web-Beaver/releases/latest) you want to use.

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
