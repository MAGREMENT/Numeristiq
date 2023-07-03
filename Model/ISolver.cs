using System.Collections.Generic;

namespace Model;

public interface ISolver
{
    bool AddDefinitiveNumber(int number, int row, int col, ISolverLog? log = null);

    bool RemovePossibility(int possibility, int row, int col, ISolverLog? log = null);

    public Sudoku Sudoku { get; }

    public CellPossibilities[,] Possibilities { get; }
    
    public List<ISolverLog> Logs { get; }
}

public class CellPossibilities
{
    private readonly bool[] _possibilities = { true, true, true, true, true, true, true, true, true};
    public int Count { private set; get; } = 9;
    
    public bool Remove(int i)
    {
        var old = _possibilities[i - 1];
        _possibilities[i - 1] = false;
        if(old) Count--;
        return old;
    }

    public bool Peek(int i)
    {
        return _possibilities[i - 1];
    }

    public List<int> GetPossibilities()
    {
        List<int> result = new();
        for (int i = 0; i < _possibilities.Length; i++)
        {
            if(_possibilities[i]) result.Add(i + 1);
        }

        return result;
    }

    

    public override bool Equals(object? obj)
    {
        if (obj is not CellPossibilities cp) return false;
        for (int i = 0; i < _possibilities.Length; i++)
        {
            if (_possibilities[i] != cp._possibilities[i]) return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        return _possibilities.GetHashCode();
    }

    public override string ToString()
    {
        string result = "[";
        for (int i = 0; i < _possibilities.Length; i++)
        {
            if (_possibilities[i]) result += (i + 1) + ", ";
        }

        result = result.Length > 1 ? result.Substring(0, result.Length - 2) : result;
        return result + "]";
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
        AsString = $"{number} added in row {row}, column {col}";
    }

}

public class BasicPossibilityRemovedLog : ISolverLog
{
    public string AsString { get; }
    public StrategyLevel Level => StrategyLevel.None;

    public BasicPossibilityRemovedLog(int number, int row, int col)
    {
        AsString = $"{number} removed from the possibilities in row {row}, column {col}";
    }

}