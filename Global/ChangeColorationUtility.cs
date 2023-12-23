using Global.Enums;

namespace Global;

public static class ChangeColorationUtility
{
    public static bool IsOff(ChangeColoration coloration)
    {
        return (int)coloration is >= 4 and <= 13;
    }
}