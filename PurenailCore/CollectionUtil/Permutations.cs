using System;
using System.Collections.Generic;
using System.Linq;

namespace PurenailCore.CollectionUtil;

public static class PermutationsExt
{
    private static void ForEachPermutation<T>(List<T> input, int startIndex, Action<List<T>> action)
    {
        if (startIndex == input.Count - 1)
        {
            action(input);
            return;
        }

        List<T> elems = [.. input.Skip(startIndex)];
        HashSet<T> used = [];
        for (int i = 0; i < elems.Count; i++)
        {
            if (!used.Add(elems[i])) continue;

            int j = startIndex;
            input[j++] = elems[i];
            for (int k = 0; k < elems.Count; k++) if (k != i) input[j++] = elems[k];

            ForEachPermutation(input, startIndex + 1, action);
        }
    }

    // Recursively enumerate all permutations of the input. The provided list is reused for efficiency.
    public static void ForEachPermutation<T>(this IEnumerable<T> self, Action<List<T>> action)
    {
        List<T> list = [.. self];
        if (list.Count == 0) return;

        ForEachPermutation(list, 0, action);
    }
}
