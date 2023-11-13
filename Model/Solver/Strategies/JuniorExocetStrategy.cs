using System.Collections.Generic;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Highlighting;
using Model.Solver.Position;
using Model.Solver.Possibility;
using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.Exocet;

namespace Model.Solver.Strategies;

public class JuniorExocetStrategy : AbstractStrategy //TODO other elims
{
    public const string OfficialName = "Junior Exocet";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;

    public JuniorExocetStrategy() : base(OfficialName, StrategyDifficulty.Extreme, DefaultBehavior)
    {
        UniquenessDependency = UniquenessDependency.PartiallyDependent;
    }

    public override void Apply(IStrategyManager strategyManager)
    {
        var jes = strategyManager.PreComputer.JuniorExocet();
        
        foreach (var je in jes)
        {
            if (Process(strategyManager, je)) return;
        }

        for (int i = 0; i < jes.Count - 1; i++)
        {
            for (int j = i + 1; j < jes.Count; j++)
            {
                if (ProcessDouble(strategyManager, jes[i], jes[j])) return;
            }
        }
    }

    private bool ProcessDouble(IStrategyManager strategyManager, JuniorExocet je1, JuniorExocet je2)
    {
        //TODO

        return strategyManager.ChangeBuffer.Commit(this, new DoubleJuniorExocetReportBuilder(je1, je2))
            && OnCommitBehavior == OnCommitBehavior.Return;
    }

    private bool Process(IStrategyManager strategyManager, JuniorExocet je)
    {
        Possibilities[] removedBaseCandidates = { Possibilities.NewEmpty(), Possibilities.NewEmpty() };
        
        //Elimination 1
        foreach (var entry in je.SCells)
        {
            if (!entry.Value.CanBeCoveredByLines(1, Unit.Row, Unit.Column)) continue;
            
            strategyManager.ChangeBuffer.ProposePossibilityRemoval(entry.Key, je.Base1);
            strategyManager.ChangeBuffer.ProposePossibilityRemoval(entry.Key, je.Base2);
            strategyManager.ChangeBuffer.ProposePossibilityRemoval(entry.Key, je.EscapeCell);
            strategyManager.ChangeBuffer.ProposePossibilityRemoval(entry.Key, je.Target1);
            strategyManager.ChangeBuffer.ProposePossibilityRemoval(entry.Key, je.Target2);

            removedBaseCandidates[0].Add(entry.Key);
            removedBaseCandidates[1].Add(entry.Key);
        }
        
        //Elimination 2
        foreach (var possibility in je.BaseCandidates)
        {
            if (!strategyManager.PossibilitiesAt(je.Target1).Peek(possibility))
            {
                foreach (var cell in je.Target1MirrorNodes)
                {
                    strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, cell);
                }
            }
            
            if (!strategyManager.PossibilitiesAt(je.Target2).Peek(possibility))
            {
                foreach (var cell in je.Target2MirrorNodes)
                {
                    strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, cell);
                }
            }
        }
        
        //Elimination 3
        foreach (var possibility in strategyManager.PossibilitiesAt(je.Target1))
        {
            if (!je.BaseCandidates.Peek(possibility))
                strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, je.Target1);
        }

        foreach (var possibility in strategyManager.PossibilitiesAt(je.Target2))
        {
            if (!je.BaseCandidates.Peek(possibility))
                strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, je.Target2);
        }
        
        //Elimination 4 => In LinkGraph
        
        //Elimination 5
        var methods = UnitMethods.GetMethods(je.GetUnit() == Unit.Row ? Unit.Column : Unit.Row);
        foreach (var entry in je.SCells)
        {
            if (entry.Value.CanBeCoveredByLines(2, je.GetUnit())) continue;

            //1 cover house goes by target 1
            var copy = entry.Value.Copy();
            methods.Void(copy, je.Target1);
            if (copy.CanBeCoveredByLines(1, je.GetUnit()))
            {
                strategyManager.ChangeBuffer.ProposePossibilityRemoval(entry.Key, je.Target1);
                continue;
            }
            
            //1 cover house goes by target 2
            copy = entry.Value.Copy();
            methods.Void(copy, je.Target2);
            if (copy.CanBeCoveredByLines(1, je.GetUnit()))
            {
                strategyManager.ChangeBuffer.ProposePossibilityRemoval(entry.Key, je.Target2);
                continue;
            }
            
            //1 cover house goes by escape cell => no eliminations
            copy = entry.Value.Copy();
            methods.Void(copy, je.EscapeCell);
            if (copy.CanBeCoveredByLines(1, je.GetUnit())) continue;
            
            //If arrived here, then the cover houses must both be perpendicular to the JE band
            if(methods.Count(entry.Value, je.Target1) > 0)
                strategyManager.ChangeBuffer.ProposePossibilityRemoval(entry.Key, je.Target1);
            
            if(methods.Count(entry.Value, je.Target2) > 0)
                strategyManager.ChangeBuffer.ProposePossibilityRemoval(entry.Key, je.Target2);
        }
        
        //Elimination 6
        foreach (var possibility in je.BaseCandidates)
        {
            bool ok = false;
            foreach (var cell in je.Target1MirrorNodes)
            {
                if (strategyManager.Contains(cell.Row, cell.Col, possibility)) ok = true;
            }

            if (!ok) strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, je.Target1);
            
            ok = false;
            foreach (var cell in je.Target2MirrorNodes)
            {
                if (strategyManager.Contains(cell.Row, cell.Col, possibility)) ok = true;
            }

            if (!ok) strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, je.Target2);
        }

        //Compatibility check
        if (je.BaseCandidates.Count != 2)
        {
            HashSet<BiValue> forbiddenPairs = new();

            int i = 0;
            while (je.BaseCandidates.Next(ref i))
            {
                int j = i;
                while (je.BaseCandidates.Next(ref j))
                {
                    if (!je.CompatibilityCheck(strategyManager, i, j)) forbiddenPairs.Add(new BiValue(i, j));
                }
            }

            foreach (var p1 in strategyManager.PossibilitiesAt(je.Base1))
            {
                bool ok = false;
                foreach (var p2 in strategyManager.PossibilitiesAt(je.Base2))
                {
                    if(p1 == p2) continue;
                
                    if (!forbiddenPairs.Contains(new BiValue(p1, p2)))
                    {
                        ok = true;
                        break;
                    }
                }

                if (!ok)
                {
                    strategyManager.ChangeBuffer.ProposePossibilityRemoval(p1, je.Base1.Row, je.Base1.Col);
                    removedBaseCandidates[0].Add(p1);
                }
            }
        
            foreach (var p2 in strategyManager.PossibilitiesAt(je.Base2))
            {
                bool ok = false;
                foreach (var p1 in strategyManager.PossibilitiesAt(je.Base1))
                {
                    if(p1 == p2) continue;

                    if (!forbiddenPairs.Contains(new BiValue(p2, p1)))
                    {
                        ok = true;
                        break;
                    }
                }

                if (!ok)
                {
                    strategyManager.ChangeBuffer.ProposePossibilityRemoval(p2, je.Base2.Row, je.Base2.Col);
                    removedBaseCandidates[1].Add(p2);
                }
            }
        }

        //Known base digits eliminations
        var revisedB1 = strategyManager.PossibilitiesAt(je.Base1).Difference(removedBaseCandidates[0]);
        var revisedB2 = strategyManager.PossibilitiesAt(je.Base2).Difference(removedBaseCandidates[1]);
        var revisedBaseCandidates = revisedB1.Or(revisedB2);

        if (revisedBaseCandidates.Count == 2)
        {
            //Elimination 7
            for (int i = 0; i < 2; i++)
            {
                if (!strategyManager.ContainsAny(je.Target1MirrorNodes[i], revisedBaseCandidates))
                    RemoveAll(strategyManager, je.Target1MirrorNodes[(i + 1) % 2], strategyManager.PossibilitiesAt(je.Target2));
                
                if(!strategyManager.ContainsAny(je.Target2MirrorNodes[i], revisedBaseCandidates))
                    RemoveAll(strategyManager, je.Target2MirrorNodes[(i + 1) % 2], strategyManager.PossibilitiesAt(je.Target1));
            }
            
            //Elimination 8
            var or = strategyManager.PossibilitiesAt(je.Target1MirrorNodes[0])
                .Or(strategyManager.PossibilitiesAt(je.Target1MirrorNodes[1]));
            var diff = or.Difference(revisedBaseCandidates);
            if (diff.Count == 1)
            {
                var p = diff.First();
                foreach (var cell in Cells.SharedSeenCells(je.Target1MirrorNodes[0], je.Target1MirrorNodes[1]))
                {
                    strategyManager.ChangeBuffer.ProposePossibilityRemoval(p, cell);
                }
            }
                
            or = strategyManager.PossibilitiesAt(je.Target2MirrorNodes[0])
                .Or(strategyManager.PossibilitiesAt(je.Target2MirrorNodes[1]));
            diff = or.Difference(revisedBaseCandidates);
            if (diff.Count == 1)
            {
                var p = diff.First();
                foreach (var cell in Cells.SharedSeenCells(je.Target2MirrorNodes[0], je.Target2MirrorNodes[1]))
                {
                    strategyManager.ChangeBuffer.ProposePossibilityRemoval(p, cell);
                }
            }
            
            //Elimination 9 => ???
            
            //Elimination 10
            foreach (var possibility in revisedBaseCandidates)
            {
                foreach (var cell in Cells.SharedSeenCells(je.Base1, je.Base2))
                {
                    strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, cell);
                }

                foreach (var cell in Cells.SharedSeenCells(je.Target1, je.Target2))
                {
                    strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, cell);
                }
            }
            
            
            //Elimination 11
            var unit = je.GetUnit();
            var miniUnit = unit == Unit.Row ? je.Base1.Row / 3 : je.Base1.Col / 3;
            var m = UnitMethods.GetMethods(unit);
            foreach (var entry in je.SCells)
            {
                for (int u = 0; u < 9; u++)
                {
                    if (u / 3 == miniUnit) continue;

                    var c = unit == Unit.Row ? new Cell(u, 0) : new Cell(0, u);
                    if (m.Count(entry.Value, c) == 0) continue;

                    var copy = entry.Value.Copy();
                    m.Void(copy, c);
                    if (!copy.CanBeCoveredByLines(1, Unit.Row, Unit.Column)) continue;

                    for (int o = 0; o < 9; o++)
                    {
                        var eliminationCell = unit == Unit.Row ? new Cell(u, o) : new Cell(o, u);
                        if (entry.Value.Peek(eliminationCell)) continue;
                        
                        strategyManager.ChangeBuffer.ProposePossibilityRemoval(entry.Key, eliminationCell);
                    }
                }
            }
            
            //Elimination 12 TODO
        }

        return strategyManager.ChangeBuffer.Commit(this, new JuniorExocetReportBuilder(je))
               && OnCommitBehavior == OnCommitBehavior.Return;
    }

    private void RemoveAll(IStrategyManager strategyManager, Cell cell, IReadOnlyPossibilities except)
    {
        foreach (var possibility in strategyManager.PossibilitiesAt(cell))
        {
            if (except.Peek(possibility)) continue;

            strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, cell);
        }
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
            
            lighter.HighlightCell(_je2.Base1, ChangeColoration.CauseOffFive);
            lighter.HighlightCell(_je2.Base2, ChangeColoration.CauseOffFive);
            
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