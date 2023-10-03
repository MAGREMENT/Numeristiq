using System.Collections.Generic;
using System.Linq;
using Model.Solver.Helpers.Changes;
using Model.Solver.Possibilities;
using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver.Strategies.ForcingNets;

public class CellForcingNetStrategy : AbstractStrategy
{
    public const string OfficialName = "Cell Forcing Net";
    
    private readonly int _max;

    public CellForcingNetStrategy(int maxPossibilities) : base(OfficialName,  StrategyDifficulty.Extreme)
    {
        _max = maxPossibilities;
    }

    public override void ApplyOnce(IStrategyManager strategyManager)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (strategyManager.PossibilitiesAt(row, col).Count < 2 ||
                    strategyManager.PossibilitiesAt(row, col).Count > _max) continue;
                var possAsArray = strategyManager.PossibilitiesAt(row, col).ToArray();

                Dictionary<ILinkGraphElement, Coloring>[] colorings =
                    new Dictionary<ILinkGraphElement, Coloring>[possAsArray.Length];

                for (int i = 0; i < possAsArray.Length; i++)
                {
                    colorings[i] = strategyManager.PreComputer.OnColoring(row, col, possAsArray[i]);
                }

                Process(strategyManager, colorings, new Cell(row, col));

                if (strategyManager.ChangeBuffer.NotEmpty()) strategyManager.ChangeBuffer.Push(this,
                        new CellForcingNetReportBuilder(colorings, row, col));
            }
        }
    }

    private void Process(IStrategyManager view, Dictionary<ILinkGraphElement, Coloring>[] colorings, Cell current)
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
                    view.ChangeBuffer.AddSolutionToAdd(cell.Possibility, cell.Row, cell.Col);
                else view.ChangeBuffer.AddPossibilityToRemove(cell.Possibility, cell.Row, cell.Col);
            }
        }
        
        //Not yet proven useful so bye bye for now
        /*HashSet<int> count = new HashSet<int>(colorings.Length);

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (current.Row == row && current.Col == col) continue;
                
                IPossibilities on = IPossibilities.NewEmpty();

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

                        view.ChangeBuffer.AddPossibilityToRemove(number, row, col);
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

                        view.ChangeBuffer.AddPossibilityToRemove(number, row, col);
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

                            view.ChangeBuffer.AddPossibilityToRemove(number, cell.Row, cell.Col);
                        }
                    }
                    count.Clear();
                }
            }
        }*/
    }

    private void RemoveAll(IStrategyManager view, int row, int col, IPossibilities except)
    {
        foreach (var possibility in view.PossibilitiesAt(row, col))
        {
            if(except.Peek(possibility)) continue;
            view.ChangeBuffer.AddPossibilityToRemove(possibility, row, col);
        }
    }
}

public class CellForcingNetReportBuilder : IChangeReportBuilder
{
    private readonly Dictionary<ILinkGraphElement, Coloring>[] _colorings;
    private readonly int _row;
    private readonly int _col;

    public CellForcingNetReportBuilder(Dictionary<ILinkGraphElement, Coloring>[] colorings, int row, int col)
    {
        _colorings = colorings;
        _row = row;
        _col = col;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        HighlightSolver[] highlights = new HighlightSolver[_colorings.Length + 1];
        highlights[0] = lighter =>
        {
            IChangeReportBuilder.HighlightChanges(lighter, changes);
            lighter.CircleCell(_row, _col);
        };

        for (int i = 0; i < _colorings.Length; i++)
        {
            var filtered = ForcingNetsUtil.FilterPossibilityCoordinates(_colorings[i]);
            highlights[i + 1] = lighter =>
            {
                ForcingNetsUtil.HighlightColoring(lighter, filtered);
                lighter.CircleCell(_row, _col);
            };
        }
        
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", highlights);
    }
}