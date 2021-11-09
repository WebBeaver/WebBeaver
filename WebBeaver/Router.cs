using System;
using System.Collections.Generic;
using System.Text;

namespace WebBeaver
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
	public class RouteAttribute : Attribute
	{
		public static bool HasParams { get; private set; }
		public string Method { get; }
		public string Route { get; }
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
			Route = Route;
		}

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

	}
}
