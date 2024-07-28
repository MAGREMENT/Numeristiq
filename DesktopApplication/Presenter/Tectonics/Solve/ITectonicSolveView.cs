using Model.Core.Steps;

namespace DesktopApplication.Presenter.Tectonics.Solve;

public interface ITectonicSolveView : ISolveWithStepsView
{
    public ITectonicDrawer Drawer { get; }

    void SetTectonicString(string s);
}