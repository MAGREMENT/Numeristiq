using System;
using System.Collections.Generic;
using Model.Helpers.Changes;
using Model.Sudoku.Solver;
using Model.Utility;

namespace Model.Sudoku;

public interface IRepository<T>
{
    public bool UploadAllowed { set; }
    
    public void Initialize();
    public T? Download();
    public void Upload(T DAO);
    public void New(T DAO);

    public void InitializeOrDefault(T defaultValue)
    {
        try
        {
            Initialize();
        }
        catch (RepositoryInitializationException)
        {
            New(defaultValue);
        }
    }
}

public class RepositoryInitializationException : Exception
{
    public RepositoryInitializationException(string s) : base(s)
    {
        
    }
}

public enum CellColor
{
    Black, Gray, Red, Green, Blue
}

public enum StateShown
{
    Before, After
}

public enum RotationDirection
{
    ClockWise, CounterClockWise
}

public enum LinkOffsetSidePriority
{
    Any, Left, Right
}

public enum IconColor
{
    White, Black
}

public record StrategyDAO(string Name, bool Used, OnCommitBehavior Behavior, Dictionary<string, string> Args);

public record SettingsDAO(StateShown StateShown, SudokuTranslationType TranslationType,
    int DelayBeforeTransition, int DelayAfterTransition, bool UniquenessAllowed, ChangeType ActionOnCellChange,
    bool TransformSoloPossibilityIntoGiven, CellColor GivenColor, CellColor SolvingColor,
    LinkOffsetSidePriority SidePriority, bool ShowSameCellLinks, bool MultiColorHighlighting,
    int StartAngle, RotationDirection RotationDirection, int Theme);
    
public record ThemeDAO(RGB Background1, RGB Background2, RGB Background3, RGB Primary1, RGB Primary2,
    RGB Secondary1, RGB Secondary2, RGB Accent, RGB Border, RGB Text, IconColor IconColor);