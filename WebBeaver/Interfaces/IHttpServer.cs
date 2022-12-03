using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBeaver.Interfaces
{
	public interface IHttpServer
	{
		public int Port { get; }
		public event RequestEventHandler onRequest;
		public void Start();
	}
}
