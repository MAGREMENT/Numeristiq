using System.Collections.Generic;

namespace Model.Strategies.SinglePossibility;

public class CellSinglePossibilityStrategy : ISubStrategy
{
    public bool ApplyOnce(ISolver solver)
    {
        bool wasProgressMade = false;

        for (int i = 0; i < 9; i++) 
        {
            for (int j = 0; j < 9; j++)
            {
                if (solver.Sudoku[i, j] != 0) continue;
                
                if (solver.Possibilities[i, j].Count == 1)
                {
                    int n = solver.Possibilities[i, j].GetFirst();
                    solver.AddDefinitiveNumber(n,
                        i, j, new SinglePossibilityLog(n, i, j));
                    wasProgressMade = true;
                }
            }
        }
        
        return wasProgressMade;
    }
}