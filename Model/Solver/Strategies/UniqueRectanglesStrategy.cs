using System.Collections.Generic;
using System.Linq;
using Global;
using Global.Enums;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Highlighting;
using Model.Solver.Possibility;
using Model.Solver.PossibilityPosition;
using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver.Strategies;

public class UniqueRectanglesStrategy : AbstractStrategy //TODO : add other sizes
{
    public const string OfficialName = "Unique Rectangles";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public UniqueRectanglesStrategy() : base(OfficialName, StrategyDifficulty.Hard, DefaultBehavior)
    {
    }
    
    public override void Apply(IStrategyManager strategyManager)
    {
        Dictionary<BiValue, List<Cell>> biValueMap = new();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var possibilities = strategyManager.PossibilitiesAt(row, col);
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
                        if (Search(strategyManager, biValue, cell, current)) return;
                    }
                }

                list.Add(current);
            }
        }

        foreach (var entry in biValueMap)
        {
            foreach (var cell in entry.Value)
            {
                if (SearchHidden(strategyManager, entry.Key, cell)) return;
            }
        }
    }

    private bool Search(IStrategyManager strategyManager, BiValue values, params Cell[] floor)
    {
        foreach (var roof in Cells.DeadlyPatternRoofs(floor))
        {
            if (Try(strategyManager, values, floor, roof)) return true;
        }

        return false;
    }

    private bool Try(IStrategyManager strategyManager, BiValue values, Cell[] floor, params Cell[] roof)
    {
        var roofOnePossibilities = strategyManager.PossibilitiesAt(roof[0]);
        var roofTwoPossibilities = strategyManager.PossibilitiesAt(roof[1]);

        if (!roofOnePossibilities.Peek(values.One) || !roofOnePossibilities.Peek(values.Two) ||
            !roofTwoPossibilities.Peek(values.One) || !roofTwoPossibilities.Peek(values.Two)) return false;
        
        //Type 1
        if (values.Equals(roofOnePossibilities))
        {
            strategyManager.ChangeBuffer.ProposePossibilityRemoval(values.One, roof[1]);
            strategyManager.ChangeBuffer.ProposePossibilityRemoval(values.Two, roof[1]);
        }
        else if (values.Equals(roofTwoPossibilities))
        {
            strategyManager.ChangeBuffer.ProposePossibilityRemoval(values.One, roof[0]);
            strategyManager.ChangeBuffer.ProposePossibilityRemoval(values.Two, roof[0]);
        }

        if (strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
                new UniqueRectanglesReportBuilder(floor, roof)) &&
                    OnCommitBehavior == OnCommitBehavior.Return) return true;
        
        //Type 2
        if (roofOnePossibilities.Count == 3 && roofTwoPossibilities.Count == 3 &&
            roofOnePossibilities.Equals(roofTwoPossibilities))
        {
            foreach (var possibility in roofOnePossibilities)
            {
                if (possibility == values.One || possibility == values.Two) continue;

                foreach (var cell in Cells.SharedSeenCells(roof[0], roof[1]))
                {
                    strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, cell);
                }
            }
        }
        
        if (strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
                new UniqueRectanglesReportBuilder(floor, roof)) &&
                    OnCommitBehavior == OnCommitBehavior.Return) return true;
        
        //Type 3
        var notBiValuePossibilities = roofOnePossibilities.Or(roofTwoPossibilities);
        notBiValuePossibilities.Remove(values.One);
        notBiValuePossibilities.Remove(values.Two);

        var ssc = new List<Cell>(Cells.SharedSeenCells(roof[0], roof[1]));
        foreach (var als in strategyManager.AlmostNakedSetSearcher.InCells(ssc))
        {
            if (!als.Possibilities.PeekAll(notBiValuePossibilities)) continue;

            ProcessUrWithAls(strategyManager, roof, als);
            if (strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
                    new UniqueRectanglesWithAlmostLockedSetReportBuilder(floor, roof, als)) &&
                        OnCommitBehavior == OnCommitBehavior.Return) return true;
        }

        //Type 4 & 5
        bool oneOk = false;
        bool twoOke = false;
        if (roof[0].Row == roof[1].Row || roof[0].Col == roof[1].Col)
        {
            //Type 4
            foreach (var cell in Cells.SharedSeenCells(roof[0], roof[1]))
            {
                if (strategyManager.PossibilitiesAt(cell).Peek(values.One)) oneOk = true;
                if (strategyManager.PossibilitiesAt(cell).Peek(values.Two)) twoOke = true;

                if (oneOk && twoOke) break;
            }
            
            if (!oneOk)
            {
                strategyManager.ChangeBuffer.ProposePossibilityRemoval(values.Two, roof[0]);
                strategyManager.ChangeBuffer.ProposePossibilityRemoval(values.Two, roof[1]);
            }
            else if (!twoOke)
            {
                strategyManager.ChangeBuffer.ProposePossibilityRemoval(values.One, roof[0]);
                strategyManager.ChangeBuffer.ProposePossibilityRemoval(values.One, roof[1]);
            }
        }
        else
        {
            //Type 5
            for (int unit = 0; unit < 9; unit++)
            {
                if (unit != roof[0].Col && unit != roof[1].Col)
                {
                    if (strategyManager.PossibilitiesAt(roof[0].Row, unit).Peek(values.One)) oneOk = true;
                    if (strategyManager.PossibilitiesAt(roof[0].Row, unit).Peek(values.Two)) twoOke = true;
                    
                    if (strategyManager.PossibilitiesAt(roof[1].Row, unit).Peek(values.One)) oneOk = true;
                    if (strategyManager.PossibilitiesAt(roof[1].Row, unit).Peek(values.Two)) twoOke = true;
                }
                
                if (unit != roof[0].Row && unit != roof[1].Row)
                {
                    if (strategyManager.PossibilitiesAt(unit, roof[0].Col).Peek(values.One)) oneOk = true;
                    if (strategyManager.PossibilitiesAt(unit, roof[0].Col).Peek(values.Two)) twoOke = true;
                    
                    if (strategyManager.PossibilitiesAt(unit, roof[1].Col).Peek(values.One)) oneOk = true;
                    if (strategyManager.PossibilitiesAt(unit, roof[1].Col).Peek(values.Two)) twoOke = true;
                }
                
                if (oneOk && twoOke) break;
            }
            
            if (!oneOk)
            {
                strategyManager.ChangeBuffer.ProposePossibilityRemoval(values.Two, floor[0]);
                strategyManager.ChangeBuffer.ProposePossibilityRemoval(values.Two, floor[1]);
            }
            else if (!twoOke)
            {
                strategyManager.ChangeBuffer.ProposePossibilityRemoval(values.One, floor[0]);
                strategyManager.ChangeBuffer.ProposePossibilityRemoval(values.One, floor[1]);
            }
        }
        
        if (strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
                new UniqueRectanglesReportBuilder(floor, roof)) &&
                    OnCommitBehavior == OnCommitBehavior.Return) return true;
        
        //Type 6 (aka hidden type 2)
        if (roof[0].Row == roof[1].Row || roof[0].Col == roof[1].Col)
        {
            strategyManager.GraphManager.ConstructSimple(ConstructRule.UnitStrongLink);
            var graph = strategyManager.GraphManager.SimpleLinkGraph;

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    var cpf1 = new CellPossibility(floor[i], values.One);
                    var cpr1 = new CellPossibility(roof[j], values.One);

                    var cpf2 = new CellPossibility(floor[i], values.Two);
                    var cpr2 = new CellPossibility(roof[j], values.Two);
                
                    if (graph.HasLinkTo(cpr1, cpf1, LinkStrength.Strong))
                    {
                        strategyManager.ChangeBuffer.ProposePossibilityRemoval(values.Two, roof[(j + 1) % 2]);
                        if (strategyManager.ChangeBuffer.Commit(this, new UniqueRectanglesWithStrongLinkReportBuilder(
                                floor, roof, new Link<CellPossibility>(cpr1, cpf1)))
                                    && OnCommitBehavior == OnCommitBehavior.Return) return true;
                    }
                
                    if (graph.HasLinkTo(cpr2, cpf2, LinkStrength.Strong))
                    {
                        strategyManager.ChangeBuffer.ProposePossibilityRemoval(values.One, roof[(j + 1) % 2]);
                        if (strategyManager.ChangeBuffer.Commit(this, new UniqueRectanglesWithStrongLinkReportBuilder(
                                floor, roof, new Link<CellPossibility>(cpr2, cpf2)))
                                    && OnCommitBehavior == OnCommitBehavior.Return) return true;
                    }
                }
            }
        }

        return false;
    }

    private void ProcessUrWithAls(IStrategyManager strategyManager, Cell[] roof, IPossibilitiesPositions als)
    {
        List<Cell> buffer = new();
        foreach (var possibility in als.Possibilities)
        {
            foreach (var cell in als.EachCell())
            {
                if(strategyManager.PossibilitiesAt(cell).Peek(possibility)) buffer.Add(cell);
            }

            foreach (var r in roof)
            {
                if (strategyManager.PossibilitiesAt(r).Peek(possibility)) buffer.Add(r);
            }

            foreach (var cell in Cells.SharedSeenCells(buffer))
            {
                strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, cell);
            }
            
            buffer.Clear();
        }
    }

    private bool SearchHidden(IStrategyManager strategyManager, BiValue values, Cell cell)
    {
        for (int row = 0; row < 9; row++)
        {
            if(row == cell.Row) continue;
            var rowPossibilities = strategyManager.PossibilitiesAt(row, cell.Col);

            if (!rowPossibilities.Peek(values.One) || !rowPossibilities.Peek(values.Two)) continue;

            for (int col = 0; col < 9; col++)
            {
                if (col == cell.Col || !Cells.AreSpreadOverTwoBoxes(row, col, cell.Row, cell.Col)) continue;
                var colPossibilities = strategyManager.PossibilitiesAt(cell.Row, col);

                if (!colPossibilities.Peek(values.One) || !colPossibilities.Peek(values.Two)) continue;

                var opposite = new Cell(row, col);
                var oppositePossibilities = strategyManager.PossibilitiesAt(opposite);

                if (!oppositePossibilities.Peek(values.One) || !oppositePossibilities.Peek(values.Two)) continue;

                if (strategyManager.RowPositionsAt(row, values.One).Count == 2 &&
                    strategyManager.ColumnPositionsAt(col, values.One).Count == 2)
                {
                    strategyManager.ChangeBuffer.ProposePossibilityRemoval(values.Two, opposite);
                    if (strategyManager.ChangeBuffer.Commit(this, new HiddenUniqueRectanglesReportBuilder(
                            cell, opposite, values.One)) && OnCommitBehavior == OnCommitBehavior.Return) return true;
                }
                
                if (strategyManager.RowPositionsAt(row, values.Two).Count == 2 &&
                    strategyManager.ColumnPositionsAt(col, values.Two).Count == 2)
                {
                    strategyManager.ChangeBuffer.ProposePossibilityRemoval(values.One, opposite);
                    if (strategyManager.ChangeBuffer.Commit(this, new HiddenUniqueRectanglesReportBuilder(
                            cell, opposite, values.One)) && OnCommitBehavior == OnCommitBehavior.Return) return true;
                }
            }
        }
        
        return false;
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

    public bool Equals(IReadOnlyPossibilities possibilities)
    {
        return possibilities.Count == 2 && possibilities.Peek(One) && possibilities.Peek(Two);
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

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
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

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
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

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
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

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            lighter.HighlightCell(_initial, ChangeColoration.CauseOffTwo);

            lighter.HighlightCell(_opposite, ChangeColoration.CauseOffOne);
            lighter.HighlightCell(_opposite.Row, _initial.Col, ChangeColoration.CauseOffOne);
            lighter.HighlightCell(_initial.Row, _opposite.Col, ChangeColoration.CauseOffOne);

            lighter.CreateLink(new CellPossibility(_opposite, _stronglyLinkedPossibility), new CellPossibility(
                _opposite.Row, _initial.Col, _stronglyLinkedPossibility), LinkStrength.Strong);
            lighter.CreateLink(new CellPossibility(_opposite, _stronglyLinkedPossibility), new CellPossibility(
                _initial.Row, _opposite.Col, _stronglyLinkedPossibility), LinkStrength.Strong);
            
            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}