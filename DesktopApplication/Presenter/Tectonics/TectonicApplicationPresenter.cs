using DesktopApplication.Presenter.Tectonics.Solve;
using Model.Tectonics.Solver;
using Model.Tectonics.Solver.Strategies;
using Model.Tectonics.Solver.Strategies.AlternatingInference;
using Model.Tectonics.Solver.Strategies.AlternatingInference.Types;

namespace DesktopApplication.Presenter.Tectonics;

public class TectonicApplicationPresenter
{
    private readonly TectonicSolver _solver = new();
    private readonly Settings _settings;

    public TectonicApplicationPresenter(Settings settings)
    {
        _settings = settings;
        _solver.StrategyManager.AddStrategies(new NakedSingleStrategy(),
            new HiddenSingleStrategy(),
            new ZoneInteractionStrategy(),
            new AlternatingInferenceGeneralization(new XChainType()),
            new GroupEliminationStrategy(),
            new AlternatingInferenceGeneralization(new AlternatingInferenceChainType()),
            new BruteForceStrategy());
    }

    public TectonicSolvePresenter Initialize(ITectonicSolveView view)
    {
        return new TectonicSolvePresenter(_solver, view, _settings);
    }
}