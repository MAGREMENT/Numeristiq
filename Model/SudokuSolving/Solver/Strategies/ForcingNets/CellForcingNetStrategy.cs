using System.Collections.Generic;
using Global;
using Model.SudokuSolving.Solver.Helpers.Changes;
using Model.SudokuSolving.Solver.Helpers.Highlighting;
using Model.SudokuSolving.Solver.StrategiesUtility;
using Model.SudokuSolving.Solver.StrategiesUtility.CellColoring;
using Model.SudokuSolving.Solver.StrategiesUtility.CellColoring.ColoringResults;
using Model.SudokuSolving.Solver.StrategiesUtility.Graphs;

namespace Model.SudokuSolving.Solver.Strategies.ForcingNets;

public class CellForcingNetStrategy : AbstractStrategy
{
    public const string OfficialName = "Cell Forcing Net";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    private readonly int _max;

    public CellForcingNetStrategy(int maxPossibilities) : base(OfficialName,  StrategyDifficulty.Extreme, DefaultBehavior)
    {
        _max = maxPossibilities;
    }

    public override void Apply(IStrategyManager strategyManager)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (strategyManager.PossibilitiesAt(row, col).Count < 2 ||
                    strategyManager.PossibilitiesAt(row, col).Count > _max) continue;
                var possAsArray = strategyManager.PossibilitiesAt(row, col).ToArray();

                var colorings = new ColoringDictionary<ISudokuElement>[possAsArray.Length];

                for (int i = 0; i < possAsArray.Length; i++)
                {
                    colorings[i] = strategyManager.PreComputer.OnColoring(row, col, possAsArray[i]);
                }

                if (Process(strategyManager, colorings, new Cell(row, col))) return;
            }
        }
    }

    private bool Process(IStrategyManager view, ColoringDictionary<ISudokuElement>[] colorings, Cell current)
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
                    if (view.ChangeBuffer.NotEmpty() && view.ChangeBuffer.Commit(this,
                            new CellForcingNetBuilder(colorings, current.Row, current.Column, cell, Coloring.On,
                                view.GraphManager.ComplexLinkGraph)) && OnCommitBehavior == OnCommitBehavior.Return) return true;
                }
                else
                {
                    view.ChangeBuffer.ProposePossibilityRemoval(cell.Possibility, cell.Row, cell.Column);
                    if (view.ChangeBuffer.NotEmpty() && view.ChangeBuffer.Commit(this,
                            new CellForcingNetBuilder(colorings, current.Row, current.Column, cell, Coloring.Off,
                                view.GraphManager.ComplexLinkGraph)) && OnCommitBehavior == OnCommitBehavior.Return) return true;
                }
            }
        }
        
        //Not yet proven useful so bye bye for now, if implemented, do not forget to add the OnCommitBehavior
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

public class CellForcingNetBuilder : IChangeReportBuilder
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


    public ChangeReport Build(IReadOnlyList<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        Highlight[] highlights = new Highlight[_colorings.Length];
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
                IChangeReportBuilder.HighlightChanges(lighter, changes);
            };
        }
        
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", highlights);
    }
}