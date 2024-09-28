namespace DesktopApplication.Presenter.Kakuros.Solve;

public interface IKakuroSolveView : IStepManagingView
{
    IKakuroSolverDrawer Drawer { get; }

    void SetKakuroAsString(string s);
}