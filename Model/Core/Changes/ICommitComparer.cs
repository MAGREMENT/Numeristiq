namespace Model.Core.Changes;

public interface ICommitComparer<TChange>
{
    /// <summary>
    /// Compare two commits
    /// more than 0 if first is better
    /// == 0 if same
    /// less than 0 if second is better
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <returns></returns>
    public int Compare(IChangeCommit<TChange> first, IChangeCommit<TChange> second);
}

public class DefaultNumericCommitComparer : ICommitComparer<NumericChange>
{
    private const int SolutionAddedValue = 3;
    private const int PossibilityRemovedValue = 1;

    private static DefaultNumericCommitComparer? _instance;

    public static DefaultNumericCommitComparer Instance
    {
        get
        {
            _instance ??= new DefaultNumericCommitComparer();
            return _instance;
        }
    }

    public int Compare(IChangeCommit<NumericChange> first, IChangeCommit<NumericChange> second)
    {
        int score = 0;

        foreach (var change in first.Changes)
        {
            score += change.Type == ChangeType.SolutionAddition ? SolutionAddedValue : PossibilityRemovedValue;
        }

        foreach (var change in second.Changes)
        {
            score -= change.Type == ChangeType.SolutionAddition ? SolutionAddedValue : PossibilityRemovedValue;
        }

        return score;
    }
}

public class DefaultDichotomousCommitComparer : ICommitComparer<DichotomousChange>
{
    private static DefaultNumericCommitComparer? _instance;

    public static DefaultNumericCommitComparer Instance
    {
        get
        {
            _instance ??= new DefaultNumericCommitComparer();
            return _instance;
        }
    }

    public int Compare(IChangeCommit<DichotomousChange> first, IChangeCommit<DichotomousChange> second)
    {
        return first.Changes.Length - second.Changes.Length;
    }
}