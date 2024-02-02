using System.Collections.Generic;
using Model.Sudoku.Solver;
using Model.Sudoku.Solver.Arguments;
using Model.Sudoku.Solver.Helpers;

namespace Model.Sudoku;

public interface IStrategyManager
{
    public StrategyInformation[] GetStrategiesInformation();
    public List<string> FindStrategies(string filter);

    public void AddStrategy(string name);
    public void AddStrategy(string name, int position);
    public void RemoveStrategy(int position);
    public void InterchangeStrategies(int positionOne, int positionTwo);
    public void ChangeStrategyBehavior(string name, OnCommitBehavior behavior);
    public void ChangeStrategyBehaviorForAll(OnCommitBehavior behavior);
    public void ChangeStrategyUsage(string name, bool yes);
    public void ChangeArgument(string strategyName, string argumentName, ArgumentValue value);
    
    public event OnListUpdate? ListUpdated;
}