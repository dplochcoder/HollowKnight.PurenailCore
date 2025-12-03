using System;
using System.Collections.Generic;
using System.Linq;

namespace PurenailCore.CollectionUtil;

public class EquatableHashSet<T> : HashSet<T>, IEquatable<EquatableHashSet<T>>
{
    public bool Equals(EquatableHashSet<T> other) => Count == other.Count && this.All(other.Contains);
}
