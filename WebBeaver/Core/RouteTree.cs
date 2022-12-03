using WebBeaver.Interfaces;

namespace WebBeaver.Core
{
/**
[Example]:

GET /
GET  /users
	 /^[0-1]$
POST /users
		/register

[Speical nodes]:
:id			- a route variable
^[a-Z]*$	- a regex match

*/

	internal class RouteTree
	{
		public StaticNode Root { get; }
		public RouteTree()
		{
			Root = new StaticNode("");
		}

		public void Insert(RouteAttribute route)
		{
			// Get the node for our request method
			IRouteTreeNode? methodNode = Root.Children.Find(node => node.Name == route.Method.ToUpper());

			// Check if our root has a node for the route request method
			//
			if (methodNode == null)
			{
				// Create a new method node
				methodNode = new StaticNode(route.Method);
				Root.Children.Add(methodNode);
			}

			// Then insert our record
			//
			Console.WriteLine("InsertRecord({0}, {1})", route.Route, methodNode.Name);
			//InsertRecord(route, route.Route, methodNode);

			// Check if we got an index route
			//
			if (route.Route == "/")
			{
				StaticNode indexNode = new StaticNode("/");
				indexNode.Handler	 = route;
				methodNode.Children.Add(indexNode);
				return;
			}

			IRouteTreeNode currentNode = methodNode;

			string[] routeParts = route.Route.Split('/', StringSplitOptions.RemoveEmptyEntries);
			for (int n = 0; n < routeParts.Length; n++)
			{
				string routePart = routeParts[n];

				IRouteTreeNode newNode = CreateNode(routePart);
				IRouteTreeNode? duplicateNode = currentNode.Children.Find(node => node.Name == routePart);

				// Check if this node is not already added
				//
				if (duplicateNode == null)
				{
					currentNode.Children.Add(newNode);
					currentNode = newNode;
				}
				else
				{
					// Nest into the next node
					currentNode = duplicateNode;
				}

				// Check if this node is the leaf node
				//
				if (n == (routeParts.Length - 1))
				{
					// Add the given route attribute as handler to the leaf node
					newNode.Handler = route;
				}
			}
		}

		internal IRouteTreeNode? Find(Net.Request request)
		{
			// Get the node for our request method
			IRouteTreeNode? methodNode = Root.Children.Find(node => node.Name == request.Method.ToUpper());

			// Check if our root has a node for the route request method
			//
			if (methodNode == null)
			{
				throw new Exception("Return for Method not found not implemented!");
			}

			// Check if index is requested
			//
			if (request.Url == "/")
			{
				return methodNode.Children.Find(node => node.Name == "/");
			}

			IRouteTreeNode result = methodNode;
			foreach (string routePart in request.Url.Split("/", StringSplitOptions.RemoveEmptyEntries))
			{
				bool success = false;
				// Check each childNode of result.
				//
				foreach (IRouteTreeNode childNode in result.Children)
				{
					// Check if our child node is a static node
					// with the same name as the routepart.
					//
					if (childNode is StaticNode && childNode.Name == routePart)
					{
						result = childNode;
						success = true;
					}
					// Check if our child node is a parameter node.
					//
					else if (childNode is ParameterNode && ((ParameterNode)childNode).IsMatch(routePart))
					{
						result = childNode;
						request.Params.Add(childNode.Name, routePart);

						success = true;
					}
				}

				// Check if we were able to find a node that matches our route part in some way
				//
				if (!success)
				{
					// We could not find the route the user is looking for
					return null;
				}
			}
			return result;
		}

		private IRouteTreeNode CreateNode(string routePart)
		{
			if (routePart == null)
			{
				throw new ArgumentNullException("routePart");
			}

			// Check if the node is a parameter node (like :name)
			//
			if (routePart.StartsWith(':'))
			{
				return new ParameterNode(routePart);
			}

			// Any other node will be a static node
			return new StaticNode(routePart);
		}
	}
}