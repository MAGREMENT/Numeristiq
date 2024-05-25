namespace DesktopApplication.Presenter.Kakuros.Solve;

public interface IKakuroSolveView
{
    IKakuroSolverDrawer Drawer { get; }

    void SetKakuroAsString(string s);
}