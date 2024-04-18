using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Model.Sudokus.Solver.Utility;

namespace DesktopApplication.View.Utility;

public static class Extensions
{
    public static bool IsUnderHalfHeight(this TextBlock tb, MouseEventArgs args)
    {
        return args.GetPosition(tb).Y > tb.ActualHeight / 2;
    }
    
    public static bool IsUnderHalfHeight(this TextBlock tb, DragEventArgs args)
    {
        return args.GetPosition(tb).Y > tb.ActualHeight / 2;
    }

    public static bool ContainsAdjacent(this CellPossibility[] cps, CellPossibility cp, int hShift, int vShift)
    {
        if (hShift < 0 && cp.Possibility is 1 or 4 or 7) return false;
        if (hShift > 0 && cp.Possibility is 3 or 6 or 9) return false;
        if (vShift < 0 && (cp.Possibility - 1) / 3 == 0) return false;
        if (vShift > 0 && (cp.Possibility - 1) / 3 == 2) return false;

        var newPossibility = cp.Possibility + vShift * 3 + hShift;
        return cps.Contains(new CellPossibility(cp.Row, cp.Column, newPossibility));
    }
}