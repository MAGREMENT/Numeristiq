using System.Collections.Generic;
using Model.Positions;
using Model.Possibilities;
using Model.StrategiesUtil;

namespace Model.Strategies.ForcingNets;

public class NishioForcingNetStrategy : IStrategy
{
    public string Name => "Nishio forcing net";
    public StrategyLevel Difficulty => StrategyLevel.Extreme;
    public int Score { get; set; }

    public void ApplyOnce(IStrategyManager strategyManager)
    {
        LinkGraph<ILinkGraphElement> graph = strategyManager.LinkGraph();
        ContradictionSearcher cs = new ContradictionSearcher(strategyManager);

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                foreach (var possibility in strategyManager.Possibilities[row, col])
                {
                    foreach (var entry in strategyManager.OnColoring(row, col, possibility))
                    {
                        if (entry.Value != Coloring.Off || entry.Key is not PossibilityCoordinate coord) continue;
                        if (cs.AddOff(coord))
                        {
                            strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, row, col);
                            break;
                        }
                    }

                    if (strategyManager.ChangeBuffer.NotEmpty())
                        strategyManager.ChangeBuffer.Push(this, new NishioForcingNetReportBuilder());
                    cs.Reset();
                }
            }
        }
    }
}

public class ContradictionSearcher
{
    private readonly Dictionary<int, IPossibilities> _cells = new();
    private readonly Dictionary<int, LinePositions> _rows = new();
    private readonly Dictionary<int, LinePositions> _cols = new();
    private readonly Dictionary<int, MiniGridPositions> _minis = new();

    private readonly IStrategyManager _view;

    public ContradictionSearcher(IStrategyManager view)
    {
        _view = view;
    }

    //returns true if contradiction
    public bool AddOff(PossibilityCoordinate coord)
    {
        var cellInt = coord.Row * 9 + coord.Col;
        if (!_cells.TryGetValue(cellInt, out var poss))
        {
            var copy = _view.Possibilities[coord.Row, coord.Col].Copy();
            copy.Remove(coord.Possibility);
            _cells[cellInt] = copy;
        }
        else
        {
            poss.Remove(coord.Possibility);
            if (poss.Count == 0) return true;
        }

        var rowInt = coord.Row * 9 + coord.Possibility;
        if (!_rows.TryGetValue(rowInt, out var rowPos))
        {
            var copy = _view.PossibilityPositionsInRow(coord.Row, coord.Possibility).Copy();
            copy.Remove(coord.Col);
            _rows[rowInt] = copy;
        }
        else
        {
            rowPos.Remove(coord.Possibility);
            if (rowPos.Count == 0){ return true;}
        }

        var colInt = coord.Col * 9 + coord.Possibility;
        if (!_cols.TryGetValue(colInt, out var colPos))
        {
            var copy = _view.PossibilityPositionsInColumn(coord.Col, coord.Possibility).Copy();
            copy.Remove(coord.Row);
            _cols[colInt] = copy;
        }
        else
        {
            colPos.Remove(coord.Row);
            if (colPos.Count == 0) return true;
        }

        var miniInt = coord.Row / 3 + coord.Col / 3 * 3 + coord.Possibility * 9;
        if (!_minis.TryGetValue(miniInt, out var miniPos))
        {
            var copy = _view.PossibilityPositionsInMiniGrid(coord.Row / 3,
                coord.Col / 3, coord.Possibility).Copy();
            copy.Remove(coord.Row % 3, coord.Col % 3);
            _minis[miniInt] = copy;
        }
        else
        {
            miniPos.Remove(coord.Row % 3, coord.Col % 3);
            if (miniPos.Count == 0) return true;
        }

        return false;
    }

    public void Reset()
    {
        _cells.Clear();
        _rows.Clear();
        _cols.Clear();
        _minis.Clear();
    }
}

public class NishioForcingNetReportBuilder : IChangeReportBuilder
{
    public ChangeReport Build(List<SolverChange> changes, IChangeManager manager)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes),
            lighter => IChangeReportBuilder.HighlightChanges(lighter, changes), "");
    }
}