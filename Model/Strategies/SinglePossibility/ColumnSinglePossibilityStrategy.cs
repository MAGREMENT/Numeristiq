namespace Model.Strategies.SinglePossibility;

public class ColumnSinglePossibilityStrategy : ISubStrategy
{
    public bool ApplyOnce(ISolver solver)
    {
        bool wasProgressMade = false;
        
        for (int col = 0; col < 9; col++)
        {
            for (int n = 1; n <= 9; n++)
            {
                int pos = CheckColumnForUnique(solver, col, n);
                if (pos != -1)
                {
                    solver.AddDefinitiveNumber(n, pos, col);
                    wasProgressMade = true;
                }
            } 
        }
        
        return wasProgressMade;
    }
    
    private int CheckColumnForUnique(ISolver solver, int col, int number)
    {
        int buffer = -1;

        for (int i = 0; i < 9; i++)
        {
            if (solver.Sudoku[i, col] == number) return -1;
            if (solver.Possibilities[i, col].Peek(number) && solver.Sudoku[i, col] == 0)
            {
                if (buffer != -1) return -1;
                buffer = i;
            }
        }

        return buffer;
    }
}