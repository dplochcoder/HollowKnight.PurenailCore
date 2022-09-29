using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurenailCore.SystemUtil
{
    public static class Extensions
    {
        public static V GetOrCreateNew<K,V>(this IDictionary<K, V> self, K key) where V : new()
        {
            if (self.TryGetValue(key, out V value)) return value;

            value = new();
            self[key] = value;
            return value;
        }
    }
}
