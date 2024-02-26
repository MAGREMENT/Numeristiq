using System;
using System.Collections.Generic;
using System.Windows;

namespace DesktopApplication.View.Utility;

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
        return NormalizeAngle(angle) switch
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

    public static List<Point> GetMultiColorHighlightingPolygon(Point center, double width, double height,
        double firstAngle, double secondAngle, int rotationalFactor)
    {
        List<Point> result = new();

        var firstPoint = ComputePolygonPoint(center, width, height, firstAngle, out var firstZone);
        var secondPoint = ComputePolygonPoint(center, width, height, secondAngle, out var secondZone);

        result.Add(center);
        result.Add(firstPoint);
        result.AddRange(AdditionalPolygonPoints(center, width, height, firstZone, secondZone, rotationalFactor));
        result.Add(secondPoint);

        return result;
    }

    private static IEnumerable<Point> AdditionalPolygonPoints(Point center, double width, double height, 
        CellZone first, CellZone second, int rotationalFactor)
    {
        if (first == second) yield break;
        
        var firstI = (int)first;
        var secondI = (int)second;

        firstI -= rotationalFactor;
        if (firstI < 0) firstI = 7;
        else if (firstI > 7) firstI = 0;
        
        while (firstI != secondI)
        {
            switch ((CellZone)firstI)
            {
                case CellZone.TopLeftCorner :
                    yield return new Point(center.X - width / 2, center.Y - height / 2);
                    break;
                case CellZone.TopRightCorner :
                    yield return new Point(center.X + width / 2, center.Y - height / 2);
                    break;
                case CellZone.BottomLeftCorner :
                    yield return new Point(center.X - width / 2, center.Y + height / 2);
                    break;
                case CellZone.BottomRightCorner :
                    yield return new Point(center.X + width / 2, center.Y + height / 2);
                    break;
            }
            
            firstI -= rotationalFactor;
            if (firstI < 0) firstI = 7;
            else if (firstI > 7) firstI = 0;
        }
    }

    private static Point ComputePolygonPoint(Point center, double width, double height, double angle, out CellZone zone)
    {
        var normalized = NormalizeAngle(angle);
        var trAngle = Math.PI / 4;
        var tlAngle = Math.PI * 3 / 4;
        var blAngle = Math.PI + Math.PI / 4;
        var brAngle = Math.PI + Math.PI * 3 / 4;
        
        if (SameAngle(normalized, trAngle))
        {
            zone = CellZone.TopRightCorner;
            return new Point(center.X + width / 2, center.Y - height / 2);
        }

        if (SameAngle(normalized, tlAngle))
        {
            zone = CellZone.TopLeftCorner;
            return new Point(center.X - width / 2, center.Y - height / 2);
        }
        
        if (SameAngle(normalized, blAngle))
        {
            zone = CellZone.BottomLeftCorner;
            return new Point(center.X - width / 2, center.Y + height / 2);
        }

        if (SameAngle(normalized, brAngle))
        {
            zone = CellZone.BottomRightCorner;
            return new Point(center.X + width / 2, center.Y + height / 2);
        }

        var slope = Math.Abs(Math.Tan(normalized));
        slope = Quadrant(normalized) is 1 or 3 ? -slope : slope;
        double deltaX;
        if (normalized < trAngle || normalized > brAngle)
        {
            zone = CellZone.RightBorder;
            deltaX = width / 2;
            return new Point(center.X + deltaX, center.Y + slope * deltaX);
        }

        if (normalized < blAngle && normalized > tlAngle)
        {
            zone = CellZone.LeftBorder;
            deltaX = -width / 2;
            return new Point(center.X + deltaX, center.Y + slope * deltaX);
        }

        double deltaY;
        if (normalized < tlAngle && normalized > trAngle)
        {
            zone = CellZone.TopBorder;
            deltaY = -height / 2;
            return new Point(center.X + deltaY / slope, center.Y + deltaY);
        }

        zone = CellZone.BottomBorder;
        deltaY = height / 2;
        return new Point(center.X + deltaY / slope, center.Y + deltaY);
    }

    private static bool SameAngle(double a1, double a2)
    {
        return Math.Abs(a1 - a2) <= 0.01;
    }

    public static double NormalizeAngle(double angle)
    {
        var fullRotation = Math.PI * 2;
        
        while (angle < 0)
        {
            angle += fullRotation;
        }

        while (angle > Math.PI * 2)
        {
            angle -= fullRotation;
        }

        return angle;
    }
}

public enum CellZone
{
    TopRightCorner, RightBorder, BottomRightCorner, BottomBorder,
    BottomLeftCorner, LeftBorder, TopLeftCorner, TopBorder
}