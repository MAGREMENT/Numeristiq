using DesktopApplication.Presenter.Tectonic.Solve;
using Model.Tectonic;

namespace DesktopApplication.Presenter.Tectonic;

public class TectonicApplicationPresenter
{
    private readonly TectonicSolver _solver = new();
    private readonly Settings _settings;

    public TectonicApplicationPresenter(Settings settings)
    {
        _settings = settings;
    }

    public TectonicSolvePresenter Initialize(ITectonicSolveView view)
    {
        return new TectonicSolvePresenter(_solver, view);
    }
}