using System;
using System.Collections.Generic;
using Model.Sudoku;
using Model.Sudoku.Solver;
using Repository;

namespace DesktopApplication.Controllers;

public static class ControllerDistributor
{
    private static readonly ControllerCallback _callback = new ();
    
    public static SudokuTextBoxController Initialize(ISudokuTextBoxView textBox)
    {
        return new SudokuTextBoxController(_callback, textBox);
    }

    public static SolvePageController Initialize(ISolvePageView page)
    {
        var c = new SolvePageController(page);
        _callback.SolvePageController = c;
        return c;
    }
}

public class ControllerCallback : IControllerCallback
{
    public SolvePageController? SolvePageController { get; set; }
    
    public void SetSudoku(string s)
    {
        SolvePageController?.SetSudoku(s);
    }

    public string GetSudokuAsString()
    {
        if (SolvePageController is null) return "";

        return SolvePageController.SudokuAsString();
    }
}

public interface IControllerCallback
{
    public void SetSudoku(string s);
    public string GetSudokuAsString();
}