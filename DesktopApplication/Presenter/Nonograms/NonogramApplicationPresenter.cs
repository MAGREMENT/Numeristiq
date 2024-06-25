using DesktopApplication.Presenter.Nonograms.Solve;

namespace DesktopApplication.Presenter.Nonograms;

public class NonogramApplicationPresenter
{
    public NonogramSolvePresenter Initialize(INonogramSolveView view) => new(view);
}