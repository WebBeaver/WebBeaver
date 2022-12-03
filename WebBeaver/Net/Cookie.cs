using System.Text;
using WebBeaver.Collections;

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

		public bool IsAssigned { get; internal set; }

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

		public static CookieArray Parse(string header)
		{
			List<Cookie> cookies = new List<Cookie>();
			string[] baseCookies = header.Split(';');
			for (int i = 0; i < baseCookies.Length; i++)
			{
				string[] keyVal = baseCookies[i].Split('=');
				cookies.Add(new Cookie() { name = keyVal[0], value = keyVal[1] });
			}
			return new CookieArray(cookies.ToArray());
		}
	}
}
