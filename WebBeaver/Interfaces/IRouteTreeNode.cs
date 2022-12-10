
namespace WebBeaver.Interfaces
{
	internal interface IRouteTreeNode
	{
		public string Name { get; }
		public Routing.RouteAttribute? Handler { get; set; }
		public List<IRouteTreeNode> Children { get; }
	}
}
