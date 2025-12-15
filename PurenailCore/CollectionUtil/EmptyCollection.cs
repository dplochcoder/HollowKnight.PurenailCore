using System.Collections.Generic;

namespace PurenailCore.CollectionUtil;

internal static class EmptyCollection<T>
{
    public static IReadOnlyList<T> Instance = [];
}
