# WebBeaver
##### WebBeaver is a flexible web framework for c#.
[![Version](https://img.shields.io/nuget/v/WebBeaver)](https://www.nuget.org/packages/WebBeaver)
[![Downloads](https://img.shields.io/nuget/dt/WebBeaver)](https://www.nuget.org/packages/WebBeaver)

## Installation

You can install WebBeaver with [NuGet](https://www.nuget.org/packages/WebBeaver).

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
	static void Index(Request req, Response res)
	{
		res.SendFile("/view/index.html");
	}
}
```
