using System.Collections.Generic;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Logs;
using Model.Sudoku.Solver;

namespace Model.Sudoku;

public interface ISolver
{
    public IStrategyManager StrategyManager { get; }
    public void SetSudoku(Sudoku sudoku);
    public void SetState(SolverState state);
    public void Solve(bool stopAtProgress);
    public BuiltChangeCommit[] EveryPossibleNextStep();
    public SolverState CurrentState { get; }
    public SolverState StartState { get; }
    public IReadOnlyList<ISolverLog> Logs { get; }
    public void AllowUniqueness(bool yes);
    public void UseStrategy(int number);
    public void UseAllStrategies(bool yes);
    public void ExcludeStrategy(int number);
    public StrategyInformation[] GetStrategyInfo();
    public void SetSolutionByHand(int number, int row, int col);
    public void RemoveSolutionByHand(int row, int col);
    public void RemovePossibilityByHand(int possibility, int row, int col);
    public void ApplyCommit(BuiltChangeCommit commit);
    
    
    public event OnLogsUpdate? LogsUpdated;
    public event OnStrategyStart? StrategyStarted;
    public event OnStrategyStop? StrategyStopped;
}

public delegate void OnLogsUpdate();
public delegate void OnStrategyStart(int index);
public delegate void OnStrategyStop(int index, int solutionAdded, int possibilitiesRemoved);