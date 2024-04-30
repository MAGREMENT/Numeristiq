using System.Windows.Controls;

namespace DesktopApplication.View.Sudokus.Pages;

public abstract class SudokuPage : Page
{
    public abstract void OnShow();
    public abstract void OnClose();
    public abstract object? TitleBarContent();
    public void UpdateSize(double width, double height)
    {
        Width = width;
        Height = height;
        InternalUpdateSize();
    }
    protected abstract void InternalUpdateSize();
}