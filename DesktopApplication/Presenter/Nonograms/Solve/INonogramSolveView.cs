namespace DesktopApplication.Presenter.Nonograms.Solve;

public interface INonogramSolveView : ISolveWithStepsView
{
    INonogramDrawer Drawer { get; }

    void ShowNonogramAsString(string s);
}