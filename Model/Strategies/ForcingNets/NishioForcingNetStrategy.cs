using System.Collections.Generic;
using Model.Changes;
using Model.Positions;
using Model.Possibilities;
using Model.Solver;
using Model.StrategiesUtil;
using Model.StrategiesUtil.LinkGraph;

namespace Model.Strategies.ForcingNets;

public class NishioForcingNetStrategy : IStrategy
{
    public string Name => "Nishio forcing net";
    public StrategyLevel Difficulty => StrategyLevel.Extreme;
    public StatisticsTracker Tracker { get; } = new();

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
                    var coloring = strategyManager.OnColoring(row, col, possibility);
                    foreach (var entry in coloring)
                    {
                        if (entry.Value != Coloring.Off || entry.Key is not CellPossibility coord) continue;
                        if (cs.AddOff(coord))
                        {
                            strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, row, col);
                            break;
                        }
                    }

                    if (strategyManager.ChangeBuffer.NotEmpty()) strategyManager.ChangeBuffer.Push(this,
                        new NishioForcingNetReportBuilder(coloring, row, col, possibility));
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
    public bool AddOff(CellPossibility coord)
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
            var copy = _view.RowPositionsAt(coord.Row, coord.Possibility).Copy();
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
            var copy = _view.ColumnPositionsAt(coord.Col, coord.Possibility).Copy();
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
            var copy = _view.MiniGridPositionsAt(coord.Row / 3,
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

    private readonly Dictionary<ILinkGraphElement, Coloring> _coloring;
    private readonly int _row;
    private readonly int _col;
    private readonly int _possibility;

    public NishioForcingNetReportBuilder(Dictionary<ILinkGraphElement, Coloring> coloring, int row, int col, int possibility)
    {
        _coloring = coloring;
        _row = row;
        _col = col;
        _possibility = possibility;
    }

    public ChangeReport Build(List<SolverChange> changes, ISolver snapshot)
    {
        var c = ForcingNetsUtil.FilterPossibilityCoordinates(_coloring);
        
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "",
            lighter =>
            {
                ForcingNetsUtil.HighlightColoring(lighter, c);
                IChangeReportBuilder.HighlightChanges(lighter, changes);
                lighter.CirclePossibility(_possibility, _row, _col);
            });
    }
}