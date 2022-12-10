using System.Reflection;
using WebBeaver.Net;

namespace WebBeaver.Routing
{
	/// <summary>
	/// An attribute used for setting route data for an endpoint.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
	public class RouteAttribute : Attribute
	{
		/// <summary>
		/// If the route contains parameters like :id.
		/// </summary>
		public bool HasParams { get; private set; }

		/// <summary>
		/// Http method.
		/// </summary>
		public string Method { get; }
		public string Route { get; internal set; }

		/// <summary>
		/// Method to run when the request uses this route
		/// </summary>
		public Action<Request, Response> Action { get; private set; } = null;

		/// <summary>
		/// The handlers attached to this route
		/// </summary>
		public HandlerAttribute[] Handlers { get; private set; } = null;

		/// <summary>
		/// If the route has any handlers attached
		/// </summary>
		private bool _hasHandlers;

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
		/// Invokes the action for this route
		/// </summary>
		/// <param name="req"></param>
		/// <param name="res"></param>
		internal void Invoke(Request req, Response res)
		{
			// Check if we have any handlers attached
			//
			if (_hasHandlers)
			{
				// Check if all handlers return success
				//
				foreach (HandlerAttribute handler in Handlers)
				{
					// Run the handler
					//
					if (!handler.Run(req, res))
					{
						return; // Don't run our Action
					}
				}
			}

			// Run our route action
			Action.Invoke(req, res);
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
		/// <summary>
		/// Create a new route
		/// </summary>
		/// <param name="method"></param>
		/// <param name="route"></param>
		public RouteAttribute(HttpMethod method, string route)
		{
			Method = method.ToString();
			Route = Format(route);
		}

		/// <summary>
		/// Sets the route of this attribute
		/// </summary>
		/// <param name="route"></param>
		public void Set(string route) => Route = Format(route);

		internal void SetAction(Action<Request, Response> method)
		{
			Action = method;

			// Load our handlers
			LoadHandlers();
		}

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

		/// <summary>
		/// Loads all handlers attached to our route
		/// </summary>
		private void LoadHandlers()
		{
			Handlers = Action.GetMethodInfo().GetCustomAttributes<HandlerAttribute>().ToArray();
			_hasHandlers = Handlers.Length != 0;
		}
	}
}
