using System.Collections.Generic;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Highlighting;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Sudoku.Solver.StrategiesUtility.CellColoring;
using Model.Sudoku.Solver.StrategiesUtility.CellColoring.ColoringResults;
using Model.Sudoku.Solver.StrategiesUtility.Graphs;

namespace Model.Sudoku.Solver.Strategies.ForcingNets;

public class UnitForcingNetStrategy : SudokuStrategy
{
    public const string OfficialName = "Unit Forcing Net";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    private readonly int _max;

    public UnitForcingNetStrategy(int maxPossibilities) : base(OfficialName, StrategyDifficulty.Extreme, DefaultInstanceHandling)
    {
        _max = maxPossibilities;
    }
    
    public override void Apply(IStrategyUser strategyUser)
    {
        for (int number = 1; number <= 9; number++)
        {
            for (int row = 0; row < 9; row++)
            {
                var ppir = strategyUser.RowPositionsAt(row, number);
                if (ppir.Count < 2 || ppir.Count > _max) continue;
                
                var colorings = new ColoringDictionary<ISudokuElement>[ppir.Count];

                var cursor = 0;
                foreach (var col in ppir)
                {
                    colorings[cursor] = strategyUser.PreComputer.OnColoring(row, col, number);
                    cursor++;
                }

                if (Process(strategyUser, colorings)) return;
            }

            for (int col = 0; col < 9; col++)
            {
                var ppic = strategyUser.ColumnPositionsAt(col, number);
                if (ppic.Count < 2 || ppic.Count > _max) continue;
                
                var colorings = new ColoringDictionary<ISudokuElement>[ppic.Count];

                var cursor = 0;
                foreach (var row in ppic)
                {
                    colorings[cursor] = strategyUser.PreComputer.OnColoring(row, col, number);
                    cursor++;
                }

                if (Process(strategyUser, colorings)) return;
            }

            for (int miniRow = 0; miniRow < 3; miniRow++)
            {
                for (int miniCol = 0; miniCol < 3; miniCol++)
                {
                    var ppimn = strategyUser.MiniGridPositionsAt(miniRow, miniCol, number);
                    if (ppimn.Count < 2 || ppimn.Count > _max) continue;
                
                    var colorings = new ColoringDictionary<ISudokuElement>[ppimn.Count];

                    var cursor = 0;
                    foreach (var pos in ppimn)
                    {
                        colorings[cursor] = strategyUser.PreComputer.OnColoring(pos.Row, pos.Column, number);
                        cursor++;
                    }

                    if (Process(strategyUser, colorings)) return;
                }
            }
        }
    }

    private bool Process(IStrategyUser view, ColoringDictionary<ISudokuElement>[] colorings)
    {
        foreach (var element in colorings[0])
        {
            if (element.Key is not CellPossibility current) continue;

            bool sameInAll = true;
            Coloring col = element.Value;

            for (int i = 1; i < colorings.Length && sameInAll; i++)
            {
                if (!colorings[i].TryGetValue(current, out var c) || c != col)
                {
                    sameInAll = false;
                    break;
                }
            }

            if (sameInAll)
            {
                if (col == Coloring.On)
                {
                    view.ChangeBuffer.ProposeSolutionAddition(current.Possibility, current.Row, current.Column);
                    if (view.ChangeBuffer.NotEmpty() && view.ChangeBuffer.Commit(
                            new UnitForcingNetReportBuilder(colorings, current, Coloring.On, view.PreComputer.Graphs.ComplexLinkGraph)) &&
                                StopOnFirstPush) return true;
                }
                else
                {
                    view.ChangeBuffer.ProposePossibilityRemoval(current.Possibility, current.Row, current.Column);
                    if (view.ChangeBuffer.NotEmpty() && view.ChangeBuffer.Commit(
                            new UnitForcingNetReportBuilder(colorings, current, Coloring.Off, view.PreComputer.Graphs.ComplexLinkGraph)) &&
                                StopOnFirstPush) return true;
                }
            }
        }

        return false;
    }
}

public class UnitForcingNetReportBuilder : IChangeReportBuilder<IUpdatableSudokuSolvingState, ISudokuHighlighter>
{
    private readonly ColoringDictionary<ISudokuElement>[] _colorings;
    private readonly CellPossibility _target;
    private readonly Coloring _targetColoring;
    private readonly ILinkGraph<ISudokuElement> _graph;

    public UnitForcingNetReportBuilder(ColoringDictionary<ISudokuElement>[] colorings, CellPossibility target, Coloring targetColoring, ILinkGraph<ISudokuElement> graph)
    {
        _colorings = colorings;
        _target = target;
        _targetColoring = targetColoring;
        _graph = graph;
    }

    public ChangeReport<ISudokuHighlighter> Build(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        var highlights = new Highlight<ISudokuHighlighter>[_colorings.Length];
        var paths = new List<LinkGraphChain<ISudokuElement>>[_colorings.Length];

        for (int i = 0; i < _colorings.Length; i++)
        {
            paths[i] = ForcingNetsUtility.FindEveryNeededPaths(_colorings[i].History!.GetPathToRootWithGuessedLinks(_target,
                _targetColoring), _colorings[i], _graph, snapshot);
            
            var iForDelegate = i;
            highlights[i] = lighter =>
            {
                ForcingNetsUtility.HighlightAllPaths(lighter, paths[iForDelegate], Coloring.On);
                
                if (paths[iForDelegate][0].Elements[0] is CellPossibility start) lighter.EncirclePossibility(start);
                ChangeReportHelper.HighlightChanges(lighter, changes);
            };
        }
        
        return new ChangeReport<ISudokuHighlighter>( "", highlights);
    }
}