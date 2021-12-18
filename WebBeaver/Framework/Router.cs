using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using WebBeaver.Security;
using System.Linq;

namespace WebBeaver.Framework
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
	public class RouteAttribute : Attribute
	{
		/// <summary>
		/// A list of rules connected to this route
		/// </summary>
		public List<RuleAttribute> Rules { get; private set; }
		/// <summary>
		/// If the route contains parameters like :id
		/// </summary>
		public bool HasParams { get; private set; }
		/// <summary>
		/// Http method
		/// </summary>
		public string Method { get; }
		public string Route { get; private set; }
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
			Route = Format(route);
		}
		/// <summary>
		/// Create a new route
		/// </summary>
		/// <param name="method"></param>
		/// <param name="route"></param>
		public RouteAttribute(string method, string route)
		{
			Method = method;
			Route = Format(route);
		}
		public void GetRules()
		{
			Rules = Action.GetMethodInfo().GetCustomAttributes<RuleAttribute>().ToList();
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

			if (routeParts.Length != inputParts.Length) return false;

			// Check if every part of the route that isn't a variable is the same 
			for (int i = 0; i < routeParts.Length; i++)
				if (!routeParts[i].StartsWith(':') && routeParts[i] != inputParts[i])
					return false;
			return true;
		}

		public bool CheckAccess(Request req, out RuleAttribute blocked)
		{
			blocked = null;
			if (Rules == null) 
				return true;
			foreach (RuleAttribute rule in Rules)
			{
				if (!rule.Validate(req))
				{
					blocked = rule;
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Sets the route of this attribute
		/// </summary>
		/// <param name="route"></param>
		public void Set(string route) => Route = Format(route);

		/// <summary>
		/// Formats the route
		/// <para>So you can both use /user/ and /user for the same result</para>
		/// </summary>
		/// <param name="route"></param>
		/// <returns>Formatted route</returns>
		public static string Format(string route)
		{
			if (route != "/" && route.EndsWith('/'))
				return route.Substring(0, route.Length - 1);
			return route;
		}
	}

	public class Router
	{
		public static Router Instance { get; private set; }
		public delegate bool MiddlewareEventHandler(Request req, Response res);
		/// <summary>
		/// The folder in your project/dll folder where your static files are stored
		/// </summary>
		private string _staticFolder = String.Empty;
		private string _logFolder = String.Empty;
		private List<RouteAttribute> _routes = new List<RouteAttribute>();

		/// <summary>
		/// Add middleware to the router
		/// <para>Return: if the router should continue handling the request</para>
		/// </summary>
		public event MiddlewareEventHandler middleware;
		public event RequestEventHandler onRequestError;
		public IDictionary<string, Func<string, object, string>> Engine { get; }
		public Router(IHttpServer server)
		{
			Instance = this;
			Engine = new Dictionary<string, Func<string, object, string>>();
			server.onRequest += HandleRequest;
		}

		/// <summary>
		/// Set the static folder position within your project/dll folder
		/// </summary>
		/// <param name="path">Path to static folder</param>
		public void Static(string path) => _staticFolder = path;
		/// <summary>
		/// Activate logging and set the folder where you want the logs to go
		/// <para>Will log when there is an exception while handling a request</para>
		/// </summary>
		/// <param name="path">Path to log folder</param>
		public void Log(string path) => _logFolder = path;
		/// <summary>
		/// Import route method
		/// </summary>
		/// <param name="method">Method to import</param>
		public void Import(Action<Request, Response> method)
		{
			if (method == null)
				throw new ArgumentNullException("method");

			// Get route attribute from method
			RouteAttribute attr = method.GetMethodInfo().GetCustomAttribute<RouteAttribute>();
			// Check if we found an attribute
			if (attr == null) throw new Exception("Couldn't find 'Route' attribute on method '" + method.GetMethodInfo().Name + "'");
			attr.Action = method;
			attr.GetRules();
			_routes.Add(attr);
		}
		/// <summary>
		/// Import all routes in class
		/// </summary>
		/// <param name="classType">Class to get routes from</param>
		[Obsolete("please use Import<Class>() instead")]
		public void Import(Type classType)
		{
			if (classType == null)
				throw new ArgumentNullException("classType");

			// Check if our class has a Route attribute
			// When we add a route attribute to a class we should use it as the base route for methods in class
			string baseRoute = classType.GetCustomAttribute<RouteAttribute>()?.Route;
			if (baseRoute == null) baseRoute = String.Empty;

			// Get all methods in class
			foreach (MethodInfo method in classType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
			{
				// Get route attribute form method
				RouteAttribute attr = method.GetCustomAttribute<RouteAttribute>();

				// Set the route to Route + base
				attr.Set(baseRoute + attr.Route);

				// Check if we found an attribute
				if (attr != null)
					// Check if we already added the same route
					if (!_routes.Contains(attr))
					{
						// Add the method to the route action
						try
						{
							attr.Action = (Action<Request, Response>)Delegate.CreateDelegate(typeof(Action<Request, Response>), method);
							attr.GetRules();
						}
						catch
						{
							throw new TargetException("Method '" + method.Name + "' is an is not Action<Request, Response>");
						}
						_routes.Add(attr);
					}
			}
		}
		/// <summary>
		/// Import all routes in class
		/// </summary>
		/// <typeparam name="T">Class to get routes from</typeparam>
		public void Import<T>() where T : class => Import(typeof(T));

		private void HandleRequest(Request req, Response res)
		{
			try
			{
				// When middleware returns false we won't continue
				if (middleware != null && !middleware.Invoke(req, res)) return;

				// Check if we are requesting a file
				if (Path.HasExtension(req.Url))
				{
					string exten = Path.GetExtension(req.Url);

					// Check if the file exists
					if (!File.Exists(Http.RootDirectory + _staticFolder + req.Url)) return;

					// Make sure we can't request protected files
					if (exten == ".cs") return;

					// Send the file
					res.SendFile(_staticFolder + req.Url);
					return;
				}

				// Get the formatted url
				string url = RouteAttribute.Format(req.Url);

				// Get the route for this request
				RouteAttribute route = _routes.Find(r => r.Method.ToString().ToUpper() == req.Method.ToUpper() && r.IsMatch(url));

				// Check if we found a route
				if (route != null)
				{
					// Parse the path parameters
					if (route.HasParams)
					{
						req.Params = new Dictionary<string, string>();
						// Get all parts for the paths
						string[] inpPath = route.Route.Split('/');
						string[] reqPath = url.Split('/');

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
					// Check if we have access to this route
					if (route.CheckAccess(req, out RuleAttribute rule))
						// Invoke the route action
						route.Action.Invoke(req, res);
					else
					{
						// Set our request status
						res.status = rule.status;

						// Check if we have a route to redirect to
						if (rule.redirect != null)
							res.Redirect(rule.redirect);
						else res.Send("text/html", Response.GetStatusMessage(rule.status));
					}
				}
			}
			catch (Exception e)
			{
				// Write the error to the console
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine(e.ToString() + '\n');
				Console.ResetColor();

				// Invoke onRequestError so we may redirect the request to forexample a 500 error page
				onRequestError?.Invoke(req, res);

				// Check if we should log the error
				if (_logFolder != String.Empty)
				{
					// Write our exception to log
					using (StreamWriter sw = new StreamWriter(_logFolder + "/error.log", true))
					{
						sw.WriteLine($"[{DateTime.Now}]:");
						sw.WriteLine(e.ToString() + '\n');
					}
				}
			}
		}
	}
}
