using System.Windows;
using DesktopApplication.Presenter.Tectonics.Solve;

namespace DesktopApplication.View.Tectonics.Controls;

public partial class EmbeddedTectonicBoardControl
{ 
    public event OnDimensionCountChange? RowCountChanged;
    public event OnDimensionCountChange? ColumnCountChanged;
    public event OnCellSelection? CellSelected;
    public event OnCellSelection? CellAddedToSelection;
    public event OnSelectionEnd? SelectionEnded;

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

    private void OnCellSelection(int row, int col)
    {
        CellSelected?.Invoke(row, col);
    }

    private void OnCellAddedToSelection(int row, int col)
    {
        CellAddedToSelection?.Invoke(row, col);
    }

    private void OnSelectionEnd()
    {
        SelectionEnded?.Invoke();
    }
}