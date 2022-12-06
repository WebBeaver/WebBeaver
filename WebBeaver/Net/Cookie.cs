using System.Text;
using WebBeaver.Collections;

namespace WebBeaver
{
	/// <summary>
	/// An object that stores information about a cookie.
	/// </summary>
	public struct Cookie
	{
		/// <summary>
		/// When true prevents access to this cookie through JavaScript.
		/// </summary>
		public bool httpOnly;
		/// <summary>
		/// When true this cookie will only be transmitted over secure protocol (https).
		/// </summary>
		public bool secure;
		/// <summary>
		/// Max age in seconds the cookie has.
		/// </summary>
		public int maxAge;
		/// <summary>
		/// The date when this cookie expires.
		/// </summary>
		public DateTime expires;
		/// <summary>
		/// The domain name of the cookie server (e.g., 'example.com' or 'subdomain.example.com').
		/// </summary>
		public string domain;
		/// <summary>
		/// The path for this cookie.
		/// </summary>
		public string path;
		/// <summary>
		/// SameSite prevents the browser from sending this cookie along with cross-site requests. Possible values are lax, strict or none.
		/// </summary>
		public string sameSite;

		/// <summary>
		/// Name of this cookie.
		/// </summary>
		public string name;
		/// <summary>
		/// The value (data) of this cookie.
		/// </summary>
		public string value;

		public bool IsAssigned { get; internal set; }

		/// <summary>
		/// Converts this cookie to a cookie string.
		/// </summary>
		/// <returns>Cookie as string</returns>
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

		/// <summary>
		/// Parses the given cookie header to an array of cookies.
		/// </summary>
		/// <param name="header">Cookie header value</param>
		/// <returns>Array of cookies</returns>
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
