using System.Reflection;
using WebBeaver.Interfaces;
using WebBeaver.Net;

namespace WebBeaver.Core
{
	/// <summary>
	/// An class that handles routing for the WebBeaver framework.
	/// </summary>
	public class Router : IRouter
	{
		#region Properties
		// Events
		/// <summary>
		/// An event that is run before routing.
		/// </summary>
		public event MiddlewareEventHandler middleware;
		/// <summary>
		/// An event where all log messages for the router are pushed.
		/// </summary>
		public event EventHandler<Collections.LogInfo> onLogMessage;

		private RouteTree _tree;
		private Func<string, object, string>? _templateEngine = null;
		#endregion

		public Router(IHttpServer server)
		{
			//Engine = new Dictionary<string, Func<string, object, string>>();
			server.onRequest += OnIncomingRequest;

			_tree = new RouteTree(this);
		}

		/// <summary>
		/// Write a log to our onLogMessage event
		/// </summary>
		/// <param name="type"></param>
		/// <param name="message"></param>
		internal void WriteLog(LogType type, string message) => onLogMessage.Invoke(this, new Collections.LogInfo(type, message));

		private void OnIncomingRequest(Request req, Response res)
		{
			// If we have middleware run it and check it's output
			//
			if (middleware != null && !middleware.Invoke(req, res))
			{
				EndRequest(req, res);
				return; // When middleware returns false we won't continue
			}

			// Try to find the requested route in our tree
			IRouteTreeNode? routeNode = _tree.Find(req);

			// Check if a node for this route was found
			//
			if (routeNode == null)
			{
				res.Status(404);
				EndRequest(req, res);
				return;
			}

			// Check if our node has a handler
			//
			if (routeNode.Handler == null)
			{
				res.Status(404);
				EndRequest(req, res);
				return;
			}

			// Set the template engine for our response
			res.templateEngine = _templateEngine;

			// Call the handler with our request and response objects
			try
			{
				routeNode.Handler.Invoke(req, res);
			}
			catch (Exception e)
			{
				WriteLog(LogType.Error, e.ToString());
			}
			EndRequest(req, res);
		}
		private void EndRequest(Request req, Response res)
		{
			LogType logType = LogType.Info;
			if (res.status >= 400)
			{
				logType = LogType.Error;
			}
			else if (res.status >= 300)
			{
				logType = LogType.Warning;
			}

			WriteLog(logType, req.Method + ' ' + req.Url + ' ' + res.status);
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
					res.Status(200).SendFile(filePath);
					return false;
				}
				return true; // Continue
			};
		}

		#region Import methods
		/// <summary>
		/// Imports a endpoint (method) and adds it to the route tree
		/// </summary>
		/// <param name="method"></param>
		/// <param name="baseRoute"></param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="Exception"></exception>
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
		/// <summary>
		/// Imports a endpoint (method) and adds it to the route tree
		/// </summary>
		public void Import(Action<Request, Response> method) => Import(method, string.Empty);

		/// <summary>
		/// Import all endpoint (static) methods from the given class.
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
