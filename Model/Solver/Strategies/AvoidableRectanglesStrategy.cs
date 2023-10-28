using System.Collections.Generic;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Highlighting;
using Model.Solver.Possibilities;
using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.AlmostLockedSets;

namespace Model.Solver.Strategies;

public class AvoidableRectanglesStrategy : OriginalBoardBasedAbstractStrategy
{
    public const string OfficialName = "Avoidable Rectangles";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;

    public AvoidableRectanglesStrategy() : base(OfficialName, StrategyDifficulty.Hard, DefaultBehavior)
    {
        UniquenessDependency = UniquenessDependency.FullyDependent;
    }

    public override void ApplyOnce(IStrategyManager strategyManager)
    {
        for (int row = 0; row < 9; row++)
        {
            if (SearchForRowMatch(strategyManager, RowSolvedNumberPairs(strategyManager, row), row)) return;
        }

        for (int col = 0; col < 9; col++)
        {
            if (SearchForColumnMatch(strategyManager, ColumnSolvedNumberPairs(strategyManager, col), col)) return;
        }
    }

    private bool SearchForRowMatch(IStrategyManager strategyManager, List<SolvedNumber[]> pairs, int except)
    {
        for (int row = 0; row < 9; row++)
        {
            if (row == except) continue;

            foreach (var pair in pairs)
            {
                bool pairInSameBox = pair[0].Column / 3 == pair[1].Column / 3;
                bool rowInSameBox = row / 3 == except / 3;

                if (!(pairInSameBox ^ rowInSameBox)) continue;

                //Type 1
                if (strategyManager.Sudoku[row, pair[0].Column] == pair[1].Number &&
                    OriginalBoard[row, pair[0].Column] == 0)
                {
                    strategyManager.ChangeBuffer.ProposePossibilityRemoval(pair[0].Number, row, pair[1].Column);
                    if(strategyManager.ChangeBuffer.NotEmpty())strategyManager.ChangeBuffer.Commit(this,
                        new AvoidableRectanglesReportBuilder(pair, row, pair[0].Column));
                }


                if (strategyManager.Sudoku[row, pair[1].Column] == pair[0].Number &&
                    OriginalBoard[row, pair[1].Column] == 0)
                {
                    strategyManager.ChangeBuffer.ProposePossibilityRemoval(pair[1].Number, row, pair[0].Column);
                    if(strategyManager.ChangeBuffer.NotEmpty())strategyManager.ChangeBuffer.Commit(this,
                        new AvoidableRectanglesReportBuilder(pair, row, pair[1].Column));
                }
                
                //Type 2 and 3
                var possOne = strategyManager.PossibilitiesAt(row, pair[0].Column);
                var possTwo = strategyManager.PossibilitiesAt(row, pair[1].Column);
                if(!possOne.Peek(pair[1].Number) || !possTwo.Peek(pair[0].Number)) continue;

                //Type 2
                if (possOne.Count == 2 && possTwo.Count == 2)
                {
                    foreach (var possibility in possOne)
                    {
                        if (!possTwo.Peek(possibility)) continue;

                        foreach (var coord in Cells.SharedSeenCells(row, pair[0].Column,
                                     row, pair[1].Column))
                        {
                            strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, coord.Row, coord.Col);
                        }

                        if (strategyManager.ChangeBuffer.NotEmpty()){
                            strategyManager.ChangeBuffer.Commit(this,
                                new AvoidableRectanglesWithBiValuesReportBuilder(pair, row, pair[0].Column,
                                    row, pair[1].Column));
                            return OnCommitBehavior == OnCommitBehavior.Return;
                        }
                    }
                }
                
                //Type 3
                var shared = new List<Cell>(Cells.SharedSeenEmptyCells(strategyManager, row, pair[0].Column,
                    row, pair[1].Column));
                var mashed = possOne.Or(possTwo);
                mashed.Remove(pair[0].Number);
                mashed.Remove(pair[1].Number);

                foreach (var als in AlmostLockedSetSearcher.InCells(strategyManager, shared, 4))
                {
                    if (als.Possibilities.Equals(mashed))
                        RemovePossibilitiesInAllExcept(strategyManager, mashed, shared, als);
                    if (strategyManager.ChangeBuffer.NotEmpty())
                    {
                        strategyManager.ChangeBuffer.Commit(this,
                            new AvoidableRectanglesWithAlsReportBuilder(pair, als));
                        return OnCommitBehavior == OnCommitBehavior.Return;
                    }
                }
            }
        }

        return false;
    }

    private List<SolvedNumber[]> RowSolvedNumberPairs(IStrategyManager strategyManager, int row)
    {
        List<SolvedNumber[]> result = new();

        for (int col = 0; col < 9; col++)
        {
            if (strategyManager.Sudoku[row, col] != 0 && OriginalBoard[row, col] == 0) 
                SearchRowForPairs(strategyManager, row, col + 1,
                    new SolvedNumber(strategyManager.Sudoku[row, col], row, col), result);
        }

        return result;
    }

    private void SearchRowForPairs(IStrategyManager strategyManager, int row, int start,
        SolvedNumber current, List<SolvedNumber[]> result)
    {
        for (int col = start; col < 9; col++)
        {
            if (strategyManager.Sudoku[row, col] != 0 && OriginalBoard[row, col] == 0)
            {
                result.Add(new []{current, new SolvedNumber(strategyManager.Sudoku[row, col], row, col)});
            }
        }
    }
    
    private bool SearchForColumnMatch(IStrategyManager strategyManager, List<SolvedNumber[]> pairs, int except)
    {
        for (int col = 0; col < 9; col++)
        {
            if (col == except) continue;

            foreach (var pair in pairs)
            {
                bool pairInSameBox = pair[0].Row / 3 == pair[1].Row / 3;
                bool colInSameBox = col / 3 == except / 3;

                if (!(pairInSameBox ^ colInSameBox)) continue;

                //Type 1
                if (strategyManager.Sudoku[pair[0].Row, col] == pair[1].Number &&
                    OriginalBoard[pair[0].Row, col] == 0)
                {
                    strategyManager.ChangeBuffer.ProposePossibilityRemoval(pair[0].Number, pair[1].Row, col);
                    if(strategyManager.ChangeBuffer.NotEmpty())strategyManager.ChangeBuffer.Commit(this,
                        new AvoidableRectanglesReportBuilder(pair, pair[0].Row, col));
                }


                if (strategyManager.Sudoku[pair[1].Row, col] == pair[0].Number &&
                    OriginalBoard[pair[1].Row, col] == 0)
                {
                    strategyManager.ChangeBuffer.ProposePossibilityRemoval(pair[1].Number, pair[0].Row, col);
                    if(strategyManager.ChangeBuffer.NotEmpty())strategyManager.ChangeBuffer.Commit(this,
                        new AvoidableRectanglesReportBuilder(pair, pair[1].Row, col));
                }
                
                //Type 2 and 3
                var possOne = strategyManager.PossibilitiesAt(pair[0].Row, col);
                var possTwo = strategyManager.PossibilitiesAt(pair[1].Row, col);
                if (!possOne.Peek(pair[1].Number) || !possTwo.Peek(pair[0].Number)) continue;

                //Type 2
                if (possOne.Count == 2 && possTwo.Count == 2)
                {
                    foreach (var possibility in possOne)
                    {
                        if (!possTwo.Peek(possibility)) continue;

                        foreach (var coord in Cells.SharedSeenCells(pair[0].Row, col,
                                     pair[1].Row, col))
                        {
                            strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, coord.Row, coord.Col);
                        }

                        if (strategyManager.ChangeBuffer.NotEmpty()){
                            strategyManager.ChangeBuffer.Commit(this,
                                new AvoidableRectanglesWithBiValuesReportBuilder(pair, pair[0].Row, col,
                                    pair[1].Row, col));
                            return OnCommitBehavior == OnCommitBehavior.Return;
                        }
                    }
                }
                
                //Type 3
                var shared = new List<Cell>(Cells.SharedSeenEmptyCells(strategyManager, pair[0].Row, col,
                    pair[1].Row, col));
                var mashed = possOne.Or(possTwo);
                mashed.Remove(pair[0].Number);
                mashed.Remove(pair[1].Number);

                foreach (var als in AlmostLockedSetSearcher.InCells(strategyManager, shared, 4))
                {
                    if (als.Possibilities.Equals(mashed))
                        RemovePossibilitiesInAllExcept(strategyManager, mashed, shared, als);
                    if (strategyManager.ChangeBuffer.NotEmpty())
                    {
                        strategyManager.ChangeBuffer.Commit(this,
                            new AvoidableRectanglesWithAlsReportBuilder(pair, als));
                        return OnCommitBehavior == OnCommitBehavior.Return;
                    }
                        
                }
            }
        }

        return false;
    }

    private List<SolvedNumber[]> ColumnSolvedNumberPairs(IStrategyManager strategyManager, int col)
    {
        List<SolvedNumber[]> result = new();

        for (int row = 0; row < 9; row++)
        {
            if (strategyManager.Sudoku[row, col] != 0 && OriginalBoard[row, col] == 0) 
                SearchColumnForPairs(strategyManager, col, row + 1,
                    new SolvedNumber(strategyManager.Sudoku[row, col], row, col), result);
        }

        return result;
    }

    private void SearchColumnForPairs(IStrategyManager strategyManager, int col, int start,
        SolvedNumber current, List<SolvedNumber[]> result)
    {
        for (int row = start; row < 9; row++)
        {
            if (strategyManager.Sudoku[row, col] != 0 && OriginalBoard[row, col] == 0)
            {
                result.Add(new []{current, new SolvedNumber(strategyManager.Sudoku[row, col], row, col)});
            }
        }
    }

    private void RemovePossibilitiesInAllExcept(IStrategyManager view, IPossibilities poss, List<Cell> coords,
        AlmostLockedSet except)
    {
        foreach (var coord in coords)
        {
            if(except.Contains(coord) || !except.ShareAUnit(coord)) continue;
            foreach (var possibility in poss)
            {
                view.ChangeBuffer.ProposePossibilityRemoval(possibility, coord.Row, coord.Col);
            }
        }
    }
}

public readonly struct SolvedNumber
{
    public int Number { get; }
    public int Row { get; }
    public int Column { get; }
    
    public SolvedNumber(int number, int row, int column)
    {
        Number = number;
        Row = row;
        Column = column;
    }
}

public class AvoidableRectanglesReportBuilder : IChangeReportBuilder
{
    private readonly SolvedNumber[] _pair;
    private readonly int _row;
    private readonly int _col;

    public AvoidableRectanglesReportBuilder(SolvedNumber[] pair, int row, int col)
    {
        _pair = pair;
        _row = row;
        _col = col;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "",
            lighter =>
            {
                foreach (var single in _pair)
                {
                    lighter.HighlightCell(single.Row, single.Column, ChangeColoration.CauseOffOne);
                }
                lighter.HighlightCell(_row, _col, ChangeColoration.CauseOffOne);
                
                IChangeReportBuilder.HighlightChanges(lighter, changes);
            });
    }
}

public class AvoidableRectanglesWithBiValuesReportBuilder : IChangeReportBuilder
{
    private readonly SolvedNumber[] _pair;
    private readonly int _row1;
    private readonly int _col1;
    private readonly int _row2;
    private readonly int _col2;

    public AvoidableRectanglesWithBiValuesReportBuilder(SolvedNumber[] pair, int row1, int col1, int row2, int col2)
    {
        _pair = pair;
        _row1 = row1;
        _col1 = col1;
        _row2 = row2;
        _col2 = col2;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "",
            lighter =>
            {
                foreach (var single in _pair)
                {
                    lighter.HighlightCell(single.Row, single.Column, ChangeColoration.CauseOffOne);
                }
                
                lighter.HighlightPossibility(_pair[1].Number, _row1, _col1, ChangeColoration.CauseOffOne);
                lighter.HighlightPossibility(_pair[0].Number, _row2, _col2, ChangeColoration.CauseOffOne);
                
                IChangeReportBuilder.HighlightChanges(lighter, changes);
            });
    }
}

public class AvoidableRectanglesWithAlsReportBuilder : IChangeReportBuilder
{
    private readonly SolvedNumber[] _pair;
    private readonly AlmostLockedSet _als;

    public AvoidableRectanglesWithAlsReportBuilder(SolvedNumber[] pair, AlmostLockedSet als)
    {
        _pair = pair;
        _als = als;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "",
            lighter =>
            {
                foreach (var single in _pair)
                {
                    lighter.HighlightCell(single.Row, single.Column, ChangeColoration.CauseOffOne);
                }

                foreach (var coord in _als.Cells)
                {
                    lighter.HighlightCell(coord.Row, coord.Col, ChangeColoration.CauseOffTwo);
                }
                
                IChangeReportBuilder.HighlightChanges(lighter, changes);
            });
    }
}