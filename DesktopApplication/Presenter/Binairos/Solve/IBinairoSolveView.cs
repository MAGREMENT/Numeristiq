namespace DesktopApplication.Presenter.Binairos.Solve;

public interface IBinairoSolveView : IStepManagingView
{
    void SetBinairoAsString(string s);
    IBinairoDrawer Drawer { get; }
}