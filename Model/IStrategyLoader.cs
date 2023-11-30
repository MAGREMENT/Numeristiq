using System.Collections.Generic;
using Model.Solver;
using Model.Solver.Helpers;

namespace Model;

public interface IStrategyLoader
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
    
    public event OnListUpdate? ListUpdated;
}