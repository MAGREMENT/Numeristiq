namespace DesktopApplication.Presenter.Kakuros.Solve;

public interface IKakuroSolveView : ISolveWithStepsView
{
    IKakuroSolverDrawer Drawer { get; }

    void SetKakuroAsString(string s);
}