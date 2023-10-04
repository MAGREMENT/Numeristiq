using System.Collections.Generic;
using Model.Solver.Helpers;
using Model.Solver.Helpers.Changes;
using Model.Solver.Positions;
using Model.Solver.Possibilities;
using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.CellColoring;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver.Strategies.ForcingNets;

public class NishioForcingNetStrategy : AbstractStrategy
{ 
    public const string OfficialName = "Nishio Forcing Net";
    
    public NishioForcingNetStrategy() : base(OfficialName, StrategyDifficulty.Extreme)
    {
        
    }

    public override void ApplyOnce(IStrategyManager strategyManager)
    {
        ContradictionSearcher cs = new ContradictionSearcher(strategyManager);

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                foreach (var possibility in strategyManager.PossibilitiesAt(row, col))
                {
                    var coloring = strategyManager.PreComputer.OnColoring(row, col, possibility);
                    foreach (var entry in coloring)
                    {
                        if (entry.Key is not CellPossibility cell) continue;
                        
                        if ((entry.Value == Coloring.Off && cs.AddOff(cell))
                            || (entry.Value == Coloring.On && cs.AddOn(cell)))
                        {
                            strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, row, col);
                            if (strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Push(this,
                                new NishioForcingNetReportBuilder(coloring, row, col, possibility))) return;
                        }
                    }

                    cs.Reset();
                }
            }
        }
    }
}

public class ContradictionSearcher
{
    private readonly Dictionary<int, IPossibilities> _offCells = new();
    private readonly Dictionary<int, LinePositions> _offRows = new();
    private readonly Dictionary<int, LinePositions> _offCols = new();
    private readonly Dictionary<int, MiniGridPositions> _offMinis = new();

    private readonly Dictionary<int, GridPositions> _onPositions = new();

    private readonly IStrategyManager _view;

    public ContradictionSearcher(IStrategyManager view)
    {
        _view = view;
    }

    /// <summary>
    /// Returns true if a contradiction if found
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    public bool AddOff(CellPossibility cell)
    {
        var cellInt = cell.Row * 9 + cell.Col;
        if (!_offCells.TryGetValue(cellInt, out var poss))
        {
            var copy = _view.PossibilitiesAt(cell.Row, cell.Col).Copy();
            copy.Remove(cell.Possibility);
            _offCells[cellInt] = copy;
        }
        else
        {
            poss.Remove(cell.Possibility);
            if (poss.Count == 0) return true;
        }

        var rowInt = cell.Row * 9 + cell.Possibility;
        if (!_offRows.TryGetValue(rowInt, out var rowPos))
        {
            var copy = _view.RowPositionsAt(cell.Row, cell.Possibility).Copy();
            copy.Remove(cell.Col);
            _offRows[rowInt] = copy;
        }
        else
        {
            rowPos.Remove(cell.Possibility);
            if (rowPos.Count == 0){ return true;}
        }

        var colInt = cell.Col * 9 + cell.Possibility;
        if (!_offCols.TryGetValue(colInt, out var colPos))
        {
            var copy = _view.ColumnPositionsAt(cell.Col, cell.Possibility).Copy();
            copy.Remove(cell.Row);
            _offCols[colInt] = copy;
        }
        else
        {
            colPos.Remove(cell.Row);
            if (colPos.Count == 0) return true;
        }

        var miniInt = cell.Row / 3 + cell.Col / 3 * 3 + cell.Possibility * 9;
        if (!_offMinis.TryGetValue(miniInt, out var miniPos))
        {
            var copy = _view.MiniGridPositionsAt(cell.Row / 3,
                cell.Col / 3, cell.Possibility).Copy();
            copy.Remove(cell.Row % 3, cell.Col % 3);
            _offMinis[miniInt] = copy;
        }
        else
        {
            miniPos.Remove(cell.Row % 3, cell.Col % 3);
            if (miniPos.Count == 0) return true;
        }

        return false;
    }

    public bool AddOn(CellPossibility cell)
    {
        if (!_onPositions.TryGetValue(cell.Possibility, out var positions))
        {
            _onPositions.Add(cell.Possibility, new GridPositions());
        }
        else
        {
            if (positions.Peek(cell.Row, cell.Col)) return true;

            positions.Add(cell.Row, cell.Col);
            if (positions.RowCount(cell.Row) > 1
                || positions.ColumnCount(cell.Col) > 1
                || positions.MiniGridCount(cell.Row / 3, cell.Col / 3) > 1) return true;
        }

        return false;
    }

    public void Reset()
    {
        _offCells.Clear();
        _offRows.Clear();
        _offCols.Clear();
        _offMinis.Clear();

        _onPositions.Clear();
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

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
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