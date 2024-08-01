using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.CellColoring;
using Model.Sudokus.Solver.Utility.CellColoring.ColoringResults;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;

namespace Model.Sudokus.Solver.Strategies.ForcingNets;

public class CellForcingNetStrategy : SudokuStrategy
{
    public const string OfficialName = "Cell Forcing Net";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    private readonly int _max;

    public CellForcingNetStrategy(int maxPossibilities) : base(OfficialName,  Difficulty.Inhuman, DefaultInstanceHandling)
    {
        _max = maxPossibilities;
    }

    public override void Apply(ISudokuSolverData solverData)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (solverData.PossibilitiesAt(row, col).Count < 2 ||
                    solverData.PossibilitiesAt(row, col).Count > _max) continue;
                var possAsArray = solverData.PossibilitiesAt(row, col).ToArray();

                var colorings = new ColoringDictionary<ISudokuElement>[possAsArray.Length];

                for (int i = 0; i < possAsArray.Length; i++)
                {
                    colorings[i] = solverData.PreComputer.OnColoring(row, col, possAsArray[i]);
                }

                if (Process(solverData, colorings, new Cell(row, col))) return;
            }
        }
    }

    private bool Process(ISudokuSolverData view, ColoringDictionary<ISudokuElement>[] colorings, Cell current)
    {
        foreach (var element in colorings[0])
        {
            if (element.Key is not CellPossibility cell) continue;
            
            var currentColoring = element.Value;
            bool isSameInAll = true;

            for (int i = 1; i < colorings.Length && isSameInAll; i++)
            {
                if (!colorings[i].TryGetValue(cell, out var c) || c != currentColoring)
                {
                    isSameInAll = false;
                    break;
                }
            }

            if (isSameInAll)
            {
                if (currentColoring == Coloring.On)
                {
                    view.ChangeBuffer.ProposeSolutionAddition(cell.Possibility, cell.Row, cell.Column);
                    if (view.ChangeBuffer.NotEmpty() && view.ChangeBuffer.Commit(
                            new CellForcingNetBuilder(colorings, current.Row, current.Column, cell, Coloring.On,
                                view.PreComputer.Graphs.ComplexLinkGraph)) && StopOnFirstPush) return true;
                }
                else
                {
                    view.ChangeBuffer.ProposePossibilityRemoval(cell.Possibility, cell.Row, cell.Column);
                    if (view.ChangeBuffer.NotEmpty() && view.ChangeBuffer.Commit(
                            new CellForcingNetBuilder(colorings, current.Row, current.Column, cell, Coloring.Off,
                                view.PreComputer.Graphs.ComplexLinkGraph)) && StopOnFirstPush) return true;
                }
            }
        }
        
        //Not yet proven useful so bye bye for now, if implemented, do not forget to add the OnInstanceHandling
        /*HashSet<int> count = new HashSet<int>(colorings.Length);

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (current.Row == row && current.Col == col) continue;
                
                Possibilities on = Possibilities.NewEmpty();

                foreach (var possibility in view.PossibilitiesAt(row, col))
                {
                    var examined = new CellPossibility(row, col, possibility);

                    for (int i = 0; i < colorings.Length; i++)
                    {
                        if (colorings[i].TryGetValue(examined, out var c) && c == Coloring.On)
                        {
                            count.Add(i);
                            on.Add(possibility);
                        }
                    }
                }

                if (count.Count == colorings.Length) RemoveAll(view, row, col, on);
                count.Clear();
            }
        }

        for (int number = 1; number <= 9; number++)
        {
            for (int row = 0; row < 9; row++)
            {
                var cols = view.RowPositionsAt(row, number);
                if (cols.Count == 0) continue;
                
                LinePositions on = new LinePositions();

                foreach (var col in cols)
                {
                    var examined = new CellPossibility(row, col, number);
                    
                    for (int i = 0; i < colorings.Length; i++)
                    {
                        if (colorings[i].TryGetValue(examined, out var c) && c == Coloring.On)
                        {
                            count.Add(i);
                            on.Add(col);
                        }
                    }
                }

                if (count.Count == colorings.Length)
                {
                    foreach (var col in cols)
                    {
                        if (on.Peek(col)) continue;

                        view.ChangeBuffer.ProposePossibilityRemoval(number, row, col);
                    }
                }
                count.Clear();
            }

            for (int col = 0; col < 9; col++)
            {
                var rows = view.ColumnPositionsAt(col, number);
                if (rows.Count == 0) continue;

                LinePositions on = new LinePositions();

                foreach (var row in rows)
                {
                    var examined = new CellPossibility(row, col, number);
                    
                    for (int i = 0; i < colorings.Length; i++)
                    {
                        if (colorings[i].TryGetValue(examined, out var c) && c == Coloring.On)
                        {
                            count.Add(i);
                            on.Add(row);
                        }
                    }
                }

                if (count.Count == colorings.Length)
                {
                    foreach (var row in rows)
                    {
                        if(on.Peek(row)) continue;

                        view.ChangeBuffer.ProposePossibilityRemoval(number, row, col);
                    }
                }
                count.Clear();
            }

            for (int miniRow = 0; miniRow < 3; miniRow++)
            {
                for (int miniCol = 0; miniCol < 3; miniCol++)
                {
                    var cells = view.MiniGridPositionsAt(miniRow, miniCol, number);
                    if (cells.Count == 0) continue;

                    MiniGridPositions on = new MiniGridPositions(miniRow, miniCol);

                    foreach (var cell in cells)
                    {
                        var examined = new CellPossibility(cell, number);
                    
                        for (int i = 0; i < colorings.Length; i++)
                        {
                            if (colorings[i].TryGetValue(examined, out var c) && c == Coloring.On)
                            {
                                count.Add(i);
                                on.Add(cell.Row % 3, cell.Col % 3);
                            }
                        }
                    }

                    if (count.Count == colorings.Length)
                    {
                        foreach (var cell in cells)
                        {
                            if (on.Peek(cell.Row % 3, cell.Col % 3)) continue;

                            view.ChangeBuffer.ProposePossibilityRemoval(number, cell.Row, cell.Col);
                        }
                    }
                    count.Clear();
                }
            }
        }*/

        return false;
    }
}

public class CellForcingNetBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly ColoringDictionary<ISudokuElement>[] _colorings;
    private readonly int _row;
    private readonly int _col;
    private readonly CellPossibility _target;
    private readonly Coloring _targetColoring;
    private readonly ILinkGraph<ISudokuElement> _graph;

    public CellForcingNetBuilder(ColoringDictionary<ISudokuElement>[] colorings, int row, int col,
        CellPossibility target, Coloring targetColoring, ILinkGraph<ISudokuElement> graph)
    {
        _colorings = colorings;
        _row = row;
        _col = col;
        _target = target;
        _targetColoring = targetColoring;
        _graph = graph;
    }


    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        Highlight<ISudokuHighlighter>[] highlights = new Highlight<ISudokuHighlighter>[_colorings.Length];
        var paths = new List<LinkGraphChain<ISudokuElement>>[_colorings.Length];

        for (int i = 0; i < _colorings.Length; i++)
        {
            paths[i] = ForcingNetsUtility.FindEveryNeededPaths(_colorings[i].History!.GetPathToRootWithGuessedLinks(_target,
                _targetColoring), _colorings[i], _graph, snapshot);
            
            var iForDelegate = i;
            highlights[i] = lighter =>
            {
                ForcingNetsUtility.HighlightAllPaths(lighter, paths[iForDelegate], Coloring.On);
                
                lighter.EncircleCell(_row, _col);
                ChangeReportHelper.HighlightChanges(lighter, changes);
            };
        }
        
        return new ChangeReport<ISudokuHighlighter>( "", highlights);
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}