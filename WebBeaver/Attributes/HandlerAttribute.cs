using System;
using WebBeaver.Net;

namespace WebBeaver
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
	public abstract class HandlerAttribute : Attribute
	{
		/// <summary>
		/// Runs before running the endpoint that this handler is attached to.
		/// </summary>
		/// <param name="req"></param>
		/// <param name="res"></param>
		/// <returns>Continue</returns>
		abstract public bool Run(Request req, Response res);
	}
}
