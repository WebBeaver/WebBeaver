using System;
using System.Collections.Generic;
using WebBeaver;
using WebBeaver.Framework;

namespace WebBeaverExample
{
	class Program
	{
		static void Main(string[] args)
		{
			// Create a http server
			Http server = new Http(80);

			// Create a router
			Router router = new Router(server);

			// Import routes
			router.Import(Home);					// Import a route from method
			router.Import(typeof(ApiController));   // Import all routes from class

			// Adding middleware
			router.middleware += (req, res) =>
			{
				Console.WriteLine("{0} {1}", req.Method, req.Url);
				return true; // Let the router continue handling the request
			};

			server.Start(); // Start our http server
		}
		[Route("/")]
		static void Home(Request req, Response res)
		{
			res.Send("text/html", "<b>Hello world</b><br><form action='/user' method='POST'>Name: <input type='text' name='name'/><input type='submit' name='submit' value='Add user' /></form>");
		}
	}
	class ApiController
	{
		public static Dictionary<int, string> users = new Dictionary<int, string>()
		{
			{ 0, "Admin" },
			{ 1, "User2" }
		};
		[Route("/user/:id")]
		static void GetUser(Request req, Response res)
		{
			int id = int.Parse(req.Params["id"]);
			if (id > users.Count)
			{
				res.status = 404;
				res.Send("text/json", "{ \"error\": \"No user found with id '" + id + "'\" }");
			}
			else res.Send("text/json", "{ \"user\": { \"id\": " + id + ", \"name\": \"" + users[id] + "\" } }");
		}
		[Route("POST", "/user")]
		static void AddUser(Request req, Response res)
		{
			users.Add(users.Count, req.Body["name"]);
			Console.WriteLine("Added user: " + req.Body["name"]);
			res.Send("text/json", "{ \"success\": true }");
		}
	}
}
