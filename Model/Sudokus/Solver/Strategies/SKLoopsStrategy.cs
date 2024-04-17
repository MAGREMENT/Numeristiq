using System.Collections.Generic;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Highlighting;
using Model.Sudokus.Solver.StrategiesUtility;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies;

public class SKLoopsStrategy : SudokuStrategy
{
    public const string OfficialName = "SK-Loops";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    public SKLoopsStrategy() : base(OfficialName, StrategyDifficulty.Hard, DefaultInstanceHandling) { }

    public override void Apply(IStrategyUser strategyUser)
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

                        if (!IsCellValid(strategyUser, row, col)) continue;

                        for (int nextMiniCol = miniCol + 1; nextMiniCol < 3; nextMiniCol++)
                        {
                            for (int nextGridCol = 0; nextGridCol < 3; nextGridCol++)
                            {
                                int nextCol = nextMiniCol * 3 + nextGridCol;
                                
                                if(!IsCellValid(strategyUser, row, nextCol)) continue;

                                for (int nextMiniRow = miniRow + 1; nextMiniRow < 3; nextMiniRow++)
                                {
                                    for (int nextGridRow = 0; nextGridRow < 3; nextGridRow++)
                                    {
                                        int nextRow = nextMiniRow * 3 + nextGridRow;

                                        if (!IsCellValid(strategyUser, nextRow, col)
                                            || !IsCellValid(strategyUser, nextRow, nextCol)) continue;

                                        if (ConfirmPattern(strategyUser, new Cell(row, col), new Cell(row, nextCol),
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
    
    private bool IsCellValid(IStrategyUser strategyUser, int row, int col)
    {
        if (strategyUser.Sudoku[row, col] == 0) return false;
        
        int startCol = col / 3 * 3;
        int startRow = row / 3 * 3;

        int countRow = 0;
        int countCol = 0;
        
        for (int gridCol = 0; gridCol < 3; gridCol++)
        {
            int crossCol = startCol + gridCol;

            if (strategyUser.Sudoku[row, crossCol] != 0) countRow++;
        }
        
        for (int gridRow = 0; gridRow < 3; gridRow++)
        {
            int crossRow = startRow + gridRow;

            if (strategyUser.Sudoku[crossRow, col] != 0) countCol++;
        }

        return countRow <= 2 && countCol <= 2;
    }

    private bool ConfirmPattern(IStrategyUser strategyUser, params Cell[] cells)
    {
        var one = CrossColPossibilities(strategyUser, cells[0]);
        var two = CrossColPossibilities(strategyUser, cells[3]);
        var and = one.Possibilities & two.Possibilities;
        if (and.Count == 0) return false;

        var combinations = EachCombination(and);
        
        foreach (var combination in combinations)
        {
            if (combination.Equals(one.Possibilities) || combination.Equals(two.Possibilities)) continue;
            if (IsLoop(strategyUser, cells, combination)) return true;
        }

        return false;
    }

    private static List<ReadOnlyBitSet16> EachCombination(ReadOnlyBitSet16 possibilities)
    {
        List<ReadOnlyBitSet16> result = new();
        EachCombination(result, possibilities, new ReadOnlyBitSet16(), 0);
        return result;
    }

    private static void EachCombination(List<ReadOnlyBitSet16> result, ReadOnlyBitSet16 total, ReadOnlyBitSet16 toSearch, int cursor)
    {
        while(total.Next(ref cursor))
        {
            if (toSearch.Contains(cursor)) continue;

            toSearch += cursor;
            result.Add(toSearch);
            EachCombination(result, total, toSearch, cursor);
            toSearch -= cursor;
        }
    }
    
    private bool IsLoop(IStrategyUser strategyUser, Cell[] cells, ReadOnlyBitSet16 start)
    {
        int possibilityCount = 0;
        int cellCount = 0;
        int total = cells.Length * 2;
        
        var links = new ReadOnlyBitSet16[total];
        var poss = start;
        for (int i = 0; i < total; i++)
        {
            var pan = (i + 1) / 2 % 2 == 1
                ? CrossRowPossibilities(strategyUser, cells[i / 2])
                : CrossColPossibilities(strategyUser, cells[i / 2]);
            pan.RemoveToPossibilities(poss);
            if (pan.Possibilities.Count == 0) return false;
            
            if (i == total - 1 && !pan.Possibilities.Equals(start)) return false;
            
            links[i] = pan.Possibilities;
            possibilityCount += pan.Possibilities.Count;
            cellCount += pan.Number;
            poss = pan.Possibilities;
        }

        if (possibilityCount > cellCount) return false;

        return ProcessPattern(strategyUser, cells, links);
    }

    private bool ProcessPattern(IStrategyUser strategyUser, Cell[] cells, ReadOnlyBitSet16[] links)
    {
        var miniCol1 = cells[0].Column / 3;
        var miniCol2 = cells[1].Column / 3;
        
        for (int col = 0; col < 9; col++)
        {
            var miniCol = col / 3;
            if(miniCol == miniCol1 || miniCol == miniCol2) continue;

            foreach (var possibility in links[1].EnumeratePossibilities())
            {
                strategyUser.ChangeBuffer.ProposePossibilityRemoval(possibility, cells[0].Row, col);
            }
            
            foreach (var possibility in links[5].EnumeratePossibilities())
            {
                strategyUser.ChangeBuffer.ProposePossibilityRemoval(possibility, cells[2].Row, col);
            }
        }

        for (int gridRow = 0; gridRow < 3; gridRow++)
        {
            for (int gridCol = 0; gridCol < 3; gridCol++)
            {
                int row = cells[0].Row / 3 * 3 + gridRow;
                int col = cells[0].Column / 3 * 3 + gridCol;

                if (row != cells[0].Row && col != cells[0].Column)
                {
                    foreach (var possibility in links[0].EnumeratePossibilities())
                    {
                        strategyUser.ChangeBuffer.ProposePossibilityRemoval(possibility, row, col);
                    }
                }
                
                row = cells[1].Row / 3 * 3 + gridRow;
                col = cells[1].Column / 3 * 3 + gridCol;

                if (row != cells[1].Row && col != cells[1].Column)
                {
                    foreach (var possibility in links[2].EnumeratePossibilities())
                    {
                        strategyUser.ChangeBuffer.ProposePossibilityRemoval(possibility, row, col);
                    }
                }
                
                row = cells[2].Row / 3 * 3 + gridRow;
                col = cells[2].Column / 3 * 3 + gridCol;

                if (row != cells[2].Row && col != cells[2].Column)
                {
                    foreach (var possibility in links[4].EnumeratePossibilities())
                    {
                        strategyUser.ChangeBuffer.ProposePossibilityRemoval(possibility, row, col);
                    }
                }
                
                row = cells[3].Row / 3 * 3 + gridRow;
                col = cells[3].Column / 3 * 3 + gridCol;

                if (row != cells[3].Row && col != cells[3].Column)
                {
                    foreach (var possibility in links[6].EnumeratePossibilities())
                    {
                        strategyUser.ChangeBuffer.ProposePossibilityRemoval(possibility, row, col);
                    }
                }
            }
        }

        var miniRow1 = cells[1].Row / 3;
        var miniRow2 = cells[2].Row / 3;

        for (int row = 0; row < 9; row++)
        {
            var miniRow = row / 3;
            if (miniRow == miniRow1 || miniRow == miniRow2) continue;
            
            foreach (var possibility in links[3].EnumeratePossibilities())
            {
                strategyUser.ChangeBuffer.ProposePossibilityRemoval(possibility, row, cells[1].Column);
            }
            
            foreach (var possibility in links[7].EnumeratePossibilities())
            {
                strategyUser.ChangeBuffer.ProposePossibilityRemoval(possibility, row, cells[3].Column);
            }
        }
        
        return strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer.Commit(
            new SKLoopsReportBuilder(cells, links)) && StopOnFirstPush;
    }

    private PossibilitiesAndNumber CrossRowPossibilities(IStrategyUser strategyUser, Cell cell)
    {
        int startCol = cell.Column / 3 * 3;
        var result = new ReadOnlyBitSet16();
        int count = 0;

        for (int gridCol = 0; gridCol < 3; gridCol++)
        {
            int crossCol = startCol + gridCol;
            if (crossCol == cell.Column) continue;

            var poss = strategyUser.PossibilitiesAt(cell.Row, crossCol);
            if (poss.Count > 0)
            {
                result += poss;
                count++;
            }
        }

        return new PossibilitiesAndNumber(result, count);
    }

    private PossibilitiesAndNumber CrossColPossibilities(IStrategyUser strategyUser, Cell cell)
    {
        int startRow = cell.Row / 3 * 3;
        var result = new ReadOnlyBitSet16();
        int count = 0;

        for (int gridRow = 0; gridRow < 3; gridRow++)
        {
            int crossRow = startRow + gridRow;
            if (crossRow == cell.Row) continue;

            var poss = strategyUser.PossibilitiesAt(crossRow, cell.Column);
            if (poss.Count > 0)
            {
                result += poss;
                count++;
            }
        }

        return new PossibilitiesAndNumber(result, count);
    }
}

public class PossibilitiesAndNumber
{
    public PossibilitiesAndNumber(ReadOnlyBitSet16 possibilities, int number)
    {
        Possibilities = possibilities;
        Number = number;
    }

    public void RemoveToPossibilities(ReadOnlyBitSet16 set)
    {
        Possibilities -= set;
    }

    public ReadOnlyBitSet16 Possibilities { get; private set; }
    public int Number { get; }
}

public class SKLoopsReportBuilder : IChangeReportBuilder<IUpdatableSudokuSolvingState, ISudokuHighlighter>
{
    private readonly Cell[] _cells;
    private readonly ReadOnlyBitSet16[] _links;

    public SKLoopsReportBuilder(Cell[] cells, ReadOnlyBitSet16[] links)
    {
        _cells = cells;
        _links = links;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
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
                foreach (var possibility in _links[before].EnumeratePossibilities())
                {
                    if(snapshot.PossibilitiesAt(cell).Contains(possibility))
                        on.Add(new CellPossibility(cell, possibility));
                }
            
                foreach (var possibility in _links[i].EnumeratePossibilities())
                {
                    if(snapshot.PossibilitiesAt(cell).Contains(possibility))
                        off.Add(new CellPossibility(cell, possibility));
                }
            }
        }

        return new ChangeReport<ISudokuHighlighter>( "", lighter =>
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

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
    
    private IEnumerable<Cell> CrossRow(IUpdatableSudokuSolvingState snapshot, Cell cell)
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

    private IEnumerable<Cell> CrossCol(IUpdatableSudokuSolvingState snapshot, Cell cell)
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
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}
