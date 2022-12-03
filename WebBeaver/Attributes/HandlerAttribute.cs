using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBeaver.Net;

namespace WebBeaver
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
	public abstract class HandlerAttribute : Attribute
	{
		abstract public bool Run(Request req, Response res);
	}
}
