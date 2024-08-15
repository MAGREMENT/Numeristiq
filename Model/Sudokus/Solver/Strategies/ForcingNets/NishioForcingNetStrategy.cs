using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Graphs;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Position;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.CellColoring;
using Model.Sudokus.Solver.Utility.CellColoring.ColoringResults;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies.ForcingNets;

public class NishioForcingNetStrategy : SudokuStrategy
{ 
    public const string OfficialName = "Nishio Forcing Net";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    public NishioForcingNetStrategy() : base(OfficialName, Difficulty.Inhuman, DefaultInstanceHandling)
    { }

    public override void Apply(ISudokuSolverData solverData)
    {
        ContradictionSearcher cs = new ContradictionSearcher(solverData);

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                foreach (var possibility in solverData.PossibilitiesAt(row, col).EnumeratePossibilities())
                {
                    var coloring = solverData.PreComputer.OnColoring(row, col, possibility);
                    foreach (var entry in coloring)
                    {
                        if (entry.Key is not CellPossibility cell) continue;
                        
                        switch (entry.Value)
                        {
                            case Coloring.Off when cs.AddOff(cell):
                                solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, row, col);
                                if (solverData.ChangeBuffer.NeedCommit())
                                {
                                    solverData.ChangeBuffer.Commit(new NishioForcingNetReportBuilder(coloring, row, col, possibility,
                                        cs.Cause, cell, Coloring.Off, ForcingNetsUtility.GetReportGraph(solverData)));
                                    if (StopOnFirstCommit) return;
                                }
                                break;
                            
                            case Coloring.On when cs.AddOn(cell):
                                solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, row, col);
                                if (solverData.ChangeBuffer.NeedCommit())
                                {
                                    solverData.ChangeBuffer.Commit(new NishioForcingNetReportBuilder(coloring, row, col, possibility,
                                        cs.Cause, cell, Coloring.On, ForcingNetsUtility.GetReportGraph(solverData)));
                                    if (StopOnFirstCommit) return;
                                }
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
    private readonly Dictionary<int, ReadOnlyBitSet16> _offCells = new();
    private readonly Dictionary<int, LinePositions> _offRows = new();
    private readonly Dictionary<int, LinePositions> _offCols = new();
    private readonly Dictionary<int, BoxPositions> _offMinis = new();

    private readonly Dictionary<int, GridPositions> _onPositions = new();

    private readonly ISudokuSolverData _view;

    public ContradictionCause Cause { get; private set; } = ContradictionCause.None;

    public ContradictionSearcher(ISudokuSolverData view)
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
        var cellInt = cell.Row * 9 + cell.Column;
        if (!_offCells.ContainsKey(cellInt))
        {
            _offCells[cellInt] = _view.PossibilitiesAt(cell.Row, cell.Column) - cell.Possibility;
        }
        else
        {
            _offCells[cellInt] -= cell.Possibility;
            if (_offCells[cellInt].Count == 0)
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
        
        rowPos.Remove(cell.Column);
        if (rowPos.Count == 0)
        {
            Cause = ContradictionCause.Row;
            return true;
        }
        

        var colInt = cell.Column * 9 + cell.Possibility;
        if (!_offCols.TryGetValue(colInt, out var colPos))
        {
            var copy = _view.ColumnPositionsAt(cell.Column, cell.Possibility).Copy();
            _offCols[colInt] = copy;
            colPos = copy;
        }
        
        colPos.Remove(cell.Row);
        if (colPos.Count == 0)
        {
            Cause = ContradictionCause.Column;
            return true;
        }
        

        var miniInt = cell.Row / 3 + cell.Column / 3 * 3 + cell.Possibility * 9;
        if (!_offMinis.TryGetValue(miniInt, out var miniPos))
        {
            var copy = _view.MiniGridPositionsAt(cell.Row / 3,
                cell.Column / 3, cell.Possibility).Copy();
            _offMinis[miniInt] = copy;
            miniPos = copy;
        }
        
        miniPos.Remove(cell.Row % 3, cell.Column % 3);
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
        possibilityPositions.Add(cell.Row, cell.Column);

        if (possibilityPositions.RowCount(cell.Row) > 1)
        {
            Cause = ContradictionCause.Row;
            return true;
        }

        if (possibilityPositions.ColumnCount(cell.Column) > 1)
        {
            Cause = ContradictionCause.Column;
            return true;
        }

        if (possibilityPositions.MiniGridCount(cell.Row / 3, cell.Column / 3) > 1)
        {
            Cause = ContradictionCause.MiniGrid;
            return true;
        }
        
        foreach (var entry in _onPositions)
        {
            if (entry.Key != cell.Possibility && entry.Value.Contains(cell.Row, cell.Column))
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

public class NishioForcingNetReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly ColoringDictionary<ISudokuElement> _coloring;
    private readonly int _row;
    private readonly int _col;
    private readonly int _possibility;
    private readonly ContradictionCause _cause;
    private readonly CellPossibility _lastChecked;
    private readonly Coloring _causeColoring;
    private readonly ILinkGraph<ISudokuElement> _graph;

    public NishioForcingNetReportBuilder(ColoringDictionary<ISudokuElement> coloring, int row, int col,
        int possibility, ContradictionCause cause, CellPossibility lastChecked, Coloring causeColoring, ILinkGraph<ISudokuElement> graph)
    {
        _coloring = coloring;
        _row = row;
        _col = col;
        _possibility = possibility;
        _cause = cause;
        _lastChecked = lastChecked;
        _causeColoring = causeColoring;
        _graph = graph;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        List<Highlight<ISudokuHighlighter>> highlighters = new();
        switch (_cause)
        {
            case ContradictionCause.Cell :
                var possibilities = snapshot.PossibilitiesAt(_lastChecked.Row, _lastChecked.Column);

                foreach (var possibility in possibilities.EnumeratePossibilities())
                {
                    var current = new CellPossibility(_lastChecked.Row, _lastChecked.Column, possibility);
                    if (!_coloring.TryGetColoredElement(current, out var c) || c != _causeColoring) continue;
                        
                    highlighters.Add( lighter =>
                    {
                        var paths = ForcingNetsUtility.FindEveryNeededPaths(_coloring.History!
                                .GetPathToRootWithGuessedLinks(current, c), _coloring, _graph, snapshot);
                        ForcingNetsUtility.HighlightAllPaths(lighter, paths, Coloring.Off);
                        
                        lighter.EncirclePossibility(_possibility, _row, _col);
                        ChangeReportHelper.HighlightChanges(lighter, changes);
                    });
                }
                break;
            case ContradictionCause.Row :
                var cols = snapshot.RowPositionsAt(_lastChecked.Row, _lastChecked.Possibility);
                
                foreach (var col in cols)
                {
                    var current = new CellPossibility(_lastChecked.Row, col, _lastChecked.Possibility);
                    if (!_coloring.TryGetColoredElement(current, out var c) || c != _causeColoring) continue;
                    
                    highlighters.Add(lighter =>
                    {
                        var paths = ForcingNetsUtility.FindEveryNeededPaths(_coloring.History!
                            .GetPathToRootWithGuessedLinks(current, c), _coloring, _graph, snapshot);
                        ForcingNetsUtility.HighlightAllPaths(lighter, paths, Coloring.Off);
                        
                        lighter.EncirclePossibility(_possibility, _row, _col);
                        ChangeReportHelper.HighlightChanges(lighter, changes);
                    });
                }
                
                break;
            case ContradictionCause.Column :
                var rows = snapshot.ColumnPositionsAt(_lastChecked.Column, _lastChecked.Possibility);
                
                foreach (var row in rows)
                {
                    var current = new CellPossibility(row, _lastChecked.Column, _lastChecked.Possibility);
                    if (!_coloring.TryGetColoredElement(current, out var c) || c != _causeColoring) continue;
                    
                    highlighters.Add(lighter =>
                    {
                        var paths = ForcingNetsUtility.FindEveryNeededPaths(_coloring.History!
                            .GetPathToRootWithGuessedLinks(current, c), _coloring, _graph, snapshot);
                        ForcingNetsUtility.HighlightAllPaths(lighter, paths, Coloring.Off);
                        
                        lighter.EncirclePossibility(_possibility, _row, _col);
                        ChangeReportHelper.HighlightChanges(lighter, changes);
                    });
                }
                
                break;
            case ContradictionCause.MiniGrid :
                var cells = snapshot.MiniGridPositionsAt(_lastChecked.Row / 3,
                    _lastChecked.Column / 3, _lastChecked.Possibility);
              
                foreach (var cell in cells)
                {
                    var current = new CellPossibility(cell.Row, cell.Column, _lastChecked.Possibility);
                    if (!_coloring.TryGetColoredElement(current, out var c) || c != _causeColoring) continue;
                    
                    highlighters.Add(lighter =>
                    {
                        var paths = ForcingNetsUtility.FindEveryNeededPaths(_coloring.History!
                            .GetPathToRootWithGuessedLinks(current, c), _coloring, _graph, snapshot);
                        ForcingNetsUtility.HighlightAllPaths(lighter, paths, Coloring.Off);
                        
                        lighter.EncirclePossibility(_possibility, _row, _col);
                        ChangeReportHelper.HighlightChanges(lighter, changes);
                    });
                }
                
                break;
        }

        return new ChangeReport<ISudokuHighlighter>( Explanation(), highlighters.ToArray());
    }

    private string Explanation()
    {
        var result = $"{_possibility}r{_row + 1}c{_col + 1} being ON will lead to ";

        result += _causeColoring == Coloring.On ? "multiple candidates being ON in " : "all candidates being OFF in ";

        result += _cause switch
        {
            ContradictionCause.Cell => $"r{_lastChecked.Row + 1}c{_lastChecked.Column + 1}",
            ContradictionCause.Row => $"n{_lastChecked.Possibility}r{_lastChecked.Row + 1}",
            ContradictionCause.Column => $"n{_lastChecked.Possibility}c{_lastChecked.Column + 1}",
            ContradictionCause.MiniGrid => $"n{_lastChecked.Possibility}b{_lastChecked.Row * 3 + _lastChecked.Column + 1}",
            _ => ""
        };

        return result;
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}