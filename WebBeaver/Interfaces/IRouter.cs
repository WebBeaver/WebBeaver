using System;
using System.Collections.Generic;
using WebBeaver.Net;

namespace WebBeaver.Interfaces
{
	internal interface IRouter
	{
		public event MiddlewareEventHandler middleware;
		public event RequestEventHandler onRequestError;

		public void Import(Action<Request, Response> method);
		void Import<T>() where T : class;
	}
}
