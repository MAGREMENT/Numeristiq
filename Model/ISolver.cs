using System.Collections.Generic;
using Global;
using Global.Enums;
using Model.Solver;
using Model.Solver.Helpers;
using Model.Solver.Helpers.Logs;

namespace Model;

public interface ISolver
{
    public IStrategyLoader StrategyLoader { get; }
    public void SetSudoku(Sudoku sudoku);
    public void SetState(SolverState state);
    public void Solve(bool stopAtProgress);
    public SolverState CurrentState { get; }
    public SolverState StartState { get; }
    public IReadOnlyList<ISolverLog> Logs { get; }
    public void AllowUniqueness(bool yes);
    public void UseStrategy(int number);
    public void ExcludeStrategy(int number);
    public StrategyInformation[] GetStrategyInfo();
    public void SetSolutionByHand(int number, int row, int col);
    public void RemoveSolutionByHand(int row, int col);
    public void RemovePossibilityByHand(int possibility, int row, int col);
    public string FullScan();
    
    
    public event OnLogsUpdate? LogsUpdated;
    public event OnCurrentStrategyChange? CurrentStrategyChanged;
}

public delegate void OnLogsUpdate();
public delegate void OnCurrentStrategyChange(int index);