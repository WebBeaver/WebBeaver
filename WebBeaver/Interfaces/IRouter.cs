using WebBeaver.Collections;
using WebBeaver.Net;

namespace WebBeaver.Interfaces
{
	internal interface IRouter
	{
		public event MiddlewareEventHandler middleware;
		public event EventHandler<LogInfo> onLogMessage;

		public void Import(Action<Request, Response> method);
		void Import<T>() where T : class;
	}
}
