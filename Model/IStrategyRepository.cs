using System.Collections.Generic;
using Model.Solver;

namespace Model;

public interface IStrategyRepository
{
    public List<StrategyDAO> DownloadDAOs();
    public void UploadDAOs(List<StrategyDAO> DAOs);
}

public record StrategyDAO(string Name, bool Used, OnCommitBehavior Behavior, Dictionary<string, string> args);