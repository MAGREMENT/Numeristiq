using System.Windows;
using DesktopApplication.Presenter.Tectonic.Solve;

namespace DesktopApplication.View.Tectonic.Controls;

public partial class EmbeddedTectonicBoardControl
{
    private double _rowSize;
    private double _columnSize;

    public ITectonicDrawer Drawer => Board;
    
    public EmbeddedTectonicBoardControl()
    {
        InitializeComponent();

        _rowSize = ComputeSize(Board.RowCount);
        _columnSize = ComputeSize(Board.ColumnCount);
        AdjustCellSize();
    }

    private void OnRowCountChange(int number)
    {
        _rowSize = ComputeSize(number);
        AdjustCellSize();
    }

    private void OnColumnCountChange(int number)
    {
        _columnSize = ComputeSize(number);
        AdjustCellSize();
    }

    private double ComputeSize(int number)
    {
        return Board.CellSize * number + Board.BigLineWidth * (number + 1);
    }

    private void AdjustCellSize()
    {
        if (Board.RowCount == 0 || Board.ColumnCount == 0)
        {
            Board.Visibility = Visibility.Collapsed;
            None.Visibility = Visibility.Visible;
            return;
        }
        
        if (Width is double.NaN || Height is double.NaN) return;
        
        None.Visibility = Visibility.Collapsed;
        Board.Visibility = Visibility.Visible;
        
        var availableWidth = Width - Border.Padding.Left - Border.Padding.Right;
        var availableHeight = Height - Border.Padding.Top - Border.Padding.Bottom;

        Board.CellSize = availableHeight - _rowSize < availableWidth - _columnSize 
            ? ComputeOptimalCellSize(availableHeight, Board.RowCount) 
            : ComputeOptimalCellSize(availableWidth, Board.ColumnCount);
    }

    private int ComputeOptimalCellSize(double space, int number)
    {
        return (int) ((space - Board.BigLineWidth * (number + 1)) / number);
    }
}