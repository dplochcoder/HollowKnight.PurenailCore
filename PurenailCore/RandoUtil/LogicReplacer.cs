using RandomizerCore.Logic;
using RandomizerCore.StringLogic;
using RandomizerCore.StringParsing;
using System.Collections.Generic;
using System.Linq;

namespace PurenailCore.RandoUtil;

public class LogicReplacer
{
    public HashSet<string> IgnoredNames = [];
    public Dictionary<string, Token> TokenReplacements = [];

    private Expression<LogicExpressionType>? Transform(Expression<LogicExpressionType> expr, ExpressionBuilder<LogicExpressionType> builder)
    {
        if (expr is LogicAtomExpression atom && TokenReplacements.TryGetValue(atom.Token.Print(), out Token repl))
            return new LogicAtomExpression(repl);
        else
            return null;
    }

    private LogicClause EditLogicClause(LogicClause lc) => new(lc.Expr.Transform(Transform, new LogicExpressionBuilder()));

    public void Apply(LogicManagerBuilder lmb)
    {
        List<string> keys = [.. lmb.LogicLookup.Keys.Where(n => !IgnoredNames.Contains(n))];
        foreach (var key in keys)
        {
            lmb.LogicLookup[key] = EditLogicClause(lmb.LogicLookup[key]);
        }
    }
}
