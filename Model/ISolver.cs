using System.Collections;
using System.Collections.Generic;
using System.Windows.Documents;
using Model.Positions;
using Model.Possibilities;

namespace Model;

public interface ISolver
{
    bool AddDefinitiveNumber(int number, int row, int col, ISolverLog? log = null);

    bool RemovePossibility(int possibility, int row, int col, ISolverLog? log = null);

    LinePositions PossibilityPositionsInColumn(int col, int number);

    LinePositions PossibilityPositionsInRow(int row, int number);

    MiniGridPositions PossibilityPositionsInMiniGrid(int miniRow, int miniCol, int number);

    public Sudoku Sudoku { get; }

    public IPossibilities[,] Possibilities { get; }
    
    public List<ISolverLog> Logs { get; }
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