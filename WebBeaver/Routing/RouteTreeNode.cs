﻿using System.Collections.Generic;
using System.Text.RegularExpressions;
using WebBeaver.Interfaces;

namespace WebBeaver.Routing
{
	internal class StaticNode : IRouteTreeNode
	{
		public string Name { get; }
		public RouteAttribute? Handler { get; set; }
		public List<IRouteTreeNode> Children { get; }

		public StaticNode(string name)
		{
			Name	 = name;
			Children = new List<IRouteTreeNode>();
		}
	}
	internal class ParameterNode : IRouteTreeNode
	{
		public string Name { get; }
		public RouteAttribute? Handler { get; set; }
		public List<IRouteTreeNode> Children { get; }

		public string pattern = ".*";

		public ParameterNode(string name)
		{
			if (name.IndexOf('(') != -1)
			{
				int regexStart = name.IndexOf('(');
				int regexEnd   = name.IndexOf(')') + 1;

				pattern = name.Substring(regexStart, regexEnd - regexStart);
				Name = name.Substring(1, regexStart - 1);
			}
			else
			{
				Name = name.Remove(0, 1);
			}
			Children = new List<IRouteTreeNode>();
			Handler  = null;
		}

		public bool IsMatch(string value) => Regex.IsMatch(value, pattern);
	}
}
