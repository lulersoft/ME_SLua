using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BestHTTP.Extensions
{
    /// <summary>
    /// Used in string parsers. Its Value is optional.
    /// </summary>
    public sealed class KeyValuePair
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public KeyValuePair(string key)
        {
            this.Key = key;
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Value))
                return String.Concat(Key, '=', Value);
            else
                return Key;
        }
    }
}