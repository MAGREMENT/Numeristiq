using System.Collections.Generic;
using Global;
using Global.Enums;
using Model.Solver;
using Model.Solver.Helpers.Logs;

namespace Model;

public interface ISolver
{
    public IReadOnlySudoku Sudoku { get; }
    public void SetSudoku(Sudoku sudoku);
    public void SolveAsync(bool stopAtProgress);
    public SolverState CurrentState { get; }
    public SolverState StartState { get; }
    public IReadOnlyList<ISolverLog> Logs { get; }
    public void AllowUniqueness(bool yes);
    public void SetOnInstanceFound(OnInstanceFound found);
    public void UseStrategy(int number);
    public void ExcludeStrategy(int number);
    public StrategyInfo[] StrategyInfos { get; }
    
    
    public event OnLogsUpdate? LogsUpdated;
    public event OnCurrentStrategyChange? CurrentStrategyChanged;
}

public delegate void OnLogsUpdate();
public delegate void OnCurrentStrategyChange(int index);