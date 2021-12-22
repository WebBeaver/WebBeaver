using System;
using System.Collections.Generic;
using System.Text;

namespace WebBeaver
{
	public struct Cookie
	{
		public bool httpOnly;
		public bool secure;
		public int maxAge;
		public DateTime expires;
		public string domain;
		public string path;
		public string sameSite;

		public string name;
		public string value;

		public override string ToString()
		{
			StringBuilder result = new StringBuilder();
			result.Append($"{name}={value}");
			if (expires != null)
				result.Append("; Expires=" + expires);
			if (maxAge > 1)
				result.Append("; Max-Age=" + maxAge);
			if (domain != null)
				result.Append("; Domain=" + domain);
			if (path != null)
				result.Append("; Path=" + path);
			if (secure)
				result.Append("; Secure");
			if (httpOnly)
				result.Append("; HttpOnly");
			if (sameSite != null)
				result.Append("; SameSite=" + sameSite);

			return result.ToString();
		}
	}
}
