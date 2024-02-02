using System.Collections.Generic;
using Model.Sudoku.Solver.Arguments;
using Model.Sudoku.Solver.Helpers;

namespace Model.Sudoku.Solver;

public interface IStrategy
{ 
    public string Name { get; }
    public StrategyDifficulty Difficulty { get; }
    public UniquenessDependency UniquenessDependency { get; }
    public OnCommitBehavior OnCommitBehavior { get; set; }
    public OnCommitBehavior DefaultOnCommitBehavior { get; }
    public StatisticsTracker Tracker { get; }
    public IReadOnlyList<IStrategyArgument> Arguments { get; }
    
    void Apply(IStrategyUser strategyUser);
    void OnNewSudoku(Sudoku s);
    void TrySetArgument(string name, ArgumentValue value);
    public Dictionary<string, string> ArgumentsAsDictionary()
    {
        Dictionary<string, string> result = new();

        foreach (var arg in Arguments)
        {
            result.Add(arg.Name, arg.ToString()!);
        }

        return result;
    }
}

public enum StrategyDifficulty
{
    None, Basic, Easy, Medium, Hard, Extreme, ByTrial
}

public enum UniquenessDependency
{
    NotDependent, PartiallyDependent, FullyDependent
}

public enum OnCommitBehavior
{
    Return, WaitForAll, ChooseBest
}







