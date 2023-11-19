using System.Collections.Generic;
using Global;
using Global.Enums;
using Model.Solver.Helpers.Changes;
using Model.Solver.Possibility;
using Model.Solver.StrategiesUtility;
using Model.Solver.StrategiesUtility.Exocet;

namespace Model.Solver.Strategies;

public class JuniorExocetStrategy : AbstractStrategy //TODO BUG FIX => 007020004930000600600300000000000050200010008006900400003700900020050001000008000
{
    public const string OfficialName = "Junior Exocet";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.WaitForAll;
    
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

    private bool ProcessDouble(IStrategyManager strategyManager, JuniorExocet je1, JuniorExocet je2) //TODO : correct this
    {
        var unit = je1.GetUnit();
        if (unit != je2.GetUnit()) return false;

        if ((unit == Unit.Row && je1.Base1.Row / 3 != je2.Base1.Row / 3) ||
            (unit == Unit.Column && je1.Base1.Col / 3 != je2.Base1.Col / 3)) return false;
        
        HashSet<Cell> baseCells = new()
        {
            je1.Base1,
            je1.Base2,
            je2.Base1,
            je2.Base2
        };

        if (baseCells.Count != 4) return false;

        HashSet<Cell> targetCells = new()
        {
            je1.Target1,
            je1.Target2,
            je2.Target1,
            je2.Target2,
        };

        bool commonTarget = targetCells.Count == 3;

        targetCells.Remove(je1.Base1);
        targetCells.Remove(je1.Base2);
        targetCells.Remove(je2.Base1);
        targetCells.Remove(je2.Base2);

        bool baseAndTargetCommon = !commonTarget && targetCells.Count == 3;

        var or = je1.BaseCandidates.Or(je2.BaseCandidates);
        if (targetCells.Count != or.Count) return false;

        //Elimination 0
        if (commonTarget || baseAndTargetCommon)
        {
            var cell = FindFirstTargetNotIn(je1, je2, targetCells);
            var and = je1.BaseCandidates.And(je2.BaseCandidates);

            foreach (var possibility in strategyManager.PossibilitiesAt(cell))
            {
                if (!and.Peek(possibility)) strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, cell);
            }
        }

        //Elimination 1
        var bCells = new List<Cell>(baseCells);
        var tCells = new List<Cell>(targetCells);
        
        foreach (var possibility in or)
        {
            foreach (var cell in Cells.SharedSeenCells(bCells))
            {
                strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, cell);
            }
            
            foreach (var cell in Cells.SharedSeenCells(tCells))
            {
                strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, cell);
            }
        }

        //Elimination 2
        var crossLines = je1.SCellsLinePositions().Or(je2.SCellsLinePositions());
        if (crossLines.Count == 3)
        {
            RemoveAllNonSCells(strategyManager, je1, je1.ComputeAllCoverHouses());
            RemoveAllNonSCells(strategyManager, je2, je2.ComputeAllCoverHouses());
        }
        else
        {
            foreach (var possibility in je1.BaseCandidates)
            {
                if (!je2.BaseCandidates.Peek(possibility)) continue;

                var cov1 = je1.ComputeCoverHouses(possibility);
                var cov2 = je2.ComputeCoverHouses(possibility);

                foreach (var house1 in cov1)
                {
                    foreach (var house2 in cov2)
                    {
                        if (!house1.Equals(house2) || house1.Unit != unit) continue;

                        var totalMap = je1.SCells[possibility].Or(je2.SCells[possibility]);
                        for (int other = 0; other < 9; other++)
                        {
                            var cell = unit == Unit.Row
                                ? new Cell(house1.Number, other)
                                : new Cell(other, house1.Number);

                            if (!totalMap.Peek(cell))
                                strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, cell);
                        }
                    }
                }
            }
        }

        return strategyManager.ChangeBuffer.Commit(this, new DoubleJuniorExocetReportBuilder(je1, je2))
               && OnCommitBehavior == OnCommitBehavior.Return;
    }

    private static Cell FindFirstTargetNotIn(JuniorExocet je1, JuniorExocet je2, HashSet<Cell> total)
    {
        if (!total.Contains(je1.Target1)) return je1.Target1;
        if (!total.Contains(je1.Target2)) return je1.Target2;
        if (!total.Contains(je2.Target1)) return je2.Target1;
        if (!total.Contains(je2.Target2)) return je2.Target2;
        return default;
    }

    private bool Process(IStrategyManager strategyManager, JuniorExocet je)
    {
        Possibilities[] removedBaseCandidates = { Possibilities.NewEmpty(), Possibilities.NewEmpty() };

        var coverHouses = new Dictionary<int, List<JuniorExocetCoverHouse>>();
        
        //Elimination 1
        foreach (var possibility in je.BaseCandidates)
        {
            var computed = je.ComputeCoverHouses(possibility);
            coverHouses.Add(possibility, computed);
            
            if (computed.Count > 1) continue;
            
            strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, je.Base1);
            strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, je.Base2);
            strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, je.EscapeCell);
            strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, je.Target1);
            strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, je.Target2);

            removedBaseCandidates[0].Add(possibility);
            removedBaseCandidates[1].Add(possibility);
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
        var unit = je.GetUnit();
        foreach (var entry in je.SCells)
        {
            var computed = coverHouses[entry.Key];

            foreach (var coverHouse in computed)
            {
                if (coverHouse.Unit == unit) continue;

                if (coverHouse.Unit == Unit.Column)
                {
                    if(je.Target1.Col == coverHouse.Number)
                        strategyManager.ChangeBuffer.ProposePossibilityRemoval(entry.Key, je.Target1);
                    if (je.Target2.Col == coverHouse.Number)
                        strategyManager.ChangeBuffer.ProposePossibilityRemoval(entry.Key, je.Target2);
                }
                else
                {
                    if(je.Target1.Row == coverHouse.Number)
                        strategyManager.ChangeBuffer.ProposePossibilityRemoval(entry.Key, je.Target1);
                    if (je.Target2.Row == coverHouse.Number)
                        strategyManager.ChangeBuffer.ProposePossibilityRemoval(entry.Key, je.Target2);
                }
            }
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
        if (je.BaseCandidates.Count != 2 && strategyManager.UniquenessDependantStrategiesAllowed)
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
            //Elimination 3 update
            foreach (var possibility in strategyManager.PossibilitiesAt(je.Target1))
            {
                if (!revisedBaseCandidates.Peek(possibility))
                    strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, je.Target1);
            }

            foreach (var possibility in strategyManager.PossibilitiesAt(je.Target2))
            {
                if (!revisedBaseCandidates.Peek(possibility))
                    strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, je.Target2);
            }
            
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
            foreach (var entry in je.SCells)
            {
                if (!revisedBaseCandidates.Peek(entry.Key)) continue;
                
                foreach (var coverHouse in coverHouses[entry.Key])
                {
                    if (coverHouse.Unit != je.GetUnit()) continue;

                    for (int i = 0; i < 9; i++)
                    {
                        var cell = je.GetUnit() == Unit.Row 
                            ? new Cell(coverHouse.Number, i)
                            : new Cell(i, coverHouse.Number);

                        if (entry.Value.Peek(cell)) continue;

                        strategyManager.ChangeBuffer.ProposePossibilityRemoval(entry.Key, cell);
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

    private void RemoveAllNonSCells(IStrategyManager strategyManager, JuniorExocet je,
        Dictionary<int, List<JuniorExocetCoverHouse>> coverHouses)
    {
        foreach (var entry in je.SCells)
        {
            foreach (var coverHouse in coverHouses[entry.Key])
            {
                if (coverHouse.Unit != je.GetUnit()) continue;

                for (int i = 0; i < 9; i++)
                {
                    var cell = je.GetUnit() == Unit.Row 
                        ? new Cell(coverHouse.Number, i)
                        : new Cell(i, coverHouse.Number);

                    if (entry.Value.Peek(cell)) continue;

                    strategyManager.ChangeBuffer.ProposePossibilityRemoval(entry.Key, cell);
                }
            }
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
        List<Cell> sCells = _je.AllPossibleSCells();

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
        List<Cell> sCells = _je1.AllPossibleSCells();

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