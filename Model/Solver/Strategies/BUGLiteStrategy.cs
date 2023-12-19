using System.Collections.Generic;
using Global;
using Model.Solver.Helpers.Changes;
using Model.Solver.Position;
using Model.Solver.Possibility;

namespace Model.Solver.Strategies;

public class BUGLiteStrategy : AbstractStrategy
{
    public const string OfficialName = "BUG-Lite";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;

    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public BUGLiteStrategy() : base(OfficialName, StrategyDifficulty.Hard, DefaultBehavior)
    {
        UniquenessDependency = UniquenessDependency.FullyDependent;
    }
    
    public override void Apply(IStrategyManager strategyManager)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var poss = strategyManager.PossibilitiesAt(row, col);
                if (poss.Count != 2) continue;

                var first = new Cell(row, col);
                var startR = row / 3;
                var startC = col / 3;

                for (int r = row % 3 + 1; r < 3; r++)
                {
                    var row2 = startR + r;
                    if (row2 == row) continue;
                    
                    for (int c = col % 3 + 1; c < 3; c++)
                    {
                        var col2 = startC + c;
                        if (col2 == col) continue;

                        if (!strategyManager.PossibilitiesAt(row2, col2).Equals(poss)) continue;

                        var second = new Cell(row2, col2);
                        if (Search(strategyManager, first, second, poss,
                            new GridPositions {first, second})) return;
                    }
                }
            }
        }
    }

    private bool Search(IStrategyManager strategyManager, Cell one, Cell two, IReadOnlyPossibilities possibilities,
        GridPositions done)
    {
        return false;
    }

    private IEnumerable<BiCellPossibilities> SearchForColumnMatch(IStrategyManager strategyManager, BiCellPossibilities bcp)
    {
        for (int miniR = 0; miniR < 3; miniR++)
        {
            if (miniR == bcp.One.Row / 3) continue;

            for (int r = 0; r < 3; r++)
            {
                var row = miniR * 3 + r;

                var first = new Cell(row, bcp.One.Column);
                var second = new Cell(row, bcp.Two.Column);

                if (strategyManager.PossibilitiesAt(first).PeekAll(bcp.Possibilities)
                    && strategyManager.PossibilitiesAt(second).PeekAll(bcp.Possibilities))
                    yield return new BiCellPossibilities(first, second, bcp.Possibilities);
            }
        }
    }
}

public record BiCellPossibilities(Cell One, Cell Two, IReadOnlyPossibilities Possibilities);

public class BUGLiteReportBuilder : IChangeReportBuilder
{
    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return ChangeReport.Default(changes);
    }
}