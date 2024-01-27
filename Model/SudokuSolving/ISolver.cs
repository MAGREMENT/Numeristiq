using System.Collections.Generic;
using Model.SudokuSolving.Solver;
using Model.SudokuSolving.Solver.Helpers;
using Model.SudokuSolving.Solver.Helpers.Changes;
using Model.SudokuSolving.Solver.Helpers.Logs;

namespace Model.SudokuSolving;

public interface ISolver
{
    public IStrategyLoader StrategyLoader { get; }
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
    public event OnCurrentStrategyChange? CurrentStrategyChanged;
}

public delegate void OnLogsUpdate();
public delegate void OnCurrentStrategyChange(int index);