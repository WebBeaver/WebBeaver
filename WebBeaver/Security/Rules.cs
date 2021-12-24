using System;
using System.Text.RegularExpressions;

namespace WebBeaver.Security
{
	public enum Target { User, Header, Body, Param }
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
	public class RuleAttribute : Attribute
	{
		public string PropertyName { get; }
		public object Value { get; }
		public Target Target { get; }
		public string redirect = null;
		public int status = 403;
		public RuleAttribute(string property, object value, Target target = Target.User)
		{
			PropertyName	= property;
			Value			= value;
			Target			= target;
		}
		/// <summary>
		/// Validate the req.user object for the property in this rule
		/// </summary>
		/// <param name="req"></param>
		/// <returns></returns>
		public bool Validate(Request req)
		{
			switch (Target)
			{
				case Target.Header:
					if (req.Headers == null)
						return false;
					if (!req.Headers.ContainsKey(PropertyName))
						return false;
					// Get the value of the property
					string header = req.Headers[PropertyName];
					if (Value.GetType() != typeof(string))
						throw new InvalidCastException($"Target '{Target}' value must be a string, '{Value.GetType().Name}' given");
					return Regex.IsMatch(header, (string)Value);

				case Target.Body:
					if (req.Body == null)
						return false;

					// Check if we have a property with the requested name
					if (!req.Body.ContainsKey(PropertyName))
						return false;

					// Get the value of the property
					string body = req.Body[PropertyName];
					if (Value.GetType() != typeof(string))
						throw new InvalidCastException($"Target '{Target}' value must be a string, '{Value.GetType().Name}' given");
					return Regex.IsMatch(body, (string)Value);

				case Target.Param:
					if (req.Params == null)
						return false;

					// Check if we have a property with the requested name
					if (!req.Params.ContainsKey(PropertyName))
						return false;

					// Get the value of the property
					string param = req.Params[PropertyName];
					if (Value.GetType() != typeof(string))
						throw new InvalidCastException($"Target '{Target}' value must be a string, '{Value.GetType().Name}' given");
					return Regex.IsMatch(param, (string)Value);

				default:
					if (req.user == null)
						return false;

					// Check if we have a property with the requested name
					if (!req.user.ContainsKey(PropertyName))
						return false;

					// Get the value of the property
					object value = req.user[PropertyName];

					// Check if we should validate the value with regex
					if (value.GetType() == typeof(string) && Value.GetType() == typeof(string))
						return Regex.IsMatch((string)value, (string)Value);
					// Else just check if the values are the same
					return (value == Value);
			}
		}
	}
}
