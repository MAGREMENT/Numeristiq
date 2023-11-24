using System.Collections.Generic;

namespace Model;

public interface IStrategyLoader
{
    public List<string> FindStrategies(string filter);
}