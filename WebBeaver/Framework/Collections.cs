using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace WebBeaver.Framework
{
	public class WebCollection<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable where TKey : class
	{
		public List<TValue> Values { get; private set; }
		public List<TKey> Keys { get; private set; }

        public WebCollection()
		{
            Values = new List<TValue>();
            Keys = new List<TKey>();
		}

        public TValue this[TKey key]
		{
            get => Values[Keys.IndexOf(key)];
            set => Values[Keys.IndexOf(key)] = value;
		}

        public void Add(TKey key, TValue value)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            Keys.Add(key);
            Values.Add(value);
        }
        public void Remove(TKey key)
		{
            Values.Remove(Values[Keys.IndexOf(key)]);
            Keys.Remove(key);
		}
        public bool ContainsKey(TKey key) => Keys.Contains(key);

		IEnumerator IEnumerable.GetEnumerator()
		{
            for (int i = 0; i < Keys.Count; i++)
                yield return new KeyValuePair<TKey, TValue>(Keys[i], Values[i]);
        }

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
            for (int i = 0; i < Keys.Count; i++)
                yield return new KeyValuePair<TKey, TValue>(Keys[i], Values[i]);
        }
	}

    public class CookieArray : IEnumerable<Cookie>, IEnumerable
    {
        private Cookie[] _inner;
        public CookieArray(Cookie[] contents)
		{
            _inner = contents;
		}
        public Cookie this[string name] => _inner.FirstOrDefault(c => c.name == name);
        public bool ContainsKey(string key) => (object)_inner.FirstOrDefault(c => c.name == key) != null;

        public int Length => _inner.Length;
        IEnumerator IEnumerable.GetEnumerator() => _inner.GetEnumerator();
        public IEnumerator<Cookie> GetEnumerator()
		{
            for (int i = 0; i < Length; i++)
                yield return _inner[i];
		}
    }
}
