namespace DesktopApplication.Presenter.Binairos.Solve;

public interface IBinairoSolveView : ISolveWithStepsView
{
    void SetBinairoAsString(string s);
    IBinairoDrawer Drawer { get; }
}