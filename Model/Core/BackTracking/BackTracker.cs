using System.Collections.Generic;
using Model.Tectonics;

namespace Model.Core.BackTracking;

public abstract class BackTracker<TPuzzle, TData> where TPuzzle : ICopyable<TPuzzle>
{
    protected TData _giver;

    public int StopAt { get; set; } = int.MaxValue;
    public TPuzzle Current { get; private set; }

    protected BackTracker(TPuzzle puzzle, TData data)
    {
        Current = puzzle;
        _giver = data;
        Initialize(false);
    }
    
    public void Set(TPuzzle puzzle, TData data)
    {
        Current = puzzle;
        _giver = data;
        Initialize(true);
    }

    public void Set(TPuzzle puzzle)
    {
        Current = puzzle;
        Initialize(true);
    }

    public void Set(TData data)
    {
        _giver = data;
    }

    public IReadOnlyList<TPuzzle> Solutions()
    {
        var result = new BackTrackingListResult<TPuzzle>();
        Search(result, 0);
        return result;
    }

    public int Count()
    {
        var result = new BackTrackingCountResult<TPuzzle>();
        Search(result, 0);
        return result.Count;
    }

    public bool Fill()
    {
        return Search(0);
    }

    /// <summary>
    /// Continue backtracking from the current position without putting the current puzzle in its original state
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    protected abstract bool Search(int position);
    /// <summary>
    /// Continue backtracking from the current position, adding the solutions to the result object and putting the
    /// current puzzle in its original state
    /// </summary>
    /// <param name="result"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    protected abstract bool Search(IBackTrackingResult<TPuzzle> result, int position);
    /// <summary>
    /// Called every time the current puzzle is changed
    /// </summary>
    /// <param name="reset"></param>
    protected abstract void Initialize(bool reset);
}

public interface IAvailabilityChecker
{
    bool IsAvailable(int row, int col);
}

public class ConstantAvailabilityChecker : IAvailabilityChecker
{
    public static ConstantAvailabilityChecker Instance { get; } = new();

    private ConstantAvailabilityChecker()
    {
        
    }

    public bool IsAvailable(int row, int col)
    {
        return true;
    }
}

public interface IPossibilitiesGiver
{
    IEnumerable<int> EnumeratePossibilitiesAt(int row, int col);
}

public class ConstantPossibilitiesGiver : IPossibilitiesGiver
{
    private static readonly IEnumerable<int> _enumerable = Create();

    public static ConstantPossibilitiesGiver Instance { get; } = new();

    private ConstantPossibilitiesGiver()
    {
        
    }
    
    public IEnumerable<int> EnumeratePossibilitiesAt(int row, int col)
    {
        return _enumerable;
    }

    private static IEnumerable<int> Create()
    {
        for (int i = 1; i <= 9; i++)
        {
            yield return i;
        }
    }
}

public class EmptyPossibilitiesGiver : IPossibilitiesGiver
{
    public IEnumerable<int> EnumeratePossibilitiesAt(int row, int col)
    {
        yield break;
    }
}

public class TectonicPossibilitiesGiver : IPossibilitiesGiver
{
    private readonly ITectonic _tectonic;

    public TectonicPossibilitiesGiver(ITectonic tectonic)
    {
        _tectonic = tectonic;
    }

    public IEnumerable<int> EnumeratePossibilitiesAt(int row, int col)
    {
        var count = _tectonic.GetZone(row, col).Count;
        for (int i = 1; i <= count; i++)
        {
            yield return i;
        }
    }
}

public interface IBackTrackingResult<in T> where T : ICopyable<T>
{
    void AddNewResult(T sudoku);
    int Count { get; }
}

public class BackTrackingListResult<T> : List<T>, IBackTrackingResult<T> where T : ICopyable<T>
{
    public void AddNewResult(T sudoku)
    {
        Add(sudoku.Copy());
    }
}

public class BackTrackingCountResult<T> : IBackTrackingResult<T> where T : ICopyable<T>
{
    public void AddNewResult(T sudoku)
    {
        Count++;
    }

    public int Count { get; private set; }
}

public interface ICopyable<out T>
{
    T Copy();
}