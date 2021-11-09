using System;
using System.Collections.Generic;
using System.Reflection;

namespace WebBeaver.Framework
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
	public class RouteAttribute : Attribute
	{
		/// <summary>
		/// If the route contains parameters like :id
		/// </summary>
		public bool HasParams { get; private set; }
		public string Method { get; }
		public string Route { get; }
		/// <summary>
		/// Method to run when the request uses this route
		/// </summary>
		public Action<Request, Response> Action { get; set; }
		/// <summary>
		/// Create a new route with GET method
		/// </summary>
		/// <param name="route"></param>
		public RouteAttribute(string route)
		{
			Method = "GET";
			Route = route;
		}
		/// <summary>
		/// Create a new route
		/// </summary>
		/// <param name="method"></param>
		/// <param name="route"></param>
		public RouteAttribute(string method, string route)
		{
			Method = method;
			Route = route;
		}
		/// <summary>
		/// Check if the request matches this route
		/// </summary>
		/// <param name="route"></param>
		/// <returns></returns>
		public bool IsMatch(string route)
		{
			// Do simple check first
			if (Route == route) return true;

			HasParams = true;
			string[] routeParts = Route.Split('/');
			string[] inputParts = route.Split('/');

			// Check if every part of the route that isn't a variable is the same 
			for (int i = 0; i < routeParts.Length; i++)
				if (!routeParts[i].StartsWith(':') && routeParts[i] != inputParts[i])
					return false;
			return true;
		}
	}

	public class Router
	{
		public delegate bool RequestEventHandler(Request req, Response res);
		/// <summary>
		/// Add middleware to the router
		/// <para>Return: if the router should continue handling the request</para>
		/// </summary>
		public event RequestEventHandler middleware;
		private List<RouteAttribute> _routes = new List<RouteAttribute>();
		public Router(Http server)
		{
			server.onRequest += HandleRequest;
		}
		/// <summary>
		/// Import route method
		/// </summary>
		/// <param name="method">Method to import</param>
		public void Import(Action<Request, Response> method)
		{
			// Get route attribute from method
			RouteAttribute attr = method.GetMethodInfo().GetCustomAttribute<RouteAttribute>();
			// Check if we found an attribute
			if (attr == null) throw new Exception("Couldn't find 'Route' attribute on method '" + method.GetMethodInfo().Name + "'");
			attr.Action = method;
			_routes.Add(attr);
		}
		/// <summary>
		/// Import all routes in class
		/// </summary>
		/// <param name="classType">Class to get routes from</param>
		public void Import(Type classType)
		{
			// Get all methods in class
			foreach (MethodInfo method in classType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
			{
				// Get route attribute form method
				RouteAttribute attr = method.GetCustomAttribute<RouteAttribute>();
				
				// Check if we found an attribute
				if (attr != null)
					// Check if we already added the same route
					if (!_routes.Contains(attr))
					{
						// Add the method to the route action
						try
						{
							attr.Action = (Action<Request, Response>)Delegate.CreateDelegate(typeof(Action<Request, Response>), method);
						}
						catch
						{
							throw new Exception("Mathod '" + method.Name + "' is an is not Action<Request, Response>");
						}
						_routes.Add(attr);
					}
			}
		}

		private void HandleRequest(Request req, Response res)
		{
			// When middleware returns false we won't continue
			if (middleware != null && !middleware.Invoke(req, res)) return;

			/// TODO:
			/// Check if we request a file
			/// and if we request a file than search for and then send the file
			if (req.Url == "/favicon.ico") return; // Don't check routes for favicon

			// Get the route for this request
			RouteAttribute route = _routes.Find(r => r.Method.ToString().ToUpper() == req.Method.ToUpper() && r.IsMatch(req.Url));

			// Check if we found a route
			if (route != null)
			{
				// Parse the path parameters
				if (route.HasParams)
				{
					req.Params = new Dictionary<string, string>();
					// Get all parts for the paths
					string[] inpPath = route.Route.Split('/');
					string[] reqPath = req.Url.Split('/');

					// Check if the length is the same
					if (inpPath.Length != reqPath.Length) return;

					// Compare all path parts
					for (int i = 0; i < inpPath.Length; i++)
					{
						// Check if the input(route) path part is a parameter
						if (inpPath[i].StartsWith(':'))
							req.Params.Add(inpPath[i].Substring(1), reqPath[i]); // Add parameters
					}
				}
				// Invoke the route action
				route.Action.Invoke(req, res);
			}
		}
	}
}
