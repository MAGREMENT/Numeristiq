using System.Collections.Generic;
using Model.Solver.Helpers;

namespace Model;

public interface IStrategyLoader
{
    public StrategyInformation[] GetStrategyInfo();
    public List<string> FindStrategies(string filter);

    public void AddStrategy(string name);
    public void AddStrategy(string name, int position);
    public void RemoveStrategy(int position);
    public void InterchangeStrategies(int positionOne, int positionTwo);
    
    public event OnListUpdate? ListUpdated;
}