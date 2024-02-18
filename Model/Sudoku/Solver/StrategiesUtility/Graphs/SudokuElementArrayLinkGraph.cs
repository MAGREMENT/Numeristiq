using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Model.Utility;

namespace Model.Sudoku.Solver.StrategiesUtility.Graphs;

public class SudokuElementArrayLinkGraph<T> : ILinkGraph<ISudokuElement>
{
    private UniqueList<ISudokuElement>?[,,,] _cps = new UniqueList<ISudokuElement>[9, 9, 9, 2];
    private readonly Dictionary<ISudokuElement, UniqueList<ISudokuElement>[]> _others = new();
    
    public void Add(ISudokuElement from, ISudokuElement to, LinkStrength strength, LinkType type = LinkType.BiDirectional)
    {
        AddMono(from, to, strength);
        if (type == LinkType.BiDirectional) AddMono(to, from, strength);
    }

    public IEnumerable<ISudokuElement> Neighbors(ISudokuElement from, LinkStrength strength)
    {
        if (strength == LinkStrength.Any) return Neighbors(from);
        
        if (from is CellPossibility cp) return _cps[cp.Row, cp.Column, cp.Possibility - 1, (int)strength] ?? Enumerable.Empty<ISudokuElement>();
        
        return _others.TryGetValue(from, out var resume) ? resume[(int)strength - 1] : Enumerable.Empty<ISudokuElement>();
    }

    public IEnumerable<ISudokuElement> Neighbors(ISudokuElement from)
    {
        if (from is CellPossibility cp)
        {
            foreach (var n in _cps[cp.Row, cp.Column, cp.Possibility - 1, 0] ?? Enumerable.Empty<ISudokuElement>())
            {
                yield return n;
            }
            
            foreach (var n in _cps[cp.Row, cp.Column, cp.Possibility - 1, 1] ?? Enumerable.Empty<ISudokuElement>())
            {
                yield return n;
            }
        }
        else
        {
            if (!_others.TryGetValue(from, out var resume)) yield break;
        
            foreach (var friend in resume[0])
            {
                yield return friend;
            }
            
            foreach (var friend in resume[1])
            {
                yield return friend;
            }
        }
    }
 
    public bool AreNeighbors(ISudokuElement from, ISudokuElement to, LinkStrength strength)
    {
        if (from is not CellPossibility cp)
            return _others.TryGetValue(from, out var resume) && resume[(int)strength - 1].Contains(to);
        
        var l = _cps[cp.Row, cp.Column, cp.Possibility - 1, (int)strength];
        return l is not null && l.Contains(to);
    }

    public bool AreNeighbors(ISudokuElement from, ISudokuElement to)
    {
        if (from is not CellPossibility cp) return _others.TryGetValue(from, out var resume) 
                                                   && (resume[0].Contains(to) || resume[1].Contains(to));
        
        var l = _cps[cp.Row, cp.Column, cp.Possibility - 1, 0];
        if (l is not null && l.Contains(to)) return true;
        
        l = _cps[cp.Row, cp.Column, cp.Possibility - 1, 1];
        return l is not null && l.Contains(to);
    }

    public void Clear()
    {
        _cps = new UniqueList<ISudokuElement>[9, 9, 9, 2];
        _others.Clear();
    }
    
    public IEnumerator<ISudokuElement> GetEnumerator()
    {
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                for (int p = 0; p < 9; p++)
                {
                    if (_cps[r, c, p, 0] is not null || _cps[r, c, p, 1] is not null)
                        yield return new CellPossibility(r, c, p + 1);
                }
            }
        }

        foreach (var key in _others.Keys)
        {
            yield return key;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    //Private-----------------------------------------------------------------------------------------------------------
    
    private void AddMono(ISudokuElement from, ISudokuElement to, LinkStrength strength)
    {
        if (from is CellPossibility cp)
        {
            _cps[cp.Row, cp.Column, cp.Possibility - 1, (int)strength] ??= new UniqueList<ISudokuElement>();
            _cps[cp.Row, cp.Column, cp.Possibility - 1, (int)strength]!.Add(to);
        }
        else
        {
            if (!_others.TryGetValue(from, out var resume))
            {
                resume = new[] { new UniqueList<ISudokuElement>(), new UniqueList<ISudokuElement>() };
                _others[from] = resume;
            }
            resume[(int)strength - 1].Add(to);
        }
    }
}