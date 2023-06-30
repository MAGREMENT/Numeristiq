using System.Collections.Generic;

namespace Model.Strategies.SinglePossibility;

public class CellSinglePossibilityStrategy : ISolverStrategy
{
    public bool ApplyOnce(ISolver solver)
    {
        bool wasProgressMade = false;

        for (int i = 0; i < 9; i++) 
        {
            for (int j = 0; j < 9; j++)
            {
                if (solver.Sudoku[i, j] != 0) continue;
                
                List<int> poss = solver.Possibilities[i, j].GetPossibilities();
                if (poss.Count == 1)
                {
                    solver.AddDefinitiveNumber(poss[0], i, j);
                    wasProgressMade = true;
                }
            }
        }
        
        return wasProgressMade;
    }
}