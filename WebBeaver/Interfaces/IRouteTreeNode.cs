
namespace WebBeaver.Interfaces
{
	internal interface IRouteTreeNode
	{
		public string Name { get; }
		public RouteAttribute? Handler { get; set; }
		public List<IRouteTreeNode> Children { get; }
	}
}
