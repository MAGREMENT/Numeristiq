namespace DesktopApplication.Presenter.Nonograms.Solve;

public interface INonogramSolveView
{
    INonogramDrawer Drawer { get; }

    void ShowNonogramAsString(string s);
}