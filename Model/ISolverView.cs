using System.Collections.Generic;
using Model.Positions;
using Model.Possibilities;

namespace Model;

public interface ISolverView
{
    bool AddDefinitiveNumber(int number, int row, int col, IStrategy strategy);

    bool RemovePossibility(int possibility, int row, int col, IStrategy strategy);

    LinePositions PossibilityPositionsInColumn(int col, int number);

    LinePositions PossibilityPositionsInRow(int row, int number);

    MiniGridPositions PossibilityPositionsInMiniGrid(int miniRow, int miniCol, int number);

    public Sudoku Sudoku { get; }

    public IPossibilities[,] Possibilities { get; }

    public Solver Copy();
}

public interface IPossibilities : IEnumerable<int>
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
    public int GetFirst();
    public IPossibilities Copy();

    public static IPossibilities DefaultMash(IPossibilities poss1, IPossibilities poss2)
    {
        IPossibilities result = New();
        for (int i = Min; i <= Max; i++)
        {
            if (!poss1.Peek(i) && !poss2.Peek(i)) result.Remove(i);
        }

        return result;
    }

    public static IPossibilities New()
    {
        return new BitPossibilities();
    }
}





