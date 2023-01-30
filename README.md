# WebBeaver | Web Framework
[![Version](https://img.shields.io/nuget/v/WebBeaver)](https://www.nuget.org/packages/WebBeaver)
[![Downloads](https://img.shields.io/nuget/dt/WebBeaver)](https://www.nuget.org/packages/WebBeaver)

## What is WebBeaver?
WebBeaver is a flexible web framework for c#.

The goal is to make a Web Framework where the user can control the project layout, what template engine to use, how your request body should be parsed, and more.

## Installation

You can install WebBeaver with [NuGet](https://www.nuget.org/packages/WebBeaver).


## Example
```cs
using WebBeaver.Routing;
using WebBeaver.Net;
class Program
{
	static void Main(string[] args)
	{
		// Create a webserver on port 80
		Http server = new Http(80);

		// Create a new router that uses our webserver
		Router router = new Router(server);

		// Handle the log messages the router sends
		router.onLogMessage += (sender, log) =>
		{
			Console.WriteLine("[{0}] [{1}]\t{2}", log.Timestamp, log.Type, log.Message);
		};


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


## SECURITY

For help with securing your WebBeaver website check the [security information document](https://github.com/WebBeaver/WebBeaver/wiki/Security-Information-Document).

