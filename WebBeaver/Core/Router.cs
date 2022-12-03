using System.Reflection;
using WebBeaver.Interfaces;
using WebBeaver.Net;

namespace WebBeaver.Core
{
	public class Router : IRouter
	{
		#region Properties
		// Events
		public event MiddlewareEventHandler middleware;
		public event RequestEventHandler onRequestError;

		private RouteTree _tree = new RouteTree();
		private Func<string, object, string>? _templateEngine = null;
		#endregion

		public Router(IHttpServer server)
		{
			//Engine = new Dictionary<string, Func<string, object, string>>();
			server.onRequest += OnIncomingRequest;
		}
		
		private void OnIncomingRequest(Request req, Response res)
		{
			Console.WriteLine("Incomming request: " + req.Method + ' ' + req.Url);

			// If we have middleware run it and check it's output
			//
			if (middleware != null && !middleware.Invoke(req, res))
			{
				return; // When middleware returns false we won't continue
			}

			// Try to find the requested route in our tree
			IRouteTreeNode? routeNode = _tree.Find(req);

			// Check if a node for this route was found
			//
			if (routeNode == null)
			{
				// TODO: Log an warning or error
				res.Status(404);
				return;
			}

			// Check if our node has a handler
			//
			if (routeNode.Handler == null)
			{
				res.Status(404);
				return;
			}

			// Set the template engine for our response
			res.templateEngine = _templateEngine;

			// Call the handler with our request and response objects
			routeNode.Handler.Invoke(req, res);
		}

		/// <summary>
		/// Sets the template engine that should be used with the response.Render method
		/// </summary>
		/// <param name="engine"></param>
		public void SetTemplateEngine(Func<string, object, string> engine) => _templateEngine = engine;
		/// <summary>
		/// Adds middleware to handle calls to static files
		/// </summary>
		/// <param name="path"></param>
		/// <exception cref="NotImplementedException"></exception>
		public void Static(string path)
		{
			middleware += (req, res) =>
			{
				// Check if the url has an file extension
				//
				if (Path.HasExtension(req.Url))
				{
					string filePath = path + req.Url;
					// Check if our file exists
					//
					if (!File.Exists(Http.RootDirectory + filePath))
					{
						res.SendStatus(404);
						return false;
					}

					// Send the contents for the requested file
					res.SendFile(filePath);
					return false;
				}
				return true; // Continue
			};
		}

		#region Import methods
		private void Import(Action<Request, Response> method, string baseRoute)
		{
			// Validate arguments
			//
			if (method == null)
			{
				throw new ArgumentNullException("method");
			}

			// Get the Route attribute from the method
			RouteAttribute? attribute = method.GetMethodInfo().GetCustomAttribute<RouteAttribute>();

			// Check if we found an attribute
			//
			if (attribute == null)
			{
				throw new Exception("Couldn't find a Route attribute on method '" + method.GetMethodInfo().Name + "'");
			}

			// Add the base url to the attribute route
			attribute.Route = baseRoute + attribute.Route;

			// Set the action for the RouteAttribute
			attribute.SetAction(method);

			// Add the attribute to our routeTree
			_tree.Insert(attribute);
		}
		public void Import(Action<Request, Response> method) => Import(method, string.Empty);

		/// <summary>
		/// Import all routes from the given class
		/// </summary>
		/// <typeparam name="T">Class to get routes from</typeparam>
		public void Import<T>() where T : class
		{
			Type classType = typeof(T);

			// Check if our class has a Route attribute
			// When we add a route attribute to a class we should use it as the base route for methods in class
			string? baseRoute = classType.GetCustomAttribute<RouteAttribute>()?.Route;

			// Get all methods from the given class
			//
			foreach (MethodInfo method in classType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
			{
				// Get the route handler
				Action<Request, Response> handler = (Action<Request, Response>)Delegate.CreateDelegate(typeof(Action<Request, Response>), method);
				
				// Import the route
				Import(handler, baseRoute ?? string.Empty);
			}
		}
		#endregion
	}
}
