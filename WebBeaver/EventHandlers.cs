using WebBeaver.Net;

namespace WebBeaver
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="req"></param>
	/// <param name="res"></param>
	/// <returns></returns>
	public delegate bool MiddlewareEventHandler(Request req, Response res);

	/// <summary>
	/// 
	/// </summary>
	/// <param name="req"></param>
	/// <param name="res"></param>
	public delegate void RequestEventHandler(Request req, Response res);
}
