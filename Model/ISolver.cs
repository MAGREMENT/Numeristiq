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
    string ViewLog();
}

public class BasicNumberAddedLog : ISolverLog
{
    private int _number;
    private int _row;
    private int _col;

    public BasicNumberAddedLog(int number, int row, int col)
    {
        _number = number;
        _row = row;
        _col = col;
    }

    public string ViewLog()
    {
        return $"{_number} added in row {_row}, column {_col}";
    }
}

public class BasicPossibilityRemovedLog : ISolverLog
{
    private int _number;
    private int _row;
    private int _col;

    public BasicPossibilityRemovedLog(int number, int row, int col)
    {
        _number = number;
        _row = row;
        _col = col;
    }
    
    public string ViewLog()
    {
        return $"{_number} removed from the possibilities in row {_row}, column {_col}";
    }
}