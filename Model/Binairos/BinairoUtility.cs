using Model.Utility;

namespace Model.Binairos;

public class BinairoUtility
{
    public static int Opposite(int n) => n == 1 ? 2 : 1;
    public static CellPossibility Opposite(CellPossibility cp) => new(cp.Row, cp.Column, Opposite(cp.Possibility));
}