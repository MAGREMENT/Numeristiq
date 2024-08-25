namespace DesktopApplication.Presenter.YourPuzzles;

public interface IYourPuzzleDrawer : IDrawer
{
    int RowCount { set; }
    int ColumnCount { set; }
}