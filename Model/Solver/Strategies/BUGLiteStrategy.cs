using System.Collections.Generic;
using Global;
using Global.Enums;
using Model.Solver.Helpers.Changes;
using Model.Solver.Position;
using Model.Solver.StrategiesUtility;

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
        Dictionary<BiValue, List<Cell>> biValueMap = new();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var poss = strategyManager.PossibilitiesAt(row, col);
                if (poss.Count != 2) continue;

                var i = 0;
                poss.Next(ref i);
                var first = i;
                poss.Next(ref i);
                var second = i;
                var bi = new BiValue(first, second);

                if (!biValueMap.TryGetValue(bi, out var list))
                {
                    list = new List<Cell>();
                    biValueMap[bi] = list;
                }

                list.Add(new Cell(row, col));
            }
        }

        foreach (var entry in biValueMap)
        {
            if (entry.Value.Count < 3) continue;

            if (Search(strategyManager, entry.Value, 0, new GridPositions(), entry.Key)) return;
        }
    }

    private bool Search(IStrategyManager strategyManager, List<Cell> cells, int start, GridPositions positions, BiValue bi)
    {
        for (int i = start; i < cells.Count; i++)
        {
            positions.Add(cells[i]);

            if (positions.Count >= 3 && positions.Count % 2 == 1 && Try(strategyManager, positions, bi)) return true;

            Search(strategyManager, cells, i + 1, positions, bi);
            
            positions.Remove(cells[i]);
        }

        return false;
    }

    private bool Try(IStrategyManager strategyManager, GridPositions positions, BiValue bi)
    {
        var soloRow = UniquenessHelper.SearchExceptionInUnit(Unit.Row, 2, positions);
        if (soloRow == -1) return false;

        var soloCol = UniquenessHelper.SearchExceptionInUnit(Unit.Column, 2, positions);
        if (soloCol == -1) return false;

        var soloMini = UniquenessHelper.SearchExceptionInUnit(Unit.MiniGrid, 2, positions);
        if (soloMini == -1) return false;

        var miniRow = soloMini / 3;
        var miniCol = soloCol / 3;
        
        if (soloRow / 3 == miniRow && soloCol / 3 == miniCol)
        {
            strategyManager.ChangeBuffer.ProposePossibilityRemoval(bi.One, soloRow, soloCol);
            strategyManager.ChangeBuffer.ProposePossibilityRemoval(bi.Two, soloRow, soloCol);
        }

        return strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
            new BUGLiteReportBuilder(positions, soloRow, soloCol)) && OnCommitBehavior == OnCommitBehavior.Return;
    }
}

public class BUGLiteReportBuilder : IChangeReportBuilder
{
    private readonly IEnumerable<Cell> _gp;
    private readonly int _row;
    private readonly int _col;

    public BUGLiteReportBuilder(IEnumerable<Cell> gp, int row, int col)
    {
        _gp = gp;
        _row = row;
        _col = col;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            foreach (var cell in _gp)
            {
                lighter.HighlightCell(cell, ChangeColoration.CauseOffOne);
            }
            
            lighter.HighlightCell(_row, _col, ChangeColoration.CauseOffTwo);

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}