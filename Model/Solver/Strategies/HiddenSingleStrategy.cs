using System.Collections.Generic;
using Global;
using Model.Solver.Explanation;
using Model.Solver.Helpers.Changes;
using Model.Solver.Position;

namespace Model.Solver.Strategies;

public class HiddenSingleStrategy : AbstractStrategy
{
    public const string OfficialName = "Hidden Single";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.WaitForAll;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public HiddenSingleStrategy() : base(OfficialName, StrategyDifficulty.Basic, DefaultBehavior){}
    
    public override void Apply(IStrategyManager strategyManager)
    {
        for (int number = 1; number <= 9; number++)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    var u = i * 3 + j;
                    
                    var rp = strategyManager.RowPositionsAt(u, number);
                    if (rp.Count == 1)
                    {
                        strategyManager.ChangeBuffer.ProposeSolutionAddition(number, u, rp.First());
                        strategyManager.ChangeBuffer.Commit(this, new HiddenSingleReportBuilder(Unit.Row));
                        if (OnCommitBehavior == OnCommitBehavior.Return) return;
                    }
                    
                    var cp = strategyManager.ColumnPositionsAt(u, number);
                    if (cp.Count == 1)
                    {
                        strategyManager.ChangeBuffer.ProposeSolutionAddition(number, cp.First(), u);
                        strategyManager.ChangeBuffer.Commit(this, new HiddenSingleReportBuilder(Unit.Column));
                        if (OnCommitBehavior == OnCommitBehavior.Return) return;
                    }
                    
                    var mp = strategyManager.MiniGridPositionsAt(i, j, number);
                    if (mp.Count != 1) continue;
                    
                    var pos = mp.First();
                    strategyManager.ChangeBuffer.ProposeSolutionAddition(number, pos.Row, pos.Column);
                    strategyManager.ChangeBuffer.Commit(this, new HiddenSingleReportBuilder(Unit.MiniGrid));
                    if (OnCommitBehavior == OnCommitBehavior.Return) return;
                }
            }
        }
    }
}

public class HiddenSingleReportBuilder : IChangeReportBuilder
{
    private readonly Unit _unit;

    public HiddenSingleReportBuilder(Unit unit)
    {
        _unit = unit;
    }

    public ChangeReport Build(IReadOnlyList<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), Description(changes),
            lighter => IChangeReportBuilder.HighlightChanges(lighter, changes), Explanation(changes));
    }

    private static string Description(IReadOnlyList<SolverChange> changes)
    {
        if (changes.Count != 1) return "";

        return $"Hidden Single in r{changes[0].Row + 1}c{changes[0].Column + 1}";
    }

    private ExplanationElement? Explanation(IReadOnlyList<SolverChange> changes)
    {
        if (changes.Count != 1) return null;

        var cell = new Cell(changes[0].Row, changes[0].Column);
        var ch = UnitMethods.Get(_unit).ToCoverHouse(cell);
        
        var start = new StringExplanationElement(changes[0].Number + " is only present in ");
        start.Append(new CellExplanationElement(cell)).Append(new StringExplanationElement(" in "))
            .Append(new CoverHouseExplanationElement(ch)).Append(new StringExplanationElement(". It is therefor the" +
                " solution for this cell."));

        return start;
    }
}