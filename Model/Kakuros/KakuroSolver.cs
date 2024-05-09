namespace Model.Kakuros;

public class KakuroSolver
{
    private IKakuro _kakuro = new ArrayKakuro();

    public void SetKakuro(IKakuro kakuro)
    {
        _kakuro = kakuro;
    }
}