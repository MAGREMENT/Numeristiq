using System;
using System.Collections.Generic;
using Global.Enums;
using Model.Solver;

namespace Model;

public interface IRepository<T>
{
    public bool UploadAllowed { set; }
    
    public void Initialize();
    public T? Download();
    public void Upload(T DAO);
    public void New(T DAO);
}

public class RepositoryInitializationException : Exception
{
    public RepositoryInitializationException(string s) : base(s)
    {
        
    }
}

public record StrategyDAO(string Name, bool Used, OnCommitBehavior Behavior, Dictionary<string, string> Args);
public record SettingsDAO(StateShown StateShown, SudokuTranslationType TranslationType,
    int DelayBeforeTransition, int DelayAfterTransition, bool UniquenessAllowed, ChangeType ActionOnCellChange,
    bool TransformSoloPossibilityIntoGiven, CellColor GivenColor, CellColor SolvingColor,
    LinkOffsetSidePriority SidePriority, bool ShowSameCellLinks);