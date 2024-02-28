using System;
using System.Collections.Generic;
using Model.Sudoku;
using Model.Sudoku.Solver;
using Repository;

namespace DesktopApplication.Controllers;

public class SolvePageController
{
    private static readonly SudokuSolver _solver = new();
    private static readonly IRepository<IReadOnlyList<SudokuStrategy>> _repository =
        new SudokuStrategiesJSONRepository("strategies.json");
    
    private readonly ISolvePageView _solvePage;

    public SolvePageController(ISolvePageView solvePage)
    {
        _solvePage = solvePage;
        if (!_repository.Initialize(true)) throw new Exception("Repository failed");
        _solver.StrategyManager.AddStrategies(_repository.Download());
    }

    public void SetSudoku(string s)
    {
        _solver.SetSudoku(SudokuTranslator.TranslateLineFormat(s));
    }

    public string SudokuAsString()
    {
        return SudokuTranslator.TranslateLineFormat(_solver.Sudoku, SudokuTranslationType.Points);
    }
}

public interface ISolvePageView
{
    
}