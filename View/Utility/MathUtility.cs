using System;
using System.Windows;

namespace View.Utility;

public static class MathUtility
{
    public static Point[] ShiftSecondPointPerpendicularly(Point a, Point b, double d)
    {
        if (a == b)
        {
            return new[] { b, b };
        }
        
        //Translate AB to 0, 0
        var x = b.X - a.X;
        var y = b.Y - a.Y;

        if (x == 0)
        {
            //Vertical line
            return new[] { new Point(b.X + d, b.Y), new Point(b.X - d, b.Y) };
        }

        if (y == 0)
        {
            //Horizontal line
            return new[] { new Point(b.X, b.Y + d), new Point(b.X, b.Y - d) };
        }

        var result = new Point[2];

        var slope = y / x;
        var sign = x < 0 ? Math.PI : 0;
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

            result[i] = new Point(newX + a.X, newY + a.Y);
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
    
    public static bool IsLeft(Point a, Point b, Point c) {
        return (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X) > 0;
    }
}