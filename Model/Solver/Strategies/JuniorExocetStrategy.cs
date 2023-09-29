using System.Collections.Generic;
using Model.Solver.Helpers.Changes;
using Model.Solver.Positions;
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
        var jes = JuniorExocet.SearchFullGrid(strategyManager);
        
        foreach (var je in jes)
        {
            Process(strategyManager, je);
        }

        for (int i = 0; i < jes.Count - 1; i++)
        {
            for (int j = i + 1; j < jes.Count; j++)
            {
                
            }
        }
    }

    private void ProcessDouble(IStrategyManager strategyManager, JuniorExocet je1, JuniorExocet je2)
    {
        if(!je1.BaseCandidates.Equals(je2.BaseCandidates) || je1.GetUnit() != je2.GetUnit()) return;

        LinePositions lp1 = new();
        LinePositions lp2 = new();
        if (je1.GetUnit() == Unit.Row)
        {
            lp1.Add(je1.Target1.Col);
            lp1.Add(je1.Target2.Col);
            lp1.Add(je1.EscapeCell.Col);
            lp2.Add(je2.Target1.Col);
            lp2.Add(je2.Target2.Col);
            lp2.Add(je2.EscapeCell.Col);
        }
        else
        {
            lp1.Add(je1.Target1.Row);
            lp1.Add(je1.Target2.Row);
            lp1.Add(je1.EscapeCell.Row);
            lp2.Add(je2.Target1.Row);
            lp2.Add(je2.Target2.Row);
            lp2.Add(je2.EscapeCell.Row);
        }

        if (!lp1.Equals(lp2)) return;
        
        
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
        List<Cell> sCells = _je.GetSCells();

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

public class DoubleJuniorExocetReportBuilder : IChangeReportBuilder
{
    private readonly JuniorExocet _je1;
    private readonly JuniorExocet _je2;

    public DoubleJuniorExocetReportBuilder(JuniorExocet je1, JuniorExocet je2)
    {
        _je1 = je1;
        _je2 = je2;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        List<Cell> sCells = _je1.GetSCells();

        List<CellPossibility> sPossibilities = new();
        foreach (var cell in sCells)
        {
            foreach (var possibility in _je1.BaseCandidates)
            {
                if(snapshot.PossibilitiesAt(cell).Peek(possibility)) sPossibilities.Add(new CellPossibility(cell, possibility));
            }
        }

        List<Cell> sSolved = new();
        foreach (var cell in sCells)
        {
            if (_je1.BaseCandidates.Peek(snapshot.Sudoku[cell.Row, cell.Col])) sSolved.Add(cell);
        }
        
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            lighter.HighlightCell(_je1.Base1, ChangeColoration.CauseOffOne);
            lighter.HighlightCell(_je1.Base2, ChangeColoration.CauseOffOne);
            
            lighter.HighlightCell(_je1.Target1, ChangeColoration.CauseOffTwo);
            lighter.HighlightCell(_je1.Target2, ChangeColoration.CauseOffTwo);
            
            lighter.HighlightCell(_je2.Base1, ChangeColoration.CauseOffThree);
            lighter.HighlightCell(_je2.Base2, ChangeColoration.CauseOffThree);
            
            lighter.HighlightCell(_je2.Target1, ChangeColoration.CauseOffFour);
            lighter.HighlightCell(_je2.Target2, ChangeColoration.CauseOffFour);

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