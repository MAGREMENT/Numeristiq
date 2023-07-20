using System.Collections.Generic;
using System.Linq;
using Model.StrategiesUtil;

namespace Model.Strategies;

public class AlternatingInferenceChainStrategyV2 : IStrategy //TODO fixme
{
    public string Name { get; } = "Alternating inference chain";
    
    public StrategyLevel Difficulty { get; } = StrategyLevel.Extreme;
    public long SearchCount { get; private set; }

    public void ApplyOnce(ISolverView solverView)
    {
        Dictionary<PossibilityCoordinate, LinkResume> map = new();

        SearchLinks(solverView, map);
        HashSet<PossibilityCoordinate> explored = new();

        foreach (var start in map.Keys)
        {
            if (!explored.Contains(start)) Search(solverView, map, new Chain(start), explored);
        }
    }

    private bool Search(ISolverView solverView, Dictionary<PossibilityCoordinate, LinkResume> map,
        Chain chain, HashSet<PossibilityCoordinate> explored)
    {
        var last = chain.Last();
        var resume = map[last];
        explored.Add(last);

        SearchCount++;

        var lastLink = chain.LastLink();
        if (!(lastLink == 1 && chain.HasDoubleStrong))
        {
            foreach (var stronk in resume.StrongLinks)
            {
                int contains = chain.Contains(stronk);
                if (contains == -1)
                {
                    Chain copy = chain.Copy();
                    copy.AddLinkTo(true, stronk);
                    if(Search(solverView, map, copy, explored))
                        return true;
                }
                else if(chain.Count - contains >= 4)
                {
                    if (ProcessChain(solverView, chain.Cut(contains), 1)) return true;
                }
            }
        }

        if (!(lastLink == 0 && chain.HasDoubleWeak))
        {
            foreach (var wiq in resume.StrongLinks)
            {
                int contains = chain.Contains(wiq);
                if (contains == -1)
                {
                    if(chain.LastLink() == 0 && chain.HasDoubleWeak) continue;
                    Chain copy = chain.Copy();
                    copy.AddLinkTo(false, wiq);
                    if(Search(solverView, map, copy, explored))
                        return true;
                }
                else if(chain.Count - contains >= 4)
                {
                    if (ProcessChain(solverView, chain.Cut(contains), 0)) return true;
                }
            }
        }
        

        return false;
    }

    private bool ProcessChain(ISolverView view, Chain chain, int lastLink)
    {
        var result = chain.Process(lastLink);
        switch (result.Type)
        {
            case ChainProcessType.DoubleStrong :
                if (RemoveAllExcept(view, result.Coordinate!.Row,
                        result.Coordinate.Col, result.Coordinate.Possibility)) return true;
                break;
            case ChainProcessType.DoubleWeak :
                if (view.RemovePossibility(result.Coordinate!.Possibility, result.Coordinate.Row, result.Coordinate.Col,
                        this)) return true;
                break;
            case ChainProcessType.Perfect :
                bool wasProgressMade = false;
                chain.ForEachWeakLink(lastLink, (one, two) =>
                {
                    if (one.Possibility == two.Possibility)
                    {
                        foreach (var coord in one.SharedSeenCells(two))
                        {
                            if ((coord.Row == one.Row && coord.Col == one.Col) ||
                                (coord.Row == two.Row && coord.Col == two.Col)) continue;
                            if (view.RemovePossibility(one.Possibility, coord.Row, coord.Col, this))
                                wasProgressMade = true;
                        }
                    }
                    else if (one.Row == two.Row && one.Col == two.Col)
                    {
                        wasProgressMade = RemoveAllExcept(view, one.Row, one.Col, one.Possibility, two.Possibility);
                    }
                });
                if (wasProgressMade) return true;
                break;
        }

        return false;
    }

    private bool RemoveAllExcept(ISolverView solverView, int row, int col, params int[] except)
    {
        var wasProgressMade = false;
        foreach (var possibility in solverView.Possibilities[row, col])
        {
            if (!except.Contains(possibility))
            {
                if (solverView.RemovePossibility(possibility, row, col, this))
                    wasProgressMade = true;
            }
        }

        return wasProgressMade;
    }

    private void SearchLinks(ISolverView solverView, Dictionary<PossibilityCoordinate, LinkResume> map)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                foreach (var possibility in solverView.Possibilities[row, col])
                {
                    LinkResume resume = new();

                    //Row
                    var ppir = solverView.PossibilityPositionsInRow(row, possibility);
                    foreach (var c in ppir)
                    {
                        if (c != col)
                        {
                            var coord = new PossibilityCoordinate(row, c, possibility);
                            if (ppir.Count == 2) resume.StrongLinks.Add(coord);
                            resume.WeakLinks.Add(coord);
                        }
                    }


                    //Col
                    var ppic = solverView.PossibilityPositionsInColumn(col, possibility);
                    foreach (var r in ppic)
                    {
                        if (r != row)
                        {
                            var coord = new PossibilityCoordinate(r, col, possibility);
                            if(ppic.Count == 2) resume.StrongLinks.Add(coord);
                            resume.WeakLinks.Add(coord);
                        }
                    }
                    


                    //MiniGrids
                    var ppimn = solverView.PossibilityPositionsInMiniGrid(row / 3, col / 3, possibility);
                    foreach (var pos in ppimn)
                    {
                        if (!(pos[0] == row && pos[1] == col))
                        {
                            var coord = new PossibilityCoordinate(pos[0], pos[1], possibility);
                            if (ppimn.Count == 2) resume.StrongLinks.Add(coord);
                            resume.WeakLinks.Add(coord);
                        }
                    }

                   
                    foreach (var pos in solverView.Possibilities[row, col])
                    {
                        if (pos != possibility)
                        {
                            var coord = new PossibilityCoordinate(row, col, pos);
                            if(solverView.Possibilities[row, col].Count == 2) resume.StrongLinks.Add(coord);
                            resume.WeakLinks.Add(coord);
                        }
                    }

                    map.Add(new PossibilityCoordinate(row, col, possibility), resume);
                }
            }
        }
    }
}

public class Chain
{
    private readonly List<int> _chain = new();
    public int Count { get; private set; }

    public bool HasDoubleWeak { get; private set; }

    public bool HasDoubleStrong { get; private set; }

    public Chain(PossibilityCoordinate first)
    {
        _chain.Add(first.Row);
        _chain.Add(first.Col);
        _chain.Add(first.Possibility);
        Count++;
    }

    private Chain(List<int> chain, int count, bool doubleStrong, bool doubleWeak)
    {
        _chain = new List<int>(chain);
        Count = count;
        HasDoubleStrong = doubleStrong;
        HasDoubleWeak = doubleWeak;
    }

    public PossibilityCoordinate Last()
    {
        return new PossibilityCoordinate(_chain[^3], _chain[^2], _chain[^1]);
    }

    public int LastLink()
    {
        if (Count < 2) return -1;
        return _chain[^4];
    }

    public void AddLinkTo(bool stronk, PossibilityCoordinate to)
    {
        int lastLink = LastLink();
        _chain.Add(stronk ? 1 : 0);
        _chain.Add(to.Row);
        _chain.Add(to.Col);
        _chain.Add(to.Possibility);
        Count++;
        
        if (stronk && lastLink == 1) HasDoubleStrong = true;
        else if (!stronk && lastLink == 0) HasDoubleWeak = true;
    }

    public int Contains(PossibilityCoordinate coord)
    {
        int index = 0;
        for (int i = 0; i < _chain.Count - 2; i += 4)
        {
            if (_chain[i] == coord.Row && _chain[i + 1] == coord.Col && _chain[i + 2] == coord.Possibility)
                return index;
            index++;
        }

        return -1;
    }

    public Chain Copy()
    {
        return new Chain(_chain, Count, HasDoubleStrong, HasDoubleWeak);
    }

    public Chain Cut(int index)
    {
        int n = index * 4;
        return new Chain(_chain.GetRange(n, _chain.Count - n), Count - index, HasDoubleStrong, HasDoubleWeak);
    }

    public ChainProcessResult Process(int lastLink)
    {
        //edgecase
        if (_chain[0] == _chain[3] && _chain[0] == lastLink) return new ChainProcessResult(ChainProcessType.Unusable);
        
        int last = -1;
        for (int i = 3; i < _chain.Count; i += 4)
        {
            int current = _chain[i];
            if (current == last)
                return new ChainProcessResult(
                    current == 1 ? ChainProcessType.DoubleStrong : ChainProcessType.DoubleWeak,
                    new PossibilityCoordinate(_chain[i - 3], _chain[i - 2], _chain[i - 1]));
            last = current;
        }

        if (last == lastLink)
        {
            return new ChainProcessResult(
                last == 1 ? ChainProcessType.DoubleStrong : ChainProcessType.DoubleWeak, Last());
        }

        return new ChainProcessResult(ChainProcessType.Perfect);
    }

    public delegate void LinkHandler(PossibilityCoordinate one, PossibilityCoordinate two);
    
    public void ForEachWeakLink(int lastLink, LinkHandler handler)
    {
        for (int i = 3; i < _chain.Count - 3; i += 4)
        {
            if (_chain[i] == 0)
            {
                handler(new PossibilityCoordinate(_chain[i - 3], _chain[i - 2], _chain[i - 1]),
                    new PossibilityCoordinate(_chain[i + 1], _chain[i + 2], _chain[i + 3]));
            }
        }

        if (lastLink == 0) handler(new PossibilityCoordinate(_chain[0], _chain[1],
            _chain[2]), Last());
    }

    public override string ToString()
    {
        string result = "";
        for (int i = 0; i < _chain.Count - 2; i += 3)
        {
            result += $"[{_chain[i]}, {_chain[i + 1]} => {_chain[i + 2]}]";
            if (i + 3 < _chain.Count)
            {
                result += _chain[i + 3] == 1 ? "=" : "-";
                i++;
            }
        }

        return result;
    }
}

public class ChainProcessResult
{
    public ChainProcessResult(ChainProcessType type, PossibilityCoordinate? coordinate = null)
    {
        Coordinate = coordinate;
        Type = type;
    }

    public PossibilityCoordinate? Coordinate { get; }
    public ChainProcessType Type { get; }
}

public enum ChainProcessType
{
    Unusable, Perfect, DoubleStrong, DoubleWeak
}