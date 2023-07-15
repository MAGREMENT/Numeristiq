using System.Collections.Generic;
using Model.Possibilities;

namespace Model;

public interface ISolver
{
    bool AddDefinitiveNumber(int number, int row, int col, ISolverLog? log = null);

    bool RemovePossibility(int possibility, int row, int col, ISolverLog? log = null);

    Positions PossibilityPositionsInColumn(int col, int number);

    Positions PossibilityPositionsInRow(int row, int number);

    List<int[]> PossibilityPositionsInMiniGrid(int miniRow, int miniCol, int number);

    public Sudoku Sudoku { get; }

    public IPossibilities[,] Possibilities { get; }
    
    public List<ISolverLog> Logs { get; }
}

public interface IPossibilities
{
    public const int Min = 1;
    public const int Max = 9;
    
    public int Count { get; }
    public bool Remove(int n);
    public void RemoveAll();
    public void RemoveAll(params int[] except);
    public void RemoveAll(IEnumerable<int> except);
    public IPossibilities Mash(IPossibilities possibilities);
    public bool Peek(int n);
    public IEnumerable<int> All();
    public int GetFirst();
    public IPossibilities Copy();

    public static IPossibilities DefaultMash(IPossibilities poss1, IPossibilities poss2)
    {
        IPossibilities result = new BoolArrayPossibilities();
        for (int i = Min; i <= Max; i++)
        {
            if (!poss1.Peek(i) && !poss2.Peek(i)) result.Remove(i);
        }

        return result;
    }
}

public class Positions
{
    private int _pos;
    public int Count { private set; get; }
    
    public Positions(){}

    private Positions(int pos, int count)
    {
        _pos = pos;
        Count = count;
    }

    public void Add(int pos)
    {
        _pos |= 1 << pos;
        Count++;
    }

    public bool Peek(int pos)
    {
        return ((_pos >> pos) & 1) > 0;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Positions pos) return false;
        return _pos == pos._pos;
    }

    public override int GetHashCode()
    {
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return _pos;
    }

    public IEnumerable<int> All()
    {
        for (int i = 0; i < 9; i++)
        {
            if(((_pos >> i) & 1) > 0) yield return i;
        }
    }

    public Positions Mash(Positions pos)
    {
        int newPos = _pos | pos._pos;
        return new Positions(newPos, System.Numerics.BitOperations.PopCount((uint)newPos));
    }

    public Positions Copy()
    {
        return new Positions(_pos, Count);
    }
}

public interface ISolverLog
{
    public string AsString { get; }
    public StrategyLevel Level { get; }
}

public class BasicNumberAddedLog : ISolverLog
{
    public string AsString { get; }
    public StrategyLevel Level => StrategyLevel.None;

    public BasicNumberAddedLog(int number, int row, int col)
    {
        AsString = $"[{row + 1}, {col + 1}] {number} added as definitive";
    }

}

public class BasicPossibilityRemovedLog : ISolverLog
{
    public string AsString { get; }
    public StrategyLevel Level => StrategyLevel.None;

    public BasicPossibilityRemovedLog(int number, int row, int col)
    {
        AsString = $"[{row + 1}, {col + 1}] {number} removed from possibilities";
    }

}