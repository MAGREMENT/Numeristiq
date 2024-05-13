namespace Model.Kakuros;

public static class KakuroCellUtility
{
    public static int MaxAmountFor(int cellCount)
    {
        int total = 0;
        int n = 9;
        for (int i = 0; i < cellCount; i++)
        {
            total += n;
            n--;
        }

        return total;
    }

    public static int MinAmountFor(int cellCount)
    {
        int total = 0;
        int n = 1;
        for (int i = 0; i < cellCount; i++)
        {
            total += n;
            n++;
        }

        return total;
    }
}