using Newtonsoft.Json;
using WebBeaver.Net;

namespace WebBeaver.Example
{
	public static class Extensions
	{

		// Add a custom response function for sending JSON responses
		/// <summary>
		/// Sends a json object as response
		/// </summary>
		/// <param name="self"></param>
		/// <param name="data"></param>
		public static void SendJson(this Response self, object data)
		{
			self.Send("text/json", JsonConvert.SerializeObject(data));
		}
	}
}
