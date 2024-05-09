using System.Collections.Generic;
using Model.Utility;

namespace Model.Futoshikis;

public class Futoshiki : IReadOnlyFutoshiki
{
    private readonly int[,] _grid;
    private readonly List<Constraint> _constraints;
    
    public int Length => _grid.GetLength(0);

    public IReadOnlyList<Constraint> Constraints => _constraints;

    public Futoshiki()
    {
        _grid = new int[0, 0];
        _constraints = new List<Constraint>();
    }
    
    public Futoshiki(int length)
    {
        _grid = new int[length, length];
        _constraints = new List<Constraint>();
    }

    public int this[int row, int col]
    {
        get => _grid[row, col];
        set => _grid[row, col] = value;
    }

    public void AddConstraint(Constraint constraint)
    {
        _constraints.Add(constraint);
    }
}

public abstract class Constraint
{
    public Cell From { get; }
    public Cell To { get; }
    
    protected Constraint(Cell from, Cell to)
    {
        From = from;
        To = to;
    }
}

public class BiggerThanConstraint : Constraint
{
    public BiggerThanConstraint(Cell from, Cell to) : base(from, to)
    {
    }
}

public class SmallerThanConstraint : Constraint
{
    public SmallerThanConstraint(Cell from, Cell to) : base(from, to)
    {
    }
}

public interface IReadOnlyFutoshiki
{
    public int this[int row, int col] { get; }
    public IReadOnlyList<Constraint> Constraints { get; }
}