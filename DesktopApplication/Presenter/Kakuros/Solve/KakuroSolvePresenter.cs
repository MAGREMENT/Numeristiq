namespace DesktopApplication.Presenter.Kakuros.Solve;

public class KakuroSolvePresenter
{
    private readonly IKakuroSolveView _view;

    public KakuroSolvePresenter(IKakuroSolveView view)
    {
        _view = view;
    }
}