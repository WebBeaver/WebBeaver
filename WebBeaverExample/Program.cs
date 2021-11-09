using System;
using WebBeaver.Http;

namespace WebBeaverExample
{
	class Program
	{
		static void Main(string[] args)
		{
			Http server = new Http(80);
			server.onRequest += (req, res) =>
			{
				res.Send("text/html", "<b>Hello World</b>");
				Console.WriteLine($"{req.Method} {req.Url}");
			};
			server.Start();
		}
	}
}
