using System;

namespace Model.Utility;

public static class CellUtility
{
    public static double Distance(Cell oneCell, int onePoss, Cell twoCell, int twoPoss)
    {
        var oneX = oneCell.Column * 3 + onePoss % 3;
        var oneY = oneCell.Row * 3 + onePoss / 3;

        var twoX = twoCell.Column * 3 + twoPoss % 3;
        var twoY = twoCell.Row * 3 + twoPoss / 3;

        var dx = twoX - oneX;
        var dy = twoY - oneY;

        return Math.Sqrt(dx * dx + dy * dy);
    }
}