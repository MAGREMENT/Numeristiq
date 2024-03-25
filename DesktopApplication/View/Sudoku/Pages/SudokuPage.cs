using System.Windows.Controls;

namespace DesktopApplication.View.Sudoku.Pages;

public abstract class SudokuPage : Page
{
    public abstract void OnShow();
    public abstract void OnClose();
    public abstract object? TitleBarContent();
}