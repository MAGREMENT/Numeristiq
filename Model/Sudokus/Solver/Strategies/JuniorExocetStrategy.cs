using System.Collections.Generic;
using System.Text;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.Exocet;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies;

public class JuniorExocetStrategy : SudokuStrategy
{
    public const string OfficialName = "Junior Exocet";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.UnorderedAll;

    public JuniorExocetStrategy() : base(OfficialName, Difficulty.Extreme, DefaultInstanceHandling)
    {
        UniquenessDependency = UniquenessDependency.PartiallyDependent;
    }

    public override void Apply(ISudokuSolverData solverData)
    {
        var jes = solverData.PreComputer.JuniorExocet();
        
        foreach (var je in jes)
        {
            if (Process(solverData, je)) return;
        }

        for (int i = 0; i < jes.Count - 1; i++)
        {
            for (int j = i + 1; j < jes.Count; j++)
            {
                if (ProcessDouble(solverData, jes[i], jes[j])) return;
            }
        }
    }

    private bool ProcessDouble(ISudokuSolverData solverData, JuniorExocet je1, JuniorExocet je2) //TODO fix : ..........5724...98....947...9..3...5..9..12...3.1.9...6....25....56.....7......6
    {
        var unit = je1.GetUnit();
        if (unit != je2.GetUnit()) return false;

        if ((unit == Unit.Row && je1.Base1.Row / 3 != je2.Base1.Row / 3) ||
            (unit == Unit.Column && je1.Base1.Column / 3 != je2.Base1.Column / 3)) return false;
        
        HashSet<Cell> baseCells = new()
        {
            je1.Base1,
            je1.Base2,
            je2.Base1,
            je2.Base2
        };

        if (baseCells.Count != 4) return false;

        Cell? targetCommon = null;
        HashSet<Cell> targetCells = new();
        if (!targetCells.Add(je1.Target1)) targetCommon = je1.Target1;
        if (!targetCells.Add(je1.Target2)) targetCommon = je1.Target2;
        if (!targetCells.Add(je2.Target1)) targetCommon = je2.Target1;
        if (!targetCells.Add(je2.Target2)) targetCommon = je2.Target2;

        if (targetCells.Count < 3) return false;
        
        Cell? baseAndTargetCommon = null;

        if (targetCells.Remove(je1.Base1))
        {
            if (baseAndTargetCommon is not null || targetCommon is not null) return false;
            baseAndTargetCommon = je1.Base1;
        }
        if(targetCells.Remove(je1.Base2))
        {
            if (baseAndTargetCommon is not null || targetCommon is not null) return false;
            baseAndTargetCommon = je1.Base2;
        }
        if(targetCells.Remove(je2.Base1))
        {
            if (baseAndTargetCommon is not null || targetCommon is not null) return false;
            baseAndTargetCommon = je2.Base1;
        }
        if(targetCells.Remove(je2.Base2))
        {
            if (baseAndTargetCommon is not null || targetCommon is not null) return false;
            baseAndTargetCommon = je2.Base2;
        }

        var totalCandidates = je1.BaseCandidates | je2.BaseCandidates;
        if (targetCells.Count != totalCandidates.Count) return false;

        //Elimination 0
        if (targetCommon is not null || baseAndTargetCommon is not null)
        {
            var and = je1.BaseCandidates & je2.BaseCandidates;
            if (and.Count == 1)
            {
                var cell = targetCommon ?? baseAndTargetCommon;
                solverData.ChangeBuffer.ProposeSolutionAddition(and.FirstPossibility(), cell!.Value);
            }
        }

        //Elimination 1
        var bCells = new List<Cell>(baseCells);
        var tCells = new List<Cell>(targetCells);
        
        foreach (var possibility in totalCandidates.EnumeratePossibilities())
        {
            foreach (var cell in SudokuCellUtility.SharedSeenCells(bCells))
            {
                solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, cell);
            }
            
            foreach (var cell in SudokuCellUtility.SharedSeenCells(tCells))
            {
                solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, cell);
            }
        }

        var cover1 = je1.ComputeAllCoverHouses();
        var cover2 = je2.ComputeAllCoverHouses();
        //Elimination 2
        var crossLines = je1.SCellsLinePositions().Or(je2.SCellsLinePositions());
        if (crossLines.Count == 3)
        {
            RemoveAllNonSCells(solverData, je1, cover1);
            RemoveAllNonSCells(solverData, je2, cover2);
        }
        else
        {
            foreach (var entry in cover1)
            {
                if (!cover2.TryGetValue(entry.Key, out var c2)) continue;
                
                foreach (var house1 in entry.Value)
                {
                    foreach (var house2 in c2)
                    {
                        if (house1 != house2) continue;

                        var totalMap = je1.SCells[entry.Key].Or(je2.SCells[entry.Key]);
                        for (int other = 0; other < 9; other++)
                        {
                            var cell = unit == Unit.Row
                                ? new Cell(house1.Number, other)
                                : new Cell(other, house1.Number);

                            if (!totalMap.Contains(cell))
                                solverData.ChangeBuffer.ProposePossibilityRemoval(entry.Key, cell);
                        }
                    }
                }
            }
        }

        return solverData.ChangeBuffer.Commit( new DoubleJuniorExocetReportBuilder(je1, je2))
               && StopOnFirstCommit;
    }

    private bool Process(ISudokuSolverData solverData, JuniorExocet je)
    {
        ReadOnlyBitSet16[] removedBaseCandidates = { new(), new() };

        var coverHouses = new Dictionary<int, List<House>>();
        
        //Elimination 1
        foreach (var possibility in je.BaseCandidates.EnumeratePossibilities())
        {
            var computed = je.ComputeCoverHouses(possibility);
            coverHouses.Add(possibility, computed);
            
            if (computed.Count > 1) continue;
            
            solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, je.Base1);
            solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, je.Base2);
            solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, je.EscapeCell);
            solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, je.Target1);
            solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, je.Target2);

            removedBaseCandidates[0] += possibility;
            removedBaseCandidates[1] += possibility;
        }

        //Elimination 2
        foreach (var possibility in je.BaseCandidates.EnumeratePossibilities())
        {
            if (!solverData.PossibilitiesAt(je.Target1).Contains(possibility))
            {
                foreach (var cell in je.Target1MirrorNodes)
                {
                    solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, cell);
                }
            }
            
            if (!solverData.PossibilitiesAt(je.Target2).Contains(possibility))
            {
                foreach (var cell in je.Target2MirrorNodes)
                {
                    solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, cell);
                }
            }
        }
        
        //Elimination 3
        foreach (var possibility in solverData.PossibilitiesAt(je.Target1).EnumeratePossibilities())
        {
            if (!je.BaseCandidates.Contains(possibility))
                solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, je.Target1);
        }

        foreach (var possibility in solverData.PossibilitiesAt(je.Target2).EnumeratePossibilities())
        {
            if (!je.BaseCandidates.Contains(possibility))
                solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, je.Target2);
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
                    if(je.Target1.Column == coverHouse.Number)
                        solverData.ChangeBuffer.ProposePossibilityRemoval(entry.Key, je.Target1);
                    if (je.Target2.Column == coverHouse.Number)
                        solverData.ChangeBuffer.ProposePossibilityRemoval(entry.Key, je.Target2);
                }
                else
                {
                    if(je.Target1.Row == coverHouse.Number)
                        solverData.ChangeBuffer.ProposePossibilityRemoval(entry.Key, je.Target1);
                    if (je.Target2.Row == coverHouse.Number)
                        solverData.ChangeBuffer.ProposePossibilityRemoval(entry.Key, je.Target2);
                }
            }
        }
        
        //Elimination 6
        foreach (var possibility in je.BaseCandidates.EnumeratePossibilities())
        {
            bool ok = false;
            foreach (var cell in je.Target1MirrorNodes)
            {
                if (solverData.Contains(cell.Row, cell.Column, possibility)) ok = true;
            }

            if (!ok) solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, je.Target1);
            
            ok = false;
            foreach (var cell in je.Target2MirrorNodes)
            {
                if (solverData.Contains(cell.Row, cell.Column, possibility)) ok = true;
            }

            if (!ok) solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, je.Target2);
        }

        //Compatibility check
        if (je.BaseCandidates.Count != 2 && solverData.UniquenessDependantStrategiesAllowed)
        {
            HashSet<BiValue> forbiddenPairs = new();

            int i = 0;
            while (je.BaseCandidates.HasNextPossibility(ref i))
            {
                int j = i;
                while (je.BaseCandidates.HasNextPossibility(ref j))
                {
                    if (!je.CompatibilityCheck(solverData, i, j)) forbiddenPairs.Add(new BiValue(i, j));
                }
            }

            foreach (var p1 in solverData.PossibilitiesAt(je.Base1).EnumeratePossibilities())
            {
                bool ok = false;
                foreach (var p2 in solverData.PossibilitiesAt(je.Base2).EnumeratePossibilities())
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
                    solverData.ChangeBuffer.ProposePossibilityRemoval(p1, je.Base1.Row, je.Base1.Column);
                    removedBaseCandidates[0] += p1;
                }
            }
        
            foreach (var p2 in solverData.PossibilitiesAt(je.Base2).EnumeratePossibilities())
            {
                bool ok = false;
                foreach (var p1 in solverData.PossibilitiesAt(je.Base1).EnumeratePossibilities())
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
                    solverData.ChangeBuffer.ProposePossibilityRemoval(p2, je.Base2.Row, je.Base2.Column);
                    removedBaseCandidates[1] += p2;
                }
            }
        }

        //Known base digits eliminations
        var revisedB1 = solverData.PossibilitiesAt(je.Base1) - removedBaseCandidates[0];
        var revisedB2 = solverData.PossibilitiesAt(je.Base2) - removedBaseCandidates[1];
        var revisedBaseCandidates = revisedB1 | revisedB2;

        if (revisedBaseCandidates.Count == 2)
        {
            //Elimination 3 update
            foreach (var possibility in solverData.PossibilitiesAt(je.Target1).EnumeratePossibilities())
            {
                if (!revisedBaseCandidates.Contains(possibility))
                    solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, je.Target1);
            }

            foreach (var possibility in solverData.PossibilitiesAt(je.Target2).EnumeratePossibilities())
            {
                if (!revisedBaseCandidates.Contains(possibility))
                    solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, je.Target2);
            }
            
            //Elimination 7
            for (int i = 0; i < 2; i++)
            {
                if (!solverData.ContainsAny(je.Target1MirrorNodes[i], revisedBaseCandidates))
                    RemoveAll(solverData, je.Target1MirrorNodes[(i + 1) % 2], solverData.PossibilitiesAt(je.Target2));
                
                if(!solverData.ContainsAny(je.Target2MirrorNodes[i], revisedBaseCandidates))
                    RemoveAll(solverData, je.Target2MirrorNodes[(i + 1) % 2], solverData.PossibilitiesAt(je.Target1));
            }
            
            //Elimination 8
            var or = solverData.PossibilitiesAt(je.Target1MirrorNodes[0])
                | solverData.PossibilitiesAt(je.Target1MirrorNodes[1]);
            var diff = or - revisedBaseCandidates;
            if (diff.Count == 1)
            {
                var p = diff.FirstPossibility();
                foreach (var cell in SudokuCellUtility.SharedSeenCells(je.Target1MirrorNodes[0], je.Target1MirrorNodes[1]))
                {
                    solverData.ChangeBuffer.ProposePossibilityRemoval(p, cell);
                }
            }
                
            or = solverData.PossibilitiesAt(je.Target2MirrorNodes[0]) 
                 | solverData.PossibilitiesAt(je.Target2MirrorNodes[1]);
            diff = or - revisedBaseCandidates;
            if (diff.Count == 1)
            {
                var p = diff.FirstPossibility();
                foreach (var cell in SudokuCellUtility.SharedSeenCells(je.Target2MirrorNodes[0], je.Target2MirrorNodes[1]))
                {
                    solverData.ChangeBuffer.ProposePossibilityRemoval(p, cell);
                }
            }
            
            //Elimination 9 => ???
            
            //Elimination 10
            foreach (var possibility in revisedBaseCandidates.EnumeratePossibilities())
            {
                foreach (var cell in SudokuCellUtility.SharedSeenCells(je.Base1, je.Base2))
                {
                    solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, cell);
                }

                foreach (var cell in SudokuCellUtility.SharedSeenCells(je.Target1, je.Target2))
                {
                    solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, cell);
                }
            }
            
            
            //Elimination 11
            foreach (var entry in je.SCells)
            {
                if (!revisedBaseCandidates.Contains(entry.Key)) continue;
                
                foreach (var coverHouse in coverHouses[entry.Key])
                {
                    if (coverHouse.Unit != je.GetUnit()) continue;

                    for (int i = 0; i < 9; i++)
                    {
                        var cell = je.GetUnit() == Unit.Row 
                            ? new Cell(coverHouse.Number, i)
                            : new Cell(i, coverHouse.Number);

                        if (entry.Value.Contains(cell)) continue;

                        solverData.ChangeBuffer.ProposePossibilityRemoval(entry.Key, cell);
                    }
                }
            }

            //Elimination 12
            foreach (var possibility in revisedBaseCandidates.EnumeratePossibilities())
            {
                var gp = je.SCells[possibility];
                foreach (var cell in gp)
                {
                    if (solverData.Sudoku[cell.Row, cell.Column] != 0) continue;

                    var copy = gp.Copy();
                    copy.VoidRow(cell.Row);
                    copy.VoidColumn(cell.Column);
                    copy.VoidMiniGrid(cell.Row / 3, cell.Column / 3);

                    if (copy.Count == 0) solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, cell);
                }
            }

            var n = revisedBaseCandidates.NextPossibility(0);
            var allSCells = je.SCells[n];
            allSCells.ApplyOr(je.SCells[revisedBaseCandidates.NextPossibility(n)]);

            if (allSCells.Count == 4)
            {
                foreach (var cell in allSCells)
                {
                    foreach (var p in solverData.PossibilitiesAt(cell).EnumeratePossibilities())
                    {
                        if(!revisedBaseCandidates.Contains(p)) solverData.ChangeBuffer.ProposePossibilityRemoval(p, cell);
                    }
                }
            }
        }

        return solverData.ChangeBuffer.Commit(new DoubleTargetExocetReportBuilder(je)) && StopOnFirstCommit;
    }

    private void RemoveAll(ISudokuSolverData solverData, Cell cell, ReadOnlyBitSet16 except)
    {
        foreach (var possibility in solverData.PossibilitiesAt(cell).EnumeratePossibilities())
        {
            if (except.Contains(possibility)) continue;

            solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, cell);
        }
    }

    public static void RemoveAllNonSCells(ISudokuSolverData solverData, Exocet je,
        Dictionary<int, List<House>> coverHouses)
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

                    if (entry.Value.Contains(cell)) continue;

                    solverData.ChangeBuffer.ProposePossibilityRemoval(entry.Key, cell);
                }
            }
        }
    }
    
}

public class DoubleTargetExocetReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly DoubleTargetExocet _e;

    public DoubleTargetExocetReportBuilder(DoubleTargetExocet e)
    {
        _e = e;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        var sCells = _e.AllPossibleSCells();

        List<CellPossibility> sPossibilities = new();
        List<Cell> sSolved = new();
        foreach (var cell in sCells)
        {
            if (_e.BaseCandidates.Contains(snapshot[cell.Row, cell.Column])) sSolved.Add(cell);
            else
            {
                foreach (var possibility in _e.BaseCandidates.EnumeratePossibilities())
                {
                    if(snapshot.PossibilitiesAt(cell).Contains(possibility)) sPossibilities.Add(new CellPossibility(cell, possibility));
                } 
            }
        }

        return new ChangeReport<ISudokuHighlighter>(Description(), lighter =>
        {
            lighter.HighlightCell(_e.Base1, StepColor.Cause1);
            lighter.HighlightCell(_e.Base2, StepColor.Cause1);
            
            lighter.HighlightCell(_e.Target1, StepColor.Cause2);
            lighter.HighlightCell(_e.Target2, StepColor.Cause2);

            foreach (var cell in sCells)
            {
                lighter.HighlightCell(cell, StepColor.Cause3);
            }

            foreach (var cp in sPossibilities)
            {
                lighter.HighlightPossibility(cp, StepColor.Neutral);
            }

            foreach (var cell in sSolved)
            {
                lighter.HighlightCell(cell, StepColor.Neutral);
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }

    private string Description()
    {
        var type = _e is JuniorExocet ? "Junior Exocet" : "Senior Exocet";
        var baseS = _e.Base1.Row == _e.Base2.Row
            ? $"r{_e.Base1.Row + 1}c{_e.Base1.Column + 1}{_e.Base2.Column + 1}"
            : $"r{_e.Base1.Row + 1}{_e.Base2.Row + 1}c{_e.Base1.Column + 1}";
        var builder = new StringBuilder($"{type} in {baseS}, {_e.Target1}, {_e.Target2}\nCover houses :\n");

        foreach (var entry in _e.ComputeAllCoverHouses())
        {
            builder.Append($"{entry.Key} : ");
            if (entry.Value.Count == 0) builder.Append("none\n");
            else
            {
                builder.Append(entry.Value[0].ToString());
                for (int i = 1; i < entry.Value.Count; i++)
                {
                    builder.Append(", " + entry.Value[i]);
                }

                builder.Append('\n');
            }
        }
        
        return builder.ToString();
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}

public class DoubleJuniorExocetReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly JuniorExocet _je1;
    private readonly JuniorExocet _je2;

    public DoubleJuniorExocetReportBuilder(JuniorExocet je1, JuniorExocet je2)
    {
        _je1 = je1;
        _je2 = je2;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        List<Cell> sCells = _je1.AllPossibleSCells();

        List<CellPossibility> sPossibilities = new();
        foreach (var cell in sCells)
        {
            foreach (var possibility in _je1.BaseCandidates.EnumeratePossibilities())
            {
                if(snapshot.PossibilitiesAt(cell).Contains(possibility)) sPossibilities.Add(new CellPossibility(cell, possibility));
            }
        }

        List<Cell> sSolved = new();
        foreach (var cell in sCells)
        {
            if (_je1.BaseCandidates.Contains(snapshot[cell.Row, cell.Column])) sSolved.Add(cell);
        }
        
        return new ChangeReport<ISudokuHighlighter>( "", lighter =>
        {
            lighter.HighlightCell(_je1.Base1, StepColor.Cause1);
            lighter.HighlightCell(_je1.Base2, StepColor.Cause1);
            
            lighter.HighlightCell(_je1.Target1, StepColor.Cause2);
            lighter.HighlightCell(_je1.Target2, StepColor.Cause2);
            
            lighter.HighlightCell(_je2.Base1, StepColor.Cause5);
            lighter.HighlightCell(_je2.Base2, StepColor.Cause5);
            
            lighter.HighlightCell(_je2.Target1, StepColor.Cause4);
            lighter.HighlightCell(_je2.Target2, StepColor.Cause4);

            foreach (var cell in sCells)
            {
                lighter.HighlightCell(cell, StepColor.Cause3);
            }

            foreach (var cp in sPossibilities)
            {
                lighter.HighlightPossibility(cp, StepColor.Neutral);
            }

            foreach (var cell in sSolved)
            {
                lighter.HighlightCell(cell, StepColor.Neutral);
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}