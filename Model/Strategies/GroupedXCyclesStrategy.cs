using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media.Animation;
using Model.StrategiesUtil;

namespace Model.Strategies;

public class GroupedXCyclesStrategy : IStrategy
{
    public string Name => "XCycles";
    public StrategyLevel Difficulty => StrategyLevel.Hard;
    public int Score { get; set; }

    public void ApplyOnce(ISolverView solverView)
    {
        for (int n = 0; n <= 9; n++)
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if(!solverView.Possibilities[row, col].Peek(n)) continue;
                    INode start = new SingleNode(row, col);
                    StartSearch(solverView, start, n);
                }
            }
        }
    }

    private void StartSearch(ISolverView view, INode start, int number)
    {
        var visited = new HashSet<INode>();
        if (visited.Contains(start)) return;
        visited.Add(start);
        foreach (var next in start.EachStrongLink(view, number))
        {
            if (!visited.Contains(next))
            {
                Search(view, next, visited, start.ForStringChain() + "=" + next.ForStringChain(), number);
            }
        }
        
        foreach (var next in start.EachWeakLink(view, number))
        {
            if (!visited.Contains(next))
            {
                Search(view, next, visited, start.ForStringChain() + "-" + next.ForStringChain(), number);
            }
        }
    }

    

    private void Search(ISolverView view, INode current, HashSet<INode> visited, string chain, int number)
    {
        bool lastWasStrong = StringChain.WasLastStrong(chain);
        visited.Add(current);
        foreach (var next in current.EachStrongLink(view, number))
        {
            if (!visited.Contains(next))
            {
                Search(view, next, visited, chain + "=" + next.ForStringChain(), number);
            }
            else
            {
                LoopFound(view, next, chain, true, number);
            }
        }

        foreach (var next in current.EachWeakLink(view, number))
        {
            if (!visited.Contains(next))
            {
                if(lastWasStrong) Search(view, next, visited, chain + "-" + next.ForStringChain(), number);
            }
            else
            {
                LoopFound(view, next, chain, false, number);
            }
        }
    }

    private void LoopFound(ISolverView view, INode to, string chain, bool lastLinkWasStrong, int number)
    {
        var finalChain = StringChain.Cut(to, chain);
        if (finalChain is null) return;
        int count = StringChain.NumberOfNodes(finalChain);
        if (count < 4) return;

        if (count % 2 == 0)
        {
            ProcessEvenLoop(view, chain, lastLinkWasStrong, number);
        }
        else
        {
            ProcessUnevenLoop(view, chain, lastLinkWasStrong, number);
        }
    }

    private void ProcessEvenLoop(ISolverView view, string chain, bool lastLinkWasStrong, int number)
    {
        //Search for double weak
        bool lastWasWeak = !lastLinkWasStrong;
        foreach (var c in chain)
        {
            if (c == '-')
            {
                if (lastWasWeak) return;
                lastWasWeak = true;
            }
            else if (c == '=')
            {
                lastWasWeak = false;
            }
        }
        if (lastWasWeak && !lastLinkWasStrong) return;
        
        //Process weak
    }
    
    private void ProcessUnevenLoop(ISolverView view, string chain, bool lastLinkWasStrong, int number)
    {
        
    }

}

public class NodeLinks
{
    public HashSet<INode> StrongLinks { get; } = new();
    public HashSet<INode> WeakLinks { get; } = new();
}

public interface INode
{
    public string ForStringChain();

    public IEnumerable<INode> EachStrongLink(ISolverView view, int number);
    public IEnumerable<INode> EachWeakLink(ISolverView view, int number);
}

public class SingleNode : Coordinate, INode
{
    public SingleNode(int row, int col) : base(row, col) { }
    
    public string ForStringChain()
    {
        return ToString();
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Row, Col);
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is not SingleNode coord) return false;
        return Row == coord.Row && Col == coord.Col;
    }

    public IEnumerable<INode> EachStrongLink(ISolverView view, int number)
    {
        HashSet<INode> result = new();
        //Rows
        var ppir = view.PossibilityPositionsInRow(Row, number);
        if (ppir.Count == 2)
        {
            foreach (var col in ppir)
            {
                if (col != Col)
                {
                    result.Add(new SingleNode(Row, col));
                    break;
                }
            }
        }
        
        //Rows
        var ppic = view.PossibilityPositionsInColumn(Col, number);
        if (ppic.Count == 2)
        {
            foreach (var row in ppic)
            {
                if (row != Row)
                {
                    result.Add(new SingleNode(row, Col));
                    break;
                }
            }
        }
        
        //Rows
        var ppimn = view.PossibilityPositionsInMiniGrid(Row / 3, Col / 3, number);
        if (ppimn.Count == 2)
        {
            foreach (var pos in ppimn)
            {
                if (!(pos[0] == Row / 3 && pos[1] / 3 == Col / 3))
                {
                    result.Add(new SingleNode(pos[0], pos[1]));
                    break;
                }
            }
        }

        return result;
    }

    public IEnumerable<INode> EachWeakLink(ISolverView view, int number)
    {
        HashSet<INode> result = new();
        //Rows
        var ppir = view.PossibilityPositionsInRow(Row, number);
        if (ppir.Count > 2)
        {
            foreach (var col in ppir)
            {
                if (col != Col)
                {
                    result.Add(new SingleNode(Row, col));
                }
            }
        }
        
        //Rows
        var ppic = view.PossibilityPositionsInColumn(Col, number);
        if (ppic.Count > 2)
        {
            foreach (var row in ppic)
            {
                if (row != Row)
                {
                    result.Add(new SingleNode(row, Col));
                }
            }
        }
        
        //Rows
        var ppimn = view.PossibilityPositionsInMiniGrid(Row / 3, Col / 3, number);
        if (ppimn.Count > 2)
        {
            foreach (var pos in ppimn)
            {
                if (!(pos[0] == Row / 3 && pos[1] / 3 == Col / 3))
                {
                    result.Add(new SingleNode(pos[0], pos[1]));
                }
            }
        }

        return result;
    }
}

public static class StringChain
{
    public static int NumberOfNodes(string s)
    {
        int result = 1;

        foreach (var c in s)
        {
            if (c == '-' || c == '=') result++;
        }

        return result;
    }

    public static string? Cut(INode from, string chain)
    {
        int index = chain.IndexOf(from.ForStringChain(), StringComparison.Ordinal);
        if (index < 0) return null;
        return chain[index..];
    }

    public static bool WasLastStrong(string chain)
    {
        int weakIndex = chain.LastIndexOf('-');
        int strongIndex = chain.LastIndexOf('=');

        return strongIndex > weakIndex;
    }
}