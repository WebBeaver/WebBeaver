using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace WebBeaver.Security
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
	public class RuleAttribute : Attribute
	{
		public string PropertyName { get; }
		public object Value { get; }
		public RuleAttribute(string property, object value)
		{
			PropertyName	= property;
			Value			= value;
		}
		/// <summary>
		/// Validate the req.user object for the property in this rule
		/// </summary>
		/// <param name="req"></param>
		/// <returns></returns>
		public bool Validate(Request req)
		{
			if (req.user == null)
				return false;
			PropertyInfo prop = req.user.GetType().GetProperty(PropertyName);
			// Check if we have found a property
			if (prop == null)
				return false;
			// Get the value of the property
			object value = prop.GetValue(req.user);

			// Check if we should validate the value with regex
			if (value.GetType() == typeof(string) && Value.GetType() == typeof(string))
				return Regex.IsMatch((string)value, (string)Value);
			// Else just check if the values are the same
			return (value == Value);
		}
	}
}
