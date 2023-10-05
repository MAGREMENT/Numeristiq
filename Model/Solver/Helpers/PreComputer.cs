using System.Collections.Generic;
using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.CellColoring;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver.Helpers;

public class PreComputer
{
    private readonly IStrategyManager _view;

    private List<AlmostLockedSet>? _als;

    private readonly Dictionary<ILinkGraphElement, Coloring>?[,,] _onColoring
        = new Dictionary<ILinkGraphElement, Coloring>[9, 9, 9];
    private bool _wasPreColorUsed;

    public PreComputer(IStrategyManager view)
    {
        _view = view;
    }

    public void Reset()
    {
        _als = null;
        
        if (_wasPreColorUsed)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    for (int k = 0; k < 9; k++)
                    {
                        _onColoring[i, j, k] = null;
                    }
                }
            }
            _wasPreColorUsed = false;
        }
    }

    public List<AlmostLockedSet> AlmostLockedSets()
    {
        _als ??= DoAlmostLockedSets();
        return _als;
    }

    public Dictionary<ILinkGraphElement, Coloring> OnColoring(int row, int col, int possibility)
    {
        _wasPreColorUsed = true;

        _onColoring[row, col, possibility - 1] ??=
            DoColor(new CellPossibility(row, col, possibility), Coloring.On);
        return _onColoring[row, col, possibility - 1]!;
    }
    
    public Dictionary<ILinkGraphElement, Coloring> OffColoring(int row, int col, int possibility)
    {
        return DoColor(new CellPossibility(row, col, possibility), Coloring.Off);
    }

    private List<AlmostLockedSet> DoAlmostLockedSets()
    {
        return AlmostLockedSetSearcher.FullGrid(_view);
    }

    private Dictionary<ILinkGraphElement, Coloring> DoColor(ILinkGraphElement start, Coloring firstColor)
    {
        _view.GraphManager.Construct();
        var graph = _view.GraphManager.LinkGraph;

        Queue<ILinkGraphElement> queue = new();
        queue.Enqueue(start);

        Dictionary<ILinkGraphElement, Coloring> result = new() { { start, firstColor } };

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            Coloring opposite = result[current] == Coloring.On ? Coloring.Off : Coloring.On;

            foreach (var friend in graph.GetLinks(current, LinkStrength.Strong))
            {
                if (!result.ContainsKey(friend))
                {
                    result[friend] = opposite;
                    queue.Enqueue(friend);
                }
            }

            if (opposite == Coloring.Off)
            {
                foreach (var friend in graph.GetLinks(current, LinkStrength.Weak))
                {
                    if (!result.ContainsKey(friend))
                    {
                        result[friend] = opposite;
                        queue.Enqueue(friend);
                    }
                }
            }
            else if (current is CellPossibility pos)
            {
                CellPossibility? row = null;
                bool rowB = true;
                CellPossibility? col = null;
                bool colB = true;
                CellPossibility? mini = null;
                bool miniB = true;
            
                foreach (var friend in graph.GetLinks(current, LinkStrength.Weak))
                {
                    if (friend is not CellPossibility friendPos) continue;
                    if (rowB && friendPos.Row == pos.Row)
                    {
                        if (result.TryGetValue(friend, out var coloring))
                        {
                            if (coloring == Coloring.On) rowB = false;
                        }
                        else
                        {
                            if (row is null) row = friendPos;
                            else rowB = false;  
                        }
                    
                    }

                    if (colB && friendPos.Col == pos.Col)
                    {
                        if (result.TryGetValue(friend, out var coloring))
                        {
                            if (coloring == Coloring.On) colB = false;
                        }
                        else
                        {
                            if (col is null) col = friendPos;
                            else colB = false;
                        }
                    }

                    if (miniB && friendPos.Row / 3 == pos.Row / 3 && friendPos.Col / 3 == pos.Col / 3)
                    {
                        if (result.TryGetValue(friend, out var coloring))
                        {
                            if (coloring == Coloring.On) miniB = false;
                        }
                        else
                        {
                            if (mini is null) mini = friendPos;
                            else miniB = false;
                        }
                    }
                }

                if (row is not null && rowB)
                {
                    result[row] = Coloring.On;
                    queue.Enqueue(row);
                }

                if (col is not null && colB)
                {
                    result[col] = Coloring.On;
                    queue.Enqueue(col);
                }

                if (mini is not null && miniB)
                {
                    result[mini] = Coloring.On;
                    queue.Enqueue(mini);
                }
            }
        }

        return result;
    }
}