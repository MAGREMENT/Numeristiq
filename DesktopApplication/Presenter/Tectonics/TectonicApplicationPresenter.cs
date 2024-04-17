using DesktopApplication.Presenter.Tectonics.Solve;
using Model.Helpers;
using Model.Helpers.Changes.Buffers;
using Model.Helpers.Highlighting;
using Model.Tectonics;

namespace DesktopApplication.Presenter.Tectonics;

public class TectonicApplicationPresenter
{
    private readonly TectonicSolver _solver = new();
    private readonly Settings _settings;

    public TectonicApplicationPresenter(Settings settings)
    {
        _solver.ChangeBuffer =
            new LogManagedChangeBuffer<IUpdatableTectonicSolvingState, ITectonicHighlighter>(_solver);
        _settings = settings;
    }

    public TectonicSolvePresenter Initialize(ITectonicSolveView view)
    {
        return new TectonicSolvePresenter(_solver, view);
    }
}