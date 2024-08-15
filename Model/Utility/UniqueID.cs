namespace Model.Utility;

public static class UniqueID
{
    private static int _current;

    public static int Next() => _current++;
}