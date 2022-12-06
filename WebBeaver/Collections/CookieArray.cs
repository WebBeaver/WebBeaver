using System.Collections;

namespace WebBeaver.Collections
{
	/// <summary>
	/// An array of cookie objects.
	/// </summary>
	public class CookieArray : IEnumerable<Cookie>, IEnumerable
	{
		private Cookie[] _inner;
		/// <summary>
		/// Create an array of cookies.
		/// </summary>
		/// <param name="contents"></param>
		public CookieArray(Cookie[] contents)
		{
			_inner = contents;
		}
		public Cookie this[string name] => _inner.FirstOrDefault(c => c.name == name);
		/// <summary>
		/// Check if the array has a cookie with name key.
		/// </summary>
		/// <param name="key">Name of the cookie to check for</param>
		/// <returns></returns>
		public bool ContainsKey(string key) => (object)_inner.FirstOrDefault(c => c.name == key) != null;

		/// <summary>
		/// The ammount of cookies in the array.
		/// </summary>
		public int Length => _inner.Length;
		IEnumerator IEnumerable.GetEnumerator() => _inner.GetEnumerator();
		public IEnumerator<Cookie> GetEnumerator()
		{
			for (int i = 0; i < Length; i++)
				yield return _inner[i];
		}
	}
}
