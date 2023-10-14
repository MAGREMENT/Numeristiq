using System;
using System.Collections.Generic;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Highlighting;
using Model.Solver.Positions;
using Model.Solver.Possibilities;
using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.CellColoring;
using Model.Solver.StrategiesUtil.CellColoring.ColoringResults;
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
                        
                        switch (entry.Value)
                        {
                            case Coloring.Off when cs.AddOff(cell):
                                strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, row, col);
                                if (strategyManager.ChangeBuffer.NotEmpty())
                                {
                                    strategyManager.ChangeBuffer.Push(this, new FromOffNishioForcingNetReportBuilder(
                                        coloring, row, col, possibility, cs.Cause, cell));
                                    return;
                                }

                                break;
                            
                            case Coloring.On when cs.AddOn(cell):
                                strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, row, col);
                                if (strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Push(this,
                                        new NishioForcingNetReportBuilder(coloring, row, col, possibility))) return;
                                break;
                            
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

    public ContradictionCause Cause { get; private set; } = ContradictionCause.None;

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
            if (poss.Count == 0)
            {
                Cause = ContradictionCause.Cell;
                return true;
            }
        }

        var rowInt = cell.Row * 9 + cell.Possibility;
        if (!_offRows.TryGetValue(rowInt, out var rowPos))
        {
            var copy = _view.RowPositionsAt(cell.Row, cell.Possibility).Copy();
            _offRows[rowInt] = copy;
            rowPos = copy;
        }
        
        rowPos.Remove(cell.Col);
        if (rowPos.Count == 0)
        {
            Cause = ContradictionCause.Row;
            return true;
        }
        

        var colInt = cell.Col * 9 + cell.Possibility;
        if (!_offCols.TryGetValue(colInt, out var colPos))
        {
            var copy = _view.ColumnPositionsAt(cell.Col, cell.Possibility).Copy();
            _offCols[colInt] = copy;
            colPos = copy;
        }
        
        colPos.Remove(cell.Row);
        if (colPos.Count == 0)
        {
            Cause = ContradictionCause.Column;
            return true;
        }
        

        var miniInt = cell.Row / 3 + cell.Col / 3 * 3 + cell.Possibility * 9;
        if (!_offMinis.TryGetValue(miniInt, out var miniPos))
        {
            var copy = _view.MiniGridPositionsAt(cell.Row / 3,
                cell.Col / 3, cell.Possibility).Copy();
            _offMinis[miniInt] = copy;
            miniPos = copy;
        }
        
        miniPos.Remove(cell.Row % 3, cell.Col % 3);
        if (miniPos.Count == 0)
        {
            Cause = ContradictionCause.MiniGrid;
            return true;
        }
        

        return false;
    }

    public bool AddOn(CellPossibility cell)
    {
        _onPositions.TryAdd(cell.Possibility, new GridPositions());
        var possibilityPositions = _onPositions[cell.Possibility];
        possibilityPositions.Add(cell.Row, cell.Col);

        if (possibilityPositions.RowCount(cell.Row) > 1)
        {
            Cause = ContradictionCause.Row;
            return true;
        }

        if (possibilityPositions.ColumnCount(cell.Col) > 1)
        {
            Cause = ContradictionCause.Column;
            return true;
        }

        if (possibilityPositions.MiniGridCount(cell.Row / 3, cell.Col / 3) > 1)
        {
            Cause = ContradictionCause.MiniGrid;
            return true;
        }
        
        foreach (var entry in _onPositions)
        {
            if (entry.Key != cell.Possibility && entry.Value.Peek(cell.Row, cell.Col))
            {
                Cause = ContradictionCause.Cell;
                return true;
            }
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

        Cause = ContradictionCause.None;
    }
}

public enum ContradictionCause
{
    None, Row, Column, MiniGrid, Cell
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

public class FromOffNishioForcingNetReportBuilder : IChangeReportBuilder //TODO FromOn
{
    private readonly ColoringDictionary<ILinkGraphElement> _coloring;
    private readonly int _row;
    private readonly int _col;
    private readonly int _possibility;
    private readonly ContradictionCause _cause;
    private readonly CellPossibility _lastChecked;

    public FromOffNishioForcingNetReportBuilder(ColoringDictionary<ILinkGraphElement> coloring, int row, int col,
        int possibility, ContradictionCause cause, CellPossibility lastChecked)
    {
        _coloring = coloring;
        _row = row;
        _col = col;
        _possibility = possibility;
        _cause = cause;
        _lastChecked = lastChecked;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        Highlight[] highlighters;
        int cursor;
        switch (_cause)
        {
            case ContradictionCause.Cell :
                var possibilities = snapshot.PossibilitiesAt(_lastChecked.Row, _lastChecked.Col);
                highlighters = new Highlight[possibilities.Count];

                cursor = 0;
                foreach (var possibility in possibilities)
                {
                    highlighters[cursor] = lighter =>
                    {
                        _coloring.History!.GetPathToRoot(new CellPossibility(_lastChecked.Row, _lastChecked.Col, possibility), Coloring.Off)
                            .Highlight(lighter);
                        lighter.CirclePossibility(_possibility, _row, _col);
                        lighter.HighlightPossibility(_possibility, _row, _col, ChangeColoration.ChangeTwo);
                    };
                    cursor++;
                }
                break;
            case ContradictionCause.Row :
                var cols = snapshot.RowPositionsAt(_lastChecked.Row, _possibility);
                highlighters = new Highlight[cols.Count];

                cursor = 0;
                foreach (var col in cols)
                {
                    highlighters[cursor] = lighter =>
                    {
                        _coloring.History!.GetPathToRoot(new CellPossibility(_lastChecked.Row, col, _possibility), Coloring.Off)
                            .Highlight(lighter);
                        lighter.CirclePossibility(_possibility, _row, _col);
                        lighter.HighlightPossibility(_possibility, _row, _col, ChangeColoration.ChangeTwo);
                    };
                    cursor++;
                }
                
                break;
            case ContradictionCause.Column :
                var rows = snapshot.ColumnPositionsAt(_lastChecked.Col, _possibility);
                highlighters = new Highlight[rows.Count];

                cursor = 0;
                foreach (var row in rows)
                {
                    highlighters[cursor] = lighter =>
                    {
                        _coloring.History!.GetPathToRoot(new CellPossibility(row, _lastChecked.Col, _possibility), Coloring.Off)
                            .Highlight(lighter);
                        lighter.CirclePossibility(_possibility, _row, _col);
                        lighter.HighlightPossibility(_possibility, _row, _col, ChangeColoration.ChangeTwo);
                    };
                    cursor++;
                }
                
                break;
            case ContradictionCause.MiniGrid :
                var cells = snapshot.MiniGridPositionsAt(_lastChecked.Row / 3,
                    _lastChecked.Col / 3, _possibility);
                highlighters = new Highlight[cells.Count];
                
                cursor = 0;
                foreach (var cell in cells)
                {
                    highlighters[cursor] = lighter =>
                    {
                        _coloring.History!.GetPathToRoot(new CellPossibility(cell.Row, cell.Col, _possibility), Coloring.Off)
                            .Highlight(lighter);
                        lighter.CirclePossibility(_possibility, _row, _col);
                        lighter.HighlightPossibility(_possibility, _row, _col, ChangeColoration.ChangeTwo);
                    };
                    cursor++;
                }
                
                break;
            default: highlighters = Array.Empty<Highlight>();
                break;
        }

        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", highlighters);
    }
}