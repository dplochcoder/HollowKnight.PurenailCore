using RandomizerCore.Logic;
using RandomizerCore.StringLogic;
using System.Collections.Generic;
using System.Linq;

namespace PurenailCore.RandoUtil;

public class LogicReplacer
{
    public HashSet<string> IgnoredNames = new();
    public Dictionary<string, SimpleToken> SimpleTokenReplacements = new();

    private LogicClause EditLogicClause(LogicClause lc)
    {
        LogicClauseBuilder? lcb = null;
        for (int i = 0; i < lc.Count; i++)
        {
            var token = lc[i];
            if (token is SimpleToken st && SimpleTokenReplacements.TryGetValue(st.Write(), out SimpleToken repl))
            {
                if (lcb == null)
                {
                    lcb = new();
                    for (int j = 0; j < i; j++) lcb.Append(lc[j]);
                }
                lcb.Append(repl);
            }
            else
            {
                lcb?.Append(token);
            }
        }

        return lcb != null ? new(lcb) : lc;
    }

    public void Apply(LogicManagerBuilder lmb)
    {
        List<string> keys = new(lmb.LogicLookup.Keys.Where(n => !IgnoredNames.Contains(n)));
        foreach (var key in keys)
        {
            lmb.LogicLookup[key] = EditLogicClause(lmb.LogicLookup[key]);
        }
    }
}
