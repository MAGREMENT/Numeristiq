using System;

namespace Model.Util;

public static class MathUtil
{
    public static double[,] ShiftSecondPointPerpendicularly(double xA, double yA, double xB, double yB, double d)
    {
        if (xA == xB && yA == yB)
        {
            return new[,] {{xB, yB}, {xB, yB} };
        }
        
        //Translate AB to 0, 0
        var x = xB - xA;
        var y = yB - yA;

        if (x == 0)
        {
            //Vertical line
            return new[,] { {xB + d, yB}, {xB - d, yB} };
        }

        if (y == 0)
        {
            //Horizontal line
            return new[,] { {xB, yB + d}, {xB, yB - d } };
        }

        var result = new double[2, 2];

        var slope = y / x;
        var sign = xB < 0 ? Math.PI : 0;
        var baseAngle = Math.Atan(slope) + sign;
        var abLength = Math.Sqrt(x * x + y * y);
        var angleShift = Math.Atan(d / abLength);
        var hypotenuseSquared = abLength * abLength + d * d;

        var newAngles = new[] { baseAngle + angleShift, baseAngle - angleShift };
        for (int i = 0; i < 2; i++)
        {
            var newSlope = Math.Tan(newAngles[i]);
            sign = Quadrant(newAngles[i]) is 2 or 3 ? -1 : 1;
            var newX = Math.Sqrt(hypotenuseSquared / (newSlope * newSlope + 1)) * sign;
            var newY = newSlope * newX;

            result[i, 0] = newX + xA;
            result[i, 1] = newY + yA;
        }

        return result;
    }

    public static int Quadrant(double angle)
    {
        while (angle < 0)
        {
            angle += Math.PI * 2;
        }

        while (angle > Math.PI * 2)
        {
            angle -= Math.PI * 2;
        }

        return angle switch
        {
            >= 0 and < Math.PI / 2 => 1,
            >= Math.PI / 2 and < Math.PI => 2,
            >= Math.PI and < Math.PI + Math.PI / 2 => 3,
            _ => 4
        };
    }
}