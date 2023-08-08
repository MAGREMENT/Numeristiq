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
                            strategyManager.RemovePossibility(possibility, row, col, this);
                            break;
                        }
                    }
                    
                    cs.Reset();
                }
            }
        }
    }

    private bool Search(ContradictionSearcher cs, LinkGraph<ILinkGraphElement> graph, Dictionary<ILinkGraphElement, Coloring> result,
        ILinkGraphElement current)
    {
        Coloring opposite = result[current] == Coloring.On ? Coloring.Off : Coloring.On;

        foreach (var friend in graph.GetLinks(current, LinkStrength.Strong))
        {
            if (!result.ContainsKey(friend))
            {
                if (opposite == Coloring.Off && friend is PossibilityCoordinate pos && cs.AddOff(pos)) return true;
                result[friend] = opposite;
                if (Search(cs, graph, result, friend)) return true;
            }
            else if (result[friend] != opposite) return true;
        }

        if (opposite == Coloring.Off)
        {
            foreach (var friend in graph.GetLinks(current, LinkStrength.Weak))
            {
                if (!result.ContainsKey(friend))
                {
                    if (friend is PossibilityCoordinate pos && cs.AddOff(pos)) return true;
                    result[friend] = opposite;
                    if (Search(cs, graph, result, friend)) return true;
                }
                else if (result[friend] != opposite) return true;
            }
        }
        else if (current is PossibilityCoordinate pos)
        {
            PossibilityCoordinate? row = null;
            bool rowB = true;
            PossibilityCoordinate? col = null;
            bool colB = true;
            PossibilityCoordinate? mini = null;
            bool miniB = true;
            
            foreach (var friend in graph.GetLinks(current, LinkStrength.Weak))
            {
                if (friend is not PossibilityCoordinate friendPos) continue;
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
                Search(cs, graph, result, row);
            }

            if (col is not null && colB)
            {
                result[col] = Coloring.On;
                Search(cs, graph, result, col);
            }

            if (mini is not null && miniB)
            {
                result[mini] = Coloring.On;
                Search(cs, graph, result, mini);
            }
        }

        return false;
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