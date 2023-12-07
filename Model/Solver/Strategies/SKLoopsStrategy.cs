using System.Collections.Generic;
using Global;
using Global.Enums;
using Model.Solver.Helpers.Changes;
using Model.Solver.Possibility;
using Model.Solver.StrategiesUtility;

namespace Model.Solver.Strategies;

public class SKLoopsStrategy : AbstractStrategy
{
    public const string OfficialName = "SK-Loops";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public SKLoopsStrategy() : base(OfficialName, StrategyDifficulty.Hard, DefaultBehavior) { }

    public override void Apply(IStrategyManager strategyManager)
    {
        for (int miniCol = 0; miniCol < 2; miniCol++)
        {
            for (int gridCol = 0; gridCol < 3; gridCol++)
            {
                for (int miniRow = 0; miniRow < 2; miniRow++)
                {
                    for (int gridRow = 0; gridRow < 3; gridRow++)
                    {
                        int row = miniRow * 3 + gridRow;
                        int col = miniCol * 3 + gridCol;

                        if (!IsCellValid(strategyManager, row, col)) continue;

                        for (int nextMiniCol = miniCol + 1; nextMiniCol < 3; nextMiniCol++)
                        {
                            for (int nextGridCol = 0; nextGridCol < 3; nextGridCol++)
                            {
                                int nextCol = nextMiniCol * 3 + nextGridCol;
                                
                                if(!IsCellValid(strategyManager, row, nextCol)) continue;

                                for (int nextMiniRow = miniRow + 1; nextMiniRow < 3; nextMiniRow++)
                                {
                                    for (int nextGridRow = 0; nextGridRow < 3; nextGridRow++)
                                    {
                                        int nextRow = nextMiniRow * 3 + nextGridRow;

                                        if (!IsCellValid(strategyManager, nextRow, col)
                                            || !IsCellValid(strategyManager, nextRow, nextCol)) continue;

                                        if (ConfirmPattern(strategyManager, new Cell(row, col), new Cell(row, nextCol),
                                                new Cell(nextRow, nextCol), new Cell(nextRow, col))) return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    
    private bool IsCellValid(IStrategyManager strategyManager, int row, int col)
    {
        if (strategyManager.Sudoku[row, col] == 0) return false;
        
        int startCol = col / 3 * 3;
        int startRow = row / 3 * 3;

        int countRow = 0;
        int countCol = 0;
        
        for (int gridCol = 0; gridCol < 3; gridCol++)
        {
            int crossCol = startCol + gridCol;

            if (strategyManager.Sudoku[row, crossCol] != 0) countRow++;
        }
        
        for (int gridRow = 0; gridRow < 3; gridRow++)
        {
            int crossRow = startRow + gridRow;

            if (strategyManager.Sudoku[crossRow, col] != 0) countCol++;
        }

        return countRow <= 2 && countCol <= 2;
    }

    private bool ConfirmPattern(IStrategyManager strategyManager, params Cell[] cells)
    {
        var one = CrossColPossibilities(strategyManager, cells[0]);
        var two = CrossColPossibilities(strategyManager, cells[3]);
        var and = one.Possibilities.And(two.Possibilities);
        if (and.Count == 0) return false;

        var combinations = EachCombination(and);
        
        foreach (var combination in combinations)
        {
            if (combination.Equals(one.Possibilities) || combination.Equals(two.Possibilities)) continue;
            if (IsLoop(strategyManager, cells, combination)) return true;
        }

        return false;
    }

    private static List<Possibilities> EachCombination(Possibilities possibilities)
    {
        List<Possibilities> result = new();
        EachCombination(result, possibilities, Possibilities.NewEmpty(), 0);
        return result;
    }

    private static void EachCombination(List<Possibilities> result, Possibilities total, Possibilities toSearch, int cursor)
    {
        while(total.Next(ref cursor))
        {
            if (toSearch.Peek(cursor)) continue;

            toSearch.Add(cursor);
            result.Add(toSearch.Copy());
            EachCombination(result, total, toSearch, cursor);
            toSearch.Remove(cursor);
        }
    }
    
    private bool IsLoop(IStrategyManager strategyManager, Cell[] cells, Possibilities start)
    {
        int possibilityCount = 0;
        int cellCount = 0;
        int total = cells.Length * 2;
        
        Possibilities[] links = new Possibilities[total];
        var poss = start;
        for (int i = 0; i < total; i++)
        {
            var pan = (i + 1) / 2 % 2 == 1
                ? CrossRowPossibilities(strategyManager, cells[i / 2])
                : CrossColPossibilities(strategyManager, cells[i / 2]);
            pan.Possibilities.Remove(poss);
            if (pan.Possibilities.Count == 0) return false;
            
            if (i == total - 1 && !pan.Possibilities.Equals(start)) return false;
            
            links[i] = pan.Possibilities;
            possibilityCount += pan.Possibilities.Count;
            cellCount += pan.Number;
            poss = pan.Possibilities;
        }

        if (possibilityCount > cellCount) return false;

        return ProcessPattern(strategyManager, cells, links);
    }

    private bool ProcessPattern(IStrategyManager strategyManager, Cell[] cells, Possibilities[] links)
    {
        foreach (var cell in cells)
        {
            var possibilities = CrossRowPossibilities(strategyManager, cell).Possibilities.And(
                CrossColPossibilities(strategyManager, cell).Possibilities);

            var startRow = cell.Row / 3 * 3;
            var startCol = cell.Column / 3 * 3;

            for (int r = 0; r < 3; r++)
            {
                var row = startRow + r;
                if (row == cell.Row) continue;

                for (int c = 0; c < 3; c++)
                {
                    var col = startCol + c;
                    if (col == cell.Column) continue;

                    foreach (var p in possibilities)
                    {
                        strategyManager.ChangeBuffer.ProposePossibilityRemoval(p, row, col);
                    }
                }
            }
        }

        for (int i = 0; i < 4; i++)
        {
            var one = cells[i];
            var two = cells[(i + 1) % 4];

            if (one.Row == two.Row)
            {
                var possibilities = CrossRowPossibilities(strategyManager, one).Possibilities.And(
                    CrossRowPossibilities(strategyManager, two).Possibilities);

                for (int column = 0; column < 9; column++)
                {
                    if (column / 3 == one.Column / 3 || column / 3 == two.Column / 3) continue;

                    foreach (var p in possibilities)
                    {
                        strategyManager.ChangeBuffer.ProposePossibilityRemoval(p, one.Row, column);
                    }
                }
            }
            else if (one.Column == two.Column)
            {
                var possibilities = CrossColPossibilities(strategyManager, one).Possibilities.And(
                    CrossColPossibilities(strategyManager, two).Possibilities);

                for (int row = 0; row < 9; row++)
                {
                    if (row / 3 == one.Row / 3 || row / 3 == two.Row / 3) continue;

                    foreach (var p in possibilities)
                    {
                        strategyManager.ChangeBuffer.ProposePossibilityRemoval(p, row, one.Column);
                    }
                }
            }
        }
        
        return strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
            new SKLoopsReportBuilder(cells, links)) && OnCommitBehavior == OnCommitBehavior.Return;
    }

    private PossibilitiesAndNumber CrossRowPossibilities(IStrategyManager strategyManager, Cell cell)
    {
        int startCol = cell.Column / 3 * 3;
        Possibilities result = Possibilities.NewEmpty();
        int count = 0;

        for (int gridCol = 0; gridCol < 3; gridCol++)
        {
            int crossCol = startCol + gridCol;
            if (crossCol == cell.Column) continue;

            var poss = strategyManager.PossibilitiesAt(cell.Row, crossCol);
            if (poss.Count > 0)
            {
                result.Add(poss);
                count++;
            }
        }

        return new PossibilitiesAndNumber(result, count);
    }

    private PossibilitiesAndNumber CrossColPossibilities(IStrategyManager strategyManager, Cell cell)
    {
        int startRow = cell.Row / 3 * 3;
        Possibilities result = Possibilities.NewEmpty();
        int count = 0;

        for (int gridRow = 0; gridRow < 3; gridRow++)
        {
            int crossRow = startRow + gridRow;
            if (crossRow == cell.Row) continue;

            var poss = strategyManager.PossibilitiesAt(crossRow, cell.Column);
            if (poss.Count > 0)
            {
                result.Add(poss);
                count++;
            }
        }

        return new PossibilitiesAndNumber(result, count);
    }
}

public class PossibilitiesAndNumber
{
    public PossibilitiesAndNumber(Possibilities possibilities, int number)
    {
        Possibilities = possibilities;
        Number = number;
    }

    public Possibilities Possibilities { get; }
    public int Number { get; }
}

public class SKLoopsReportBuilder : IChangeReportBuilder
{
    private readonly Cell[] _cells;
    private readonly Possibilities[] _links;

    public SKLoopsReportBuilder(Cell[] cells, Possibilities[] links)
    {
        _cells = cells;
        _links = links;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        List<CellPossibility> on = new();
        List<CellPossibility> off = new();

        for (int i = 0; i < _cells.Length * 2; i++)
        {
            var cells = (i + 1) / 2 % 2 == 1
                ? CrossRow(snapshot, _cells[i / 2])
                : CrossCol(snapshot, _cells[i / 2]);

            foreach (var cell in cells)
            {
                var before = i - 1;
                if (before < 0) before = _links.Length - 1;
                foreach (var possibility in _links[before])
                {
                    if(snapshot.PossibilitiesAt(cell).Peek(possibility))
                        on.Add(new CellPossibility(cell, possibility));
                }
            
                foreach (var possibility in _links[i])
                {
                    if(snapshot.PossibilitiesAt(cell).Peek(possibility))
                        off.Add(new CellPossibility(cell, possibility));
                }
            }
        }

        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            foreach (var cell in _cells)
            {
                lighter.HighlightCell(cell, ChangeColoration.CauseOffThree);
            }

            foreach (var cell in on)
            {
                lighter.HighlightPossibility(cell, ChangeColoration.CauseOnOne);
            }

            foreach (var cell in off)
            {
                lighter.HighlightPossibility(cell, ChangeColoration.CauseOffTwo);
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
    
    private IEnumerable<Cell> CrossRow(IPossibilitiesHolder snapshot, Cell cell)
    {
        int startCol = cell.Column / 3 * 3;

        for (int gridCol = 0; gridCol < 3; gridCol++)
        {
            int crossCol = startCol + gridCol;
            if (crossCol == cell.Column) continue;

            var current = new Cell(cell.Row, crossCol);
            if (snapshot.PossibilitiesAt(current).Count > 0) yield return current;
        }
    }

    private IEnumerable<Cell> CrossCol(IPossibilitiesHolder snapshot, Cell cell)
    {
        int startRow = cell.Row / 3 * 3;

        for (int gridRow = 0; gridRow < 3; gridRow++)
        {
            int crossRow = startRow + gridRow;
            if (crossRow == cell.Row) continue;

            var current = new Cell(crossRow, cell.Column);
            if (snapshot.PossibilitiesAt(current).Count > 0) yield return current;
        }
    }
}