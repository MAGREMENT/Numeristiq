using System.Collections.Generic;
using Model.Solver.Helpers.Changes;
using Model.Solver.StrategiesUtil;

namespace Model.Solver.Strategies;

public class JuniorExocetStrategy : AbstractStrategy //TODO other elims
{
    public const string OfficialName = "Junior Exocet";

    public JuniorExocetStrategy() : base(OfficialName, StrategyDifficulty.Hard)
    {
        UniquenessDependency = UniquenessDependency.PartiallyDependent;
    }

    public override void ApplyOnce(IStrategyManager strategyManager)
    {
        foreach (var je in JuniorExocet.SearchFullGrid(strategyManager))
        {
            Process(strategyManager, je);
        }
    }

    private void Process(IStrategyManager strategyManager, JuniorExocet je)
    {
        //---Base candidates rules---
        
        //Rule 1
        foreach (var sCell in je.SCells)
        {
            if ((sCell.FromTarget1.Count > 0 && sCell.FromTarget2.Count == 0 && sCell.FromEscapeCell.Count == 0)
                || (sCell.FromTarget1.Count == 0 && sCell.FromTarget2.Count > 0 && sCell.FromEscapeCell.Count == 0)
                || (sCell.FromTarget1.Count == 0 && sCell.FromTarget2.Count == 0 && sCell.FromEscapeCell.Count > 0)
                || sCell.FromTarget1.Or(sCell.FromTarget2).Or(sCell.FromEscapeCell).Count == 1)
            {
                strategyManager.ChangeBuffer.AddPossibilityToRemove(sCell.Possibility, je.Target1.Row, je.Target1.Col);
                strategyManager.ChangeBuffer.AddPossibilityToRemove(sCell.Possibility, je.Target2.Row, je.Target2.Col);
                strategyManager.ChangeBuffer.AddPossibilityToRemove(sCell.Possibility, je.Base1.Row, je.Base1.Col);
                strategyManager.ChangeBuffer.AddPossibilityToRemove(sCell.Possibility, je.Base2.Row, je.Base2.Col);
                strategyManager.ChangeBuffer.AddPossibilityToRemove(sCell.Possibility, je.EscapeCell.Row,
                    je.EscapeCell.Col);
            }
        }
        
        //Rule 2
        var unit = je.GetUnit();
        var t1Mirror = JuniorExocet.GetMirrorNodes(je.Target2, unit); //Yes, it's normal that it is je.Target2
        var t2Mirror = JuniorExocet.GetMirrorNodes(je.Target1, unit); 
        
        foreach (var possibility in je.BaseCandidates)
        {
            if (!PossibilityPeekOrIsSolved(strategyManager, je.Target1, possibility))
            {
                foreach (var mirror in t1Mirror)
                {
                    strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, mirror.Row, mirror.Col);
                }
            }
            
            if (!PossibilityPeekOrIsSolved(strategyManager, je.Target2, possibility))
            {
                foreach (var mirror in t2Mirror)
                {
                    strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, mirror.Row, mirror.Col);
                }
            }
            
            if(!PossibilityPeekOrIsSolved(strategyManager, t1Mirror[0], possibility) 
               && !PossibilityPeekOrIsSolved(strategyManager, t1Mirror[1], possibility))
                strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, je.Target1.Row, je.Target1.Col);
            
            if(!PossibilityPeekOrIsSolved(strategyManager, t2Mirror[0], possibility) 
               && !PossibilityPeekOrIsSolved(strategyManager, t2Mirror[1], possibility))
                strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, je.Target2.Row, je.Target2.Col);
        }
        
        //Rule 3
        for (int possibility = 1; possibility <= 9; possibility++)
        {
            if(je.BaseCandidates.Peek(possibility)) continue;
            
            strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, je.Target1.Row, je.Target1.Col);
            strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, je.Target2.Row, je.Target2.Col);
        }
        
        //Rule 4 -> TODO add in LinkGraph
        
        //Rule 5
        foreach (var sCell in je.SCells)
        {
            if (sCell.IsDoubleParallel()) continue;
            
            if (sCell.FromTarget1.Count == 0 && sCell.FromTarget2.Count > 0)
                strategyManager.ChangeBuffer.AddPossibilityToRemove(sCell.Possibility, je.Target2.Row, je.Target2.Col);

            if (sCell.FromTarget2.Count == 0 && sCell.FromTarget1.Count > 0)
                strategyManager.ChangeBuffer.AddPossibilityToRemove(sCell.Possibility, je.Target1.Row, je.Target1.Col);
            

            if (sCell.FromEscapeCell.Count == 0)
            {
                if(sCell.FromTarget2.Count > 0)
                    strategyManager.ChangeBuffer.AddPossibilityToRemove(sCell.Possibility, je.Target2.Row, je.Target2.Col);
                if(sCell.FromTarget1.Count > 0)
                    strategyManager.ChangeBuffer.AddPossibilityToRemove(sCell.Possibility, je.Target1.Row, je.Target1.Col);
            }

            if (SCells.IsPerpendicularWithOneParallel(sCell.FromTarget1, sCell.FromTarget2, sCell.FromEscapeCell))
                strategyManager.ChangeBuffer.AddPossibilityToRemove(sCell.Possibility, je.Target1.Row, je.Target1.Col);
            
            if (SCells.IsPerpendicularWithOneParallel(sCell.FromTarget2, sCell.FromTarget1, sCell.FromEscapeCell))
                strategyManager.ChangeBuffer.AddPossibilityToRemove(sCell.Possibility, je.Target2.Row, je.Target2.Col);
        }
        
        //Rule 6 -> Done in rule 2
        
        if (strategyManager.ChangeBuffer.NotEmpty())
            strategyManager.ChangeBuffer.Push(this, new JuniorExocetReportBuilder(je));
        
        //Incompatibility test TODO
        if (!strategyManager.UniquenessDependantStrategiesAllowed) return; 

        //---Known true digits rule---

        //Rule 7

        //Rule 8

        //Rule 9

        //Rule 10

        //Rule 11

        //Rule 12
    }
    
    private static bool PossibilityPeekOrIsSolved(IStrategyManager strategyManager, Cell cell, int possibility)
    {
        return strategyManager.Sudoku[cell.Row, cell.Col] == possibility ||
               strategyManager.PossibilitiesAt(cell).Peek(possibility);
    }
}

public class JuniorExocetReportBuilder : IChangeReportBuilder
{
    private readonly JuniorExocet _je;

    public JuniorExocetReportBuilder(JuniorExocet je)
    {
        _je = je;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        List<Cell> sCells = new();
        if (_je.GetUnit() == Unit.Row)
        {
            for (int row = 0; row < 9; row++)
            {
                if (row / 3 == _je.Base1.Row / 3) continue;
                
                sCells.Add(new Cell(row, _je.Target1.Col));
                sCells.Add(new Cell(row, _je.Target2.Col));
                sCells.Add(new Cell(row, _je.EscapeCell.Col));
            }
        }
        else
        {
            for (int col = 0; col < 9; col++)
            {
                if (col / 3 == _je.Base1.Col / 3) continue;
                
                sCells.Add(new Cell(_je.Target1.Row, col));
                sCells.Add(new Cell(_je.Target2.Row, col));
                sCells.Add(new Cell(_je.EscapeCell.Row, col));
            }
        }

        List<CellPossibility> sPossibilities = new();
        foreach (var cell in sCells)
        {
            foreach (var possibility in _je.BaseCandidates)
            {
                if(snapshot.PossibilitiesAt(cell).Peek(possibility)) sPossibilities.Add(new CellPossibility(cell, possibility));
            }
        }

        List<Cell> sSolved = new();
        foreach (var cell in sCells)
        {
            if (_je.BaseCandidates.Peek(snapshot.Sudoku[cell.Row, cell.Col])) sSolved.Add(cell);
        }

        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            lighter.HighlightCell(_je.Base1, ChangeColoration.CauseOffOne);
            lighter.HighlightCell(_je.Base2, ChangeColoration.CauseOffOne);
            
            lighter.HighlightCell(_je.Target1, ChangeColoration.CauseOffTwo);
            lighter.HighlightCell(_je.Target2, ChangeColoration.CauseOffTwo);

            foreach (var cell in sCells)
            {
                lighter.HighlightCell(cell, ChangeColoration.CauseOffThree);
            }

            foreach (var cp in sPossibilities)
            {
                lighter.HighlightPossibility(cp, ChangeColoration.Neutral);
            }

            foreach (var cell in sSolved)
            {
                lighter.HighlightCell(cell, ChangeColoration.Neutral);
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}