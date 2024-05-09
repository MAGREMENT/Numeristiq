using DesktopApplication.Presenter.Kakuros.Solve;

namespace DesktopApplication.Presenter.Kakuros;

public class KakuroApplicationPresenter
{
    public KakuroSolvePresenter Initialize(IKakuroSolveView view) => new(view);
}