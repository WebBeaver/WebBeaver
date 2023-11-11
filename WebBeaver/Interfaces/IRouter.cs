using System;
using WebBeaver.Collections;
using WebBeaver.Net;

namespace WebBeaver.Interfaces
{
	/// <summary>
	/// An interface for implementing a router.
	/// </summary>
	internal interface IRouter
	{
		public event MiddlewareEventHandler middleware;
		public event EventHandler<LogInfo> onLogMessage;

		public void Import(Action<Request, Response> method);
		void Import<T>() where T : class;
	}
}
