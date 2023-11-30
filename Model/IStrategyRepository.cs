using System;
using System.Collections.Generic;
using Model.Solver;

namespace Model;

public interface IStrategyRepository
{
    public bool UploadAllowed { get; set; }
    
    public void Initialize();
    public List<StrategyDAO> DownloadStrategies();
    public void UploadStrategies(List<StrategyDAO> DAOs);
}

public class StrategyRepositoryInitializationException : Exception
{
    public StrategyRepositoryInitializationException(string s) : base(s)
    {
        
    }
}

public record StrategyDAO(string Name, bool Used, OnCommitBehavior Behavior, Dictionary<string, string> args);