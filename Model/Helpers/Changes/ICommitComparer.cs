namespace Model.Helpers.Changes;

public interface ICommitComparer
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
    public int Compare(IChangeCommit first, IChangeCommit second);
}

public class DefaultCommitComparer : ICommitComparer
{
    private const int SolutionAddedValue = 3;
    private const int PossibilityRemovedValue = 1;

    private static DefaultCommitComparer? _instance;

    public static DefaultCommitComparer Instance
    {
        get
        {
            _instance ??= new DefaultCommitComparer();
            return _instance;
        }
    }

    public int Compare(IChangeCommit first, IChangeCommit second)
    {
        int score = 0;

        foreach (var change in first.Changes)
        {
            score += change.ProgressType == ProgressType.SolutionAddition ? SolutionAddedValue : PossibilityRemovedValue;
        }

        foreach (var change in second.Changes)
        {
            score -= change.ProgressType == ProgressType.SolutionAddition ? SolutionAddedValue : PossibilityRemovedValue;
        }

        return score;
    }
}