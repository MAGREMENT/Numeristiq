namespace DesktopApplication.Presenter.Tectonics.Solve;

public interface ITectonicSolveView : IStepManagingView
{
    public ITectonicDrawer Drawer { get; }

    void SetTectonicString(string s);
}