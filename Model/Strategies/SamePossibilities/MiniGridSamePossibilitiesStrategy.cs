using System.Collections.Generic;

namespace Model.Strategies.SamePossibilities;

public class MiniGridSamePossibilitiesStrategy : ISolverStrategy
{
    public bool ApplyOnce(ISolver solver)
    {
        bool wasProgressMade = false;
        
        for(int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                var listOfPossibilities = GetListOfPossibilities(solver, miniRow, miniCol);

                if (listOfPossibilities.Count != 0)
                {
                    listOfPossibilities.Sort((poss1, poss2) => poss1.Count - poss2.Count);
                
                    List<CellPossibilities> currentList = new() { listOfPossibilities[0] };

                    for (int i = 1; i < listOfPossibilities.Count; i++)
                    {
                        var current = listOfPossibilities[i];

                        if (current.Count != currentList[0].Count || !current.Equals(currentList[0])) currentList.Clear();
                        currentList.Add(current);

                        if (currentList.Count == currentList[0].Count)
                        {
                            wasProgressMade = RemovePossibilitiesFromMiniGrid(solver, miniRow, miniCol, currentList[0]);
                        }
                    }
                }
            }
        }

        return wasProgressMade;
    }

    private List<CellPossibilities> GetListOfPossibilities(ISolver solver, int miniRow, int miniCol)
    {
        List<CellPossibilities> result = new();
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

    private bool RemovePossibilitiesFromMiniGrid(ISolver solver, int miniRow, int miniCol, CellPossibilities toRemove)
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
                        if (solver.RemovePossibility(number, realRow, realCol)) wasProgressMade = true;
                    }
                }
            }
        }

        return wasProgressMade;
    }
}