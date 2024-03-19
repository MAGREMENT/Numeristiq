using System.Windows;
using DesktopApplication.Presenter.Tectonic.Solve;

namespace DesktopApplication.View.Tectonic.Controls;

public partial class EmbeddedTectonicBoardControl //TODO size change bug
{ 
    public event OnDimensionCountChange? RowCountChanged;
    public event OnDimensionCountChange? ColumnCountChanged;

    public ITectonicDrawer Drawer => Board;
    
    public EmbeddedTectonicBoardControl()
    {
        InitializeComponent();
        
        AdjustCellSize();
    }

    private void OnRowCountChange(int number)
    {
        AdjustCellSize();
        RowCountChanged?.Invoke(number);
    }

    private void OnColumnCountChange(int number)
    {
        AdjustCellSize();
        ColumnCountChanged?.Invoke(number);
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

        var rowSize = ComputeSize(Board.RowCount);
        var columnSize = ComputeSize(Board.ColumnCount);
        
        var availableWidth = Width - Border.Padding.Left - Border.Padding.Right;
        var availableHeight = Height - Border.Padding.Top - Border.Padding.Bottom;

        Board.CellSize = availableHeight - rowSize < availableWidth - columnSize 
            ? ComputeOptimalCellSize(availableHeight, Board.RowCount) 
            : ComputeOptimalCellSize(availableWidth, Board.ColumnCount);
    }
    
    private double ComputeSize(int number)
    {
        return Board.CellSize * number + Board.BigLineWidth * (number + 1);
    }

    private int ComputeOptimalCellSize(double space, int number)
    {
        return (int) ((space - Board.BigLineWidth * (number + 1)) / number);
    }
}