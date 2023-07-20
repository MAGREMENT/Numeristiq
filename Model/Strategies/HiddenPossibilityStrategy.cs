using System.Collections.Generic;
using Model.Positions;

namespace Model.Strategies;

public class HiddenPossibilityStrategy : IStrategy
{
    public string Name { get; }

    public StrategyLevel Difficulty { get; }
    
    private readonly int _type;

    public HiddenPossibilityStrategy(int type)
    {
        _type = type;
        switch (type)
        {
            case 1 : Name = "Hidden single";
                Difficulty = StrategyLevel.Basic;
                break;
            case 2 : Name = "Hidden double";
                Difficulty = StrategyLevel.Easy;
                break;
            case 3 : Name = "Hidden triple";
                Difficulty = StrategyLevel.Easy;
                break;
            case 4 : Name = "Hidden quad";
                Difficulty = StrategyLevel.Medium;
                break;
            default : Name = "Hidden unknown";
                Difficulty = StrategyLevel.None;
                break;
        }
    }
    
    public void ApplyOnce(ISolverView solverView)
    {
        //Rows
        for (int row = 0; row < 9; row++)
        {
            Dictionary<LinePositions, List<int>> possibilitiesToExamine = new();
            for (int number = 1; number <= 9; number++)
            {
                var positions = solverView.PossibilityPositionsInRow(row, number);
                if (positions.Count == _type)
                {
                    if (!possibilitiesToExamine.TryAdd(positions, new List<int> { number }))
                    {
                        possibilitiesToExamine[positions].Add(number);
                    }
                }
            }

            foreach (var entry in possibilitiesToExamine)
            {
                if (entry.Value.Count == _type)
                {
                    foreach (var col in entry.Key)
                    {
                        RemoveAllPossibilitiesExcept(solverView, row, col, entry.Value);
                    }
                }
            }
        }
        
        //Columns
        for (int col = 0; col < 9; col++)
        {
            Dictionary<LinePositions, List<int>> possibilitiesToExamine = new();
            for (int number = 1; number <= 9; number++)
            {
                var positions = solverView.PossibilityPositionsInColumn(col, number);
                if (positions.Count == _type)
                {
                    if (!possibilitiesToExamine.TryAdd(positions, new List<int> { number }))
                    {
                        possibilitiesToExamine[positions].Add(number);
                    }
                }
            }
            
            foreach (var entry in possibilitiesToExamine)
            {
                if (entry.Value.Count == _type)
                {
                    foreach (var row in entry.Key)
                    {
                        RemoveAllPossibilitiesExcept(solverView, row, col, entry.Value);
                    }
                }
            }
        }
        
        //MiniGrid
        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                Dictionary<MiniGridPositions, List<int>> possibilitiesToExamine = new();
                for (int number = 1; number <= 9; number++)
                {
                    var positions = solverView.PossibilityPositionsInMiniGrid(miniRow, miniCol, number);
                    if (positions.Count == _type)
                    {
                        if (!possibilitiesToExamine.TryAdd(positions, new List<int> { number }))
                        {
                            possibilitiesToExamine[positions].Add(number);
                        }
                    }
                }
                
                foreach (var entry in possibilitiesToExamine)
                {
                    if (entry.Value.Count == _type)
                    {
                        foreach (var pos in entry.Key)
                        {
                            RemoveAllPossibilitiesExcept(solverView, pos[0], pos[1], entry.Value);
                        }
                    }
                }
            }
        }
    }

    private void RemoveAllPossibilitiesExcept(ISolverView solverView, int row, int col, List<int> except)
    {
        if (_type == 1) solverView.AddDefinitiveNumber(except[0], row, col, this);
        for (int i = 1; i <= 9; i++)
        {
            if (!except.Contains(i))
            {
                solverView.RemovePossibility(i, row, col, this);
            }
        }
    }
}