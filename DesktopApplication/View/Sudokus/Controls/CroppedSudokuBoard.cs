using System;
using Model.Sudokus.Solver.Descriptions;

namespace DesktopApplication.View.Sudokus.Controls;

public class CroppedSudokuBoard : SudokuBoard
{
    private readonly SudokuCropping _cropping;
    
    public CroppedSudokuBoard(SudokuCropping cropping)
    {
        _cropping = cropping;
        Layers[LinesIndex][0] = new SudokuGridDrawableComponent(_cropping);
    }

    public override double GetLeftOfPossibility(int col, int possibility)
    {
        return base.GetLeftOfPossibility(col, possibility) - GetLeftCropping();
    }

    public override double GetTopOfPossibility(int row, int possibility)
    {
        return base.GetTopOfPossibility(row, possibility) - GetTopCropping();
    }

    public override double GetLeftOfCell(int col)
    {
        return base.GetLeftOfCell(col) - GetLeftCropping();
    }

    public override double GetTopOfCell(int row)
    {
        return base.GetTopOfCell(row) - GetTopCropping();
    }
    
    protected override void UpdateSize(bool fireEvent)
    {
        var width = GetLeftOfCell(_cropping.ColumnTo + 1) - GetLeftOfCellWithBorder(_cropping.ColumnFrom);
        var height = GetTopOfCell(_cropping.RowTo + 1) - GetTopOfCell(_cropping.RowFrom);
        if (Math.Abs(Width - width) < 0.01 && Math.Abs(Height - height) < 0.01) return;
        
        Width = width;
        Height = height;
        Refresh();
        
        if(fireEvent) FireOptimizableSizeChanged();
    }

    private double GetLeftCropping()
    {
        var miniCol = _cropping.ColumnFrom / 3;
        return _cropping.ColumnFrom * CellSize + miniCol * BigLineWidth + BigLineWidth 
               + (_cropping.ColumnFrom - miniCol) * SmallLineWidth
               - (_cropping.ColumnFrom % 3 == 0 ? BigLineWidth : SmallLineWidth);
    }

    private double GetTopCropping()
    {
        var miniRow = _cropping.RowFrom / 3;
        return _cropping.RowFrom * CellSize + miniRow * BigLineWidth + BigLineWidth 
            + (_cropping.RowFrom - miniRow) * SmallLineWidth
            - (_cropping.RowFrom % 3 == 0 ? BigLineWidth : SmallLineWidth);
    }
}