namespace DesktopApplication.Presenter.Nonograms.Solve;

public interface INonogramSolveView : IStepManagingView
{
    INonogramDrawer Drawer { get; }

    void ShowNonogramAsString(string s);
}