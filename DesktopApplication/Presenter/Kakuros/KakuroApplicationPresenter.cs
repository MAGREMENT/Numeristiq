using DesktopApplication.Presenter.Kakuros.Solve;

namespace DesktopApplication.Presenter.Kakuros;

public class KakuroApplicationPresenter
{
    private readonly Settings _settings;

    public KakuroApplicationPresenter(Settings settings)
    {
        _settings = settings;
    }

    public KakuroSolvePresenter Initialize(IKakuroSolveView view) => new(view, _settings);
}