using Model.Sudokus;

namespace DesktopApplication.View.HelperWindows.Dialog;

public partial class ShowSudokuDialog
{
    public ShowSudokuDialog(Sudoku sudoku)
    {
        InitializeComponent();

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if(sudoku[row, col] != 0) Board.ShowSolution(row, col, sudoku[row, col]);
            }
        }
        
        Board.Refresh();
    }
}