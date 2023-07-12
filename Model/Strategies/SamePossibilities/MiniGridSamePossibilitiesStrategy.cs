using System.Collections.Generic;

namespace Model.Strategies.SamePossibilities;

public class MiniGridSamePossibilitiesStrategy : ISubStrategy
{
    public bool ApplyOnce(ISolver solver)
    {
        bool wasProgressMade = false;
        
        for(int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                foreach (var keyValuePair in GetDictionaryOfPossibilities(solver, miniRow, miniCol))
                {
                    if (keyValuePair.Key.Count == keyValuePair.Value)
                        wasProgressMade = RemovePossibilitiesFromMiniGrid(solver, miniRow, miniCol, keyValuePair.Key);
                }
            }
        }

        return wasProgressMade;
    }

    private List<IPossibilities> GetListOfPossibilities(ISolver solver, int miniRow, int miniCol)
    {
        List<IPossibilities> result = new();
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                int realRow = miniRow * 3 + row;
                int realCol = miniCol * 3 + col;
                if(solver.Sudoku[realRow, realCol] == 0) result.Add(solver.Possibilities[realRow, realCol]);
            }
        }
        

        return result;
    }
    
    private Dictionary<IPossibilities, int> GetDictionaryOfPossibilities(ISolver solver, int miniRow, int miniCol)
    {
        Dictionary<IPossibilities, int> result = new();
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                int realRow = miniRow * 3 + row;
                int realCol = miniCol * 3 + col;
                if (solver.Sudoku[realRow, realCol] == 0)
                {
                    if (!result.TryAdd(solver.Possibilities[realRow, realCol], 1))
                        result[solver.Possibilities[realRow, realCol]] += 1;
                }
            }
        }

        return result;
    }

    private bool RemovePossibilitiesFromMiniGrid(ISolver solver, int miniRow, int miniCol, IPossibilities toRemove)
    {
        bool wasProgressMade = false;
        
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                int realRow = miniRow * 3 + row;
                int realCol = miniCol * 3 + col;
                
                if (solver.Sudoku[realRow, realCol] == 0 && !solver.Possibilities[realRow, realCol].Equals(toRemove))
                {
                    foreach (var number in toRemove.GetPossibilities())
                    {
                        if (solver.RemovePossibility(number, realRow, realCol, 
                                new SamePossibilitiesLog(number, realRow, realCol))) wasProgressMade = true;
                    }
                }
            }
        }

        return wasProgressMade;
    }
}