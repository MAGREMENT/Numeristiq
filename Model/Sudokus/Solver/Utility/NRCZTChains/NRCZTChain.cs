using System.Collections;
using System.Collections.Generic;
using System.Text;
using Model.Utility;

namespace Model.Sudokus.Solver.Utility.NRCZTChains;

public class NRCZTChain : IEnumerable<ConjugateRelation> //TODO to interface with another class where possible targets is computed and not kept in memory
{
    private readonly List<ConjugateRelation> _relations = new();
    public HashSet<CellPossibility> PossibleTargets { get; } = new();

    public int Count => _relations.Count;

    public NRCZTChain(ISudokuSolverData solverData, CellPossibility from, CellPossibility to)
    {
        _relations.Add(new ConjugateRelation(from, to));
        PossibleTargets.UnionWith(SudokuCellUtility.SeenExistingPossibilities(solverData, from));
        PossibleTargets.Remove(to);
    }

    private NRCZTChain(List<ConjugateRelation> relations, ConjugateRelation relation, HashSet<CellPossibility> possibleTargets)
    {
        _relations = new List<ConjugateRelation>(relations) { relation };
        PossibleTargets = new HashSet<CellPossibility>(possibleTargets);
        PossibleTargets.Remove(relation.From);
        PossibleTargets.Remove(relation.To);
    }
    
    private NRCZTChain(List<ConjugateRelation> relations, ConjugateRelation relation, HashSet<CellPossibility> possibleTargets,
        CellPossibility mustSee)
    {
        _relations = new List<ConjugateRelation>(relations) { relation };
        PossibleTargets = new HashSet<CellPossibility>(possibleTargets);
        PossibleTargets.Remove(relation.From);
        PossibleTargets.Remove(relation.To);
        PossibleTargets.RemoveWhere(cp => !SudokuCellUtility.AreLinked(cp, mustSee));
    }

    public NRCZTChain? TryAdd(CellPossibility from, CellPossibility to)
    {
        if (Contains(to)) return null;

        return new NRCZTChain(_relations, new ConjugateRelation(from, to), PossibleTargets);
    }
    
    public NRCZTChain? TryAdd(CellPossibility from, CellPossibility to, CellPossibility mustSee)
    {
        if (Contains(to, mustSee)) return null;

        var chain = new NRCZTChain(_relations, new ConjugateRelation(from, to), PossibleTargets, mustSee);
        return chain.PossibleTargets.Count == 0 ? null : chain;
    }

    public bool Contains(CellPossibility cp)
    {
        foreach (var relation in _relations)
        {
            if (relation.From == cp || relation.To == cp) return true;
        }

        return false;
    }
    
    public bool Contains(CellPossibility cp1, CellPossibility cp2)
    {
        foreach (var relation in _relations)
        {
            if (relation.From == cp1 || relation.To == cp1 || relation.From == cp2 || relation.To == cp2) return true;
        }

        return false;
    }

    public CellPossibility Last() => _relations[^1].To;

    public IEnumerator<ConjugateRelation> GetEnumerator()
    {
        return _relations.GetEnumerator();
    }

    public override string ToString()
    {
        if (_relations.Count == 0) return "";
        
        var builder = new StringBuilder();
        for (int i = 0; i < _relations.Count - 1; i++)
        {
            builder.Append(_relations[i] + " - ");
        }

        builder.Append(_relations[^1].ToString());

        return builder.ToString();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}