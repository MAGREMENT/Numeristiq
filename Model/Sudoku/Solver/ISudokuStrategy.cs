using System.Collections.Generic;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Sudoku.Solver.Arguments;

namespace Model.Sudoku.Solver;

public interface ISudokuStrategy : ICommitMaker
{ 
    public UniquenessDependency UniquenessDependency { get; }
    public OnCommitBehavior DefaultOnCommitBehavior { get; }
    public IReadOnlyList<IStrategyArgument> Arguments { get; }
    
    void Apply(IStrategyUser strategyUser);
    void OnNewSudoku(Sudoku s);
    void TrySetArgument(string name, ArgumentValue value);
    public Dictionary<string, string> ArgumentsAsDictionary()
    {
        Dictionary<string, string> result = new();

        foreach (var arg in Arguments)
        {
            result.Add(arg.Name, arg.Get().ToString()!);
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







