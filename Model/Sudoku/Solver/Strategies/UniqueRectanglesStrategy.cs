using System.Collections.Generic;
using Model.Helpers.Changes;
using Model.Helpers.Settings.Types;
using Model.Sudoku.Solver.BitSets;
using Model.Sudoku.Solver.PossibilityPosition;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Sudoku.Solver.StrategiesUtility.Graphs;
using Model.Utility;

namespace Model.Sudoku.Solver.Strategies;

public class UniqueRectanglesStrategy : SudokuStrategy
{
    public const string OfficialName = "Unique Rectangles";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;

    private readonly BooleanSetting _allowMissingCandidates;
    
    public UniqueRectanglesStrategy(bool allowMissingCandidates) : base(OfficialName, StrategyDifficulty.Hard, DefaultBehavior)
    {
        _allowMissingCandidates = new BooleanSetting("Missing candidates allowed", allowMissingCandidates);
        UniquenessDependency = UniquenessDependency.FullyDependent;
        AddSetting(_allowMissingCandidates);
    }
    
    public override void Apply(IStrategyUser strategyUser)
    {
        Dictionary<BiValue, List<Cell>> biValueMap = new();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var possibilities = strategyUser.PossibilitiesAt(row, col);
                if (possibilities.Count != 2) continue;

                var asArray = possibilities.ToArray();
                var biValue = new BiValue(asArray[0], asArray[1]);
                var current = new Cell(row, col);

                if (!biValueMap.TryGetValue(biValue, out var list))
                {
                    list = new List<Cell>();
                    biValueMap[biValue] = list;
                }
                else
                {
                    foreach (var cell in list)
                    {
                        if (Search(strategyUser, biValue, cell, current)) return;
                    }
                }

                list.Add(current);
            }
        }

        foreach (var entry in biValueMap)
        {
            foreach (var cell in entry.Value)
            {
                if (SearchHidden(strategyUser, entry.Key, cell)) return;
            }
        }
    }

    private bool Search(IStrategyUser strategyUser, BiValue values, params Cell[] floor)
    {
        foreach (var roof in Cells.DeadlyPatternRoofs(floor))
        {
            if (Try(strategyUser, values, floor, roof)) return true;
        }

        return false;
    }

    private bool Try(IStrategyUser strategyUser, BiValue values, Cell[] floor, params Cell[] roof)
    {
        var roofOnePossibilities = strategyUser.PossibilitiesAt(roof[0]);
        var roofTwoPossibilities = strategyUser.PossibilitiesAt(roof[1]);

        if (!ValidateRoof(strategyUser, values, roof[0], ref roofOnePossibilities) ||
            !ValidateRoof(strategyUser, values, roof[1], ref roofTwoPossibilities)) return false;
        
        //Type 1
        if (values == roofOnePossibilities)
        {
            strategyUser.ChangeBuffer.ProposePossibilityRemoval(values.One, roof[1]);
            strategyUser.ChangeBuffer.ProposePossibilityRemoval(values.Two, roof[1]);
        }
        else if (values == roofTwoPossibilities)
        {
            strategyUser.ChangeBuffer.ProposePossibilityRemoval(values.One, roof[0]);
            strategyUser.ChangeBuffer.ProposePossibilityRemoval(values.Two, roof[0]);
        }

        if (strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer.Commit(
                new UniqueRectanglesReportBuilder(floor, roof)) &&
                    OnCommitBehavior == OnCommitBehavior.Return) return true;
        
        //Type 2
        if (roofOnePossibilities.Count == 3 && roofTwoPossibilities.Count == 3 &&
            roofOnePossibilities.Equals(roofTwoPossibilities))
        {
            foreach (var possibility in roofOnePossibilities.EnumeratePossibilities())
            {
                if (possibility == values.One || possibility == values.Two) continue;

                foreach (var cell in Cells.SharedSeenCells(roof[0], roof[1]))
                {
                    strategyUser.ChangeBuffer.ProposePossibilityRemoval(possibility, cell);
                }
            }
        }
        
        if (strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer.Commit(
                new UniqueRectanglesReportBuilder(floor, roof)) &&
                    OnCommitBehavior == OnCommitBehavior.Return) return true;
        
        //Type 3
        var notBiValuePossibilities = roofOnePossibilities | roofTwoPossibilities;
        notBiValuePossibilities -= values.One;
        notBiValuePossibilities -= values.Two;

        var ssc = new List<Cell>(Cells.SharedSeenCells(roof[0], roof[1]));
        foreach (var als in strategyUser.AlmostNakedSetSearcher.InCells(ssc))
        {
            if (!als.Possibilities.ContainsAll(notBiValuePossibilities)) continue;

            ProcessUrWithAls(strategyUser, roof, als);
            if (strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer.Commit(
                    new UniqueRectanglesWithAlmostLockedSetReportBuilder(floor, roof, als)) &&
                        OnCommitBehavior == OnCommitBehavior.Return) return true;
        }

        //Type 4 & 5
        bool oneOk = false;
        bool twoOke = false;
        if (roof[0].Row == roof[1].Row || roof[0].Column == roof[1].Column)
        {
            //Type 4
            foreach (var cell in Cells.SharedSeenCells(roof[0], roof[1]))
            {
                if (strategyUser.PossibilitiesAt(cell).Contains(values.One)) oneOk = true;
                if (strategyUser.PossibilitiesAt(cell).Contains(values.Two)) twoOke = true;

                if (oneOk && twoOke) break;
            }
            
            if (!oneOk)
            {
                strategyUser.ChangeBuffer.ProposePossibilityRemoval(values.Two, roof[0]);
                strategyUser.ChangeBuffer.ProposePossibilityRemoval(values.Two, roof[1]);
            }
            else if (!twoOke)
            {
                strategyUser.ChangeBuffer.ProposePossibilityRemoval(values.One, roof[0]);
                strategyUser.ChangeBuffer.ProposePossibilityRemoval(values.One, roof[1]);
            }
        }
        else
        {
            //Type 5
            for (int unit = 0; unit < 9; unit++)
            {
                if (unit != roof[0].Column && unit != roof[1].Column)
                {
                    if (strategyUser.PossibilitiesAt(roof[0].Row, unit).Contains(values.One)) oneOk = true;
                    if (strategyUser.PossibilitiesAt(roof[0].Row, unit).Contains(values.Two)) twoOke = true;
                    
                    if (strategyUser.PossibilitiesAt(roof[1].Row, unit).Contains(values.One)) oneOk = true;
                    if (strategyUser.PossibilitiesAt(roof[1].Row, unit).Contains(values.Two)) twoOke = true;
                }
                
                if (unit != roof[0].Row && unit != roof[1].Row)
                {
                    if (strategyUser.PossibilitiesAt(unit, roof[0].Column).Contains(values.One)) oneOk = true;
                    if (strategyUser.PossibilitiesAt(unit, roof[0].Column).Contains(values.Two)) twoOke = true;
                    
                    if (strategyUser.PossibilitiesAt(unit, roof[1].Column).Contains(values.One)) oneOk = true;
                    if (strategyUser.PossibilitiesAt(unit, roof[1].Column).Contains(values.Two)) twoOke = true;
                }
                
                if (oneOk && twoOke) break;
            }
            
            if (!oneOk)
            {
                strategyUser.ChangeBuffer.ProposePossibilityRemoval(values.Two, floor[0]);
                strategyUser.ChangeBuffer.ProposePossibilityRemoval(values.Two, floor[1]);
            }
            else if (!twoOke)
            {
                strategyUser.ChangeBuffer.ProposePossibilityRemoval(values.One, floor[0]);
                strategyUser.ChangeBuffer.ProposePossibilityRemoval(values.One, floor[1]);
            }
        }
        
        if (strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer.Commit(
                new UniqueRectanglesReportBuilder(floor, roof)) &&
                    OnCommitBehavior == OnCommitBehavior.Return) return true;
        
        //Type 6 (aka hidden type 2)
        if (roof[0].Row == roof[1].Row || roof[0].Column == roof[1].Column)
        {
            strategyUser.PreComputer.Graphs.ConstructSimple(ConstructRule.UnitStrongLink);
            var graph = strategyUser.PreComputer.Graphs.SimpleLinkGraph;

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    var cpf1 = new CellPossibility(floor[i], values.One);
                    var cpr1 = new CellPossibility(roof[j], values.One);

                    var cpf2 = new CellPossibility(floor[i], values.Two);
                    var cpr2 = new CellPossibility(roof[j], values.Two);
                
                    if (graph.AreNeighbors(cpr1, cpf1, LinkStrength.Strong))
                    {
                        strategyUser.ChangeBuffer.ProposePossibilityRemoval(values.Two, roof[(j + 1) % 2]);
                        if (strategyUser.ChangeBuffer.Commit( new UniqueRectanglesWithStrongLinkReportBuilder(
                                floor, roof, new Link<CellPossibility>(cpr1, cpf1)))
                                    && OnCommitBehavior == OnCommitBehavior.Return) return true;
                    }
                
                    if (graph.AreNeighbors(cpr2, cpf2, LinkStrength.Strong))
                    {
                        strategyUser.ChangeBuffer.ProposePossibilityRemoval(values.One, roof[(j + 1) % 2]);
                        if (strategyUser.ChangeBuffer.Commit( new UniqueRectanglesWithStrongLinkReportBuilder(
                                floor, roof, new Link<CellPossibility>(cpr2, cpf2)))
                                    && OnCommitBehavior == OnCommitBehavior.Return) return true;
                    }
                }
            }
        }

        return false;
    }

    private void ProcessUrWithAls(IStrategyUser strategyUser, Cell[] roof, IPossibilitiesPositions als)
    {
        List<Cell> buffer = new();
        foreach (var possibility in als.Possibilities.EnumeratePossibilities())
        {
            foreach (var cell in als.EachCell())
            {
                if(strategyUser.PossibilitiesAt(cell).Contains(possibility)) buffer.Add(cell);
            }

            foreach (var r in roof)
            {
                if (strategyUser.PossibilitiesAt(r).Contains(possibility)) buffer.Add(r);
            }

            foreach (var cell in Cells.SharedSeenCells(buffer))
            {
                strategyUser.ChangeBuffer.ProposePossibilityRemoval(possibility, cell);
            }
            
            buffer.Clear();
        }
    }

    private bool SearchHidden(IStrategyUser strategyUser, BiValue values, Cell cell)
    {
        for (int row = 0; row < 9; row++)
        {
            if(row == cell.Row) continue;
            var rowPossibilities = strategyUser.PossibilitiesAt(row, cell.Column);

            var oneWasChanged = false;
            var twoWasChanged = false;
            if (!ValidateRoof(strategyUser, values, new Cell(row, cell.Column), ref rowPossibilities,
                    ref oneWasChanged, ref twoWasChanged) || (oneWasChanged && twoWasChanged)) continue;

            for (int col = 0; col < 9; col++)
            {
                if (col == cell.Column || !Cells.AreSpreadOverTwoBoxes(row, col, cell.Row, cell.Column)) continue;
                var colPossibilities = strategyUser.PossibilitiesAt(cell.Row, col);

                var oneWasChangedCopy = oneWasChanged;
                var twoWasChangedCopy = twoWasChanged;
                if (!ValidateRoof(strategyUser, values, new Cell(cell.Row, col), ref colPossibilities,
                        ref oneWasChangedCopy, ref twoWasChangedCopy)) continue;

                var opposite = new Cell(row, col);
                var oppositePossibilities = strategyUser.PossibilitiesAt(opposite);
                
                if (!oppositePossibilities.Contains(values.One) || !oppositePossibilities.Contains(values.Two)) continue;

                if (!oneWasChangedCopy && strategyUser.RowPositionsAt(row, values.One).Count == 2 &&
                    strategyUser.ColumnPositionsAt(col, values.One).Count == 2)
                {
                    strategyUser.ChangeBuffer.ProposePossibilityRemoval(values.Two, opposite);
                    if (strategyUser.ChangeBuffer.Commit( new HiddenUniqueRectanglesReportBuilder(
                            cell, opposite, values.One)) && OnCommitBehavior == OnCommitBehavior.Return) return true;
                }
                
                if (!twoWasChangedCopy && strategyUser.RowPositionsAt(row, values.Two).Count == 2 &&
                    strategyUser.ColumnPositionsAt(col, values.Two).Count == 2)
                {
                    strategyUser.ChangeBuffer.ProposePossibilityRemoval(values.One, opposite);
                    if (strategyUser.ChangeBuffer.Commit( new HiddenUniqueRectanglesReportBuilder(
                            cell, opposite, values.One)) && OnCommitBehavior == OnCommitBehavior.Return) return true;
                }
            }
        }
        
        return false;
    }

    private bool ValidateRoof(IStrategyUser strategyUser, BiValue biValue, Cell cell, ref ReadOnlyBitSet16 possibilities)
    {
        if (!possibilities.Contains(biValue.One))
        {
            if (_allowMissingCandidates.Value && strategyUser.RawPossibilitiesAt(cell).Contains(biValue.One))
            {
                possibilities += biValue.One;
            }
            else return false;
        }
        
        if (!possibilities.Contains(biValue.Two))
        {
            if (_allowMissingCandidates.Value && strategyUser.RawPossibilitiesAt(cell).Contains(biValue.Two))
            {
                possibilities += biValue.Two;
            }
            else return false;
        }
        
        return true;
    }
    
    private bool ValidateRoof(IStrategyUser strategyUser, BiValue biValue, Cell cell,
        ref ReadOnlyBitSet16 possibilities, ref bool oneWasChanged, ref bool twoWasChanged)
    {
        if (!possibilities.Contains(biValue.One))
        {
            if (_allowMissingCandidates.Value && strategyUser.RawPossibilitiesAt(cell).Contains(biValue.One))
            {
                possibilities += biValue.One;
                oneWasChanged = true;
            }
            else return false;
        }
        
        if (!possibilities.Contains(biValue.Two))
        {
            if (_allowMissingCandidates.Value && strategyUser.RawPossibilitiesAt(cell).Contains(biValue.Two))
            {
                possibilities += biValue.Two;
                twoWasChanged = true;
            }
            else return false;
        }
        
        return true;
    }
}

public readonly struct BiValue
{
    public BiValue(int one, int two)
    {
        One = one;
        Two = two;
    }

    public int One { get; }
    public int Two { get; }

    public override int GetHashCode()
    {
        return One ^ Two;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not BiValue bi) return false;
        return (bi.One == One && bi.Two == Two) || (bi.One == Two && bi.Two == One);
    }

    public override string ToString()
    {
        return $"Bi-Value : {One}, {Two}";
    }

    public static bool operator ==(BiValue left, BiValue right)
    {
        return (left.One == right.One && left.Two == right.Two) || (left.One == right.Two && left.Two == right.One);
    }

    public static bool operator !=(BiValue left, BiValue right)
    {
        return !(left == right);
    }
}

public class UniqueRectanglesReportBuilder : IChangeReportBuilder
{
    private readonly Cell[] _floor;
    private readonly Cell[] _roof;

    public UniqueRectanglesReportBuilder(Cell[] floor, Cell[] roof)
    {
        _floor = floor;
        _roof = roof;
    }

    public ChangeReport Build(IReadOnlyList<SolverProgress> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport( "", lighter =>
        {
            foreach (var floor in _floor)
            {
                lighter.HighlightCell(floor, ChangeColoration.CauseOffTwo);
            }

            foreach (var roof in _roof)
            {
                lighter.HighlightCell(roof, snapshot.PossibilitiesAt(roof).Count == 2 ? 
                    ChangeColoration.CauseOffTwo : ChangeColoration.CauseOffOne);
            }
            
            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}

public class UniqueRectanglesWithStrongLinkReportBuilder : IChangeReportBuilder
{
    private readonly Cell[] _floor;
    private readonly Cell[] _roof;
    private readonly Link<CellPossibility> _link;

    public UniqueRectanglesWithStrongLinkReportBuilder(Cell[] floor, Cell[] roof, Link<CellPossibility> link)
    {
        _floor = floor;
        _roof = roof;
        _link = link;
    }

    public ChangeReport Build(IReadOnlyList<SolverProgress> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport( "", lighter =>
        {
            foreach (var floor in _floor)
            {
                lighter.HighlightCell(floor, ChangeColoration.CauseOffTwo);
            }

            foreach (var roof in _roof)
            {
                lighter.HighlightCell(roof, ChangeColoration.CauseOffOne);
            }

            lighter.CreateLink(_link.From, _link.To, LinkStrength.Strong);
            
            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}

public class UniqueRectanglesWithAlmostLockedSetReportBuilder : IChangeReportBuilder
{
    private readonly Cell[] _floor;
    private readonly Cell[] _roof;
    private readonly IPossibilitiesPositions _als;

    public UniqueRectanglesWithAlmostLockedSetReportBuilder(Cell[] floor, Cell[] roof, IPossibilitiesPositions als)
    {
        _floor = floor;
        _roof = roof;
        _als = als;
    }

    public ChangeReport Build(IReadOnlyList<SolverProgress> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport( "", lighter =>
        {
            foreach (var floor in _floor)
            {
                lighter.HighlightCell(floor, ChangeColoration.CauseOffTwo);
            }

            foreach (var roof in _roof)
            {
                lighter.HighlightCell(roof, ChangeColoration.CauseOffOne);
            }

            foreach (var cell in _als.EachCell())
            {
                lighter.HighlightCell(cell, ChangeColoration.CauseOffThree);
            }
            
            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}

public class HiddenUniqueRectanglesReportBuilder : IChangeReportBuilder
{
    private readonly Cell _initial;
    private readonly Cell _opposite;
    private readonly int _stronglyLinkedPossibility;

    public HiddenUniqueRectanglesReportBuilder(Cell initial, Cell opposite, int stronglyLinkedPossibility)
    {
        _initial = initial;
        _opposite = opposite;
        _stronglyLinkedPossibility = stronglyLinkedPossibility;
    }

    public ChangeReport Build(IReadOnlyList<SolverProgress> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport( "", lighter =>
        {
            lighter.HighlightCell(_initial, ChangeColoration.CauseOffTwo);

            lighter.HighlightCell(_opposite, ChangeColoration.CauseOffOne);
            lighter.HighlightCell(_opposite.Row, _initial.Column, ChangeColoration.CauseOffOne);
            lighter.HighlightCell(_initial.Row, _opposite.Column, ChangeColoration.CauseOffOne);

            lighter.CreateLink(new CellPossibility(_opposite, _stronglyLinkedPossibility), new CellPossibility(
                _opposite.Row, _initial.Column, _stronglyLinkedPossibility), LinkStrength.Strong);
            lighter.CreateLink(new CellPossibility(_opposite, _stronglyLinkedPossibility), new CellPossibility(
                _initial.Row, _opposite.Column, _stronglyLinkedPossibility), LinkStrength.Strong);
            
            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}